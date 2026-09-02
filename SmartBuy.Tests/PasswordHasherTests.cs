using SmartBuy.Core.Common.Seguridad;
using Xunit;

namespace SmartBuy.Tests
{
    /// <summary>
    /// PBKDF2-SHA256 con salt por usuario y formato autocontenido
    /// "iteraciones.salt64.hash64". Lo crítico: el round-trip funciona, la
    /// password incorrecta falla, y un hash corrupto nunca explota (devuelve false).
    /// </summary>
    public class PasswordHasherTests
    {
        [Fact]
        public void Hash_y_verificar_round_trip()
        {
            var hash = PasswordHasher.Hash("123456");

            Assert.True(PasswordHasher.Verificar("123456", hash));
        }

        [Fact]
        public void Password_incorrecta_no_verifica()
        {
            var hash = PasswordHasher.Hash("123456");

            Assert.False(PasswordHasher.Verificar("1234567", hash));
            Assert.False(PasswordHasher.Verificar("", hash));
        }

        [Fact]
        public void La_misma_password_produce_hashes_distintos_por_el_salt()
        {
            var hash1 = PasswordHasher.Hash("123456");
            var hash2 = PasswordHasher.Hash("123456");

            Assert.NotEqual(hash1, hash2);
            // Y ambos siguen verificando: el salt viaja dentro del hash.
            Assert.True(PasswordHasher.Verificar("123456", hash1));
            Assert.True(PasswordHasher.Verificar("123456", hash2));
        }

        [Fact]
        public void El_formato_es_iteraciones_salt_hash()
        {
            var partes = PasswordHasher.Hash("abc").Split('.');

            Assert.Equal(3, partes.Length);
            Assert.Equal("100000", partes[0]);
            Assert.True(Convert.FromBase64String(partes[1]).Length > 0);
            Assert.True(Convert.FromBase64String(partes[2]).Length > 0);
        }

        [Theory]
        [InlineData("basura")]
        [InlineData("a.b.c")]              // base64 inválido
        [InlineData("100000.solo-dos")]    // faltan partes
        [InlineData("")]
        [InlineData("xx.QUJD.QUJD")]       // iteraciones no numéricas
        public void Hash_guardado_corrupto_devuelve_false_sin_explotar(string hashCorrupto)
        {
            Assert.False(PasswordHasher.Verificar("123456", hashCorrupto));
        }

        [Fact]
        public void Hash_adulterado_no_verifica()
        {
            var hash = PasswordHasher.Hash("123456");
            var partes = hash.Split('.');
            // Reemplaza el hash final por el salt (base64 válido, contenido incorrecto).
            var adulterado = $"{partes[0]}.{partes[1]}.{partes[1]}";

            Assert.False(PasswordHasher.Verificar("123456", adulterado));
        }
    }
}
