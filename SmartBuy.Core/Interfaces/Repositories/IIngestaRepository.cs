using SmartBuy.Core.Common.Responses;
using SmartBuy.Core.Models;
using SmartBuy.Core.Models.Ingesta;

namespace SmartBuy.Core.Interfaces.Repositories
{
    public interface IIngestaRepository
    {
        Task<StandarResponse<IdDto>> CrearCapturaAsync(long cadenaId, string fuente, CancellationToken cancellationToken);

        /// <summary>
        /// Registra un ítem en una sola sentencia atómica: upsert de la publicación
        /// (con matching automático por EAN) + inserción del precio del día.
        /// </summary>
        Task<StandarResponse<ItemCapturaResultado>> RegistrarItemAsync(long capturaId, long cadenaId, IngestaItemRequest item, decimal precioEfectivo, CancellationToken cancellationToken);

        Task<StandarResponse<object>> FinalizarCapturaAsync(long capturaId, string estado, int cantItems, string? errorDetalle, CancellationToken cancellationToken);

        /// <summary>Id de la captura 'ok' de la cadena desde el instante dado (Id 0 si no hay). El orquestador pasa el inicio de la ventana vigente.</summary>
        Task<StandarResponse<IdDto>> GetCapturaOkDesdeAsync(long cadenaId, DateTimeOffset desde, CancellationToken cancellationToken);

        /// <summary>Cierra como 'error' las capturas en_proceso más viejas que horasMaximas (bot caído). Devuelve cuántas.</summary>
        Task<StandarResponse<CantidadDto>> CerrarCapturasAbandonadasAsync(int horasMaximas, CancellationToken cancellationToken);

        /// <summary>Últimas corridas de bots (bitácora del panel de capturas), más recientes primero.</summary>
        Task<StandarResponse<List<CapturaListado>>> GetCapturasAsync(int limite, CancellationToken cancellationToken);

        Task<StandarResponse<List<TipoOfertaDto>>> GetTiposOfertaAsync(CancellationToken cancellationToken);

        /// <summary>Pasada base del recálculo: min(lista, oferta directa) para todo el histórico.</summary>
        Task<StandarResponse<CantidadDto>> RecalcularOfertasBaseAsync(CancellationToken cancellationToken);

        /// <summary>Aplica el factor de una promo por cantidad a todas las filas con ese descriptor.</summary>
        Task<StandarResponse<CantidadDto>> RecalcularOfertasPorTipoAsync(string tipoOferta, decimal factor, CancellationToken cancellationToken);
    }

    /// <summary>Descriptor de promo distinto del histórico (para el recálculo por tipo).</summary>
    public class TipoOfertaDto
    {
        public string TipoOferta { get; set; } = string.Empty;
    }
}
