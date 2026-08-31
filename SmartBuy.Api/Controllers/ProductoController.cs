using SmartBuy.Core.Interfaces.Services;
using SmartBuy.Core.Models.Catalogo;
using Microsoft.AspNetCore.Mvc;

namespace SmartBuy.Api.Controllers
{
    /// <summary>
    /// ABM del catálogo maestro de productos, más marcas y categorías de apoyo
    /// para los combos del FE.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ProductoController : ControllerBase
    {
        private readonly IProductoServices _productoServices;

        public ProductoController(IProductoServices productoServices)
        {
            _productoServices = productoServices;
        }

        [HttpGet("GetAllProductos")]
        public async Task<IActionResult> GetAllProductos([FromQuery] string? filtro, [FromQuery] int? limit, [FromQuery] int? offset, CancellationToken cancellationToken)
        {
            var productos = await _productoServices.GetAllProductosAsync(filtro, limit, offset, cancellationToken);
            return productos.Success ? Ok(productos) : BadRequest(productos);
        }

        [HttpGet("GetProductoById")]
        public async Task<IActionResult> GetProductoById([FromQuery] long id, CancellationToken cancellationToken)
        {
            var producto = await _productoServices.GetProductoByIdAsync(id, cancellationToken);
            return producto.Success ? Ok(producto) : NotFound(producto);
        }

        [HttpPost("CrearProducto")]
        public async Task<IActionResult> CrearProducto([FromBody] GuardarProductoRequest request, CancellationToken cancellationToken)
        {
            var resultado = await _productoServices.CrearProductoAsync(request, cancellationToken);
            return resultado.Success ? Ok(resultado) : BadRequest(resultado);
        }

        [HttpPut("ActualizarProducto")]
        public async Task<IActionResult> ActualizarProducto([FromBody] GuardarProductoRequest request, CancellationToken cancellationToken)
        {
            var resultado = await _productoServices.ActualizarProductoAsync(request, cancellationToken);
            return resultado.Success ? Ok(resultado) : BadRequest(resultado);
        }

        [HttpDelete("EliminarProducto")]
        public async Task<IActionResult> EliminarProducto([FromQuery] long id, CancellationToken cancellationToken)
        {
            var resultado = await _productoServices.EliminarProductoAsync(id, cancellationToken);
            return resultado.Success ? Ok(resultado) : BadRequest(resultado);
        }

        /// <summary>
        /// Generación masiva de catálogo desde la cola de pendientes: un producto
        /// provisorio (curado=false) por cada EAN presente en minCadenas o más
        /// cadenas (default 2), con re-matcheo retroactivo incluido. Idempotente.
        /// </summary>
        [HttpPost("GenerarDesdePendientes")]
        public async Task<IActionResult> GenerarDesdePendientes([FromQuery] int? minCadenas, CancellationToken cancellationToken)
        {
            var resultado = await _productoServices.GenerarDesdePendientesAsync(minCadenas, cancellationToken);
            return resultado.Success ? Ok(resultado) : BadRequest(resultado);
        }

        [HttpGet("GetAllMarcas")]
        public async Task<IActionResult> GetAllMarcas(CancellationToken cancellationToken)
        {
            var marcas = await _productoServices.GetAllMarcasAsync(cancellationToken);
            return marcas.Success ? Ok(marcas) : BadRequest(marcas);
        }

        [HttpPost("CrearMarca")]
        public async Task<IActionResult> CrearMarca([FromBody] CrearMarcaRequest request, CancellationToken cancellationToken)
        {
            var resultado = await _productoServices.CrearMarcaAsync(request, cancellationToken);
            return resultado.Success ? Ok(resultado) : BadRequest(resultado);
        }

        [HttpGet("GetAllCategorias")]
        public async Task<IActionResult> GetAllCategorias(CancellationToken cancellationToken)
        {
            var categorias = await _productoServices.GetAllCategoriasAsync(cancellationToken);
            return categorias.Success ? Ok(categorias) : BadRequest(categorias);
        }
    }
}
