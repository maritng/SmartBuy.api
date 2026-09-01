using Orion.Domain.Attributes;
using Orion.Domain.Models;
using Orion.Infrastructure.Executors;

namespace SmartBuy.Data.OrionCatalog
{
    public sealed partial class SmartBuyOrionCatalog
    {
        #region Usuario

        [OrionAction(
            "CrearUsuario",
            Query = "smartbuy/crear_usuario.sql",
            Provider = "PostgresSmartBuy",
            ExecutorType = typeof(SqlOrionExecutor))]
        public Task CrearUsuario(OrionContext context) => Task.CompletedTask;

        [OrionAction(
            "GetUsuarioByEmail",
            Query = "smartbuy/get_usuario_by_email.sql",
            Provider = "PostgresSmartBuy",
            ExecutorType = typeof(SqlOrionExecutor))]
        public Task GetUsuarioByEmail(OrionContext context) => Task.CompletedTask;

        [OrionAction(
            "ActualizarUltimoAcceso",
            Query = "smartbuy/actualizar_ultimo_acceso.sql",
            Provider = "PostgresSmartBuy",
            ExecutorType = typeof(SqlOrionExecutor))]
        public Task ActualizarUltimoAcceso(OrionContext context) => Task.CompletedTask;

        [OrionAction(
            "GetMisCadenas",
            Query = "smartbuy/get_mis_cadenas.sql",
            Provider = "PostgresSmartBuy",
            ExecutorType = typeof(SqlOrionExecutor))]
        public Task GetMisCadenas(OrionContext context) => Task.CompletedTask;

        [OrionAction(
            "GuardarMisCadenas",
            Query = "smartbuy/guardar_mis_cadenas.sql",
            Provider = "PostgresSmartBuy",
            ExecutorType = typeof(SqlOrionExecutor))]
        public Task GuardarMisCadenas(OrionContext context) => Task.CompletedTask;

        #endregion
    }
}
