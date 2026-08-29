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

        public long CadenaId { get; set; }

        public string Cadena { get; set; } = string.Empty;

        public long PublicacionId { get; set; }

        public string NombrePublicado { get; set; } = string.Empty;

        public DateOnly Fecha { get; set; }

        public decimal PrecioLista { get; set; }

        public decimal? PrecioOferta { get; set; }

        public string? TipoOferta { get; set; }

        public decimal PrecioEfectivo { get; set; }
    }
}
