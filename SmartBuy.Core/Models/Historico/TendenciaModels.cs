namespace SmartBuy.Core.Models.Historico
{
    /// <summary>Fila cruda del repo: un eslabón diario del índice de una categoría.</summary>
    public class EslabonCategoriaFila
    {
        public string Categoria { get; set; } = string.Empty;

        public DateOnly Fecha { get; set; }

        /// <summary>Suma de precios de la canasta común en su observación previa.</summary>
        public decimal SumaPrevia { get; set; }

        /// <summary>Suma de precios de la misma canasta hoy.</summary>
        public decimal SumaActual { get; set; }

        /// <summary>Tamaño de la canasta común del eslabón.</summary>
        public int Publicaciones { get; set; }
    }

    /// <summary>Respuesta de GetEvolucionCategorias: un índice base 100 por categoría.</summary>
    public class EvolucionCategorias
    {
        /// <summary>Ventana consultada, en días.</summary>
        public int Dias { get; set; }

        public List<SerieCategoria> Series { get; set; } = new();
    }

    public class SerieCategoria
    {
        public string Categoria { get; set; } = string.Empty;

        /// <summary>El índice encadenado, arrancando de una base virtual de 100.</summary>
        public List<PuntoIndice> Puntos { get; set; } = new();

        /// <summary>% acumulado de la ventana (índice final − 100). Null sin eslabones.</summary>
        public decimal? VariacionVentana { get; set; }

        /// <summary>% del último eslabón (el movimiento más reciente).</summary>
        public decimal? VariacionUltimoDia { get; set; }

        /// <summary>Canasta común del último eslabón: cuántos precios sostienen la lectura.</summary>
        public int PublicacionesUltimoDia { get; set; }

        public string Mensaje { get; set; } = string.Empty;
    }

    public class PuntoIndice
    {
        public DateOnly Fecha { get; set; }

        public decimal Indice { get; set; }

        /// <summary>% de variación de ese día contra el anterior.</summary>
        public decimal VariacionDia { get; set; }

        public int Publicaciones { get; set; }
    }
}
