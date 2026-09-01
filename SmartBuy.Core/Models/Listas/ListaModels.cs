namespace SmartBuy.Core.Models.Listas
{
    /// <summary>Fila del listado "mis listas".</summary>
    public class ListaResumen
    {
        public long Id { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public int CantItems { get; set; }

        public DateTime Fecha { get; set; }
    }

    /// <summary>Fila cruda del repo para una lista con sus ítems (LEFT JOIN: lista vacía = 1 fila con ProductoId null).</summary>
    public class ListaItemFila
    {
        public long ListaId { get; set; }

        public string ListaNombre { get; set; } = string.Empty;

        public long? ProductoId { get; set; }

        public string? Producto { get; set; }

        public int? Cantidad { get; set; }
    }

    public class ListaDetalle
    {
        public long Id { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public List<ListaDetalleItem> Items { get; set; } = new();
    }

    public class ListaDetalleItem
    {
        public long ProductoId { get; set; }

        public string Producto { get; set; } = string.Empty;

        public int Cantidad { get; set; }
    }

    public class GuardarListaRequest
    {
        /// <summary>Solo para GuardarLista; ignorado en CrearLista.</summary>
        public long Id { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public List<GuardarListaItem> Items { get; set; } = new();
    }

    public class GuardarListaItem
    {
        public long ProductoId { get; set; }

        public int Cantidad { get; set; } = 1;
    }
}
