using Orion.Domain.Attributes;
using Orion.Domain.Models;
using Orion.Infrastructure.Executors;

namespace SmartBuy.Data.OrionCatalog
{
    public sealed partial class SmartBuyOrionCatalog
    {
        #region Tendencia

        [OrionAction(
            "GetEvolucionCategorias",
            Query = "smartbuy/get_evolucion_categorias.sql",
            Provider = "PostgresSmartBuy",
            ExecutorType = typeof(SqlOrionExecutor))]
        public Task GetEvolucionCategorias(OrionContext context) => Task.CompletedTask;

        #endregion
    }
}
