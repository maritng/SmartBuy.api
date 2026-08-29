namespace SmartBuy.Core.Models.Recomendacion
{
    /// <summary>La lista de compras del usuario: productos del catálogo + cantidades.</summary>
    public class ListaCompraRequest
    {
        public List<ListaCompraItem> Items { get; set; } = new();
    }

    public class ListaCompraItem
    {
        public long ProductoId { get; set; }

        /// <summary>Default 1 si no se informa.</summary>
        public int Cantidad { get; set; } = 1;
    }
}
