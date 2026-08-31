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
        Task<StandarResponse<ItemCapturaResultado>> RegistrarItemAsync(long capturaId, long cadenaId, IngestaItemRequest item, CancellationToken cancellationToken);

        Task<StandarResponse<object>> FinalizarCapturaAsync(long capturaId, string estado, int cantItems, string? errorDetalle, CancellationToken cancellationToken);

        /// <summary>Id de la captura 'ok' de hoy para la cadena (Id 0 si el bot aún no corrió).</summary>
        Task<StandarResponse<IdDto>> GetCapturaOkDeHoyAsync(long cadenaId, CancellationToken cancellationToken);
    }
}
