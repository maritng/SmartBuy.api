namespace SmartBuy.Core.Common
{
    /// <summary>
    /// Calcula la ventana de captura vigente a partir de los horarios
    /// configurados (hora argentina). La regla del orquestador es "una captura
    /// OK por ventana": el inicio de la ventana vigente es el último horario
    /// programado que ya pasó (puede ser de ayer, si todavía no llegó el primer
    /// horario de hoy).
    /// </summary>
    public static class VentanaCaptura
    {
        /// <summary>
        /// Argentina es UTC-3 fijo, sin horario de verano desde 2009: un offset
        /// constante evita depender de la tz database del contenedor.
        /// </summary>
        public static readonly TimeSpan OffsetArgentina = TimeSpan.FromHours(-3);

        private const int HorarioDefault = 7;

        /// <summary>
        /// Inicio (en UTC) de la ventana vigente en el instante dado. Con
        /// horarios [7, 19]: a las 10 ART devuelve hoy 07:00 ART; a las 03 ART
        /// devuelve ayer 19:00 ART. Horarios inválidos se ignoran; sin horarios
        /// válidos rige el default (7).
        /// </summary>
        public static DateTimeOffset InicioVentanaActualUtc(DateTimeOffset ahoraUtc, IReadOnlyCollection<int>? horarios)
        {
            var validos = (horarios ?? Array.Empty<int>())
                .Where(h => h is >= 0 and <= 23)
                .Distinct()
                .OrderBy(h => h)
                .ToList();

            if (validos.Count == 0)
                validos.Add(HorarioDefault);

            var ahoraArt = ahoraUtc.ToOffset(OffsetArgentina);
            var horaVigente = validos.LastOrDefault(h => h <= ahoraArt.Hour, -1);

            var inicioArt = horaVigente >= 0
                ? new DateTimeOffset(ahoraArt.Year, ahoraArt.Month, ahoraArt.Day, horaVigente, 0, 0, OffsetArgentina)
                : new DateTimeOffset(ahoraArt.Year, ahoraArt.Month, ahoraArt.Day, validos[^1], 0, 0, OffsetArgentina).AddDays(-1);

            return inicioArt.ToUniversalTime();
        }
    }
}
