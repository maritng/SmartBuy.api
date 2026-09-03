using System.Globalization;
using SmartBuy.Core.Models.Historico;
using SmartBuy.Core.Models.Listas;

namespace SmartBuy.Core.Common
{
    /// <summary>
    /// La inflación personal de una canasta: arma la serie diaria del costo
    /// total óptimo (mejor precio del día por producto × cantidad) y calcula la
    /// variación SOLO entre días completos — un total al que le falta un
    /// producto no es comparable. Los productos sin ningún precio en la ventana
    /// se informan aparte y no cuentan para la completitud (si no, un producto
    /// recién agregado bloquearía la serie entera).
    /// </summary>
    public static class InflacionCanasta
    {
        // Los mensajes van al usuario: números en formato argentino, siempre.
        private static readonly CultureInfo EsAr = new("es-AR");

        public static InflacionCanastaResumen Calcular(
            IReadOnlyCollection<ListaDetalleItem> items,
            IReadOnlyCollection<InflacionPrecioFila> filas,
            int ventanaDias)
        {
            var resumen = new InflacionCanastaResumen
            {
                Dias = ventanaDias,
                ProductosEnLista = items.Count
            };

            var cantidades = items.ToDictionary(i => i.ProductoId, i => i.Cantidad);

            var conPrecio = filas.Select(f => f.ProductoId).ToHashSet();
            resumen.ProductosSinPrecio = items
                .Where(i => !conPrecio.Contains(i.ProductoId))
                .Select(i => i.Producto)
                .ToList();

            var comparables = items.Count - resumen.ProductosSinPrecio.Count;

            resumen.Puntos = filas
                .GroupBy(f => f.Fecha)
                .OrderBy(g => g.Key)
                .Select(g => new InflacionPunto
                {
                    Fecha = g.Key,
                    Total = g.Sum(f => f.Precio * cantidades.GetValueOrDefault(f.ProductoId, 1)),
                    ProductosConPrecio = g.Count(),
                    Completo = comparables > 0 && g.Count() == comparables
                })
                .ToList();

            resumen.Variacion = CalcularVariacion(resumen.Puntos, items.Count);

            return resumen;
        }

        private static InflacionVariacion CalcularVariacion(List<InflacionPunto> puntos, int productosEnLista)
        {
            var completos = puntos.Where(p => p.Completo).ToList();

            var variacion = new InflacionVariacion { DiasCompletos = completos.Count };

            if (productosEnLista == 0)
            {
                variacion.Mensaje = "La lista está vacía: agregale productos para medir tu inflación.";
                return variacion;
            }

            if (completos.Count == 0)
            {
                variacion.Mensaje = "Todavía no hay días con precios de todos los productos: la serie arranca sola con las próximas capturas.";
                return variacion;
            }

            var inicial = completos[0];
            var final = completos[^1];

            variacion.FechaInicial = inicial.Fecha;
            variacion.FechaFinal = final.Fecha;
            variacion.TotalInicial = inicial.Total;
            variacion.TotalFinal = final.Total;

            if (completos.Count == 1)
            {
                variacion.Mensaje = "Un solo día completo por ahora: la variación aparece sola cuando haya más capturas.";
                return variacion;
            }

            var porcentaje = inicial.Total == 0 ? 0 : Math.Round((final.Total - inicial.Total) / inicial.Total * 100, 1);
            var monto = Math.Round(final.Total - inicial.Total, 2);

            variacion.VariacionPorcentaje = porcentaje;
            variacion.VariacionMonto = monto;

            var desde = $"{inicial.Fecha:dd/MM}";
            var hasta = $"{final.Fecha:dd/MM}";

            variacion.Mensaje = porcentaje switch
            {
                > 0.5m => $"Tu canasta subió {porcentaje.ToString("0.#", EsAr)}% entre el {desde} y el {hasta} (+$ {monto.ToString("N2", EsAr)}).",
                < -0.5m => $"Tu canasta bajó {Math.Abs(porcentaje).ToString("0.#", EsAr)}% entre el {desde} y el {hasta} (-$ {Math.Abs(monto).ToString("N2", EsAr)}).",
                _ => $"Tu canasta está prácticamente igual entre el {desde} y el {hasta}."
            };

            return variacion;
        }
    }
}
