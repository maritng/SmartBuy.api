namespace SmartBuy.Core.Models.Ingesta
{
    /// <summary>Fila del panel de capturas: una corrida de bot con su resultado.</summary>
    public class CapturaListado
    {
        public long Id { get; set; }

        public long CadenaId { get; set; }

        public string Cadena { get; set; } = string.Empty;

        public string Fuente { get; set; } = string.Empty;

        public string Estado { get; set; } = string.Empty;

        public DateTime FechaInicio { get; set; }

        public DateTime? FechaFin { get; set; }

        public int? CantItems { get; set; }

        public string? ErrorDetalle { get; set; }

        /// <summary>Null mientras la captura sigue en proceso.</summary>
        public int? DuracionSegundos { get; set; }
    }
}
