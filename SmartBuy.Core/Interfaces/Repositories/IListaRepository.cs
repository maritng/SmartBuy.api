using SmartBuy.Core.Common.Responses;
using SmartBuy.Core.Models;
using SmartBuy.Core.Models.Historico;
using SmartBuy.Core.Models.Listas;

namespace SmartBuy.Core.Interfaces.Repositories
{
    /// <summary>
    /// Todas las operaciones llevan el usuarioId (que viene del token, nunca del
    /// cliente) y las queries filtran por él: anti-IDOR en la capa de datos.
    /// </summary>
    public interface IListaRepository
    {
        Task<StandarResponse<List<ListaResumen>>> GetMisListasAsync(long usuarioId, CancellationToken cancellationToken);

        Task<StandarResponse<List<ListaItemFila>>> GetListaItemsAsync(long usuarioId, long listaId, CancellationToken cancellationToken);

        Task<StandarResponse<IdDto>> CrearListaAsync(long usuarioId, string nombre, CancellationToken cancellationToken);

        /// <summary>Renombra y vacía la lista (0 filas = no existe o no es tuya).</summary>
        Task<StandarResponse<IdDto>> GuardarCabeceraAsync(long usuarioId, long listaId, string nombre, CancellationToken cancellationToken);

        Task<StandarResponse<List<IdDto>>> InsertarItemsAsync(long listaId, IReadOnlyCollection<GuardarListaItem> items, CancellationToken cancellationToken);

        Task<StandarResponse<IdDto>> EliminarListaAsync(long usuarioId, long listaId, CancellationToken cancellationToken);

        /// <summary>Mejor precio por día de cada producto de la lista en los últimos N días (lista ajena = cero filas).</summary>
        Task<StandarResponse<List<InflacionPrecioFila>>> GetInflacionListaAsync(long usuarioId, long listaId, int dias, CancellationToken cancellationToken);
    }
}
