using SmartBuy.Core.Common.Responses;
using SmartBuy.Core.Interfaces.Repositories;
using SmartBuy.Core.Models;
using SmartBuy.Core.Models.Usuarios;
using Orion.Application.Abstractions;

namespace SmartBuy.Data.Repositories
{
    public class UsuarioRepository : OrionRepositoryBase, IUsuarioRepository
    {
        public UsuarioRepository(IOrionGateway orion) : base(orion)
        {
        }

        public Task<StandarResponse<IdDto>> CrearUsuarioAsync(string email, string nombre, string passwordHash, CancellationToken cancellationToken)
            => ExecuteAsync<IdDto>("SmartBuy.CrearUsuario", new
            {
                email = email,
                nombre = nombre,
                passwordhash = passwordHash
            }, cancellationToken);

        public Task<StandarResponse<List<UsuarioCuenta>>> GetUsuarioByEmailAsync(string email, CancellationToken cancellationToken)
            => ExecuteAsync<List<UsuarioCuenta>>("SmartBuy.GetUsuarioByEmail", new { email = email }, cancellationToken);

        public Task<StandarResponse<object>> ActualizarUltimoAccesoAsync(long usuarioId, CancellationToken cancellationToken)
            => ExecuteAsync<object>("SmartBuy.ActualizarUltimoAcceso", new { usuarioid = usuarioId }, cancellationToken);

        public Task<StandarResponse<List<CadenaIdDto>>> GetMisCadenasAsync(long usuarioId, CancellationToken cancellationToken)
            => ExecuteAsync<List<CadenaIdDto>>("SmartBuy.GetMisCadenas", new { usuarioid = usuarioId }, cancellationToken);

        public Task<StandarResponse<CantidadDto>> GuardarMisCadenasAsync(long usuarioId, IReadOnlyCollection<long> cadenasIds, CancellationToken cancellationToken)
            => ExecuteAsync<CantidadDto>("SmartBuy.GuardarMisCadenas", new
            {
                usuarioid = usuarioId,
                cadenasids = cadenasIds.Count > 0 ? string.Join(',', cadenasIds) : null
            }, cancellationToken);
    }
}
