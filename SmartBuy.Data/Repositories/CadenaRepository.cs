using SmartBuy.Core.Common.Responses;
using SmartBuy.Core.Interfaces.Repositories;
using SmartBuy.Core.Models;
using Orion.Application.Abstractions;

namespace SmartBuy.Data.Repositories
{
    public class CadenaRepository : OrionRepositoryBase, ICadenaRepository
    {
        public CadenaRepository(IOrionGateway orion) : base(orion)
        {
        }

        public Task<StandarResponse<List<Cadena>>> GetAllCadenasAsync(CancellationToken cancellationToken)
            => ExecuteAsync<List<Cadena>>("SmartBuy.GetAllCadenas", null, cancellationToken);
    }
}
