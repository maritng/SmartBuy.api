using SmartBuy.Core.Common;
using SmartBuy.Core.Models.Historico;
using Xunit;

namespace SmartBuy.Tests
{
    /// <summary>
    /// El índice por categoría se encadena desde una base virtual de 100:
    /// cada eslabón compara la canasta común del día contra su observación
    /// previa (suma vs. suma), así el catálogo puede crecer sin distorsionar.
    /// </summary>
    public class IndiceCategoriaTests
    {
        private static EslabonCategoriaFila Eslabon(string categoria, int dia, decimal previa, decimal actual, int publicaciones = 10)
            => new()
            {
                Categoria = categoria,
                Fecha = new DateOnly(2026, 9, dia),
                SumaPrevia = previa,
                SumaActual = actual,
                Publicaciones = publicaciones
            };

        [Fact]
        public void Encadena_los_eslabones_desde_base_100()
        {
            var filas = new[]
            {
                Eslabon("lacteos", 2, 1000m, 1100m), // +10% -> 110
                Eslabon("lacteos", 3, 1100m, 1210m)  // +10% -> 121
            };

            var series = IndiceCategoria.Calcular(filas, 90);

            var serie = Assert.Single(series);
            Assert.Equal(new[] { 110m, 121m }, serie.Puntos.Select(p => p.Indice));
            Assert.Equal(new[] { 10m, 10m }, serie.Puntos.Select(p => p.VariacionDia));
            Assert.Equal(21m, serie.VariacionVentana);
            Assert.Equal(10m, serie.VariacionUltimoDia);
            Assert.Contains("Subió 21%", serie.Mensaje);
        }

        [Fact]
        public void La_canasta_puede_cambiar_de_tamanio_sin_distorsionar()
        {
            // Día 2: canasta de 30 ítems sube 5%. Día 3: canasta de 45 (entraron
            // nuevos con historia) baja 2%. El índice solo refleja los cambios.
            var filas = new[]
            {
                Eslabon("bebidas", 2, 30000m, 31500m, publicaciones: 30),
                Eslabon("bebidas", 3, 49000m, 48020m, publicaciones: 45)
            };

            var serie = Assert.Single(IndiceCategoria.Calcular(filas, 90));

            Assert.Equal(105m, serie.Puntos[0].Indice);
            Assert.Equal(102.9m, serie.Puntos[1].Indice); // 105 * 0.98
            Assert.Equal(45, serie.PublicacionesUltimoDia);
        }

        [Fact]
        public void Cada_categoria_arma_su_propia_serie()
        {
            var filas = new[]
            {
                Eslabon("lacteos", 2, 1000m, 1050m),
                Eslabon("limpieza", 2, 2000m, 1900m)
            };

            var series = IndiceCategoria.Calcular(filas, 90);

            Assert.Equal(2, series.Count);
            Assert.Equal(105m, series.Single(s => s.Categoria == "lacteos").Puntos[0].Indice);
            Assert.Equal(95m, series.Single(s => s.Categoria == "limpieza").Puntos[0].Indice);
            Assert.Contains("Bajó 5%", series.Single(s => s.Categoria == "limpieza").Mensaje);
        }

        [Fact]
        public void Eslabones_desordenados_se_encadenan_por_fecha()
        {
            var filas = new[]
            {
                Eslabon("almacen", 3, 1100m, 1210m),
                Eslabon("almacen", 2, 1000m, 1100m)
            };

            var serie = Assert.Single(IndiceCategoria.Calcular(filas, 90));

            Assert.Equal(new DateOnly(2026, 9, 2), serie.Puntos[0].Fecha);
            Assert.Equal(121m, serie.Puntos[1].Indice);
        }

        [Fact]
        public void Suma_previa_invalida_se_descarta_sin_voltear_la_serie()
        {
            var filas = new[]
            {
                Eslabon("perfumeria", 2, 0m, 500m),     // dato roto: se ignora
                Eslabon("perfumeria", 3, 1000m, 1030m)
            };

            var serie = Assert.Single(IndiceCategoria.Calcular(filas, 90));

            var punto = Assert.Single(serie.Puntos);
            Assert.Equal(103m, punto.Indice);
        }

        [Fact]
        public void Variacion_chica_es_estable()
        {
            var filas = new[] { Eslabon("almacen", 2, 10000m, 10020m) };

            var serie = Assert.Single(IndiceCategoria.Calcular(filas, 90));

            Assert.Contains("estable", serie.Mensaje);
        }

        [Fact]
        public void Sin_eslabones_no_hay_series()
        {
            Assert.Empty(IndiceCategoria.Calcular(Array.Empty<EslabonCategoriaFila>(), 90));
        }
    }
}
