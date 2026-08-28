using SmartBuy.Core.Common.Responses;
using SmartBuy.Core.Models.Ingesta;

namespace SmartBuy.Core.Interfaces.Services
{
    public interface IIngestaServices
    {
        Task<StandarResponse<IngestaResumen>> RegistrarCapturaAsync(IngestaRequest request, CancellationToken cancellationToken);
    }
}
