using SmartBuy.Core.Common.Responses;
using SmartBuy.Core.Models.Ingesta;

namespace SmartBuy.Core.Interfaces.Services
{
    public interface IIngestaServices
    {
        Task<StandarResponse<IngestaResumen>> RegistrarCapturaAsync(IngestaRequest request, CancellationToken cancellationToken);

        /// <summary>Re-computa precio_efectivo en todo el histórico (base + promos por cantidad reconocidas).</summary>
        Task<StandarResponse<RecalculoOfertasResumen>> RecalcularOfertasAsync(CancellationToken cancellationToken);
    }

    /// <summary>Resumen de RecalcularOfertas.</summary>
    public class RecalculoOfertasResumen
    {
        public long FilasBase { get; set; }

        public int TiposRevisados { get; set; }

        public int TiposComputables { get; set; }

        public long FilasConPromoAplicada { get; set; }
    }
}
