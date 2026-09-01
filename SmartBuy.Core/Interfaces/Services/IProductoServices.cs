using SmartBuy.Core.Common.Responses;
using SmartBuy.Core.Models;
using SmartBuy.Core.Models.Catalogo;

namespace SmartBuy.Core.Interfaces.Services
{
    public interface IProductoServices
    {
        Task<StandarResponse<List<ProductoListado>>> GetAllProductosAsync(string? filtro, int? limit, int? offset, CancellationToken cancellationToken);

        Task<StandarResponse<ProductoDetalle>> GetProductoByIdAsync(long id, CancellationToken cancellationToken);

        Task<StandarResponse<IdDto>> CrearProductoAsync(GuardarProductoRequest request, CancellationToken cancellationToken);

        Task<StandarResponse<IdDto>> ActualizarProductoAsync(GuardarProductoRequest request, CancellationToken cancellationToken);

        Task<StandarResponse<IdDto>> EliminarProductoAsync(long id, CancellationToken cancellationToken);

        Task<StandarResponse<GeneracionPendientesResumen>> GenerarDesdePendientesAsync(int? minCadenas, CancellationToken cancellationToken);

        /// <summary>Parsea el gramaje del nombre y completa el contenido de productos que no lo tienen.</summary>
        Task<StandarResponse<ContenidosResumen>> CompletarContenidosAsync(CancellationToken cancellationToken);

        Task<StandarResponse<List<Marca>>> GetAllMarcasAsync(CancellationToken cancellationToken);

        Task<StandarResponse<IdDto>> CrearMarcaAsync(CrearMarcaRequest request, CancellationToken cancellationToken);

        Task<StandarResponse<List<CategoriaNodo>>> GetAllCategoriasAsync(CancellationToken cancellationToken);
    }
}
