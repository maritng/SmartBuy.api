using SmartBuy.Core.Common;
using SmartBuy.Core.Models.Historico;
using SmartBuy.Core.Models.Listas;
using Xunit;

namespace SmartBuy.Tests
{
    /// <summary>
    /// La inflación personal: los totales diarios solo son comparables entre
    /// días COMPLETOS (todos los productos comparables con precio); los
    /// productos sin ningún precio en la ventana se informan aparte y no
    /// bloquean la serie.
    /// </summary>
    public class InflacionCanastaTests
    {
        private static ListaDetalleItem Item(long id, string nombre, int cantidad = 1)
            => new() { ProductoId = id, Producto = nombre, Cantidad = cantidad };

        private static InflacionPrecioFila Fila(int dia, long productoId, decimal precio)
            => new() { Fecha = new DateOnly(2026, 9, dia), ProductoId = productoId, Precio = precio };

        [Fact]
        public void Suma_por_cantidad_y_marca_dias_completos()
        {
            var items = new[] { Item(1, "Coca", 2), Item(2, "Yerba") };
            var filas = new[]
            {
                Fila(1, 1, 1000m), Fila(1, 2, 3000m),
                Fila(2, 1, 1000m) // día 2: falta la yerba
            };

            var resumen = InflacionCanasta.Calcular(items, filas, 90);

            Assert.Equal(2, resumen.Puntos.Count);
            var dia1 = resumen.Puntos[0];
            Assert.Equal(5000m, dia1.Total); // 1000×2 + 3000
            Assert.True(dia1.Completo);
            var dia2 = resumen.Puntos[1];
            Assert.Equal(2000m, dia2.Total);
            Assert.False(dia2.Completo);
            Assert.Equal(1, dia2.ProductosConPrecio);
        }

        [Fact]
        public void La_variacion_se_calcula_entre_el_primer_y_ultimo_dia_completo()
        {
            var items = new[] { Item(1, "Coca") };
            var filas = new[] { Fila(1, 1, 1000m), Fila(2, 1, 1200m), Fila(3, 1, 1100m) };

            var resumen = InflacionCanasta.Calcular(items, filas, 90);

            var variacion = resumen.Variacion;
            Assert.Equal(3, variacion.DiasCompletos);
            Assert.Equal(1000m, variacion.TotalInicial);
            Assert.Equal(1100m, variacion.TotalFinal);
            Assert.Equal(10.0m, variacion.VariacionPorcentaje);
            Assert.Equal(100m, variacion.VariacionMonto);
            Assert.Contains("subió 10%", variacion.Mensaje);
        }

        [Fact]
        public void Los_dias_incompletos_no_participan_de_la_variacion()
        {
            var items = new[] { Item(1, "Coca"), Item(2, "Yerba") };
            var filas = new[]
            {
                Fila(1, 1, 1000m), Fila(1, 2, 3000m),  // completo: 4000
                Fila(2, 1, 50m),                        // incompleto: no cuenta
                Fila(3, 1, 1100m), Fila(3, 2, 3100m)   // completo: 4200
            };

            var resumen = InflacionCanasta.Calcular(items, filas, 90);

            Assert.Equal(2, resumen.Variacion.DiasCompletos);
            Assert.Equal(4000m, resumen.Variacion.TotalInicial);
            Assert.Equal(4200m, resumen.Variacion.TotalFinal);
            Assert.Equal(5.0m, resumen.Variacion.VariacionPorcentaje);
        }

        [Fact]
        public void Canasta_que_baja_lo_dice_en_positivo()
        {
            var items = new[] { Item(1, "Coca") };
            var filas = new[] { Fila(1, 1, 1000m), Fila(2, 1, 900m) };

            var resumen = InflacionCanasta.Calcular(items, filas, 90);

            Assert.Equal(-10.0m, resumen.Variacion.VariacionPorcentaje);
            Assert.Contains("bajó 10%", resumen.Variacion.Mensaje);
        }

        [Fact]
        public void Variacion_chica_es_practicamente_igual()
        {
            var items = new[] { Item(1, "Coca") };
            var filas = new[] { Fila(1, 1, 1000m), Fila(2, 1, 1003m) };

            var resumen = InflacionCanasta.Calcular(items, filas, 90);

            Assert.Contains("prácticamente igual", resumen.Variacion.Mensaje);
        }

        [Fact]
        public void Producto_sin_ningun_precio_se_informa_y_no_bloquea_la_completitud()
        {
            var items = new[] { Item(1, "Coca"), Item(99, "Yerba nueva") };
            var filas = new[] { Fila(1, 1, 1000m), Fila(2, 1, 1100m) };

            var resumen = InflacionCanasta.Calcular(items, filas, 90);

            var sinPrecio = Assert.Single(resumen.ProductosSinPrecio);
            Assert.Equal("Yerba nueva", sinPrecio);
            // Los días son completos respecto de los productos comparables.
            Assert.All(resumen.Puntos, p => Assert.True(p.Completo));
            Assert.Equal(10.0m, resumen.Variacion.VariacionPorcentaje);
        }

        [Fact]
        public void Un_solo_dia_completo_no_da_variacion_pero_si_totales()
        {
            var items = new[] { Item(1, "Coca") };
            var filas = new[] { Fila(1, 1, 1000m) };

            var resumen = InflacionCanasta.Calcular(items, filas, 90);

            Assert.Equal(1, resumen.Variacion.DiasCompletos);
            Assert.Null(resumen.Variacion.VariacionPorcentaje);
            Assert.Equal(1000m, resumen.Variacion.TotalInicial);
            Assert.Contains("Un solo día", resumen.Variacion.Mensaje);
        }

        [Fact]
        public void Sin_filas_no_hay_puntos_y_el_mensaje_lo_explica()
        {
            var items = new[] { Item(1, "Coca") };

            var resumen = InflacionCanasta.Calcular(items, Array.Empty<InflacionPrecioFila>(), 90);

            Assert.Empty(resumen.Puntos);
            Assert.Equal(0, resumen.Variacion.DiasCompletos);
            Assert.Contains("Todavía no hay días", resumen.Variacion.Mensaje);
        }

        [Fact]
        public void Lista_vacia_lo_dice_claro()
        {
            var resumen = InflacionCanasta.Calcular(Array.Empty<ListaDetalleItem>(), Array.Empty<InflacionPrecioFila>(), 90);

            Assert.Equal(0, resumen.ProductosEnLista);
            Assert.Contains("vacía", resumen.Variacion.Mensaje);
        }
    }
}
