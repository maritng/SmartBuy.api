using SmartBuy.Api.Auth;
using SmartBuy.Core.Interfaces.Services;
using SmartBuy.Core.Models.Usuarios;
using Microsoft.AspNetCore.Mvc;

namespace SmartBuy.Api.Controllers
{
    /// <summary>
    /// Registro y login. Core valida credenciales; acá se emite el JWT que el
    /// FE manda como Bearer en los endpoints protegidos.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthServices _authServices;
        private readonly JwtTokenService _jwtTokenService;

        public AuthController(IAuthServices authServices, JwtTokenService jwtTokenService)
        {
            _authServices = authServices;
            _jwtTokenService = jwtTokenService;
        }

        [HttpPost("Registrar")]
        public async Task<IActionResult> Registrar([FromBody] RegistrarRequest request, CancellationToken cancellationToken)
        {
            var resultado = await _authServices.RegistrarAsync(request, cancellationToken);

            if (!resultado.Success || resultado.Result == null)
                return BadRequest(resultado);

            return Ok(Sesion(resultado.Result));
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
        {
            var resultado = await _authServices.LoginAsync(request, cancellationToken);

            if (!resultado.Success || resultado.Result == null)
                return Unauthorized(resultado);

            return Ok(Sesion(resultado.Result));
        }

        private object Sesion(UsuarioPublico usuario) => new
        {
            success = true,
            result = new
            {
                token = _jwtTokenService.GenerarToken(usuario),
                usuario
            },
            errors = Array.Empty<string>()
        };
    }
}
