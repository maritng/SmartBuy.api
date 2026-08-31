namespace SmartBuy.Core.Models.Recomendacion
{
    /// <summary>La lista de compras del usuario: productos del catálogo + cantidades.</summary>
    public class ListaCompraRequest
    {
        public List<ListaCompraItem> Items { get; set; } = new();

        /// <summary>
        /// Cadenas accesibles para el usuario: todo el cálculo (reparto óptimo,
        /// mejor cadena única, ahorro) se restringe a este universo. Null o vacío
        /// = todas las cadenas. Un producto que solo existe en cadenas excluidas
        /// cae en noDisponibles.
        /// </summary>
        public List<long>? CadenasIds { get; set; }
    }

    public class ListaCompraItem
    {
        public long ProductoId { get; set; }

        /// <summary>Default 1 si no se informa.</summary>
        public int Cantidad { get; set; } = 1;
    }
}
