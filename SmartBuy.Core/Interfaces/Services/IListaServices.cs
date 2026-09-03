using SmartBuy.Core.Common.Responses;
using SmartBuy.Core.Models;
using SmartBuy.Core.Models.Historico;
using SmartBuy.Core.Models.Listas;
using SmartBuy.Core.Models.Usuarios;

namespace SmartBuy.Core.Interfaces.Services
{
    public interface IListaServices
    {
        Task<StandarResponse<List<ListaResumen>>> GetMisListasAsync(long usuarioId, CancellationToken cancellationToken);

        Task<StandarResponse<ListaDetalle>> GetListaAsync(long usuarioId, long listaId, CancellationToken cancellationToken);

        Task<StandarResponse<IdDto>> CrearListaAsync(long usuarioId, GuardarListaRequest request, CancellationToken cancellationToken);

        Task<StandarResponse<IdDto>> GuardarListaAsync(long usuarioId, GuardarListaRequest request, CancellationToken cancellationToken);

        Task<StandarResponse<IdDto>> EliminarListaAsync(long usuarioId, long listaId, CancellationToken cancellationToken);

        /// <summary>La inflación personal de la lista: serie diaria del costo óptimo + variación entre días completos.</summary>
        Task<StandarResponse<InflacionCanastaResumen>> GetInflacionAsync(long usuarioId, long listaId, int? dias, CancellationToken cancellationToken);

        Task<StandarResponse<List<long>>> GetMisCadenasAsync(long usuarioId, CancellationToken cancellationToken);

        Task<StandarResponse<CantidadDto>> GuardarMisCadenasAsync(long usuarioId, MisCadenasRequest request, CancellationToken cancellationToken);
    }
}
