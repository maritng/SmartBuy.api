namespace SmartBuy.Core.Models.Catalogo
{
    /// <summary>Fila del listado paginado del catálogo. Total viene por window function (COUNT OVER).</summary>
    public class ProductoListado
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

        /// <summary>false = generado desde pendientes, con nombre provisorio: pendiente de curación.</summary>
        public bool Curado { get; set; }

        /// <summary>Total de filas del filtro (sin paginar), para la paginación del FE.</summary>
        public long Total { get; set; }
    }

    /// <summary>Resumen de GenerarDesdePendientes: catálogo creado + matching retroactivo.</summary>
    public class GeneracionPendientesResumen
    {
        public long ProductosCreados { get; set; }

        public long PublicacionesMatcheadas { get; set; }
    }
}
