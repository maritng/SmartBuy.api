using SmartBuy.Core.Common.Responses;
using SmartBuy.Core.Models;
using SmartBuy.Core.Models.Catalogo;
using SmartBuy.Core.Models.Historico;

namespace SmartBuy.Core.Interfaces.Repositories
{
    public interface IProductoRepository
    {
        Task<StandarResponse<List<ProductoListado>>> GetAllProductosAsync(string? filtro, int limit, int offset, CancellationToken cancellationToken);

        Task<StandarResponse<List<ProductoDetalle>>> GetProductoByIdAsync(long id, CancellationToken cancellationToken);

        Task<StandarResponse<IdDto>> CrearProductoAsync(GuardarProductoRequest producto, CancellationToken cancellationToken);

        Task<StandarResponse<IdDto>> ActualizarProductoAsync(GuardarProductoRequest producto, CancellationToken cancellationToken);

        Task<StandarResponse<IdDto>> EliminarProductoAsync(long id, CancellationToken cancellationToken);

        /// <summary>
        /// Crea productos (curado=false) desde los EANs pendientes presentes en al
        /// menos minCadenas cadenas. Devuelve cuántos creó.
        /// </summary>
        Task<StandarResponse<CantidadDto>> GenerarDesdePendientesAsync(int minCadenas, CancellationToken cancellationToken);

        /// <summary>Mejor precio por día y cadena del producto en los últimos N días. conPromos: efectivo vs. góndola.</summary>
        Task<StandarResponse<List<HistoricoPrecioPunto>>> GetHistoricoProductoAsync(long productoId, int dias, bool conPromos, CancellationToken cancellationToken);

        Task<StandarResponse<List<Marca>>> GetAllMarcasAsync(CancellationToken cancellationToken);

        Task<StandarResponse<IdDto>> CrearMarcaAsync(string nombre, CancellationToken cancellationToken);

        Task<StandarResponse<List<CategoriaNodo>>> GetAllCategoriasAsync(CancellationToken cancellationToken);

        Task<StandarResponse<List<ProductoSinContenido>>> GetProductosSinContenidoAsync(CancellationToken cancellationToken);

        /// <summary>Solo completa si sigue null: la curación manual nunca se pisa.</summary>
        Task<StandarResponse<IdDto>> ActualizarContenidoAsync(long id, decimal valor, string unidad, CancellationToken cancellationToken);
    }
}
