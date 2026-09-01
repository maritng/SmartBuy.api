using SmartBuy.Core.Common.Responses;
using SmartBuy.Core.Models;
using SmartBuy.Core.Models.Usuarios;

namespace SmartBuy.Core.Interfaces.Repositories
{
    public interface IUsuarioRepository
    {
        Task<StandarResponse<IdDto>> CrearUsuarioAsync(string email, string nombre, string passwordHash, CancellationToken cancellationToken);

        Task<StandarResponse<List<UsuarioCuenta>>> GetUsuarioByEmailAsync(string email, CancellationToken cancellationToken);

        Task<StandarResponse<object>> ActualizarUltimoAccesoAsync(long usuarioId, CancellationToken cancellationToken);

        Task<StandarResponse<List<CadenaIdDto>>> GetMisCadenasAsync(long usuarioId, CancellationToken cancellationToken);

        Task<StandarResponse<CantidadDto>> GuardarMisCadenasAsync(long usuarioId, IReadOnlyCollection<long> cadenasIds, CancellationToken cancellationToken);
    }
}
