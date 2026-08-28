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

        public Task<StandarResponse<ItemCapturaResultado>> RegistrarItemAsync(long capturaId, long cadenaId, IngestaItemRequest item, CancellationToken cancellationToken)
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
                tipooferta = item.TipoOferta
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
