using SmartBuy.Core.Common;
using Xunit;

namespace SmartBuy.Tests
{
    /// <summary>
    /// El saneador de ListPrice basura: una lista legítima nunca supera ~3× la
    /// venta (el "70% off" real); la anomalía de Cencosud es ~×82. El factor 10
    /// separa ambos mundos con margen. Sin precio de venta no se juzga.
    /// </summary>
    public class SaneadorPreciosTests
    {
        [Fact]
        public void Par_legitimo_con_descuento_pasa_intacto()
        {
            var par = SaneadorPrecios.Sanear(1000m, 800m);

            Assert.Equal(1000m, par.PrecioLista);
            Assert.Equal(800m, par.PrecioOferta);
            Assert.False(par.Saneado);
        }

        [Fact]
        public void Venta_igual_o_mayor_a_la_lista_no_es_oferta()
        {
            Assert.Null(SaneadorPrecios.Sanear(1000m, 1000m).PrecioOferta);
            Assert.Null(SaneadorPrecios.Sanear(1000m, 1200m).PrecioOferta);
        }

        [Fact]
        public void Descuento_agresivo_legitimo_70_off_no_se_sanea()
        {
            // Lista 3.3× la venta: promo real, debe sobrevivir.
            var par = SaneadorPrecios.Sanear(3300m, 1000m);

            Assert.Equal(3300m, par.PrecioLista);
            Assert.Equal(1000m, par.PrecioOferta);
            Assert.False(par.Saneado);
        }

        [Fact]
        public void Lista_basura_x82_se_sanea_con_la_venta_como_lista()
        {
            // El caso real de Cencosud: Elvive lista 1.785.124, venta 21.600.
            var par = SaneadorPrecios.Sanear(1785124m, 21600m);

            Assert.Equal(21600m, par.PrecioLista);
            Assert.Null(par.PrecioOferta);
            Assert.True(par.Saneado);
        }

        [Fact]
        public void El_borde_del_factor_es_estricto()
        {
            // Exactamente 10×: se respeta (no hay evidencia de corrupción).
            Assert.False(SaneadorPrecios.Sanear(10000m, 1000m).Saneado);
            // Apenas pasado: se sanea.
            Assert.True(SaneadorPrecios.Sanear(10000.01m, 1000m).Saneado);
        }

        [Fact]
        public void Sin_precio_de_venta_no_hay_referencia_y_pasa_intacto()
        {
            var sinVenta = SaneadorPrecios.Sanear(1785124m, null);

            Assert.Equal(1785124m, sinVenta.PrecioLista);
            Assert.Null(sinVenta.PrecioOferta);
            Assert.False(sinVenta.Saneado);

            Assert.False(SaneadorPrecios.Sanear(1000m, 0m).Saneado);
        }
    }
}
