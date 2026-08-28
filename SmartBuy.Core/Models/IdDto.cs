namespace SmartBuy.Core.Models
{
    /// <summary>Resultado de inserts que devuelven solo el id generado (RETURNING id).</summary>
    public class IdDto
    {
        public long Id { get; set; }
    }
}
