using SmartBuy.Core.Common.Responses;
using SmartBuy.Core.Models.Bots;
using SmartBuy.Core.Models.Ingesta;

namespace SmartBuy.Core.Interfaces.Services
{
    /// <summary>
    /// Un bot de captura de precios. Cada plataforma (vtex, coto, mail) tiene su
    /// implementación; el orquestador elige por Tipo. Los bots nunca tocan la
    /// base: entregan lo capturado a IIngestaServices, con sus validaciones y
    /// su auditoría en captura.
    /// </summary>
    public interface ICapturaBot
    {
        /// <summary>Plataforma que sabe capturar (coincide con BotCadenaConfiguration.Tipo).</summary>
        string Tipo { get; }

        Task<StandarResponse<IngestaResumen>> EjecutarAsync(BotCadenaConfiguration config, CancellationToken cancellationToken);
    }
}
