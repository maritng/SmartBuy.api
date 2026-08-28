using System.Text.RegularExpressions;
using SmartBuy.Core.Common.Responses;
using SmartBuy.Core.Interfaces.Repositories;
using SmartBuy.Core.Interfaces.Services;
using SmartBuy.Core.Models;
using SmartBuy.Core.Models.Catalogo;

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

        public ProductoServices(IProductoRepository productoRepository)
        {
            _productoRepository = productoRepository;
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

        public async Task<StandarResponse<IdDto>> CrearProductoAsync(GuardarProductoRequest request, CancellationToken cancellationToken)
        {
            var errores = Validar(request, esEdicion: false);
            if (errores.Count > 0)
                return Fallo<IdDto>(errores);

            var respuesta = await _productoRepository.CrearProductoAsync(Normalizar(request), cancellationToken);

            return TraducirErroresDeBase(respuesta);
        }

        public async Task<StandarResponse<IdDto>> ActualizarProductoAsync(GuardarProductoRequest request, CancellationToken cancellationToken)
        {
            var errores = Validar(request, esEdicion: true);
            if (errores.Count > 0)
                return Fallo<IdDto>(errores);

            var respuesta = await _productoRepository.ActualizarProductoAsync(Normalizar(request), cancellationToken);

            if (respuesta.Success && (respuesta.Result == null || respuesta.Result.Id <= 0))
                return Fallo<IdDto>($"No existe el producto {request.Id} o está dado de baja.");

            return TraducirErroresDeBase(respuesta);
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
