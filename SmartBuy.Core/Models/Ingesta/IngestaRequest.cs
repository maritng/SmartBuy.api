namespace SmartBuy.Core.Models.Ingesta
{
    /// <summary>
    /// Payload que envían los bots de captura. El bot no conoce el esquema de la
    /// base: manda datos crudos y el backend resuelve publicaciones, matching y
    /// precios.
    /// </summary>
    public class IngestaRequest
    {
        public long CadenaId { get; set; }

        /// <summary>Canal de captura: web | mail | api | manual.</summary>
        public string Fuente { get; set; } = string.Empty;

        public List<IngestaItemRequest> Items { get; set; } = new();
    }
}
