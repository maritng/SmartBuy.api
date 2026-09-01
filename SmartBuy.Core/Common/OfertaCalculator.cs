using System.Text.RegularExpressions;

namespace SmartBuy.Core.Common
{
    /// <summary>
    /// Computa el precio efectivo desde el descriptor crudo de promo
    /// (tipo_oferta). DELIBERADAMENTE CONSERVADOR: solo promos condicionadas a
    /// cantidad, que seguro NO están en el precio publicado:
    ///   * "NxM" (2x1, 3x2) -> factor M/N
    ///   * "X% la 2da unidad" / "2do al X%" -> factor 1 - X/200
    /// Lo demás queda informativo: "% off" planos ya vienen reflejados en el
    /// precio (computarlos descontaría dos veces) y las promos de tarjeta
    /// dependen del medio de pago. Un precio honesto vale más que un ahorro
    /// inflado.
    /// </summary>
    public static partial class OfertaCalculator
    {
        // El (?![.,]?\d) evita falsos positivos tipo "2 x 1.75 litros" (packs).
        [GeneratedRegex(@"\b(\d{1,2})\s*x\s*(\d{1,2})(?![.,]?\d)", RegexOptions.IgnoreCase)]
        private static partial Regex PatronNxM();

        [GeneratedRegex(@"(\d{1,3})\s*%[^%]{0,20}?(?:2\s*da|2\s*°|segunda)\s*unidad", RegexOptions.IgnoreCase)]
        private static partial Regex PatronSegundaUnidad();

        [GeneratedRegex(@"(?:2\s*d[oa]|segunda)\s*(?:al?|unidad\s+al?)\s*(\d{1,3})\s*%?", RegexOptions.IgnoreCase)]
        private static partial Regex PatronSegundaAl();

        /// <summary>Factor multiplicador sobre el precio de lista, o null si el texto no es computable.</summary>
        public static decimal? CalcularFactor(string? tipoOferta)
        {
            if (string.IsNullOrWhiteSpace(tipoOferta))
                return null;

            var nxm = PatronNxM().Match(tipoOferta);
            if (nxm.Success)
            {
                var n = int.Parse(nxm.Groups[1].Value);
                var m = int.Parse(nxm.Groups[2].Value);
                if (n > m && m >= 1)
                    return (decimal)m / n;
            }

            var segunda = PatronSegundaUnidad().Match(tipoOferta);
            if (!segunda.Success)
                segunda = PatronSegundaAl().Match(tipoOferta);

            if (segunda.Success)
            {
                var porcentaje = int.Parse(segunda.Groups[1].Value);
                if (porcentaje is > 0 and <= 100)
                    return 1m - porcentaje / 200m;
            }

            return null;
        }

        /// <summary>Precio efectivo por unidad comprada desde la promo, o null si no es computable.</summary>
        public static decimal? CalcularEfectivo(decimal precioLista, string? tipoOferta)
        {
            var factor = CalcularFactor(tipoOferta);
            return factor == null ? null : Math.Round(precioLista * factor.Value, 2);
        }
    }
}
