using SmartBuy.Core.Common;
using Xunit;

namespace SmartBuy.Tests
{
    /// <summary>
    /// El deep link de carrito VTEX: /checkout/cart/add con pares sku/qty.
    /// La regla defensiva: ante cualquier cosa inválida, null — mejor sin botón
    /// que con un carrito roto.
    /// </summary>
    public class CarritoLinkBuilderTests
    {
        [Fact]
        public void Arma_la_url_con_varios_productos_y_cantidades()
        {
            var url = CarritoLinkBuilder.ArmarVtex("https://www.jumbo.com.ar", new[]
            {
                ("123", 2),
                ("456", 1)
            });

            Assert.Equal("https://www.jumbo.com.ar/checkout/cart/add?sku=123&qty=2&seller=1&sku=456&qty=1&seller=1", url);
        }

        [Fact]
        public void Tolera_base_url_con_barra_final()
        {
            var url = CarritoLinkBuilder.ArmarVtex("https://www.jumbo.com.ar/", new[] { ("123", 1) });

            Assert.Equal("https://www.jumbo.com.ar/checkout/cart/add?sku=123&qty=1&seller=1", url);
        }

        [Fact]
        public void Filtra_items_invalidos_y_conserva_los_validos()
        {
            var url = CarritoLinkBuilder.ArmarVtex("https://www.jumbo.com.ar", new[]
            {
                ("123", 2),
                ("", 1),      // sin SKU
                ("456", 0)    // cantidad inválida
            });

            Assert.Equal("https://www.jumbo.com.ar/checkout/cart/add?sku=123&qty=2&seller=1", url);
        }

        [Fact]
        public void Sin_items_validos_o_sin_base_url_devuelve_null()
        {
            Assert.Null(CarritoLinkBuilder.ArmarVtex("https://www.jumbo.com.ar", Array.Empty<(string, int)>()));
            Assert.Null(CarritoLinkBuilder.ArmarVtex("https://www.jumbo.com.ar", new[] { ("", 1) }));
            Assert.Null(CarritoLinkBuilder.ArmarVtex(null, new[] { ("123", 1) }));
            Assert.Null(CarritoLinkBuilder.ArmarVtex("  ", new[] { ("123", 1) }));
        }

        [Fact]
        public void Escapa_skus_con_caracteres_raros()
        {
            var url = CarritoLinkBuilder.ArmarVtex("https://sitio.com", new[] { ("a&b c", 1) });

            Assert.Equal("https://sitio.com/checkout/cart/add?sku=a%26b%20c&qty=1&seller=1", url);
        }
    }
}
