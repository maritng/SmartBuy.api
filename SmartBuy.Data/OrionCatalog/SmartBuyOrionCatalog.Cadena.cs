using Orion.Domain.Attributes;
using Orion.Domain.Models;
using Orion.Infrastructure.Executors;

namespace SmartBuy.Data.OrionCatalog
{
    public sealed partial class SmartBuyOrionCatalog
    {
        #region Cadena

        [OrionAction(
            "GetAllCadenas",
            Query = "smartbuy/get_all_cadenas.sql",
            Provider = "PostgresSmartBuy",
            ExecutorType = typeof(SqlOrionExecutor))]
        public Task GetAllCadenas(OrionContext context) => Task.CompletedTask;

        #endregion
    }
}
