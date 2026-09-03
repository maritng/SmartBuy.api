using SmartBuy.Core.Common.Responses;
using SmartBuy.Core.Interfaces.Repositories;
using SmartBuy.Core.Models;
using SmartBuy.Core.Models.Ingesta;
using Orion.Application.Abstractions;

namespace SmartBuy.Data.Repositories
{
    public class IngestaRepository : OrionRepositoryBase, IIngestaRepository
    {
        public IngestaRepository(IOrionGateway orion) : base(orion)
        {
        }

        public Task<StandarResponse<IdDto>> CrearCapturaAsync(long cadenaId, string fuente, CancellationToken cancellationToken)
            => ExecuteAsync<IdDto>("SmartBuy.IngestaCrearCaptura", new
            {
                cadenaid = cadenaId,
                fuente = fuente
            }, cancellationToken);

        public Task<StandarResponse<ItemCapturaResultado>> RegistrarItemAsync(long capturaId, long cadenaId, IngestaItemRequest item, decimal precioEfectivo, CancellationToken cancellationToken)
            => ExecuteAsync<ItemCapturaResultado>("SmartBuy.IngestaRegistrarItem", new
            {
                capturaid = capturaId,
                cadenaid = cadenaId,
                codigoexterno = item.CodigoExterno,
                nombrepublicado = item.NombrePublicado,
                eanpublicado = item.EanPublicado,
                url = item.Url,
                preciolista = item.PrecioLista,
                preciooferta = item.PrecioOferta,
                tipooferta = item.TipoOferta,
                precioefectivo = precioEfectivo
            }, cancellationToken);

        public Task<StandarResponse<IdDto>> GetCapturaOkDesdeAsync(long cadenaId, DateTimeOffset desde, CancellationToken cancellationToken)
            => ExecuteAsync<IdDto>("SmartBuy.IngestaCapturaOkDeHoy", new
            {
                cadenaid = cadenaId,
                desde = desde.UtcDateTime
            }, cancellationToken);

        public Task<StandarResponse<CantidadDto>> CerrarCapturasAbandonadasAsync(int horasMaximas, CancellationToken cancellationToken)
            => ExecuteAsync<CantidadDto>("SmartBuy.CerrarCapturasAbandonadas", new { horasmaximas = horasMaximas }, cancellationToken);

        public Task<StandarResponse<List<CapturaListado>>> GetCapturasAsync(int limite, CancellationToken cancellationToken)
            => ExecuteAsync<List<CapturaListado>>("SmartBuy.GetCapturas", new { limite = limite }, cancellationToken);

        public Task<StandarResponse<List<TipoOfertaDto>>> GetTiposOfertaAsync(CancellationToken cancellationToken)
            => ExecuteAsync<List<TipoOfertaDto>>("SmartBuy.GetTiposOfertaDistintos", null, cancellationToken);

        public Task<StandarResponse<CantidadDto>> RecalcularOfertasBaseAsync(CancellationToken cancellationToken)
            => ExecuteAsync<CantidadDto>("SmartBuy.RecalcularOfertasBase", null, cancellationToken);

        public Task<StandarResponse<CantidadDto>> RecalcularOfertasPorTipoAsync(string tipoOferta, decimal factor, CancellationToken cancellationToken)
            => ExecuteAsync<CantidadDto>("SmartBuy.RecalcularOfertasPorTipo", new
            {
                tipooferta = tipoOferta,
                factor = factor
            }, cancellationToken);

        public Task<StandarResponse<object>> FinalizarCapturaAsync(long capturaId, string estado, int cantItems, string? errorDetalle, CancellationToken cancellationToken)
            => ExecuteAsync<object>("SmartBuy.IngestaFinalizarCaptura", new
            {
                capturaid = capturaId,
                estado = estado,
                cantitems = cantItems,
                errordetalle = errorDetalle
            }, cancellationToken);
    }
}
