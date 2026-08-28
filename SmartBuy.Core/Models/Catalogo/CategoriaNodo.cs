namespace SmartBuy.Core.Models.Catalogo
{
    /// <summary>Categoría aplanada con el nombre de su padre; el FE arma el árbol con PadreId.</summary>
    public class CategoriaNodo
    {
        public long Id { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public long? PadreId { get; set; }

        public string? Padre { get; set; }
    }
}
