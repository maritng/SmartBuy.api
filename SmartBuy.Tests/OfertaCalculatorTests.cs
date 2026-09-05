using SmartBuy.Core.Common;
using Xunit;

namespace SmartBuy.Tests
{
    /// <summary>
    /// El calculador es DELIBERADAMENTE conservador: computa solo promos por
    /// cantidad. Tan importante como lo que computa es lo que NO computa
    /// ("% off" planos y tarjetas ya vienen en el precio publicado: computarlos
    /// descontaría dos veces).
    /// </summary>
    public class OfertaCalculatorTests
    {
        // ---- Promos NxM ----

        [Theory]
        [InlineData("3X2", 2.0 / 3.0)]
        [InlineData("2x1", 0.5)]
        [InlineData("Lleva 4 x 3", 0.75)]
        [InlineData("Promo 3 x 2 en toda la línea", 2.0 / 3.0)]
        public void NxM_computa_factor_m_sobre_n(string tipoOferta, double esperado)
        {
            var factor = OfertaCalculator.CalcularFactor(tipoOferta);

            Assert.NotNull(factor);
            Assert.Equal((decimal)esperado, factor.Value, precision: 6);
        }

        [Fact]
        public void NxM_caso_real_coto_3x2_de_550_da_366_67()
        {
            // Promo real capturada: galletitas $550 con "3X2".
            Assert.Equal(366.67m, OfertaCalculator.CalcularEfectivo(550m, "3X2"));
        }

        [Theory]
        [InlineData("2 x 1.75 litros")] // pack: es una medida, no una promo
        [InlineData("3x2.5L")]
        [InlineData("2 x 1,5 lts")]
        public void NxM_no_confunde_medidas_de_pack_con_promos(string texto)
        {
            Assert.Null(OfertaCalculator.CalcularFactor(texto));
        }

        [Theory]
        [InlineData("1x2")] // "promo" que encarece: no computable
        [InlineData("2x2")] // no es descuento
        public void NxM_ignora_combinaciones_sin_descuento(string texto)
        {
            Assert.Null(OfertaCalculator.CalcularFactor(texto));
        }

        // ---- Segunda unidad ----

        [Theory]
        [InlineData("2do al 70%", 0.65)]
        [InlineData("2da al 50%", 0.75)]
        [InlineData("70% en la 2da unidad", 0.65)]
        [InlineData("50% de descuento en la segunda unidad", 0.75)]
        [InlineData("Segunda unidad al 80", 0.60)]
        public void Segunda_unidad_computa_1_menos_x_sobre_200(string tipoOferta, double esperado)
        {
            var factor = OfertaCalculator.CalcularFactor(tipoOferta);

            Assert.NotNull(factor);
            Assert.Equal((decimal)esperado, factor.Value, precision: 6);
        }

        [Fact]
        public void Segunda_unidad_caso_real_dia_2do_al_70_de_5900_da_3835()
        {
            // La promo real que le cambió el ganador a la Coca-Cola.
            Assert.Equal(3835.00m, OfertaCalculator.CalcularEfectivo(5900m, "2do al 70%"));
        }

        // ---- Lo que NUNCA debe computar ----

        [Theory]
        [InlineData("20% off")]
        [InlineData("Hasta 35% de descuento")]
        [InlineData("Tarjeta Carrefour 15%")]
        [InlineData("15% con Tarjeta Cencosud")]
        [InlineData("Precio especial")]
        [InlineData("Oferta")]
        public void Descuentos_planos_y_tarjetas_no_computan(string texto)
        {
            Assert.Null(OfertaCalculator.CalcularFactor(texto));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("texto sin promo alguna")]
        public void Textos_vacios_o_sin_patron_devuelven_null(string? texto)
        {
            Assert.Null(OfertaCalculator.CalcularFactor(texto));
            Assert.Null(OfertaCalculator.CalcularEfectivo(1000m, texto));
        }

        [Fact]
        public void Porcentaje_invalido_mayor_a_100_no_computa()
        {
            Assert.Null(OfertaCalculator.CalcularFactor("2do al 150%"));
        }

        [Fact]
        public void Efectivo_redondea_a_dos_decimales()
        {
            // 1000 * 2/3 = 666.666... -> 666.67
            Assert.Equal(666.67m, OfertaCalculator.CalcularEfectivo(1000m, "3x2"));
        }

        // ---- La mecánica escalonada del renglón (matemática real del carrito) ----

        [Theory]
        [InlineData("3x2", 1, 1000, false)]  // no llega a la promo: lleno
        [InlineData("3x2", 2, 2000, false)]
        [InlineData("3x2", 3, 2000, true)]   // un grupo: pagás 2
        [InlineData("3x2", 4, 3000, true)]   // grupo + 1 suelta llena
        [InlineData("3x2", 6, 4000, true)]   // dos grupos
        [InlineData("2x1", 2, 1000, true)]
        [InlineData("2x1", 5, 3000, true)]   // dos pares + 1 llena
        public void NxM_escalonado_paga_por_grupos_completos(string promo, int cantidad, double esperado, bool aplicada)
        {
            var renglon = OfertaCalculator.CalcularRenglon(1000m, promo, cantidad);

            Assert.Equal((decimal)esperado, renglon.Total);
            Assert.Equal(aplicada, renglon.PromoAplicada);
            Assert.NotNull(renglon.DetallePromo);
        }

        [Theory]
        [InlineData("2do al 70%", 1, 1000, false)]
        [InlineData("2do al 70%", 2, 1300, true)]  // 2000 - 700
        [InlineData("2do al 70%", 3, 2300, true)]  // par con descuento + 1 llena
        [InlineData("2do al 70%", 4, 2600, true)]  // dos pares
        public void Segunda_unidad_escalonada_descuenta_por_par_completo(string promo, int cantidad, double esperado, bool aplicada)
        {
            var renglon = OfertaCalculator.CalcularRenglon(1000m, promo, cantidad);

            Assert.Equal((decimal)esperado, renglon.Total);
            Assert.Equal(aplicada, renglon.PromoAplicada);
        }

        [Fact]
        public void Renglon_sin_promo_computable_es_precio_por_cantidad_sin_detalle()
        {
            var renglon = OfertaCalculator.CalcularRenglon(1000m, "Tarjeta Carrefour 15%", 3);

            Assert.Equal(3000m, renglon.Total);
            Assert.False(renglon.PromoAplicada);
            Assert.Null(renglon.DetallePromo);

            Assert.Equal(2000m, OfertaCalculator.CalcularRenglon(1000m, null, 2).Total);
        }

        [Fact]
        public void Renglon_explica_la_promo_en_criollo()
        {
            Assert.Equal("3x2 aplicado: llevás 3, pagás 2", OfertaCalculator.CalcularRenglon(1000m, "3x2", 3).DetallePromo);
            Assert.Equal("Hay 3x2 llevando 3 — pagás precio lleno", OfertaCalculator.CalcularRenglon(1000m, "3x2", 1).DetallePromo);
        }

        [Fact]
        public void Renglon_con_cantidad_invalida_devuelve_cero()
        {
            Assert.Equal(0m, OfertaCalculator.CalcularRenglon(1000m, "3x2", 0).Total);
        }
    }
}
