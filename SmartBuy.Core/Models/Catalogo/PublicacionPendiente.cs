namespace SmartBuy.Core.Models.Catalogo
{
    /// <summary>
    /// Fila de la cola de revisión: la publicación cruda con lo necesario para
    /// decidir el matching (texto publicado, EAN si vino, último precio).
    /// </summary>
    public class PublicacionPendiente
    {
        public long Id { get; set; }

        public long CadenaId { get; set; }

        public string Cadena { get; set; } = string.Empty;

        public string CodigoExterno { get; set; } = string.Empty;

        public string NombrePublicado { get; set; } = string.Empty;

        public string? EanPublicado { get; set; }

        public string? Url { get; set; }

        public DateTime FechaCreacion { get; set; }

        public decimal? UltimoPrecioLista { get; set; }

        public decimal? UltimoPrecioOferta { get; set; }

        public DateOnly? UltimaFechaPrecio { get; set; }

        /// <summary>Total de pendientes del filtro (sin paginar).</summary>
        public long Total { get; set; }
    }
}
