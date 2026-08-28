namespace SmartBuy.Core.Models.Catalogo
{
    /// <summary>Detalle de un producto para el form de edición (incluye inactivos, con su flag).</summary>
    public class ProductoDetalle
    {
        public long Id { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public long? MarcaId { get; set; }

        public string? Marca { get; set; }

        public long? CategoriaId { get; set; }

        public string? Categoria { get; set; }

        public decimal? ContenidoValor { get; set; }

        public string? ContenidoUnidad { get; set; }

        public string? Ean { get; set; }

        public bool Activo { get; set; }
    }
}
