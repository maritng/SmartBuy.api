using SmartBuy.Core.Interfaces.Services;
using SmartBuy.Core.Models.Catalogo;
using Microsoft.AspNetCore.Mvc;

namespace SmartBuy.Api.Controllers
{
    /// <summary>
    /// Cola de revisión de matching: publicaciones pendientes y su resolución
    /// (matchear contra un producto del catálogo o descartar).
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class PublicacionController : ControllerBase
    {
        private readonly IPublicacionServices _publicacionServices;

        public PublicacionController(IPublicacionServices publicacionServices)
        {
            _publicacionServices = publicacionServices;
        }

        [HttpGet("GetPendientes")]
        public async Task<IActionResult> GetPendientes([FromQuery] long? cadenaId, [FromQuery] int? limit, [FromQuery] int? offset, CancellationToken cancellationToken)
        {
            var pendientes = await _publicacionServices.GetPendientesAsync(cadenaId, limit, offset, cancellationToken);
            return pendientes.Success ? Ok(pendientes) : BadRequest(pendientes);
        }

        [HttpPost("ResolverMatching")]
        public async Task<IActionResult> ResolverMatching([FromBody] ResolverMatchingRequest request, CancellationToken cancellationToken)
        {
            var resultado = await _publicacionServices.ResolverMatchingAsync(request, cancellationToken);
            return resultado.Success ? Ok(resultado) : BadRequest(resultado);
        }
    }
}
