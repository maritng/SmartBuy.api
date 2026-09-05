using Microsoft.AspNetCore.Mvc;
using SmartBuy.Core.Interfaces.Services;

namespace SmartBuy.Api.Controllers
{
    /// <summary>
    /// Tendencias de precios por categoría de captura: lectura pública, como el
    /// resto del análisis de precios.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class TendenciaController : ControllerBase
    {
        private readonly ITendenciaServices _tendenciaServices;

        public TendenciaController(ITendenciaServices tendenciaServices)
        {
            _tendenciaServices = tendenciaServices;
        }

        [HttpGet("GetEvolucionCategorias")]
        public async Task<IActionResult> GetEvolucionCategorias([FromQuery] int? dias, [FromQuery] bool? conPromos, CancellationToken cancellationToken)
        {
            var evolucion = await _tendenciaServices.GetEvolucionCategoriasAsync(dias, conPromos ?? true, cancellationToken);
            return evolucion.Success ? Ok(evolucion) : BadRequest(evolucion);
        }
    }
}
