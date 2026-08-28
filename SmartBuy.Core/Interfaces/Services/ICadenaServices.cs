using SmartBuy.Core.Common.Responses;
using SmartBuy.Core.Models;

namespace SmartBuy.Core.Interfaces.Services
{
    public interface ICadenaServices
    {
        Task<StandarResponse<List<Cadena>>?> GetAllCadenasAsync(CancellationToken cancellationToken);
    }
}
