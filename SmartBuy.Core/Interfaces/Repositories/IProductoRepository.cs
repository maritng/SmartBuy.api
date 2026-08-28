using SmartBuy.Core.Common.Responses;
using SmartBuy.Core.Models;
using SmartBuy.Core.Models.Catalogo;

namespace SmartBuy.Core.Interfaces.Repositories
{
    public interface IProductoRepository
    {
        Task<StandarResponse<List<ProductoListado>>> GetAllProductosAsync(string? filtro, int limit, int offset, CancellationToken cancellationToken);

        Task<StandarResponse<List<ProductoDetalle>>> GetProductoByIdAsync(long id, CancellationToken cancellationToken);

        Task<StandarResponse<IdDto>> CrearProductoAsync(GuardarProductoRequest producto, CancellationToken cancellationToken);

        Task<StandarResponse<IdDto>> ActualizarProductoAsync(GuardarProductoRequest producto, CancellationToken cancellationToken);

        Task<StandarResponse<IdDto>> EliminarProductoAsync(long id, CancellationToken cancellationToken);

        Task<StandarResponse<List<Marca>>> GetAllMarcasAsync(CancellationToken cancellationToken);

        Task<StandarResponse<IdDto>> CrearMarcaAsync(string nombre, CancellationToken cancellationToken);

        Task<StandarResponse<List<CategoriaNodo>>> GetAllCategoriasAsync(CancellationToken cancellationToken);
    }
}
