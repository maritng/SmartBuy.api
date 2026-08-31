using SmartBuy.Core.Common.Responses;
using SmartBuy.Core.Models;
using SmartBuy.Core.Models.Catalogo;

namespace SmartBuy.Core.Interfaces.Repositories
{
    public interface IPublicacionRepository
    {
        Task<StandarResponse<List<PublicacionPendiente>>> GetPendientesAsync(long? cadenaId, int limit, int offset, CancellationToken cancellationToken);

        /// <summary>
        /// Marca la publicación como 'manual' (productoId con valor) o 'descartada'
        /// (productoId null). Solo afecta publicaciones 'pendiente'; 0 filas => ya resuelta.
        /// </summary>
        Task<StandarResponse<IdDto>> ResolverMatchingAsync(long publicacionId, long? productoId, string estado, CancellationToken cancellationToken);

        /// <summary>
        /// Re-matcheo retroactivo: pendientes cuyo EAN coincide con un producto
        /// activo pasan a 'auto_ean'. Con ean null re-matchea todos; con valor,
        /// solo ese EAN. Devuelve cuántas engancharon.
        /// </summary>
        Task<StandarResponse<CantidadDto>> MatchearPendientesPorEanAsync(string? ean, CancellationToken cancellationToken);
    }
}
