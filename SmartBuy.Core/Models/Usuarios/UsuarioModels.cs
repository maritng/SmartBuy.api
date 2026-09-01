namespace SmartBuy.Core.Models.Usuarios
{
    public class RegistrarRequest
    {
        public string Email { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// Fila interna de usuario para el flujo de auth. El PasswordHash viaja solo
    /// hasta AuthServices para verificar: NUNCA sale en respuestas de la API.
    /// </summary>
    public class UsuarioCuenta
    {
        public long Id { get; set; }

        public string Email { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public bool Activo { get; set; }
    }

    /// <summary>Lo único del usuario que sale hacia afuera.</summary>
    public class UsuarioPublico
    {
        public long Id { get; set; }

        public string Email { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;
    }

    public class MisCadenasRequest
    {
        /// <summary>Vacío o null = todas las cadenas.</summary>
        public List<long>? CadenasIds { get; set; }
    }

    public class CadenaIdDto
    {
        public long CadenaId { get; set; }
    }
}
