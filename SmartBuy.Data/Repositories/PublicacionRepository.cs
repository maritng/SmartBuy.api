using SmartBuy.Core.Common.Responses;
using SmartBuy.Core.Interfaces.Repositories;
using SmartBuy.Core.Models;
using SmartBuy.Core.Models.Catalogo;
using Orion.Application.Abstractions;

namespace SmartBuy.Data.Repositories
{
    public class PublicacionRepository : OrionRepositoryBase, IPublicacionRepository
    {
        public PublicacionRepository(IOrionGateway orion) : base(orion)
        {
        }

        public Task<StandarResponse<List<PublicacionPendiente>>> GetPendientesAsync(long? cadenaId, int limit, int offset, CancellationToken cancellationToken)
            => ExecuteAsync<List<PublicacionPendiente>>("SmartBuy.GetPublicacionesPendientes", new
            {
                cadenaid = cadenaId,
                limit = limit,
                offset = offset
            }, cancellationToken);

        public Task<StandarResponse<CantidadDto>> MatchearPendientesPorEanAsync(string? ean, CancellationToken cancellationToken)
            => ExecuteAsync<CantidadDto>("SmartBuy.MatchearPendientesPorEan", new { ean = ean }, cancellationToken);

        public Task<StandarResponse<IdDto>> ResolverMatchingAsync(long publicacionId, long? productoId, string estado, CancellationToken cancellationToken)
            => ExecuteAsync<IdDto>("SmartBuy.ResolverMatchingPublicacion", new
            {
                publicacionid = publicacionId,
                productoid = productoId,
                estado = estado
            }, cancellationToken);
    }
}
