using Orion.Domain.Attributes;
using Orion.Domain.Models;
using Orion.Infrastructure.Executors;

namespace SmartBuy.Data.OrionCatalog
{
    public sealed partial class SmartBuyOrionCatalog
    {
        #region Lista

        [OrionAction(
            "GetMisListas",
            Query = "smartbuy/get_mis_listas.sql",
            Provider = "PostgresSmartBuy",
            ExecutorType = typeof(SqlOrionExecutor))]
        public Task GetMisListas(OrionContext context) => Task.CompletedTask;

        [OrionAction(
            "GetListaItems",
            Query = "smartbuy/get_lista_items.sql",
            Provider = "PostgresSmartBuy",
            ExecutorType = typeof(SqlOrionExecutor))]
        public Task GetListaItems(OrionContext context) => Task.CompletedTask;

        [OrionAction(
            "GetInflacionLista",
            Query = "smartbuy/get_inflacion_lista.sql",
            Provider = "PostgresSmartBuy",
            ExecutorType = typeof(SqlOrionExecutor))]
        public Task GetInflacionLista(OrionContext context) => Task.CompletedTask;

        [OrionAction(
            "CrearLista",
            Query = "smartbuy/crear_lista.sql",
            Provider = "PostgresSmartBuy",
            ExecutorType = typeof(SqlOrionExecutor))]
        public Task CrearLista(OrionContext context) => Task.CompletedTask;

        [OrionAction(
            "GuardarListaCabecera",
            Query = "smartbuy/guardar_lista_cabecera.sql",
            Provider = "PostgresSmartBuy",
            ExecutorType = typeof(SqlOrionExecutor))]
        public Task GuardarListaCabecera(OrionContext context) => Task.CompletedTask;

        [OrionAction(
            "InsertarListaItems",
            Query = "smartbuy/insertar_lista_items.sql",
            Provider = "PostgresSmartBuy",
            ExecutorType = typeof(SqlOrionExecutor))]
        public Task InsertarListaItems(OrionContext context) => Task.CompletedTask;

        [OrionAction(
            "EliminarLista",
            Query = "smartbuy/eliminar_lista.sql",
            Provider = "PostgresSmartBuy",
            ExecutorType = typeof(SqlOrionExecutor))]
        public Task EliminarLista(OrionContext context) => Task.CompletedTask;

        #endregion
    }
}
