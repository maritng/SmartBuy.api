namespace SmartBuy.Core.Models
{
    /// <summary>Cadena de supermercado de la que se capturan precios.</summary>
    public class Cadena
    {
        public long Id { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string? SitioWeb { get; set; }
    }
}
