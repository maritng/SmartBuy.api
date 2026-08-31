namespace SmartBuy.Core.Models.Bots
{
    /// <summary>
    /// Configuración de los bots de captura (sección "Bots" de appsettings).
    /// Agregar una cadena VTEX nueva es agregar un bloque acá: no hay código
    /// nuevo (Carrefour, Día, Jumbo, Disco, Vea y ChangoMás comparten plataforma).
    /// </summary>
    public class BotsConfiguration
    {
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
