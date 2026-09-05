using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using SmartBuy.Core.Common;
using SmartBuy.Core.Common.Responses;
using SmartBuy.Core.Interfaces.Repositories;
using SmartBuy.Core.Interfaces.Services;
using SmartBuy.Core.Models;
using SmartBuy.Core.Models.Catalogo;
using SmartBuy.Core.Models.Historico;

namespace SmartBuy.Core.Services
{
    /// <summary>
    /// ABM del catálogo maestro. El catálogo es la pieza que hace posible la
    /// comparación entre cadenas: acá se cuida su calidad (validaciones, EAN
    /// único, baja lógica).
    /// </summary>
    public partial class ProductoServices : IProductoServices
    {
        private const int LimitDefault = 50;
        private const int LimitMax = 200;
        private static readonly string[] UnidadesValidas = { "L", "ml", "kg", "g", "un" };

        [GeneratedRegex(@"^\d{8,14}$")]
        private static partial Regex EanRegex();

        private readonly IProductoRepository _productoRepository;
        private readonly IPublicacionRepository _publicacionRepository;
        private readonly ILogger<ProductoServices> _logger;

        public ProductoServices(
            IProductoRepository productoRepository,
            IPublicacionRepository publicacionRepository,
            ILogger<ProductoServices> logger)
        {
            _productoRepository = productoRepository;
            _publicacionRepository = publicacionRepository;
            _logger = logger;
        }

        public Task<StandarResponse<List<ProductoListado>>> GetAllProductosAsync(string? filtro, int? limit, int? offset, CancellationToken cancellationToken)
        {
            var filtroNormalizado = string.IsNullOrWhiteSpace(filtro) ? null : filtro.Trim();
            var limitNormalizado = Math.Clamp(limit ?? LimitDefault, 1, LimitMax);
            var offsetNormalizado = Math.Max(offset ?? 0, 0);

            return _productoRepository.GetAllProductosAsync(filtroNormalizado, limitNormalizado, offsetNormalizado, cancellationToken);
        }

        public async Task<StandarResponse<ProductoDetalle>> GetProductoByIdAsync(long id, CancellationToken cancellationToken)
        {
            if (id <= 0)
                return Fallo<ProductoDetalle>("El id de producto es obligatorio.");

            var respuesta = await _productoRepository.GetProductoByIdAsync(id, cancellationToken);

            if (!respuesta.Success)
                return new StandarResponse<ProductoDetalle> { Success = false, Errors = respuesta.Errors, Execution = respuesta.Execution };

            var producto = respuesta.Result?.FirstOrDefault();

            if (producto == null || producto.Id <= 0)
                return Fallo<ProductoDetalle>($"No existe el producto {id}.");

            return new StandarResponse<ProductoDetalle> { Success = true, Result = producto, Execution = respuesta.Execution };
        }

        public async Task<StandarResponse<HistoricoProducto>> GetHistoricoAsync(long productoId, int? dias, bool conPromos, CancellationToken cancellationToken)
        {
            if (productoId <= 0)
                return Fallo<HistoricoProducto>("El id de producto es obligatorio.");

            var ventana = Math.Clamp(dias ?? 90, 7, 365);

            var producto = await GetProductoByIdAsync(productoId, cancellationToken);
            if (!producto.Success || producto.Result == null)
                return new StandarResponse<HistoricoProducto> { Success = false, Errors = producto.Errors };

            var puntos = await _productoRepository.GetHistoricoProductoAsync(productoId, ventana, conPromos, cancellationToken);
            if (!puntos.Success)
                return new StandarResponse<HistoricoProducto> { Success = false, Errors = puntos.Errors, Execution = puntos.Execution };

            var filas = puntos.Result ?? new List<HistoricoPrecioPunto>();

            var series = filas
                .GroupBy(p => new { p.CadenaId, p.Cadena })
                .Select(g => new HistoricoSerieCadena
                {
                    CadenaId = g.Key.CadenaId,
                    Cadena = g.Key.Cadena,
                    Puntos = g.OrderBy(p => p.Fecha)
                        .Select(p => new HistoricoPunto { Fecha = p.Fecha, Precio = p.Precio })
                        .ToList()
                })
                .OrderBy(s => s.Cadena)
                .ToList();

            // La señal se calcula sobre el mejor precio diario ENTRE cadenas:
            // lo que efectivamente pagarías ese día repartiendo.
            var mejoresPorDia = filas
                .Select(p => new HistoricoPunto { Fecha = p.Fecha, Precio = p.Precio })
                .ToList();

            var resumen = new HistoricoProducto
            {
                ProductoId = productoId,
                Producto = producto.Result.Nombre,
                Dias = ventana,
                Series = series,
                Senal = SenalCompra.Calcular(mejoresPorDia, ventana)
            };

            return new StandarResponse<HistoricoProducto> { Success = true, Result = resumen };
        }

        public async Task<StandarResponse<IdDto>> CrearProductoAsync(GuardarProductoRequest request, CancellationToken cancellationToken)
        {
            var errores = Validar(request, esEdicion: false);
            if (errores.Count > 0)
                return Fallo<IdDto>(errores);

            var respuesta = await _productoRepository.CrearProductoAsync(Normalizar(request), cancellationToken);

            var resultado = TraducirErroresDeBase(respuesta);

            if (resultado.Success)
                await MatchearPendientesDelEanAsync(request.Ean, cancellationToken);

            return resultado;
        }

        public async Task<StandarResponse<IdDto>> ActualizarProductoAsync(GuardarProductoRequest request, CancellationToken cancellationToken)
        {
            var errores = Validar(request, esEdicion: true);
            if (errores.Count > 0)
                return Fallo<IdDto>(errores);

            var respuesta = await _productoRepository.ActualizarProductoAsync(Normalizar(request), cancellationToken);

            if (respuesta.Success && (respuesta.Result == null || respuesta.Result.Id <= 0))
                return Fallo<IdDto>($"No existe el producto {request.Id} o está dado de baja.");

            var resultado = TraducirErroresDeBase(respuesta);

            if (resultado.Success)
                await MatchearPendientesDelEanAsync(request.Ean, cancellationToken);

            return resultado;
        }

        /// <summary>
        /// Hook post alta/edición: si el producto tiene EAN, re-matchea al toque
        /// las publicaciones pendientes ya capturadas con ese EAN (el matching de
        /// la ingesta solo corre al capturar). Un fallo acá nunca voltea el alta:
        /// se loguea y el re-matcheo global queda como red de seguridad.
        /// </summary>
        private async Task MatchearPendientesDelEanAsync(string? ean, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(ean))
                return;

            try
            {
                var matcheo = await _publicacionRepository.MatchearPendientesPorEanAsync(ean, cancellationToken);

                if (matcheo.Success && matcheo.Result is { Cantidad: > 0 })
                    _logger.LogInformation("Re-matcheo por EAN {Ean}: {Cantidad} publicaciones pendientes engancharon.", ean, matcheo.Result.Cantidad);
                else if (!matcheo.Success)
                    _logger.LogWarning("Re-matcheo por EAN {Ean} falló: {Errores}", ean, string.Join(" | ", matcheo.Errors));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Re-matcheo por EAN {Ean}: error no controlado.", ean);
            }
        }

        public async Task<StandarResponse<IdDto>> EliminarProductoAsync(long id, CancellationToken cancellationToken)
        {
            if (id <= 0)
                return Fallo<IdDto>("El id de producto es obligatorio.");

            var respuesta = await _productoRepository.EliminarProductoAsync(id, cancellationToken);

            if (respuesta.Success && (respuesta.Result == null || respuesta.Result.Id <= 0))
                return Fallo<IdDto>($"No existe el producto {id} o ya estaba dado de baja.");

            return respuesta;
        }

        /// <summary>
        /// Generación masiva de catálogo desde la cola: un producto (curado=false)
        /// por cada EAN pendiente presente en minCadenas o más cadenas, y después
        /// el re-matcheo retroactivo global para que las publicaciones enganchen.
        /// Idempotente: re-ejecutar no duplica (unique de ean) y devuelve 0.
        /// </summary>
        public async Task<StandarResponse<GeneracionPendientesResumen>> GenerarDesdePendientesAsync(int? minCadenas, CancellationToken cancellationToken)
        {
            var minimo = Math.Clamp(minCadenas ?? 2, 1, 10);

            var creacion = await _productoRepository.GenerarDesdePendientesAsync(minimo, cancellationToken);

            if (!creacion.Success)
                return new StandarResponse<GeneracionPendientesResumen> { Success = false, Errors = creacion.Errors, Execution = creacion.Execution };

            var resumen = new GeneracionPendientesResumen { ProductosCreados = creacion.Result?.Cantidad ?? 0 };

            var matcheo = await _publicacionRepository.MatchearPendientesPorEanAsync(null, cancellationToken);

            if (matcheo.Success)
                resumen.PublicacionesMatcheadas = matcheo.Result?.Cantidad ?? 0;
            else
                // Los productos ya quedaron creados; el re-matcheo global a demanda
                // (MatchearPendientesPorEan) es la recuperación si esto falla.
                _logger.LogWarning("GenerarDesdePendientes: los productos se crearon pero el re-matcheo falló: {Errores}", string.Join(" | ", matcheo.Errors));

            // Los productos generados nacen sin contenido: el parser lo completa
            // acá mismo desde el nombre (curado sigue false: la curación confirma).
            var contenidos = await CompletarContenidosAsync(cancellationToken);
            if (contenidos.Success && contenidos.Result != null)
                resumen.ContenidosCompletados = contenidos.Result.Completados;

            _logger.LogInformation("GenerarDesdePendientes (minCadenas={Min}): {Productos} productos creados, {Matcheadas} publicaciones matcheadas, {Contenidos} contenidos completados.",
                minimo, resumen.ProductosCreados, resumen.PublicacionesMatcheadas, resumen.ContenidosCompletados);

            return new StandarResponse<GeneracionPendientesResumen> { Success = true, Result = resumen };
        }

        public async Task<StandarResponse<ContenidosResumen>> CompletarContenidosAsync(CancellationToken cancellationToken)
        {
            var pendientes = await _productoRepository.GetProductosSinContenidoAsync(cancellationToken);

            if (!pendientes.Success)
                return new StandarResponse<ContenidosResumen> { Success = false, Errors = pendientes.Errors };

            var resumen = new ContenidosResumen();

            // El parser corre en memoria; a la base va UN solo lote. (Lección del
            // 06/09: el update de a uno sobre ~900 productos superaba el timeout
            // del proxy y quedaba a medias.)
            var lote = new List<(long Id, decimal Valor, string Unidad)>();

            foreach (var producto in pendientes.Result ?? new List<ProductoSinContenido>())
            {
                if (producto.Id <= 0)
                    continue;

                resumen.Revisados++;

                var contenido = ContenidoParser.Parsear(producto.Nombre);

                if (contenido == null)
                {
                    resumen.SinReconocer++;
                    continue;
                }

                lote.Add((producto.Id, contenido.Value.Valor, contenido.Value.Unidad));
            }

            if (lote.Count > 0)
            {
                var actualizacion = await _productoRepository.ActualizarContenidosLoteAsync(lote, cancellationToken);

                if (!actualizacion.Success)
                    return new StandarResponse<ContenidosResumen> { Success = false, Errors = actualizacion.Errors };

                resumen.Completados = (int)(actualizacion.Result?.Cantidad ?? 0);
            }

            _logger.LogInformation("CompletarContenidos: {Revisados} revisados, {Completados} completados, {SinReconocer} sin gramaje reconocible.",
                resumen.Revisados, resumen.Completados, resumen.SinReconocer);

            return new StandarResponse<ContenidosResumen> { Success = true, Result = resumen };
        }

        public Task<StandarResponse<List<Marca>>> GetAllMarcasAsync(CancellationToken cancellationToken)
            => _productoRepository.GetAllMarcasAsync(cancellationToken);

        public Task<StandarResponse<IdDto>> CrearMarcaAsync(CrearMarcaRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request?.Nombre) || request.Nombre.Trim().Length > 100)
                return Task.FromResult(Fallo<IdDto>("El nombre de la marca es obligatorio (máximo 100 caracteres)."));

            // Alta idempotente: si ya existe devuelve el id existente (ver crear_marca.sql).
            return _productoRepository.CrearMarcaAsync(request.Nombre.Trim(), cancellationToken);
        }

        public Task<StandarResponse<List<CategoriaNodo>>> GetAllCategoriasAsync(CancellationToken cancellationToken)
            => _productoRepository.GetAllCategoriasAsync(cancellationToken);

        private static List<string> Validar(GuardarProductoRequest? request, bool esEdicion)
        {
            var errores = new List<string>();

            if (request == null)
                return new List<string> { "El cuerpo del producto es obligatorio." };

            if (esEdicion && request.Id <= 0)
                errores.Add("id es obligatorio para editar.");

            if (string.IsNullOrWhiteSpace(request.Nombre) || request.Nombre.Trim().Length > 200)
                errores.Add("nombre es obligatorio (máximo 200 caracteres).");

            // Contenido: valor y unidad van juntos o no van (mismo check que la base,
            // validado acá para devolver un mensaje claro en vez de un error de constraint).
            if (request.ContenidoValor.HasValue != !string.IsNullOrWhiteSpace(request.ContenidoUnidad))
                errores.Add("contenidoValor y contenidoUnidad van juntos: o se informan ambos o ninguno.");

            if (request.ContenidoValor is <= 0)
                errores.Add("contenidoValor debe ser mayor a cero.");

            if (!string.IsNullOrWhiteSpace(request.ContenidoUnidad) && !UnidadesValidas.Contains(request.ContenidoUnidad))
                errores.Add($"contenidoUnidad debe ser una de: {string.Join(", ", UnidadesValidas)}.");

            if (request.Ean != null && !EanRegex().IsMatch(request.Ean))
                errores.Add("ean debe tener entre 8 y 14 dígitos.");

            if (request.MarcaId is <= 0)
                errores.Add("marcaId inválido.");

            if (request.CategoriaId is <= 0)
                errores.Add("categoriaId inválido.");

            return errores;
        }

        private static GuardarProductoRequest Normalizar(GuardarProductoRequest request)
        {
            request.Nombre = request.Nombre.Trim();
            request.Ean = string.IsNullOrWhiteSpace(request.Ean) ? null : request.Ean.Trim();
            request.ContenidoUnidad = string.IsNullOrWhiteSpace(request.ContenidoUnidad) ? null : request.ContenidoUnidad.Trim();
            return request;
        }

        /// <summary>
        /// Traduce errores de constraints a mensajes claros. 23505 (unique de ean)
        /// es la única defensa real contra dos altas simultáneas con el mismo EAN;
        /// 23503 (FK) cubre marca/categoría inexistentes sin un round-trip previo.
        /// </summary>
        private static StandarResponse<IdDto> TraducirErroresDeBase(StandarResponse<IdDto> respuesta)
        {
            if (respuesta.Success)
                return respuesta;

            if (respuesta.Errors.Any(e => e.Contains("23505")))
                return Fallo<IdDto>("Ya existe un producto con ese EAN.");

            if (respuesta.Errors.Any(e => e.Contains("23503")))
                return Fallo<IdDto>("La marca o la categoría indicada no existe.");

            return respuesta;
        }

        private static StandarResponse<T> Fallo<T>(string error)
            => new() { Success = false, Errors = new List<string> { error } };

        private static StandarResponse<T> Fallo<T>(List<string> errores)
            => new() { Success = false, Errors = errores };
    }
}
