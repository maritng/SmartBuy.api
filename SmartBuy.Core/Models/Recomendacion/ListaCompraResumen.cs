namespace SmartBuy.Core.Models.Recomendacion
{
    /// <summary>Respuesta de ResolverLista: dónde comprar cada producto y cuánto se ahorra.</summary>
    public class ListaCompraResumen
    {
        public List<RecomendacionItem> Items { get; set; } = new();

        /// <summary>Productos del catálogo sin ningún precio capturado: nunca desaparecen en silencio.</summary>
        public List<ProductoNoDisponible> NoDisponibles { get; set; } = new();

        public RecomendacionTotales Totales { get; set; } = new();

        /// <summary>
        /// Deep links de carrito: un link por cadena del reparto que soporte
        /// carga por URL (VTEX), con todos sus productos y cantidades. Las
        /// cadenas sin soporte simplemente no aparecen.
        /// </summary>
        public List<CarritoCadena> Carritos { get; set; } = new();
    }

    /// <summary>Un click que arma el carrito real de la cadena con lo que va ahí.</summary>
    public class CarritoCadena
    {
        public long CadenaId { get; set; }

        public string Cadena { get; set; } = string.Empty;

        public string Url { get; set; } = string.Empty;
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

        /// <summary>SKU en la cadena elegida (alimenta el deep link de carrito).</summary>
        public string CodigoExterno { get; set; } = string.Empty;

        /// <summary>Página del producto en el sitio de la cadena elegida, si está capturada.</summary>
        public string? Url { get; set; }

        public decimal PrecioUnitario { get; set; }

        /// <summary>Texto crudo de la promo del súper (informativo).</summary>
        public string? TipoOferta { get; set; }

        /// <summary>true si la cantidad pedida aprovecha la promo por cantidad (el subtotal ya la incluye).</summary>
        public bool PromoAplicada { get; set; }

        /// <summary>Explicación de la promo resuelta ("3x2 aplicado: llevás 3, pagás 2" / "Hay 3x2 llevando 3 — pagás precio lleno").</summary>
        public string? DetallePromo { get; set; }

        /// <summary>Fecha del precio: si el bot no corre hace días, acá se nota.</summary>
        public DateOnly FechaPrecio { get; set; }

        /// <summary>Precio normalizado por unidad base ($/L, $/kg, $/un). Null si el producto no tiene contenido cargado.</summary>
        public decimal? PrecioPorUnidad { get; set; }

        /// <summary>Unidad base del precio normalizado: L, kg o un.</summary>
        public string? UnidadBase { get; set; }

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
