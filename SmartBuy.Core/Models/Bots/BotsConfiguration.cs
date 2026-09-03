namespace SmartBuy.Core.Models.Bots
{
    /// <summary>
    /// Configuración de los bots de captura (sección "Bots" de appsettings).
    /// Agregar una cadena VTEX nueva es agregar un bloque acá: no hay código
    /// nuevo (Carrefour, Día, Jumbo, Disco, Vea y ChangoMás comparten plataforma).
    /// </summary>
    public class BotsConfiguration
    {
        /// <summary>
        /// Horas del día (0-23, HORA ARGENTINA) en que abre una ventana de
        /// captura. [7, 19] = a la mañana con las ofertas del día ya publicadas
        /// y una segunda pasada a la noche. Cada cadena captura una vez por
        /// ventana; si la máquina estaba apagada a esa hora, captura al
        /// encenderse (la ventana sigue vigente hasta el próximo horario).
        /// Sin valores rige el default de VentanaCaptura (7). OJO: no poner
        /// default acá — el binder de configuración AGREGA a la lista en vez de
        /// reemplazarla.
        /// </summary>
        public List<int> HorariosCaptura { get; set; } = new();

        public List<BotCadenaConfiguration> Cadenas { get; set; } = new();
    }

    public class BotCadenaConfiguration
    {
        public long CadenaId { get; set; }

        /// <summary>Solo para logs y mensajes.</summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>Plataforma del sitio: hoy solo "vtex" (Coto tendrá la suya).</summary>
        public string Tipo { get; set; } = "vtex";

        public string BaseUrl { get; set; } = string.Empty;

        /// <summary>Rutas de categoría del sitio a capturar (ej. "bebidas", "almacen").</summary>
        public List<string> RutasCategorias { get; set; } = new();

        /// <summary>
        /// Normalización ruta del sitio → etiqueta canónica de categoria_captura
        /// (bebidas/almacen/lacteos/limpieza/perfumeria). Ruta no mapeada = se
        /// guarda la ruta cruda (dato imperfecto > dato perdido). Solo hace
        /// falta mapear las rutas cuyo nombre difiere de la etiqueta.
        /// </summary>
        public Dictionary<string, string>? CategoriaPorRuta { get; set; }

        /// <summary>Tope de páginas (de 50 productos) por categoría: acota el volumen por corrida.</summary>
        public int MaxPaginasPorCategoria { get; set; } = 2;

        /// <summary>Pausa entre requests al sitio: comportamiento educado, ritmo de visitante.</summary>
        public int PausaEntreRequestsMs { get; set; } = 1500;

        /// <summary>Gobierna solo la corrida automática diaria; la ejecución manual lo ignora.</summary>
        public bool Habilitado { get; set; }

        /// <summary>
        /// Clave PÚBLICA que el sitio de la cadena entrega a todo visitante en su
        /// propio JS (ej. Constructor.io en Coto). No es un secreto nuestro; se
        /// versiona sin problema. Solo la usan los bots que la necesiten.
        /// </summary>
        public string? ApiKeyPublica { get; set; }

        /// <summary>
        /// Para cadenas con precio por sucursal (Coto): qué tienda usar como
        /// referencia. Si no está o no aparece, se usa el precio de lista general.
        /// </summary>
        public string? SucursalPreferida { get; set; }
    }
}
