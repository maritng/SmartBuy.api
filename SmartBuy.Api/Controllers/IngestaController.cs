using SmartBuy.Api.Filters;
using SmartBuy.Core.Interfaces.Services;
using SmartBuy.Core.Models.Ingesta;
using Microsoft.AspNetCore.Mvc;

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

        public IngestaController(IIngestaServices ingestaServices)
        {
            _ingestaServices = ingestaServices;
        }

        [HttpPost("RegistrarCaptura")]
        public async Task<IActionResult> RegistrarCaptura([FromBody] IngestaRequest request, CancellationToken cancellationToken)
        {
            var resultado = await _ingestaServices.RegistrarCapturaAsync(request, cancellationToken);

            if (!resultado.Success)
                return BadRequest(resultado);

            return Ok(resultado);
        }
    }
}
