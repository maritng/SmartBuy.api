using SmartBuy.Core.Common.Responses;
using SmartBuy.Core.Models.Historico;

namespace SmartBuy.Core.Interfaces.Services
{
    public interface ITendenciaServices
    {
        /// <summary>La evolución de precios por categoría de captura: índice encadenado base 100.</summary>
        Task<StandarResponse<EvolucionCategorias>> GetEvolucionCategoriasAsync(int? dias, CancellationToken cancellationToken);
    }
}
