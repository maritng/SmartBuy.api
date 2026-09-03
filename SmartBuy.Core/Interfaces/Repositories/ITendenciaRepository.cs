using SmartBuy.Core.Common.Responses;
using SmartBuy.Core.Models.Historico;

namespace SmartBuy.Core.Interfaces.Repositories
{
    public interface ITendenciaRepository
    {
        /// <summary>Eslabones diarios del índice por categoría (canasta común contra la observación previa), últimos N días.</summary>
        Task<StandarResponse<List<EslabonCategoriaFila>>> GetEvolucionCategoriasAsync(int dias, CancellationToken cancellationToken);
    }
}
