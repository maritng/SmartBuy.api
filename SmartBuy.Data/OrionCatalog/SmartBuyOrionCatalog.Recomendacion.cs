using Orion.Domain.Attributes;
using Orion.Domain.Models;
using Orion.Infrastructure.Executors;

namespace SmartBuy.Data.OrionCatalog
{
    public sealed partial class SmartBuyOrionCatalog
    {
        #region Recomendacion

        [OrionAction(
            "GetPreciosParaLista",
            Query = "smartbuy/get_precios_para_lista.sql",
            Provider = "PostgresSmartBuy",
            ExecutorType = typeof(SqlOrionExecutor))]
        public Task GetPreciosParaLista(OrionContext context) => Task.CompletedTask;

        #endregion
    }
}
