namespace SmartBuy.Core.Models.Ingesta
{
    /// <summary>Un producto publicado tal como lo vio el bot en la fuente.</summary>
    public class IngestaItemRequest
    {
        /// <summary>SKU o id del producto en el sitio de la cadena. Clave de upsert.</summary>
        public string CodigoExterno { get; set; } = string.Empty;

        /// <summary>Nombre tal cual lo publica la cadena, sin normalizar.</summary>
        public string NombrePublicado { get; set; } = string.Empty;

        /// <summary>Código de barras si la fuente lo expone (8 a 14 dígitos).</summary>
        public string? EanPublicado { get; set; }

        public string? Url { get; set; }

        /// <summary>
        /// Categoría del sitio de donde el bot trajo el ítem, ya normalizada
        /// (bebidas/almacen/lacteos/limpieza/perfumeria). Opcional: una fuente
        /// sin categoría ingresa igual.
        /// </summary>
        public string? CategoriaCaptura { get; set; }

        public decimal PrecioLista { get; set; }

        public decimal? PrecioOferta { get; set; }

        /// <summary>Texto crudo de la promo ("2x1", "70% 2da unidad").</summary>
        public string? TipoOferta { get; set; }
    }
}
