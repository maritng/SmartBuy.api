using SmartBuy.Core.Common.Responses;
using SmartBuy.Core.Interfaces.Repositories;
using SmartBuy.Core.Models;
using SmartBuy.Core.Models.Listas;
using Orion.Application.Abstractions;

namespace SmartBuy.Data.Repositories
{
    public class ListaRepository : OrionRepositoryBase, IListaRepository
    {
        public ListaRepository(IOrionGateway orion) : base(orion)
        {
        }

        public Task<StandarResponse<List<ListaResumen>>> GetMisListasAsync(long usuarioId, CancellationToken cancellationToken)
            => ExecuteAsync<List<ListaResumen>>("SmartBuy.GetMisListas", new { usuarioid = usuarioId }, cancellationToken);

        public Task<StandarResponse<List<ListaItemFila>>> GetListaItemsAsync(long usuarioId, long listaId, CancellationToken cancellationToken)
            => ExecuteAsync<List<ListaItemFila>>("SmartBuy.GetListaItems", new
            {
                usuarioid = usuarioId,
                listaid = listaId
            }, cancellationToken);

        public Task<StandarResponse<IdDto>> CrearListaAsync(long usuarioId, string nombre, CancellationToken cancellationToken)
            => ExecuteAsync<IdDto>("SmartBuy.CrearLista", new
            {
                usuarioid = usuarioId,
                nombre = nombre
            }, cancellationToken);

        public Task<StandarResponse<IdDto>> GuardarCabeceraAsync(long usuarioId, long listaId, string nombre, CancellationToken cancellationToken)
            => ExecuteAsync<IdDto>("SmartBuy.GuardarListaCabecera", new
            {
                usuarioid = usuarioId,
                listaid = listaId,
                nombre = nombre
            }, cancellationToken);

        public Task<StandarResponse<List<IdDto>>> InsertarItemsAsync(long listaId, IReadOnlyCollection<GuardarListaItem> items, CancellationToken cancellationToken)
            => ExecuteAsync<List<IdDto>>("SmartBuy.InsertarListaItems", new
            {
                listaid = listaId,
                // CSV de pares "productoId:cantidad" (ver insertar_lista_items.sql).
                items = items.Count > 0 ? string.Join(',', items.Select(i => $"{i.ProductoId}:{i.Cantidad}")) : null
            }, cancellationToken);

        public Task<StandarResponse<IdDto>> EliminarListaAsync(long usuarioId, long listaId, CancellationToken cancellationToken)
            => ExecuteAsync<IdDto>("SmartBuy.EliminarLista", new
            {
                usuarioid = usuarioId,
                listaid = listaId
            }, cancellationToken);
    }
}
