namespace SmartBuy.Core.Models.Historico
{
    /// <summary>Fila cruda del repo: mejor precio de un producto de la lista un día.</summary>
    public class InflacionPrecioFila
    {
        public DateOnly Fecha { get; set; }

        public long ProductoId { get; set; }

        public decimal Precio { get; set; }
    }

    /// <summary>La inflación de una canasta (lista guardada) lista para graficar.</summary>
    public class InflacionCanastaResumen
    {
        public long ListaId { get; set; }

        public string Lista { get; set; } = string.Empty;

        /// <summary>Ventana consultada, en días.</summary>
        public int Dias { get; set; }

        public int ProductosEnLista { get; set; }

        /// <summary>Productos de la lista sin ningún precio en la ventana: no suman al total.</summary>
        public List<string> ProductosSinPrecio { get; set; } = new();

        public List<InflacionPunto> Puntos { get; set; } = new();

        public InflacionVariacion Variacion { get; set; } = new();
    }

    /// <summary>El costo de la canasta un día. Solo los días completos son comparables.</summary>
    public class InflacionPunto
    {
        public DateOnly Fecha { get; set; }

        public decimal Total { get; set; }

        public int ProductosConPrecio { get; set; }

        /// <summary>true si TODOS los productos con precio en la ventana tienen precio ese día.</summary>
        public bool Completo { get; set; }
    }

    /// <summary>La variación entre el primer y el último día completo de la ventana.</summary>
    public class InflacionVariacion
    {
        public int DiasCompletos { get; set; }

        public DateOnly? FechaInicial { get; set; }

        public DateOnly? FechaFinal { get; set; }

        public decimal? TotalInicial { get; set; }

        public decimal? TotalFinal { get; set; }

        /// <summary>% entre inicial y final. Positivo = tu canasta se encareció.</summary>
        public decimal? VariacionPorcentaje { get; set; }

        public decimal? VariacionMonto { get; set; }

        public string Mensaje { get; set; } = string.Empty;
    }
}
