namespace SmartBuy.Core.Models.Historico
{
    /// <summary>Fila cruda del repo: mejor precio de un producto en una cadena un día.</summary>
    public class HistoricoPrecioPunto
    {
        public DateOnly Fecha { get; set; }

        public long CadenaId { get; set; }

        public string Cadena { get; set; } = string.Empty;

        public decimal Precio { get; set; }
    }

    /// <summary>La historia de un producto lista para graficar, con su señal de compra.</summary>
    public class HistoricoProducto
    {
        public long ProductoId { get; set; }

        public string Producto { get; set; } = string.Empty;

        /// <summary>Ventana consultada, en días.</summary>
        public int Dias { get; set; }

        public List<HistoricoSerieCadena> Series { get; set; } = new();

        public SenalCompraResultado Senal { get; set; } = new();
    }

    /// <summary>Una línea del gráfico: la serie diaria de una cadena.</summary>
    public class HistoricoSerieCadena
    {
        public long CadenaId { get; set; }

        public string Cadena { get; set; } = string.Empty;

        public List<HistoricoPunto> Puntos { get; set; } = new();
    }

    public class HistoricoPunto
    {
        public DateOnly Fecha { get; set; }

        public decimal Precio { get; set; }
    }

    /// <summary>El veredicto "¿conviene comprar hoy?" calculado sobre la ventana.</summary>
    public class SenalCompraResultado
    {
        /// <summary>sin_datos | minimo | bueno | normal | caro | maximo</summary>
        public string Veredicto { get; set; } = "sin_datos";

        public string Mensaje { get; set; } = string.Empty;

        /// <summary>El mejor precio del último día con datos (entre todas las cadenas).</summary>
        public decimal? PrecioActual { get; set; }

        public decimal? Promedio { get; set; }

        public decimal? Minimo { get; set; }

        public decimal? Maximo { get; set; }

        /// <summary>% vs. el promedio de la ventana. Positivo = hoy más caro.</summary>
        public decimal? VariacionVsPromedio { get; set; }

        public int DiasConDatos { get; set; }
    }
}
