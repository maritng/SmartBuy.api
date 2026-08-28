using SmartBuy.Core.Common.Responses;
using SmartBuy.Core.Models;

namespace SmartBuy.Core.Interfaces.Repositories
{
    public interface ICadenaRepository
    {
        Task<StandarResponse<List<Cadena>>> GetAllCadenasAsync(CancellationToken cancellationToken);
    }
}
