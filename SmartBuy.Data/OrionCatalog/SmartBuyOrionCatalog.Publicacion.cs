using Orion.Domain.Attributes;
using Orion.Domain.Models;
using Orion.Infrastructure.Executors;

namespace SmartBuy.Data.OrionCatalog
{
    public sealed partial class SmartBuyOrionCatalog
    {
        #region Publicacion

        [OrionAction(
            "GetPublicacionesPendientes",
            Query = "smartbuy/get_publicaciones_pendientes.sql",
            Provider = "PostgresSmartBuy",
            ExecutorType = typeof(SqlOrionExecutor))]
        public Task GetPublicacionesPendientes(OrionContext context) => Task.CompletedTask;

        [OrionAction(
            "ResolverMatchingPublicacion",
            Query = "smartbuy/resolver_matching_publicacion.sql",
            Provider = "PostgresSmartBuy",
            ExecutorType = typeof(SqlOrionExecutor))]
        public Task ResolverMatchingPublicacion(OrionContext context) => Task.CompletedTask;

        #endregion
    }
}
