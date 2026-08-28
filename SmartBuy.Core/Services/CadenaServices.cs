using SmartBuy.Core.Common.Responses;
using SmartBuy.Core.Interfaces.Repositories;
using SmartBuy.Core.Interfaces.Services;
using SmartBuy.Core.Models;

namespace SmartBuy.Core.Services
{
    public class CadenaServices : ICadenaServices
    {
        private readonly ICadenaRepository _cadenaRepository;

        public CadenaServices(ICadenaRepository cadenaRepository)
        {
            _cadenaRepository = cadenaRepository;
        }

        public async Task<StandarResponse<List<Cadena>>?> GetAllCadenasAsync(CancellationToken cancellationToken)
        {
            return await _cadenaRepository.GetAllCadenasAsync(cancellationToken);
        }
    }
}
