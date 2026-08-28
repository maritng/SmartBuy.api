namespace SmartBuy.Core.Models.Ingesta
{
    /// <summary>Resultado del registro atómico de un ítem (upsert publicación + precio).</summary>
    public class ItemCapturaResultado
    {
        public long PublicacionId { get; set; }

        public string EstadoMatching { get; set; } = string.Empty;
    }
}
