namespace SmartBuy.Core.Models.Recomendacion
{
    /// <summary>Respuesta de ResolverLista: dónde comprar cada producto y cuánto se ahorra.</summary>
    public class ListaCompraResumen
    {
        public List<RecomendacionItem> Items { get; set; } = new();

        /// <summary>Productos del catálogo sin ningún precio capturado: nunca desaparecen en silencio.</summary>
        public List<ProductoNoDisponible> NoDisponibles { get; set; } = new();

        public RecomendacionTotales Totales { get; set; } = new();
    }

    public class RecomendacionItem
    {
        public long ProductoId { get; set; }

        public string Producto { get; set; } = string.Empty;

        public int Cantidad { get; set; }

        public long CadenaId { get; set; }

        /// <summary>Dónde conviene comprarlo.</summary>
        public string Cadena { get; set; } = string.Empty;

        public string NombrePublicado { get; set; } = string.Empty;

        public decimal PrecioUnitario { get; set; }

        /// <summary>Promo cruda informativa ("2x1"): todavía no entra en el cálculo.</summary>
        public string? TipoOferta { get; set; }

        /// <summary>Fecha del precio: si el bot no corre hace días, acá se nota.</summary>
        public DateOnly FechaPrecio { get; set; }

        public decimal Subtotal { get; set; }

        /// <summary>En cuántas cadenas se encontró el producto (transparencia de la comparación).</summary>
        public int CadenasComparadas { get; set; }
    }

    public class ProductoNoDisponible
    {
        public long ProductoId { get; set; }

        public string Producto { get; set; } = string.Empty;
    }

    public class RecomendacionTotales
    {
        /// <summary>Cada producto en su cadena más barata.</summary>
        public decimal TotalOptimizado { get; set; }

        /// <summary>A cuántos supermercados hay que ir con el reparto óptimo.</summary>
        public int CadenasInvolucradas { get; set; }

        /// <summary>
        /// La cadena más barata para comprar TODO junto (solo entre las que tienen
        /// todos los productos disponibles). Null si ninguna tiene todo.
        /// </summary>
        public MejorCadenaUnica? MejorCadenaUnica { get; set; }

        /// <summary>Cuánto se ahorra repartiendo vs. la mejor cadena única. Null si no hay comparación posible.</summary>
        public decimal? Ahorro { get; set; }
    }

    public class MejorCadenaUnica
    {
        public long CadenaId { get; set; }

        public string Cadena { get; set; } = string.Empty;

        public decimal Total { get; set; }
    }
}
