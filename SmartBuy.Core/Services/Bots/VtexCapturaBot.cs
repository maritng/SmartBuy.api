using System.Text.Json;
using Microsoft.Extensions.Logging;
using SmartBuy.Core.Common.Responses;
using SmartBuy.Core.Interfaces.Services;
using SmartBuy.Core.Models.Bots;
using SmartBuy.Core.Models.Ingesta;

namespace SmartBuy.Core.Services.Bots
{
    /// <summary>
    /// Bot para cadenas sobre plataforma VTEX (Carrefour, Día, Jumbo, Disco,
    /// Vea, ChangoMás): consume la API pública de catálogo que el propio sitio
    /// usa (/api/catalog_system/pub/products/search) paginando de a 50, con
    /// pausa entre requests. Mapea al contrato de ingesta y entrega TODO a
    /// IIngestaServices: mismas validaciones y auditoría que un bot externo.
    ///
    /// Nota de parseo: el JSON de VTEX trae claves duplicadas que difieren solo
    /// en mayúsculas (specifications) — JsonDocument las tolera; no usar
    /// deserializadores estrictos con este payload.
    /// </summary>
    public class VtexCapturaBot : ICapturaBot
    {
        private const int TamanioPagina = 50;

        private readonly HttpClient _http;
        private readonly IIngestaServices _ingestaServices;
        private readonly ILogger<VtexCapturaBot> _logger;

        public string Tipo => "vtex";

        public VtexCapturaBot(HttpClient http, IIngestaServices ingestaServices, ILogger<VtexCapturaBot> logger)
        {
            _http = http;
            _ingestaServices = ingestaServices;
            _logger = logger;
        }

        public async Task<StandarResponse<IngestaResumen>> EjecutarAsync(BotCadenaConfiguration config, CancellationToken cancellationToken)
        {
            // Dedupe por SKU: el mismo producto puede aparecer en más de una
            // categoría o página; una captura lleva cada publicación una vez.
            var items = new Dictionary<string, IngestaItemRequest>();

            foreach (var ruta in config.RutasCategorias)
            {
                for (var pagina = 0; pagina < config.MaxPaginasPorCategoria; pagina++)
                {
                    var desde = pagina * TamanioPagina;
                    var hasta = desde + TamanioPagina - 1;
                    var url = $"{config.BaseUrl.TrimEnd('/')}/api/catalog_system/pub/products/search/{ruta}?map=c&_from={desde}&_to={hasta}";

                    using var respuesta = await _http.GetAsync(url, cancellationToken);

                    if (!respuesta.IsSuccessStatusCode)
                    {
                        // 429/5xx: se corta la categoría y se entrega lo capturado
                        // hasta acá en vez de perder toda la corrida.
                        _logger.LogWarning("Bot vtex {Cadena}: HTTP {Status} en {Ruta} página {Pagina}; se corta la categoría.",
                            config.Nombre, (int)respuesta.StatusCode, ruta, pagina);
                        break;
                    }

                    var cantidadPagina = 0;

                    await using (var stream = await respuesta.Content.ReadAsStreamAsync(cancellationToken))
                    using (var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken))
                    {
                        foreach (var producto in doc.RootElement.EnumerateArray())
                        {
                            cantidadPagina++;
                            MapearProducto(producto, items, config.BaseUrl.TrimEnd('/'));
                        }
                    }

                    if (cantidadPagina < TamanioPagina)
                        break; // última página de la categoría

                    await Task.Delay(config.PausaEntreRequestsMs, cancellationToken);
                }
            }

            if (items.Count == 0)
                return new StandarResponse<IngestaResumen>
                {
                    Success = false,
                    Errors = new List<string> { $"El bot de {config.Nombre} no obtuvo ningún producto del sitio." }
                };

            _logger.LogInformation("Bot vtex {Cadena}: {Items} publicaciones capturadas; entregando a ingesta.", config.Nombre, items.Count);

            return await _ingestaServices.RegistrarCapturaAsync(new IngestaRequest
            {
                CadenaId = config.CadenaId,
                Fuente = "web",
                Items = items.Values.ToList()
            }, cancellationToken);
        }

        /// <summary>
        /// Un "product" VTEX tiene variantes (items), cada una con su SKU, EAN y
        /// precios propios. Se toman las disponibles con precio de lista válido.
        /// </summary>
        private static void MapearProducto(JsonElement producto, Dictionary<string, IngestaItemRequest> destino, string baseUrl)
        {
            if (!producto.TryGetProperty("items", out var variantes) || variantes.ValueKind != JsonValueKind.Array)
                return;

            var nombreProducto = producto.TryGetProperty("productName", out var pn) ? pn.GetString() : null;

            // linkText es el slug de la página del producto: {sitio}/{linkText}/p.
            // Alimenta los deep links del resultado; la ingesta upsertea, así que
            // las publicaciones viejas se completan solas en la próxima corrida.
            var linkText = producto.TryGetProperty("linkText", out var lt) ? lt.GetString() : null;
            var urlProducto = string.IsNullOrWhiteSpace(linkText) ? null : $"{baseUrl}/{linkText}/p";

            foreach (var variante in variantes.EnumerateArray())
            {
                var sku = variante.TryGetProperty("itemId", out var it) ? it.GetString() : null;
                if (string.IsNullOrWhiteSpace(sku))
                    continue;

                if (!variante.TryGetProperty("sellers", out var sellers) || sellers.GetArrayLength() == 0)
                    continue;

                var oferta = sellers[0].TryGetProperty("commertialOffer", out var of) ? of : default;
                if (oferta.ValueKind != JsonValueKind.Object)
                    continue;

                var disponible = oferta.TryGetProperty("IsAvailable", out var av) && av.GetBoolean();
                var precioLista = LeerDecimal(oferta, "ListPrice");
                var precio = LeerDecimal(oferta, "Price");

                if (!disponible || precioLista is null or <= 0)
                    continue;

                var nombre = variante.TryGetProperty("nameComplete", out var nc) ? nc.GetString() : null;
                nombre = string.IsNullOrWhiteSpace(nombre) ? nombreProducto : nombre;
                if (string.IsNullOrWhiteSpace(nombre))
                    continue;

                // EANs basura ("sin ean", vacíos, longitud inválida) no viajan:
                // un solo EAN inválido haría rechazar la captura completa en la
                // validación de ingesta.
                var ean = variante.TryGetProperty("ean", out var e) ? e.GetString() : null;
                if (ean != null && (!ean.All(char.IsDigit) || ean.Length < 8 || ean.Length > 14))
                    ean = null;

                destino[sku] = new IngestaItemRequest
                {
                    CodigoExterno = sku,
                    NombrePublicado = nombre.Length > 500 ? nombre[..500] : nombre,
                    EanPublicado = string.IsNullOrWhiteSpace(ean) ? null : ean,
                    Url = urlProducto,
                    PrecioLista = precioLista.Value,
                    PrecioOferta = precio.HasValue && precio < precioLista ? precio : null,
                    TipoOferta = LeerTeasers(oferta)
                };
            }
        }

        private static decimal? LeerDecimal(JsonElement elemento, string propiedad)
        {
            if (elemento.TryGetProperty(propiedad, out var valor) && valor.ValueKind == JsonValueKind.Number)
                return valor.GetDecimal();

            return null;
        }

        private static string? LeerString(JsonElement elemento, string propiedad)
            => elemento.TryGetProperty(propiedad, out var valor) && valor.ValueKind == JsonValueKind.String ? valor.GetString() : null;

        /// <summary>
        /// Descriptores de promo del commertialOffer: PromotionTeasers trae Name
        /// limpio; Teasers usa claves serializadas raras ("&lt;Name&gt;k__BackingField").
        /// Se capturan como texto crudo informativo; OfertaCalculator decide qué
        /// es computable.
        /// </summary>
        private static string? LeerTeasers(JsonElement oferta)
        {
            var nombres = new List<string>();

            if (oferta.TryGetProperty("PromotionTeasers", out var promos) && promos.ValueKind == JsonValueKind.Array)
            {
                foreach (var teaser in promos.EnumerateArray())
                {
                    var nombre = LeerString(teaser, "Name");
                    if (!string.IsNullOrWhiteSpace(nombre))
                        nombres.Add(nombre);
                }
            }

            if (nombres.Count == 0 && oferta.TryGetProperty("Teasers", out var teasers) && teasers.ValueKind == JsonValueKind.Array)
            {
                foreach (var teaser in teasers.EnumerateArray())
                {
                    var nombre = LeerString(teaser, "<Name>k__BackingField");
                    if (!string.IsNullOrWhiteSpace(nombre))
                        nombres.Add(nombre);
                }
            }

            if (nombres.Count == 0)
                return null;

            var texto = string.Join(" | ", nombres.Distinct());
            return texto.Length > 200 ? texto[..200] : texto;
        }
    }
}
