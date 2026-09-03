namespace SmartBuy.Core.Models.Recomendacion
{
    /// <summary>
    /// Fila cruda del repo: el mejor precio vigente de un producto en una cadena.
    /// Sobre esta lista el servicio arma el reparto óptimo y los totales.
    /// </summary>
    public class PrecioProductoCadena
    {
        public long ProductoId { get; set; }

        public string Producto { get; set; } = string.Empty;

        public decimal? ContenidoValor { get; set; }

        public string? ContenidoUnidad { get; set; }

        public long CadenaId { get; set; }

        public string Cadena { get; set; } = string.Empty;

        public long PublicacionId { get; set; }

        public string NombrePublicado { get; set; } = string.Empty;

        /// <summary>SKU en la cadena: es lo que entiende el carrito del sitio (VTEX).</summary>
        public string CodigoExterno { get; set; } = string.Empty;

        /// <summary>Página del producto en el sitio de la cadena, si el bot la capturó.</summary>
        public string? Url { get; set; }

        public DateOnly Fecha { get; set; }

        public decimal PrecioLista { get; set; }

        public decimal? PrecioOferta { get; set; }

        public string? TipoOferta { get; set; }

        public decimal PrecioEfectivo { get; set; }
    }
}
