using SmartBuy.Core.Common;
using Xunit;

namespace SmartBuy.Tests
{
    /// <summary>
    /// El parser propone (valor, unidad) desde el nombre publicado. Regla clave:
    /// toma el ÚLTIMO gramaje del texto (suele ir al final) y normaliza unidades
    /// (cc->ml, litros->L, kilos->kg, grs->g). "un" no se parsea: muy ambigua.
    /// </summary>
    public class ContenidoParserTests
    {
        [Theory]
        [InlineData("Gaseosa Coca-Cola 2,25Lt", 2.25, "L")]
        [InlineData("Agua mineral 2.25 litros", 2.25, "L")]
        [InlineData("Vino tinto 750 ml", 750, "ml")]
        [InlineData("Cerveza rubia 473cc", 473, "ml")]
        [InlineData("Fideos Tirabuzón 500 G", 500, "g")]
        [InlineData("Arroz largo fino 1kg", 1, "kg")]
        [InlineData("Yerba mate 1 kilo", 1, "kg")]
        [InlineData("Azúcar 2 kilos", 2, "kg")]
        [InlineData("Galletitas 300grs", 300, "g")]
        [InlineData("Café molido 250 gramos", 250, "g")]
        [InlineData("Leche entera 1 L", 1, "L")]
        [InlineData("Aceite girasol 1,5 lts", 1.5, "L")]
        public void Parsea_gramajes_tipicos_y_normaliza_unidades(string nombre, double valor, string unidad)
        {
            var resultado = ContenidoParser.Parsear(nombre);

            Assert.NotNull(resultado);
            Assert.Equal((decimal)valor, resultado.Value.Valor);
            Assert.Equal(unidad, resultado.Value.Unidad);
        }

        [Theory]
        [InlineData("Pepitos 3u 357g", 357, "g")] // el último gramaje gana
        [InlineData("Pack 6 x 500ml", 500, "ml")]
        [InlineData("Promo 2 x 1,5 litros", 1.5, "L")]
        public void Con_varios_numeros_gana_el_ultimo_gramaje(string nombre, double valor, string unidad)
        {
            var resultado = ContenidoParser.Parsear(nombre);

            Assert.NotNull(resultado);
            Assert.Equal((decimal)valor, resultado.Value.Valor);
            Assert.Equal(unidad, resultado.Value.Unidad);
        }

        [Theory]
        [InlineData("Coca-Cola Sabor Original")] // sin gramaje
        [InlineData("Huevos 12 un")]             // "un" es deliberadamente no parseable
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Sin_gramaje_reconocible_devuelve_null(string? nombre)
        {
            Assert.Null(ContenidoParser.Parsear(nombre));
        }

        [Theory]
        [InlineData("Producto raro 0 g")]       // valor cero
        [InlineData("Producto raro 200000 g")]  // fuera de rango sano
        public void Valores_absurdos_devuelven_null(string nombre)
        {
            Assert.Null(ContenidoParser.Parsear(nombre));
        }
    }
}
