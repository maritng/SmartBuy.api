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

        /// <summary>
        /// El total de un renglón con la mecánica ESCALONADA real del súper,
        /// dada la cantidad que se lleva: un "3x2" solo descuenta por cada grupo
        /// de 3 completo (llevando 1 se paga lleno), y un "2do al X%" descuenta
        /// por cada PAR completo (llevando 3: dos llenos + uno con descuento).
        /// Es la matemática de la recomendación; el precio_efectivo por unidad
        /// del histórico sigue usando CalcularEfectivo.
        /// </summary>
        public static RenglonConPromo CalcularRenglon(decimal precioUnitario, string? tipoOferta, int cantidad)
        {
            if (cantidad <= 0)
                return new RenglonConPromo { Total = 0 };

            var sinPromo = new RenglonConPromo { Total = Math.Round(precioUnitario * cantidad, 2) };

            if (string.IsNullOrWhiteSpace(tipoOferta))
                return sinPromo;

            var nxm = PatronNxM().Match(tipoOferta);
            if (nxm.Success)
            {
                var n = int.Parse(nxm.Groups[1].Value);
                var m = int.Parse(nxm.Groups[2].Value);
                if (n > m && m >= 1)
                {
                    var grupos = cantidad / n;
                    var unidadesPagas = cantidad - grupos * (n - m);

                    return new RenglonConPromo
                    {
                        Total = Math.Round(precioUnitario * unidadesPagas, 2),
                        PromoAplicada = grupos > 0,
                        DetallePromo = grupos > 0
                            ? $"{n}x{m} aplicado: llevás {cantidad}, pagás {unidadesPagas}"
                            : $"Hay {n}x{m} llevando {n} — pagás precio lleno"
                    };
                }
            }

            var segunda = PatronSegundaUnidad().Match(tipoOferta);
            if (!segunda.Success)
                segunda = PatronSegundaAl().Match(tipoOferta);

            if (segunda.Success)
            {
                var porcentaje = int.Parse(segunda.Groups[1].Value);
                if (porcentaje is > 0 and <= 100)
                {
                    var pares = cantidad / 2;

                    return new RenglonConPromo
                    {
                        Total = Math.Round(precioUnitario * cantidad - pares * precioUnitario * porcentaje / 100m, 2),
                        PromoAplicada = pares > 0,
                        DetallePromo = pares > 0
                            ? $"2da unidad al {porcentaje}% aplicada: llevás {cantidad}, {pares} con descuento"
                            : $"Hay 2da unidad al {porcentaje}% llevando 2 — pagás precio lleno"
                    };
                }
            }

            return sinPromo;
        }
    }

    /// <summary>El total de un renglón (precio × cantidad) con su promo por cantidad resuelta.</summary>
    public sealed class RenglonConPromo
    {
        public decimal Total { get; init; }

        /// <summary>true si la cantidad alcanzó para aprovechar la promo al menos una vez.</summary>
        public bool PromoAplicada { get; init; }

        /// <summary>Explicación para el usuario ("3x2 aplicado: llevás 3, pagás 2"). Null sin promo computable.</summary>
        public string? DetallePromo { get; init; }
    }
}
