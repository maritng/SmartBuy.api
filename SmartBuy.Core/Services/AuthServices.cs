using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using SmartBuy.Core.Common.Responses;
using SmartBuy.Core.Common.Seguridad;
using SmartBuy.Core.Interfaces.Repositories;
using SmartBuy.Core.Interfaces.Services;
using SmartBuy.Core.Models.Usuarios;

namespace SmartBuy.Core.Services
{
    /// <summary>
    /// Registro y login. Los mensajes de fallo de login son deliberadamente
    /// idénticos (credenciales inválidas): no se revela si el email existe.
    /// Nada sensible se loguea jamás (ni passwords ni hashes).
    /// </summary>
    public partial class AuthServices : IAuthServices
    {
        // Mínimo 6 para la etapa de pruebas del MVP (el usuario usa 123456 en
        // las cuentas de prueba). ENDURECER antes de cualquier entorno real:
        // mínimo 8+ y chequeo contra contraseñas comunes.
        private const int PasswordMinimo = 6;
        private const string CredencialesInvalidas = "Email o contraseña incorrectos.";

        [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
        private static partial Regex EmailRegex();

        private readonly IUsuarioRepository _usuarioRepository;
        private readonly ILogger<AuthServices> _logger;

        public AuthServices(IUsuarioRepository usuarioRepository, ILogger<AuthServices> logger)
        {
            _usuarioRepository = usuarioRepository;
            _logger = logger;
        }

        public async Task<StandarResponse<UsuarioPublico>> RegistrarAsync(RegistrarRequest request, CancellationToken cancellationToken)
        {
            var errores = new List<string>();
            var email = request?.Email?.Trim().ToLowerInvariant() ?? string.Empty;
            var nombre = request?.Nombre?.Trim() ?? string.Empty;

            if (!EmailRegex().IsMatch(email) || email.Length > 320)
                errores.Add("El email no es válido.");

            if (string.IsNullOrWhiteSpace(nombre) || nombre.Length > 100)
                errores.Add("El nombre es obligatorio (máximo 100 caracteres).");

            if (string.IsNullOrEmpty(request?.Password) || request.Password.Length < PasswordMinimo)
                errores.Add($"La contraseña debe tener al menos {PasswordMinimo} caracteres.");

            if (errores.Count > 0)
                return Fallo(errores);

            var creacion = await _usuarioRepository.CrearUsuarioAsync(email, nombre, PasswordHasher.Hash(request!.Password), cancellationToken);

            if (!creacion.Success)
            {
                if (creacion.Errors.Any(e => e.Contains("23505")))
                    return Fallo(new List<string> { "Ya existe una cuenta con ese email." });

                return new StandarResponse<UsuarioPublico> { Success = false, Errors = creacion.Errors };
            }

            _logger.LogInformation("Cuenta creada: usuario {UsuarioId}.", creacion.Result?.Id);

            return new StandarResponse<UsuarioPublico>
            {
                Success = true,
                Result = new UsuarioPublico { Id = creacion.Result!.Id, Email = email, Nombre = nombre }
            };
        }

        public async Task<StandarResponse<UsuarioPublico>> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
        {
            var email = request?.Email?.Trim().ToLowerInvariant() ?? string.Empty;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(request?.Password))
                return Fallo(new List<string> { CredencialesInvalidas });

            var busqueda = await _usuarioRepository.GetUsuarioByEmailAsync(email, cancellationToken);

            if (!busqueda.Success)
                return new StandarResponse<UsuarioPublico> { Success = false, Errors = busqueda.Errors };

            var cuenta = busqueda.Result?.FirstOrDefault();

            if (cuenta == null || cuenta.Id <= 0 || !cuenta.Activo || !PasswordHasher.Verificar(request.Password, cuenta.PasswordHash))
            {
                _logger.LogWarning("Login fallido para un email (no se registra cuál).");
                return Fallo(new List<string> { CredencialesInvalidas });
            }

            await _usuarioRepository.ActualizarUltimoAccesoAsync(cuenta.Id, cancellationToken);

            return new StandarResponse<UsuarioPublico>
            {
                Success = true,
                Result = new UsuarioPublico { Id = cuenta.Id, Email = cuenta.Email, Nombre = cuenta.Nombre }
            };
        }

        private static StandarResponse<UsuarioPublico> Fallo(List<string> errores)
            => new() { Success = false, Errors = errores };
    }
}
