using Microsoft.AspNetCore.Mvc;
using SmartBuy.Core.Interfaces.Services;

namespace SmartBuy.Api.Controllers
{
    /// <summary>
    /// Bitácora de solo lectura de las corridas de los bots. A diferencia de
    /// IngestaController, no está detrás de la API key: no expone acciones,
    /// solo el estado de las capturas para el panel del front.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class CapturaController : ControllerBase
    {
        private readonly IIngestaServices _ingestaServices;

        public CapturaController(IIngestaServices ingestaServices)
        {
            _ingestaServices = ingestaServices;
        }

        [HttpGet("GetCapturas")]
        public async Task<IActionResult> GetCapturas(CancellationToken cancellationToken, int limite = 50)
        {
            var capturas = await _ingestaServices.GetCapturasAsync(limite, cancellationToken);

            return Ok(capturas);
        }
    }
}
