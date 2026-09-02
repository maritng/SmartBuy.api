using SmartBuy.Core.Models.Historico;

namespace SmartBuy.Core.Common
{
    /// <summary>
    /// "¿Conviene comprar hoy?": compara el mejor precio del último día contra
    /// el promedio y los extremos de la ventana. Trabaja sobre el mejor precio
    /// diario ENTRE cadenas (lo que efectivamente pagarías repartiendo).
    /// Umbral del 3%: por debajo de esa diferencia el precio se considera "en
    /// línea" — el ruido diario de los súper no es señal.
    /// </summary>
    public static class SenalCompra
    {
        private const int MinimoDias = 3;
        private const decimal UmbralPorcentaje = 3m;

        /// <param name="mejoresPorDia">El mejor precio de cada día con datos (cualquier orden).</param>
        /// <param name="ventanaDias">Tamaño de la ventana consultada, solo para los mensajes.</param>
        public static SenalCompraResultado Calcular(IReadOnlyCollection<HistoricoPunto> mejoresPorDia, int ventanaDias)
        {
            var dias = mejoresPorDia
                .GroupBy(p => p.Fecha)
                .Select(g => new HistoricoPunto { Fecha = g.Key, Precio = g.Min(p => p.Precio) })
                .OrderBy(p => p.Fecha)
                .ToList();

            if (dias.Count == 0)
                return new SenalCompraResultado
                {
                    Veredicto = "sin_datos",
                    Mensaje = "Todavía no hay precios capturados para este producto.",
                    DiasConDatos = 0
                };

            var actual = dias[^1].Precio;
            var promedio = Math.Round(dias.Average(p => p.Precio), 2);
            var minimo = dias.Min(p => p.Precio);
            var maximo = dias.Max(p => p.Precio);
            var variacion = promedio == 0 ? 0 : Math.Round((actual - promedio) / promedio * 100, 1);

            var resultado = new SenalCompraResultado
            {
                PrecioActual = actual,
                Promedio = promedio,
                Minimo = minimo,
                Maximo = maximo,
                VariacionVsPromedio = variacion,
                DiasConDatos = dias.Count
            };

            if (dias.Count < MinimoDias)
            {
                resultado.Veredicto = "sin_datos";
                resultado.Mensaje = $"Todavía hay pocos datos ({dias.Count} {(dias.Count == 1 ? "día" : "días")}): la señal va a mejorar sola a medida que los bots acumulen histórico.";
                return resultado;
            }

            // El orden importa: los extremos le ganan a la comparación con el promedio.
            if (actual <= minimo)
            {
                resultado.Veredicto = "minimo";
                resultado.Mensaje = $"Está en su precio más bajo de los últimos {ventanaDias} días. Buen momento para comprar.";
            }
            else if (actual >= maximo)
            {
                resultado.Veredicto = "maximo";
                resultado.Mensaje = $"Está en su precio más alto de los últimos {ventanaDias} días. Si podés esperar, esperá.";
            }
            else if (variacion <= -UmbralPorcentaje)
            {
                resultado.Veredicto = "bueno";
                resultado.Mensaje = $"Hoy está {Math.Abs(variacion):0.#}% abajo del promedio del período. Buen precio.";
            }
            else if (variacion >= UmbralPorcentaje)
            {
                resultado.Veredicto = "caro";
                resultado.Mensaje = $"Hoy está {variacion:0.#}% arriba del promedio del período.";
            }
            else
            {
                resultado.Veredicto = "normal";
                resultado.Mensaje = "En línea con el promedio del período.";
            }

            return resultado;
        }
    }
}
