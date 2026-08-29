using SmartBuy.Core.Common.Responses;
using SmartBuy.Core.Models.Recomendacion;

namespace SmartBuy.Core.Interfaces.Services
{
    public interface IRecomendacionServices
    {
        Task<StandarResponse<ListaCompraResumen>> ResolverListaAsync(ListaCompraRequest request, CancellationToken cancellationToken);
    }
}
