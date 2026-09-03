using System.Security.Claims;
using SmartBuy.Core.Interfaces.Services;
using SmartBuy.Core.Models.Listas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SmartBuy.Api.Controllers
{
    /// <summary>
    /// Listas guardadas del usuario autenticado. El usuarioId sale SIEMPRE del
    /// token (claim NameIdentifier), jamás del cliente: anti-IDOR.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ListaController : ControllerBase
    {
        private readonly IListaServices _listaServices;

        public ListaController(IListaServices listaServices)
        {
            _listaServices = listaServices;
        }

        private long UsuarioId => long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet("GetMisListas")]
        public async Task<IActionResult> GetMisListas(CancellationToken cancellationToken)
        {
            var listas = await _listaServices.GetMisListasAsync(UsuarioId, cancellationToken);
            return listas.Success ? Ok(listas) : BadRequest(listas);
        }

        [HttpGet("GetLista")]
        public async Task<IActionResult> GetLista([FromQuery] long id, CancellationToken cancellationToken)
        {
            var lista = await _listaServices.GetListaAsync(UsuarioId, id, cancellationToken);
            return lista.Success ? Ok(lista) : NotFound(lista);
        }

        /// <summary>
        /// La inflación personal de la lista: serie diaria del costo total óptimo
        /// (últimos N días, default 90) y la variación entre días completos.
        /// </summary>
        [HttpGet("GetInflacion")]
        public async Task<IActionResult> GetInflacion([FromQuery] long listaId, [FromQuery] int? dias, CancellationToken cancellationToken)
        {
            var inflacion = await _listaServices.GetInflacionAsync(UsuarioId, listaId, dias, cancellationToken);
            return inflacion.Success ? Ok(inflacion) : BadRequest(inflacion);
        }

        [HttpPost("CrearLista")]
        public async Task<IActionResult> CrearLista([FromBody] GuardarListaRequest request, CancellationToken cancellationToken)
        {
            var resultado = await _listaServices.CrearListaAsync(UsuarioId, request, cancellationToken);
            return resultado.Success ? Ok(resultado) : BadRequest(resultado);
        }

        [HttpPut("GuardarLista")]
        public async Task<IActionResult> GuardarLista([FromBody] GuardarListaRequest request, CancellationToken cancellationToken)
        {
            var resultado = await _listaServices.GuardarListaAsync(UsuarioId, request, cancellationToken);
            return resultado.Success ? Ok(resultado) : BadRequest(resultado);
        }

        [HttpDelete("EliminarLista")]
        public async Task<IActionResult> EliminarLista([FromQuery] long id, CancellationToken cancellationToken)
        {
            var resultado = await _listaServices.EliminarListaAsync(UsuarioId, id, cancellationToken);
            return resultado.Success ? Ok(resultado) : BadRequest(resultado);
        }
    }
}
