using SmartBuy.Core.Common.Responses;
using SmartBuy.Core.Interfaces.Repositories;
using SmartBuy.Core.Interfaces.Services;
using SmartBuy.Core.Models;
using SmartBuy.Core.Models.Catalogo;

namespace SmartBuy.Core.Services
{
    /// <summary>
    /// Cola de revisión de matching. La resolución manual de hoy es el mismo
    /// contrato que usará el flujo con LLM: proponer matching o descarte sobre
    /// una publicación pendiente.
    /// </summary>
    public class PublicacionServices : IPublicacionServices
    {
        private const int LimitDefault = 50;
        private const int LimitMax = 200;

        private readonly IPublicacionRepository _publicacionRepository;
        private readonly IProductoRepository _productoRepository;

        public PublicacionServices(IPublicacionRepository publicacionRepository, IProductoRepository productoRepository)
        {
            _publicacionRepository = publicacionRepository;
            _productoRepository = productoRepository;
        }

        public Task<StandarResponse<List<PublicacionPendiente>>> GetPendientesAsync(long? cadenaId, int? limit, int? offset, CancellationToken cancellationToken)
        {
            var limitNormalizado = Math.Clamp(limit ?? LimitDefault, 1, LimitMax);
            var offsetNormalizado = Math.Max(offset ?? 0, 0);
            var cadenaNormalizada = cadenaId is > 0 ? cadenaId : null;

            return _publicacionRepository.GetPendientesAsync(cadenaNormalizada, limitNormalizado, offsetNormalizado, cancellationToken);
        }

        public async Task<StandarResponse<IdDto>> ResolverMatchingAsync(ResolverMatchingRequest request, CancellationToken cancellationToken)
        {
            if (request == null || request.PublicacionId <= 0)
                return Fallo("publicacionId es obligatorio.");

            var matchear = request.ProductoId is > 0;

            // Exactamente una de las dos acciones: matchear o descartar.
            if (matchear == request.Descartar)
                return Fallo("Indicar productoId (para matchear) o descartar=true, pero no ambos ni ninguno.");

            if (matchear)
            {
                // El producto tiene que existir y estar activo: matchear contra un
                // producto de baja dejaría la publicación apuntando a algo que no
                // participa de las recomendaciones. La FK cubre la carrera restante.
                var producto = await _productoRepository.GetProductoByIdAsync(request.ProductoId!.Value, cancellationToken);

                if (!producto.Success)
                    return new StandarResponse<IdDto> { Success = false, Errors = producto.Errors };

                var detalle = producto.Result?.FirstOrDefault();
                if (detalle == null || detalle.Id <= 0 || !detalle.Activo)
                    return Fallo($"El producto {request.ProductoId} no existe o está dado de baja.");
            }

            // El estado lo decide el servicio, nunca el cliente.
            var estado = matchear ? "manual" : "descartada";
            var productoId = matchear ? request.ProductoId : null;

            var respuesta = await _publicacionRepository.ResolverMatchingAsync(request.PublicacionId, productoId, estado, cancellationToken);

            if (respuesta.Success && (respuesta.Result == null || respuesta.Result.Id <= 0))
                return Fallo($"La publicación {request.PublicacionId} no existe o ya fue resuelta.");

            return respuesta;
        }

        private static StandarResponse<IdDto> Fallo(string error)
            => new() { Success = false, Errors = new List<string> { error } };
    }
}
