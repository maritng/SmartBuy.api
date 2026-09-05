using SmartBuy.Core.Common.Responses;
using SmartBuy.Core.Interfaces.Repositories;
using SmartBuy.Core.Models;
using SmartBuy.Core.Models.Catalogo;
using SmartBuy.Core.Models.Historico;
using Orion.Application.Abstractions;

namespace SmartBuy.Data.Repositories
{
    public class ProductoRepository : OrionRepositoryBase, IProductoRepository
    {
        public ProductoRepository(IOrionGateway orion) : base(orion)
        {
        }

        public Task<StandarResponse<List<ProductoListado>>> GetAllProductosAsync(string? filtro, int limit, int offset, CancellationToken cancellationToken)
            => ExecuteAsync<List<ProductoListado>>("SmartBuy.GetAllProductos", new
            {
                filtro = filtro,
                limit = limit,
                offset = offset
            }, cancellationToken);

        public Task<StandarResponse<List<ProductoDetalle>>> GetProductoByIdAsync(long id, CancellationToken cancellationToken)
            => ExecuteAsync<List<ProductoDetalle>>("SmartBuy.GetProductoById", new { id = id }, cancellationToken);

        public Task<StandarResponse<IdDto>> CrearProductoAsync(GuardarProductoRequest producto, CancellationToken cancellationToken)
            => ExecuteAsync<IdDto>("SmartBuy.CrearProducto", new
            {
                nombre = producto.Nombre,
                marcaid = producto.MarcaId,
                categoriaid = producto.CategoriaId,
                contenidovalor = producto.ContenidoValor,
                contenidounidad = producto.ContenidoUnidad,
                ean = producto.Ean
            }, cancellationToken);

        public Task<StandarResponse<IdDto>> ActualizarProductoAsync(GuardarProductoRequest producto, CancellationToken cancellationToken)
            => ExecuteAsync<IdDto>("SmartBuy.ActualizarProducto", new
            {
                id = producto.Id,
                nombre = producto.Nombre,
                marcaid = producto.MarcaId,
                categoriaid = producto.CategoriaId,
                contenidovalor = producto.ContenidoValor,
                contenidounidad = producto.ContenidoUnidad,
                ean = producto.Ean
            }, cancellationToken);

        public Task<StandarResponse<IdDto>> EliminarProductoAsync(long id, CancellationToken cancellationToken)
            => ExecuteAsync<IdDto>("SmartBuy.EliminarProducto", new { id = id }, cancellationToken);

        public Task<StandarResponse<CantidadDto>> GenerarDesdePendientesAsync(int minCadenas, CancellationToken cancellationToken)
            => ExecuteAsync<CantidadDto>("SmartBuy.GenerarProductosDesdePendientes", new { mincadenas = minCadenas }, cancellationToken);

        public Task<StandarResponse<List<HistoricoPrecioPunto>>> GetHistoricoProductoAsync(long productoId, int dias, bool conPromos, CancellationToken cancellationToken)
            => ExecuteAsync<List<HistoricoPrecioPunto>>("SmartBuy.GetHistoricoProducto", new
            {
                productoid = productoId,
                dias = dias,
                conpromos = conPromos
            }, cancellationToken);

        public Task<StandarResponse<List<Marca>>> GetAllMarcasAsync(CancellationToken cancellationToken)
            => ExecuteAsync<List<Marca>>("SmartBuy.GetAllMarcas", null, cancellationToken);

        public Task<StandarResponse<IdDto>> CrearMarcaAsync(string nombre, CancellationToken cancellationToken)
            => ExecuteAsync<IdDto>("SmartBuy.CrearMarca", new { nombre = nombre }, cancellationToken);

        public Task<StandarResponse<List<CategoriaNodo>>> GetAllCategoriasAsync(CancellationToken cancellationToken)
            => ExecuteAsync<List<CategoriaNodo>>("SmartBuy.GetAllCategorias", null, cancellationToken);

        public Task<StandarResponse<List<ProductoSinContenido>>> GetProductosSinContenidoAsync(CancellationToken cancellationToken)
            => ExecuteAsync<List<ProductoSinContenido>>("SmartBuy.GetProductosSinContenido", null, cancellationToken);

        public Task<StandarResponse<IdDto>> ActualizarContenidoAsync(long id, decimal valor, string unidad, CancellationToken cancellationToken)
            => ExecuteAsync<IdDto>("SmartBuy.ActualizarContenidoProducto", new
            {
                id = id,
                contenidovalor = valor,
                contenidounidad = unidad
            }, cancellationToken);
    }
}
