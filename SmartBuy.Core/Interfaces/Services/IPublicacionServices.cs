using SmartBuy.Core.Common.Responses;
using SmartBuy.Core.Models;
using SmartBuy.Core.Models.Catalogo;

namespace SmartBuy.Core.Interfaces.Services
{
    public interface IPublicacionServices
    {
        Task<StandarResponse<List<PublicacionPendiente>>> GetPendientesAsync(long? cadenaId, int? limit, int? offset, CancellationToken cancellationToken);

        Task<StandarResponse<IdDto>> ResolverMatchingAsync(ResolverMatchingRequest request, CancellationToken cancellationToken);
    }
}
