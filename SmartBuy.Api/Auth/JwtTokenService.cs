using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using SmartBuy.Core.Models.Usuarios;

namespace SmartBuy.Api.Auth
{
    /// <summary>
    /// Emisión del JWT. Vive en Api (no en Core) porque es infraestructura de
    /// tokens: Core valida credenciales, esta capa firma. La clave viene de
    /// configuración (Jwt:Key): la de appsettings es SOLO de desarrollo local;
    /// en un entorno real va por variable de entorno Jwt__Key.
    /// </summary>
    public class JwtTokenService
    {
        private readonly IConfiguration _configuration;

        public JwtTokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerarToken(UsuarioPublico usuario)
        {
            var clave = _configuration["Jwt:Key"]
                ?? throw new InvalidOperationException("Falta Jwt:Key en la configuración.");
            var dias = int.TryParse(_configuration["Jwt:ExpiracionDias"], out var d) ? d : 7;

            var credenciales = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(clave)),
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                    new Claim(ClaimTypes.Name, usuario.Nombre),
                    new Claim(ClaimTypes.Email, usuario.Email)
                },
                expires: DateTime.UtcNow.AddDays(dias),
                signingCredentials: credenciales);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
