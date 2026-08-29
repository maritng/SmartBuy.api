using SmartBuy.Core.Common.Responses;
using SmartBuy.Core.Models.Recomendacion;

namespace SmartBuy.Core.Interfaces.Repositories
{
    public interface IRecomendacionRepository
    {
        /// <summary>El mejor precio vigente de cada producto de la lista en cada cadena.</summary>
        Task<StandarResponse<List<PrecioProductoCadena>>> GetPreciosParaListaAsync(IEnumerable<long> productoIds, CancellationToken cancellationToken);
    }
}
