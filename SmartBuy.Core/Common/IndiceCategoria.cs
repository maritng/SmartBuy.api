using System.Globalization;
using SmartBuy.Core.Models.Historico;

namespace SmartBuy.Core.Common
{
    /// <summary>
    /// Encadena los eslabones diarios (canasta común de cada día contra su
    /// observación previa) en un índice base 100 por categoría. La base es
    /// virtual: el primer eslabón ya mueve el índice desde 100. Eslabones con
    /// suma previa cero o negativa se descartan (división imposible = dato roto,
    /// nunca voltea la serie).
    /// </summary>
    public static class IndiceCategoria
    {
        private static readonly CultureInfo EsAr = new("es-AR");

        public static List<SerieCategoria> Calcular(IReadOnlyCollection<EslabonCategoriaFila> eslabones, int ventanaDias)
        {
            return eslabones
                .GroupBy(e => e.Categoria)
                .Select(grupo => CalcularSerie(grupo.Key, grupo, ventanaDias))
                .OrderBy(s => s.Categoria)
                .ToList();
        }

        private static SerieCategoria CalcularSerie(string categoria, IEnumerable<EslabonCategoriaFila> eslabones, int ventanaDias)
        {
            var serie = new SerieCategoria { Categoria = categoria };
            var indice = 100m;

            foreach (var eslabon in eslabones.Where(e => e.SumaPrevia > 0).OrderBy(e => e.Fecha))
            {
                var factor = eslabon.SumaActual / eslabon.SumaPrevia;
                indice *= factor;

                serie.Puntos.Add(new PuntoIndice
                {
                    Fecha = eslabon.Fecha,
                    Indice = Math.Round(indice, 2),
                    VariacionDia = Math.Round((factor - 1m) * 100m, 2),
                    Publicaciones = eslabon.Publicaciones
                });
            }

            if (serie.Puntos.Count == 0)
            {
                serie.Mensaje = "Todavía juntando historia: hacen falta al menos dos días de capturas para el primer movimiento.";
                return serie;
            }

            var ultimo = serie.Puntos[^1];
            serie.VariacionVentana = Math.Round(ultimo.Indice - 100m, 1);
            serie.VariacionUltimoDia = Math.Round(ultimo.VariacionDia, 1);
            serie.PublicacionesUltimoDia = ultimo.Publicaciones;

            var variacion = serie.VariacionVentana.Value;
            var texto = variacion.ToString("0.#", EsAr);

            serie.Mensaje = variacion switch
            {
                > 0.5m => $"Subió {texto}% en la ventana (canasta de {ultimo.Publicaciones} precios).",
                < -0.5m => $"Bajó {Math.Abs(variacion).ToString("0.#", EsAr)}% en la ventana (canasta de {ultimo.Publicaciones} precios).",
                _ => $"Prácticamente estable en la ventana (canasta de {ultimo.Publicaciones} precios)."
            };

            return serie;
        }
    }
}
