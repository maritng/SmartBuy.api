using SmartBuy.Api.Filters;
using SmartBuy.Core.Interfaces.Services;
using SmartBuy.Core.Models.Bots;
using SmartBuy.Core.Models.Ingesta;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace SmartBuy.Api.Controllers
{
    /// <summary>
    /// Puerta de entrada de los bots de captura. Requiere API key (X-Api-Key):
    /// el bot manda datos crudos y el backend resuelve publicaciones, matching
    /// por EAN y precios. Los bots nunca tocan la base directamente.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [ServiceFilter(typeof(ApiKeyAuthFilter))]
    public class IngestaController : ControllerBase
    {
        private readonly IIngestaServices _ingestaServices;
        private readonly IEnumerable<ICapturaBot> _bots;
        private readonly IOptions<BotsConfiguration> _botsConfiguration;

        public IngestaController(
            IIngestaServices ingestaServices,
            IEnumerable<ICapturaBot> bots,
            IOptions<BotsConfiguration> botsConfiguration)
        {
            _ingestaServices = ingestaServices;
            _bots = bots;
            _botsConfiguration = botsConfiguration;
        }

        [HttpPost("RegistrarCaptura")]
        public async Task<IActionResult> RegistrarCaptura([FromBody] IngestaRequest request, CancellationToken cancellationToken)
        {
            var resultado = await _ingestaServices.RegistrarCapturaAsync(request, cancellationToken);

            if (!resultado.Success)
                return BadRequest(resultado);

            return Ok(resultado);
        }

        /// <summary>
        /// Re-computa precio_efectivo en todo el histórico: la fórmula base más
        /// las promos por cantidad que OfertaCalculator reconoce. Re-ejecutable
        /// cada vez que el parser aprenda un patrón nuevo (el crudo nunca se pierde).
        /// </summary>
        [HttpPost("RecalcularOfertas")]
        public async Task<IActionResult> RecalcularOfertas(CancellationToken cancellationToken)
        {
            var resultado = await _ingestaServices.RecalcularOfertasAsync(cancellationToken);
            return resultado.Success ? Ok(resultado) : BadRequest(resultado);
        }

        /// <summary>
        /// Dispara el bot de una cadena a demanda (para pruebas y recuperaciones),
        /// sin esperar al orquestador. Ignora el flag Habilitado, que gobierna
        /// solo la corrida automática; sí exige que la cadena esté configurada.
        /// </summary>
        [HttpPost("EjecutarBot")]
        public async Task<IActionResult> EjecutarBot([FromQuery] long cadenaId, CancellationToken cancellationToken)
        {
            var config = _botsConfiguration.Value.Cadenas.FirstOrDefault(c => c.CadenaId == cadenaId);

            if (config == null)
                return BadRequest(new { success = false, errors = new[] { $"No hay bot configurado para la cadena {cadenaId}." } });

            var bot = _bots.FirstOrDefault(b => b.Tipo.Equals(config.Tipo, StringComparison.OrdinalIgnoreCase));

            if (bot == null)
                return BadRequest(new { success = false, errors = new[] { $"No hay implementación para la plataforma '{config.Tipo}'." } });

            var resultado = await bot.EjecutarAsync(config, cancellationToken);

            return resultado.Success ? Ok(resultado) : BadRequest(resultado);
        }
    }
}
