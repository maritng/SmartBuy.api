using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace SmartBuy.Api.Extensions
{
    /// <summary>
    /// Autenticación JWT Bearer (mismo esquema que Empleos 360). Protege solo
    /// los endpoints marcados con [Authorize] (listas y preferencias del
    /// usuario); el resto de la API sigue abierto en esta etapa.
    /// </summary>
    public static class AuthenticationExtensions
    {
        public static IServiceCollection AddAppAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var clave = configuration["Jwt:Key"]
                ?? throw new InvalidOperationException("Falta Jwt:Key en la configuración.");

            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = configuration["Jwt:Issuer"],
                        ValidateAudience = true,
                        ValidAudience = configuration["Jwt:Audience"],
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(clave)),
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.FromMinutes(2)
                    };
                });

            return services;
        }
    }
}
