using SmartBuy.Core.Interfaces.Services;
using SmartBuy.Core.Models.Recomendacion;
using Microsoft.AspNetCore.Mvc;

namespace SmartBuy.Api.Controllers
{
    /// <summary>
    /// La consulta estrella de SmartBuy: dada la lista de compras, dónde
    /// conviene comprar cada producto y cuánto se ahorra repartiendo.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class RecomendacionController : ControllerBase
    {
        private readonly IRecomendacionServices _recomendacionServices;

        public RecomendacionController(IRecomendacionServices recomendacionServices)
        {
            _recomendacionServices = recomendacionServices;
        }

        [HttpPost("ResolverLista")]
        public async Task<IActionResult> ResolverLista([FromBody] ListaCompraRequest request, CancellationToken cancellationToken)
        {
            var resultado = await _recomendacionServices.ResolverListaAsync(request, cancellationToken);
            return resultado.Success ? Ok(resultado) : BadRequest(resultado);
        }
    }
}
