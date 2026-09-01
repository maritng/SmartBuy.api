using System.Security.Claims;
using SmartBuy.Core.Interfaces.Services;
using SmartBuy.Core.Models.Usuarios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SmartBuy.Api.Controllers
{
    /// <summary>Preferencias del usuario autenticado ("mis cadenas").</summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsuarioController : ControllerBase
    {
        private readonly IListaServices _listaServices;

        public UsuarioController(IListaServices listaServices)
        {
            _listaServices = listaServices;
        }

        private long UsuarioId => long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet("GetMisCadenas")]
        public async Task<IActionResult> GetMisCadenas(CancellationToken cancellationToken)
        {
            var cadenas = await _listaServices.GetMisCadenasAsync(UsuarioId, cancellationToken);
            return cadenas.Success ? Ok(cadenas) : BadRequest(cadenas);
        }

        [HttpPut("GuardarMisCadenas")]
        public async Task<IActionResult> GuardarMisCadenas([FromBody] MisCadenasRequest request, CancellationToken cancellationToken)
        {
            var resultado = await _listaServices.GuardarMisCadenasAsync(UsuarioId, request, cancellationToken);
            return resultado.Success ? Ok(resultado) : BadRequest(resultado);
        }
    }
}
