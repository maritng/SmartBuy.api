using System.Globalization;
using System.Text.RegularExpressions;

namespace SmartBuy.Core.Common
{
    /// <summary>
    /// Extrae el contenido (valor + unidad) del nombre publicado de un producto:
    /// "Fideos Tirabuzón 500 G" -> (500, g); "Gaseosa 2,25Lt" -> (2.25, L).
    /// Toma el ÚLTIMO gramaje del texto (suele ir al final; en "Pepitos 3u 357g"
    /// gana 357g). Es un proponedor: llena contenido en productos curado=false y
    /// la curación humana confirma o corrige. La unidad "un" no se parsea (muy
    /// ambigua en texto libre).
    /// </summary>
    public static partial class ContenidoParser
    {
        [GeneratedRegex(@"(\d+(?:[.,]\d+)?)\s*(ml|cc|kg|kilos?|l(?:ts?|itros?)?|g(?:rs?|ramos)?)\b", RegexOptions.IgnoreCase)]
        private static partial Regex Patron();

        public static (decimal Valor, string Unidad)? Parsear(string? nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return null;

            var coincidencias = Patron().Matches(nombre);
            if (coincidencias.Count == 0)
                return null;

            var ultima = coincidencias[^1];

            if (!decimal.TryParse(ultima.Groups[1].Value.Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out var valor))
                return null;

            if (valor <= 0 || valor > 100_000)
                return null;

            var unidad = NormalizarUnidad(ultima.Groups[2].Value);

            return unidad == null ? null : (valor, unidad);
        }

        private static string? NormalizarUnidad(string cruda)
        {
            var unidad = cruda.ToLowerInvariant();

            if (unidad is "ml" or "cc")
                return "ml";

            if (unidad == "kg" || unidad.StartsWith("kilo"))
                return "kg";

            if (unidad.StartsWith("l"))
                return "L";

            if (unidad.StartsWith("g"))
                return "g";

            return null;
        }
    }
}
