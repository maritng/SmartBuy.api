namespace SmartBuy.Core.Models.Catalogo
{
    /// <summary>
    /// Alta/edición de producto del catálogo maestro. En la edición viaja además
    /// el Id. Contenido: valor y unidad van juntos o no van (check en la base y
    /// validación en el servicio).
    /// </summary>
    public class GuardarProductoRequest
    {
        /// <summary>Solo para ActualizarProducto; ignorado en el alta.</summary>
        public long Id { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public long? MarcaId { get; set; }

        public long? CategoriaId { get; set; }

        public decimal? ContenidoValor { get; set; }

        /// <summary>L | ml | kg | g | un</summary>
        public string? ContenidoUnidad { get; set; }

        /// <summary>Código de barras, 8 a 14 dígitos. Único en el catálogo.</summary>
        public string? Ean { get; set; }
    }
}
