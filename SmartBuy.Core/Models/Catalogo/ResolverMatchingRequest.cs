namespace SmartBuy.Core.Models.Catalogo
{
    /// <summary>
    /// Resolución de una publicación pendiente: o se matchea contra un producto
    /// (ProductoId) o se descarta (Descartar = true). Exactamente una de las dos.
    /// </summary>
    public class ResolverMatchingRequest
    {
        public long PublicacionId { get; set; }

        public long? ProductoId { get; set; }

        public bool Descartar { get; set; }
    }
}
