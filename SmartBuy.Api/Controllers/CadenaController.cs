using SmartBuy.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace SmartBuy.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CadenaController : ControllerBase
    {
        private readonly ICadenaServices _cadenaServices;

        public CadenaController(ICadenaServices cadenaServices)
        {
            _cadenaServices = cadenaServices;
        }

        [HttpGet("GetAllCadenas")]
        public async Task<IActionResult> GetAllCadenas(CancellationToken cancellationToken)
        {
            var cadenas = await _cadenaServices.GetAllCadenasAsync(cancellationToken);

            if (cadenas == null)
                return NotFound("No se encontró ninguna cadena.");

            return Ok(cadenas);
        }
    }
}
