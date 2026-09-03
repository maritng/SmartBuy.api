using SmartBuy.Core.Common.Responses;
using SmartBuy.Core.Interfaces.Repositories;
using SmartBuy.Core.Models.Historico;
using Orion.Application.Abstractions;

namespace SmartBuy.Data.Repositories
{
    public class TendenciaRepository : OrionRepositoryBase, ITendenciaRepository
    {
        public TendenciaRepository(IOrionGateway orion) : base(orion)
        {
        }

        public Task<StandarResponse<List<EslabonCategoriaFila>>> GetEvolucionCategoriasAsync(int dias, CancellationToken cancellationToken)
            => ExecuteAsync<List<EslabonCategoriaFila>>("SmartBuy.GetEvolucionCategorias", new { dias = dias }, cancellationToken);
    }
}
