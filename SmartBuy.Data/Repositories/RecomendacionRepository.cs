using SmartBuy.Core.Common.Responses;
using SmartBuy.Core.Interfaces.Repositories;
using SmartBuy.Core.Models.Recomendacion;
using Orion.Application.Abstractions;

namespace SmartBuy.Data.Repositories
{
    public class RecomendacionRepository : OrionRepositoryBase, IRecomendacionRepository
    {
        public RecomendacionRepository(IOrionGateway orion) : base(orion)
        {
        }

        public Task<StandarResponse<List<PrecioProductoCadena>>> GetPreciosParaListaAsync(IEnumerable<long> productoIds, IReadOnlyCollection<long>? cadenasIds, CancellationToken cancellationToken)
            => ExecuteAsync<List<PrecioProductoCadena>>("SmartBuy.GetPreciosParaLista", new
            {
                // CSV parametrizado; el SQL lo abre con string_to_array(...)::bigint[].
                productoids = string.Join(',', productoIds),
                cadenasids = cadenasIds is { Count: > 0 } ? string.Join(',', cadenasIds) : null
            }, cancellationToken);
    }
}
