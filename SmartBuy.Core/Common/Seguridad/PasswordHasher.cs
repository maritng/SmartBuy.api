using System.Security.Cryptography;

namespace SmartBuy.Core.Common.Seguridad
{
    /// <summary>
    /// Hash de contraseñas con PBKDF2-SHA256, salt aleatorio por usuario y
    /// comparación en tiempo constante. Formato: "iteraciones.salt64.hash64",
    /// autocontenido para poder subir las iteraciones a futuro sin migración
    /// (cada hash sabe con cuántas se creó).
    /// </summary>
    public static class PasswordHasher
    {
        private const int Iteraciones = 100_000;
        private const int SaltBytes = 16;
        private const int HashBytes = 32;

        public static string Hash(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(SaltBytes);
            var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iteraciones, HashAlgorithmName.SHA256, HashBytes);

            return $"{Iteraciones}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }

        public static bool Verificar(string password, string hashGuardado)
        {
            var partes = hashGuardado.Split('.');
            if (partes.Length != 3 || !int.TryParse(partes[0], out var iteraciones))
                return false;

            byte[] salt;
            byte[] esperado;
            try
            {
                salt = Convert.FromBase64String(partes[1]);
                esperado = Convert.FromBase64String(partes[2]);
            }
            catch (FormatException)
            {
                return false;
            }

            var calculado = Rfc2898DeriveBytes.Pbkdf2(password, salt, iteraciones, HashAlgorithmName.SHA256, esperado.Length);

            return CryptographicOperations.FixedTimeEquals(calculado, esperado);
        }
    }
}
