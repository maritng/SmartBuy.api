using Orion.Domain.Attributes;
using Orion.Domain.Models;
using Orion.Infrastructure.Executors;

namespace SmartBuy.Data.OrionCatalog
{
    public sealed partial class SmartBuyOrionCatalog
    {
        #region Ingesta

        [OrionAction(
            "IngestaCrearCaptura",
            Query = "smartbuy/ingesta_crear_captura.sql",
            Provider = "PostgresSmartBuy",
            ExecutorType = typeof(SqlOrionExecutor))]
        public Task IngestaCrearCaptura(OrionContext context) => Task.CompletedTask;

        [OrionAction(
            "IngestaRegistrarItem",
            Query = "smartbuy/ingesta_registrar_item.sql",
            Provider = "PostgresSmartBuy",
            ExecutorType = typeof(SqlOrionExecutor))]
        public Task IngestaRegistrarItem(OrionContext context) => Task.CompletedTask;

        [OrionAction(
            "IngestaCapturaOkDeHoy",
            Query = "smartbuy/get_ultima_captura_ok.sql",
            Provider = "PostgresSmartBuy",
            ExecutorType = typeof(SqlOrionExecutor))]
        public Task IngestaCapturaOkDeHoy(OrionContext context) => Task.CompletedTask;

        [OrionAction(
            "IngestaFinalizarCaptura",
            Query = "smartbuy/ingesta_finalizar_captura.sql",
            Provider = "PostgresSmartBuy",
            ExecutorType = typeof(SqlOrionExecutor))]
        public Task IngestaFinalizarCaptura(OrionContext context) => Task.CompletedTask;

        #endregion
    }
}
