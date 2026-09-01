using Orion.Domain.Attributes;
using Orion.Domain.Models;
using Orion.Infrastructure.Executors;

namespace SmartBuy.Data.OrionCatalog
{
    public sealed partial class SmartBuyOrionCatalog
    {
        #region Producto

        [OrionAction(
            "GetAllProductos",
            Query = "smartbuy/get_all_productos.sql",
            Provider = "PostgresSmartBuy",
            ExecutorType = typeof(SqlOrionExecutor))]
        public Task GetAllProductos(OrionContext context) => Task.CompletedTask;

        [OrionAction(
            "GetProductoById",
            Query = "smartbuy/get_producto_by_id.sql",
            Provider = "PostgresSmartBuy",
            ExecutorType = typeof(SqlOrionExecutor))]
        public Task GetProductoById(OrionContext context) => Task.CompletedTask;

        [OrionAction(
            "CrearProducto",
            Query = "smartbuy/crear_producto.sql",
            Provider = "PostgresSmartBuy",
            ExecutorType = typeof(SqlOrionExecutor))]
        public Task CrearProducto(OrionContext context) => Task.CompletedTask;

        [OrionAction(
            "ActualizarProducto",
            Query = "smartbuy/actualizar_producto.sql",
            Provider = "PostgresSmartBuy",
            ExecutorType = typeof(SqlOrionExecutor))]
        public Task ActualizarProducto(OrionContext context) => Task.CompletedTask;

        [OrionAction(
            "EliminarProducto",
            Query = "smartbuy/eliminar_producto.sql",
            Provider = "PostgresSmartBuy",
            ExecutorType = typeof(SqlOrionExecutor))]
        public Task EliminarProducto(OrionContext context) => Task.CompletedTask;

        [OrionAction(
            "GenerarProductosDesdePendientes",
            Query = "smartbuy/generar_productos_desde_pendientes.sql",
            Provider = "PostgresSmartBuy",
            ExecutorType = typeof(SqlOrionExecutor))]
        public Task GenerarProductosDesdePendientes(OrionContext context) => Task.CompletedTask;

        [OrionAction(
            "GetProductosSinContenido",
            Query = "smartbuy/get_productos_sin_contenido.sql",
            Provider = "PostgresSmartBuy",
            ExecutorType = typeof(SqlOrionExecutor))]
        public Task GetProductosSinContenido(OrionContext context) => Task.CompletedTask;

        [OrionAction(
            "ActualizarContenidoProducto",
            Query = "smartbuy/actualizar_contenido_producto.sql",
            Provider = "PostgresSmartBuy",
            ExecutorType = typeof(SqlOrionExecutor))]
        public Task ActualizarContenidoProducto(OrionContext context) => Task.CompletedTask;

        [OrionAction(
            "GetAllMarcas",
            Query = "smartbuy/get_all_marcas.sql",
            Provider = "PostgresSmartBuy",
            ExecutorType = typeof(SqlOrionExecutor))]
        public Task GetAllMarcas(OrionContext context) => Task.CompletedTask;

        [OrionAction(
            "CrearMarca",
            Query = "smartbuy/crear_marca.sql",
            Provider = "PostgresSmartBuy",
            ExecutorType = typeof(SqlOrionExecutor))]
        public Task CrearMarca(OrionContext context) => Task.CompletedTask;

        [OrionAction(
            "GetAllCategorias",
            Query = "smartbuy/get_all_categorias.sql",
            Provider = "PostgresSmartBuy",
            ExecutorType = typeof(SqlOrionExecutor))]
        public Task GetAllCategorias(OrionContext context) => Task.CompletedTask;

        #endregion
    }
}
