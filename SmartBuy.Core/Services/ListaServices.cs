using SmartBuy.Core.Common.Responses;
using SmartBuy.Core.Interfaces.Repositories;
using SmartBuy.Core.Interfaces.Services;
using SmartBuy.Core.Models;
using SmartBuy.Core.Models.Listas;
using SmartBuy.Core.Models.Usuarios;

namespace SmartBuy.Core.Services
{
    /// <summary>
    /// Listas guardadas y preferencia de cadenas. El usuarioId llega siempre
    /// del token (lo resuelve el controller); acá jamás se confía en un id de
    /// usuario venido del cliente.
    /// </summary>
    public class ListaServices : IListaServices
    {
        private const int MaxItems = 100;
        private const int MaxCantidad = 999;

        private readonly IListaRepository _listaRepository;
        private readonly IUsuarioRepository _usuarioRepository;

        public ListaServices(IListaRepository listaRepository, IUsuarioRepository usuarioRepository)
        {
            _listaRepository = listaRepository;
            _usuarioRepository = usuarioRepository;
        }

        public Task<StandarResponse<List<ListaResumen>>> GetMisListasAsync(long usuarioId, CancellationToken cancellationToken)
            => _listaRepository.GetMisListasAsync(usuarioId, cancellationToken);

        public async Task<StandarResponse<ListaDetalle>> GetListaAsync(long usuarioId, long listaId, CancellationToken cancellationToken)
        {
            if (listaId <= 0)
                return Fallo<ListaDetalle>("El id de lista es obligatorio.");

            var filas = await _listaRepository.GetListaItemsAsync(usuarioId, listaId, cancellationToken);

            if (!filas.Success)
                return new StandarResponse<ListaDetalle> { Success = false, Errors = filas.Errors };

            var primera = filas.Result?.FirstOrDefault();

            if (primera == null || primera.ListaId <= 0)
                return Fallo<ListaDetalle>($"La lista {listaId} no existe o no es tuya.");

            return new StandarResponse<ListaDetalle>
            {
                Success = true,
                Result = new ListaDetalle
                {
                    Id = primera.ListaId,
                    Nombre = primera.ListaNombre,
                    // Lista vacía = 1 fila con ProductoId null (LEFT JOIN).
                    Items = filas.Result!
                        .Where(f => f.ProductoId is > 0)
                        .Select(f => new ListaDetalleItem
                        {
                            ProductoId = f.ProductoId!.Value,
                            Producto = f.Producto ?? string.Empty,
                            Cantidad = f.Cantidad ?? 1
                        })
                        .ToList()
                }
            };
        }

        public async Task<StandarResponse<IdDto>> CrearListaAsync(long usuarioId, GuardarListaRequest request, CancellationToken cancellationToken)
        {
            var errores = Validar(request);
            if (errores.Count > 0)
                return Fallo<IdDto>(errores);

            var creacion = await _listaRepository.CrearListaAsync(usuarioId, request.Nombre.Trim(), cancellationToken);

            if (!creacion.Success)
                return TraducirErrores(creacion);

            if (request.Items.Count > 0)
            {
                var carga = await _listaRepository.InsertarItemsAsync(creacion.Result!.Id, request.Items, cancellationToken);
                if (!carga.Success)
                    return TraducirErrores(new StandarResponse<IdDto> { Success = false, Errors = carga.Errors });
            }

            return creacion;
        }

        public async Task<StandarResponse<IdDto>> GuardarListaAsync(long usuarioId, GuardarListaRequest request, CancellationToken cancellationToken)
        {
            var errores = Validar(request);
            if (request.Id <= 0)
                errores.Add("El id de lista es obligatorio para guardar.");
            if (errores.Count > 0)
                return Fallo<IdDto>(errores);

            var cabecera = await _listaRepository.GuardarCabeceraAsync(usuarioId, request.Id, request.Nombre.Trim(), cancellationToken);

            if (!cabecera.Success)
                return TraducirErrores(cabecera);

            if (cabecera.Result == null || cabecera.Result.Id <= 0)
                return Fallo<IdDto>($"La lista {request.Id} no existe o no es tuya.");

            if (request.Items.Count > 0)
            {
                var carga = await _listaRepository.InsertarItemsAsync(request.Id, request.Items, cancellationToken);
                if (!carga.Success)
                    return TraducirErrores(new StandarResponse<IdDto> { Success = false, Errors = carga.Errors });
            }

            return cabecera;
        }

        public async Task<StandarResponse<IdDto>> EliminarListaAsync(long usuarioId, long listaId, CancellationToken cancellationToken)
        {
            if (listaId <= 0)
                return Fallo<IdDto>("El id de lista es obligatorio.");

            var borrado = await _listaRepository.EliminarListaAsync(usuarioId, listaId, cancellationToken);

            if (borrado.Success && (borrado.Result == null || borrado.Result.Id <= 0))
                return Fallo<IdDto>($"La lista {listaId} no existe o no es tuya.");

            return borrado;
        }

        public async Task<StandarResponse<List<long>>> GetMisCadenasAsync(long usuarioId, CancellationToken cancellationToken)
        {
            var respuesta = await _usuarioRepository.GetMisCadenasAsync(usuarioId, cancellationToken);

            return new StandarResponse<List<long>>
            {
                Success = respuesta.Success,
                Errors = respuesta.Errors,
                Result = respuesta.Result?.Where(c => c.CadenaId > 0).Select(c => c.CadenaId).ToList() ?? new List<long>()
            };
        }

        public Task<StandarResponse<CantidadDto>> GuardarMisCadenasAsync(long usuarioId, MisCadenasRequest request, CancellationToken cancellationToken)
        {
            var cadenas = request?.CadenasIds?.Distinct().ToList() ?? new List<long>();

            if (cadenas.Any(id => id <= 0) || cadenas.Count > 20)
                return Task.FromResult(Fallo<CantidadDto>("cadenasIds inválidas."));

            return _usuarioRepository.GuardarMisCadenasAsync(usuarioId, cadenas, cancellationToken);
        }

        private static List<string> Validar(GuardarListaRequest? request)
        {
            var errores = new List<string>();

            if (request == null)
                return new List<string> { "El cuerpo de la lista es obligatorio." };

            if (string.IsNullOrWhiteSpace(request.Nombre) || request.Nombre.Trim().Length > 100)
                errores.Add("El nombre de la lista es obligatorio (máximo 100 caracteres).");

            request.Items ??= new List<GuardarListaItem>();

            if (request.Items.Count > MaxItems)
                errores.Add($"La lista supera el máximo de {MaxItems} productos.");

            if (request.Items.Any(i => i.ProductoId <= 0))
                errores.Add("Todos los ítems deben tener productoId mayor a cero.");

            if (request.Items.Any(i => i.Cantidad <= 0 || i.Cantidad > MaxCantidad))
                errores.Add($"cantidad debe estar entre 1 y {MaxCantidad}.");

            var repetidos = request.Items.GroupBy(i => i.ProductoId).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            if (repetidos.Count > 0)
                errores.Add($"Productos repetidos en la lista: {string.Join(", ", repetidos)}.");

            return errores;
        }

        /// <summary>23505 (unique usuario+nombre) y 23503 (FK producto) a mensajes claros.</summary>
        private static StandarResponse<IdDto> TraducirErrores(StandarResponse<IdDto> respuesta)
        {
            if (respuesta.Errors.Any(e => e.Contains("23505")))
                return Fallo<IdDto>("Ya tenés una lista con ese nombre.");

            if (respuesta.Errors.Any(e => e.Contains("23503")))
                return Fallo<IdDto>("Alguno de los productos de la lista no existe.");

            return respuesta;
        }

        private static StandarResponse<T> Fallo<T>(string error)
            => new() { Success = false, Errors = new List<string> { error } };

        private static StandarResponse<T> Fallo<T>(List<string> errores)
            => new() { Success = false, Errors = errores };
    }
}
