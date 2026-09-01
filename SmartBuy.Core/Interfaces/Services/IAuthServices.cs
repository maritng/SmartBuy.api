using SmartBuy.Core.Common.Responses;
using SmartBuy.Core.Models.Usuarios;

namespace SmartBuy.Core.Interfaces.Services
{
    /// <summary>
    /// Core valida credenciales y administra cuentas; el JWT lo emite la capa
    /// Api (JwtTokenService), que es donde vive la infraestructura de tokens.
    /// </summary>
    public interface IAuthServices
    {
        Task<StandarResponse<UsuarioPublico>> RegistrarAsync(RegistrarRequest request, CancellationToken cancellationToken);

        Task<StandarResponse<UsuarioPublico>> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
    }
}
