using SmartBuy.Core.Common;
using SmartBuy.Core.Models.Historico;
using Xunit;

namespace SmartBuy.Tests
{
    /// <summary>
    /// La señal "¿conviene comprar hoy?" compara el mejor precio del último día
    /// contra el promedio y los extremos de la ventana. Reglas: menos de 3 días
    /// = sin_datos; los extremos (mínimo/máximo) le ganan al promedio; dentro
    /// del ±3% del promedio es "normal" (el ruido diario no es señal).
    /// </summary>
    public class SenalCompraTests
    {
        private static HistoricoPunto Punto(int dia, decimal precio)
            => new() { Fecha = new DateOnly(2026, 9, dia), Precio = precio };

        [Fact]
        public void Sin_puntos_devuelve_sin_datos()
        {
            var senal = SenalCompra.Calcular(new List<HistoricoPunto>(), 90);

            Assert.Equal("sin_datos", senal.Veredicto);
            Assert.Equal(0, senal.DiasConDatos);
            Assert.Null(senal.PrecioActual);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public void Con_menos_de_tres_dias_avisa_pocos_datos_pero_da_los_numeros(int cantidadDias)
        {
            var puntos = Enumerable.Range(1, cantidadDias).Select(d => Punto(d, 1000m)).ToList();

            var senal = SenalCompra.Calcular(puntos, 90);

            Assert.Equal("sin_datos", senal.Veredicto);
            Assert.Equal(cantidadDias, senal.DiasConDatos);
            Assert.Equal(1000m, senal.PrecioActual);
        }

        [Fact]
        public void En_el_minimo_de_la_ventana_recomienda_comprar()
        {
            var puntos = new List<HistoricoPunto> { Punto(1, 1200m), Punto(2, 1100m), Punto(3, 1000m) };

            var senal = SenalCompra.Calcular(puntos, 90);

            Assert.Equal("minimo", senal.Veredicto);
            Assert.Equal(1000m, senal.PrecioActual);
            Assert.Equal(1000m, senal.Minimo);
        }

        [Fact]
        public void En_el_maximo_de_la_ventana_recomienda_esperar()
        {
            var puntos = new List<HistoricoPunto> { Punto(1, 1000m), Punto(2, 1100m), Punto(3, 1200m) };

            var senal = SenalCompra.Calcular(puntos, 90);

            Assert.Equal("maximo", senal.Veredicto);
            Assert.Equal(1200m, senal.Maximo);
        }

        [Fact]
        public void Bajo_el_promedio_mas_del_umbral_es_bueno()
        {
            // Promedio (1000+1100+950)/3 = 1016.67; actual 950 pero NO es el mínimo... sí lo es.
            // Para "bueno" el actual debe estar >3% bajo el promedio SIN ser el mínimo:
            var puntos = new List<HistoricoPunto> { Punto(1, 900m), Punto(2, 1150m), Punto(3, 950m) };
            // Promedio = 1000; actual 950 (-5%), mínimo 900 -> "bueno".

            var senal = SenalCompra.Calcular(puntos, 90);

            Assert.Equal("bueno", senal.Veredicto);
            Assert.Equal(-5.0m, senal.VariacionVsPromedio);
        }

        [Fact]
        public void Sobre_el_promedio_mas_del_umbral_es_caro()
        {
            // Promedio = 1000; actual 1050 (+5%), máximo 1100 -> "caro".
            var puntos = new List<HistoricoPunto> { Punto(1, 850m), Punto(2, 1100m), Punto(3, 1050m) };

            var senal = SenalCompra.Calcular(puntos, 90);

            Assert.Equal("caro", senal.Veredicto);
            Assert.Equal(5.0m, senal.VariacionVsPromedio);
        }

        [Fact]
        public void Dentro_del_umbral_es_normal()
        {
            // Promedio = 1000; actual 1010 (+1%) y no es extremo -> "normal".
            var puntos = new List<HistoricoPunto> { Punto(1, 970m), Punto(2, 1020m), Punto(3, 1010m) };

            var senal = SenalCompra.Calcular(puntos, 90);

            Assert.Equal("normal", senal.Veredicto);
        }

        [Fact]
        public void Con_varias_cadenas_el_mismo_dia_usa_el_mejor_precio_diario()
        {
            // Dos "cadenas" el día 3: 1300 y 1000. El mejor diario es 1000 = mínimo.
            var puntos = new List<HistoricoPunto>
            {
                Punto(1, 1200m), Punto(2, 1100m),
                Punto(3, 1300m), Punto(3, 1000m)
            };

            var senal = SenalCompra.Calcular(puntos, 90);

            Assert.Equal(3, senal.DiasConDatos);
            Assert.Equal("minimo", senal.Veredicto);
            Assert.Equal(1000m, senal.PrecioActual);
        }

        [Fact]
        public void El_ultimo_dia_manda_aunque_los_puntos_vengan_desordenados()
        {
            var puntos = new List<HistoricoPunto> { Punto(3, 1200m), Punto(1, 1000m), Punto(2, 1100m) };

            var senal = SenalCompra.Calcular(puntos, 90);

            Assert.Equal(1200m, senal.PrecioActual);
            Assert.Equal("maximo", senal.Veredicto);
        }

        [Fact]
        public void Precio_estable_es_minimo_y_maximo_a_la_vez_y_gana_minimo()
        {
            // Todo igual: actual <= mínimo se evalúa primero -> mensaje optimista.
            var puntos = new List<HistoricoPunto> { Punto(1, 1000m), Punto(2, 1000m), Punto(3, 1000m) };

            var senal = SenalCompra.Calcular(puntos, 90);

            Assert.Equal("minimo", senal.Veredicto);
        }
    }
}
