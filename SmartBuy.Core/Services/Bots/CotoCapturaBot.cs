using System.Text.Json;
using Microsoft.Extensions.Logging;
using SmartBuy.Core.Common.Responses;
using SmartBuy.Core.Interfaces.Services;
using SmartBuy.Core.Models.Bots;
using SmartBuy.Core.Models.Ingesta;

namespace SmartBuy.Core.Services.Bots
{
    /// <summary>
    /// Bot para Coto (coto.com.ar): el sitio usa Constructor.io como motor de
    /// catálogo, con API JSON pública (ac.cnstrc.com) y clave pública que viaja
    /// en el JS a todo visitante (config ApiKeyPublica). Se navega por categoría
    /// (browse/group_id) paginando, y se entrega TODO a IIngestaServices.
    ///
    /// Particularidad valiosa: Coto publica precio por sucursal (array price[]
    /// por tienda). Para el MVP se usa la SucursalPreferida de la config como
    /// referencia, con product_list_price general como fallback.
    /// </summary>
    public class CotoCapturaBot : ICapturaBot
    {
        private const int TamanioPagina = 50;

        private readonly HttpClient _http;
        private readonly IIngestaServices _ingestaServices;
        private readonly ILogger<CotoCapturaBot> _logger;

        public string Tipo => "coto";

        public CotoCapturaBot(HttpClient http, IIngestaServices ingestaServices, ILogger<CotoCapturaBot> logger)
        {
            _http = http;
            _ingestaServices = ingestaServices;
            _logger = logger;
        }

        public async Task<StandarResponse<IngestaResumen>> EjecutarAsync(BotCadenaConfiguration config, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(config.ApiKeyPublica))
                return new StandarResponse<IngestaResumen>
                {
                    Success = false,
                    Errors = new List<string> { $"El bot de {config.Nombre} requiere ApiKeyPublica en la configuración (la clave pública del sitio)." }
                };

            var items = new Dictionary<string, IngestaItemRequest>();

            foreach (var grupo in config.RutasCategorias)
            {
                for (var pagina = 1; pagina <= config.MaxPaginasPorCategoria; pagina++)
                {
                    var url = $"{config.BaseUrl.TrimEnd('/')}/browse/group_id/{grupo}" +
                              $"?key={config.ApiKeyPublica}&num_results_per_page={TamanioPagina}&page={pagina}";

                    using var respuesta = await _http.GetAsync(url, cancellationToken);

                    if (!respuesta.IsSuccessStatusCode)
                    {
                        _logger.LogWarning("Bot coto {Cadena}: HTTP {Status} en {Grupo} página {Pagina}; se corta la categoría.",
                            config.Nombre, (int)respuesta.StatusCode, grupo, pagina);
                        break;
                    }

                    var cantidadPagina = 0;

                    await using (var stream = await respuesta.Content.ReadAsStreamAsync(cancellationToken))
                    using (var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken))
                    {
                        if (doc.RootElement.TryGetProperty("response", out var response)
                            && response.TryGetProperty("results", out var results)
                            && results.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var resultado in results.EnumerateArray())
                            {
                                cantidadPagina++;
                                MapearResultado(resultado, config.SucursalPreferida, items);
                            }
                        }
                    }

                    if (cantidadPagina < TamanioPagina)
                        break; // última página del grupo

                    await Task.Delay(config.PausaEntreRequestsMs, cancellationToken);
                }
            }

            if (items.Count == 0)
                return new StandarResponse<IngestaResumen>
                {
                    Success = false,
                    Errors = new List<string> { $"El bot de {config.Nombre} no obtuvo ningún producto del sitio." }
                };

            _logger.LogInformation("Bot coto {Cadena}: {Items} publicaciones capturadas; entregando a ingesta.", config.Nombre, items.Count);

            return await _ingestaServices.RegistrarCapturaAsync(new IngestaRequest
            {
                CadenaId = config.CadenaId,
                Fuente = "web",
                Items = items.Values.ToList()
            }, cancellationToken);
        }

        /// <summary>
        /// Un resultado de Constructor trae los campos de producto en data y/o en
        /// las variantes (variations[].data). Se recorren todos los nodos con
        /// sku_id de forma defensiva: la estructura es del sitio, no nuestra.
        /// </summary>
        private static void MapearResultado(JsonElement resultado, string? sucursalPreferida, Dictionary<string, IngestaItemRequest> destino)
        {
            if (!resultado.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Object)
                return;

            var nombreProducto = resultado.TryGetProperty("value", out var v) ? v.GetString() : null;
            var urlRelativa = data.TryGetProperty("url", out var u) ? u.GetString() : null;

            foreach (var nodo in NodosConSku(resultado, data))
            {
                var sku = nodo.TryGetProperty("sku_id", out var s) ? s.GetString() : null;
                if (string.IsNullOrWhiteSpace(sku))
                    continue;

                var nombre = LeerString(nodo, "sku_display_name") ?? LeerString(nodo, "sku_description") ?? nombreProducto;
                if (string.IsNullOrWhiteSpace(nombre))
                    continue;

                var precioLista = PrecioDeSucursal(data, sucursalPreferida) ?? LeerDecimal(nodo, "product_list_price");
                if (precioLista is null or <= 0)
                    continue;

                var precioOferta = LeerDecimal(nodo, "product_sale_price");

                // EANs basura o de longitud inválida no viajan: uno solo haría
                // rechazar la captura completa en la validación de ingesta.
                var ean = LeerEan(nodo);

                destino[sku] = new IngestaItemRequest
                {
                    CodigoExterno = sku,
                    NombrePublicado = nombre.Length > 500 ? nombre[..500] : nombre,
                    EanPublicado = ean,
                    Url = string.IsNullOrWhiteSpace(urlRelativa) ? null : $"https://www.coto.com.ar/{urlRelativa.TrimStart('/')}",
                    PrecioLista = precioLista.Value,
                    PrecioOferta = precioOferta.HasValue && precioOferta < precioLista ? precioOferta : null
                };
            }
        }

        private static IEnumerable<JsonElement> NodosConSku(JsonElement resultado, JsonElement data)
        {
            yield return data;

            if (resultado.TryGetProperty("variations", out var variaciones) && variaciones.ValueKind == JsonValueKind.Array)
            {
                foreach (var variacion in variaciones.EnumerateArray())
                {
                    if (variacion.TryGetProperty("data", out var dataVariacion) && dataVariacion.ValueKind == JsonValueKind.Object)
                        yield return dataVariacion;
                }
            }
        }

        /// <summary>Precio de lista de la sucursal preferida, si está en el array price[] por tienda.</summary>
        private static decimal? PrecioDeSucursal(JsonElement data, string? sucursalPreferida)
        {
            if (string.IsNullOrWhiteSpace(sucursalPreferida))
                return null;

            if (!data.TryGetProperty("price", out var precios) || precios.ValueKind != JsonValueKind.Array)
                return null;

            foreach (var precio in precios.EnumerateArray())
            {
                if (precio.TryGetProperty("store", out var store) && store.GetString() == sucursalPreferida)
                    return LeerDecimal(precio, "listPrice");
            }

            return null;
        }

        private static string? LeerEan(JsonElement nodo)
        {
            if (!nodo.TryGetProperty("product_main_ean", out var ean))
                return null;

            var texto = ean.ValueKind switch
            {
                JsonValueKind.Number => ean.GetInt64().ToString(),
                JsonValueKind.String => ean.GetString(),
                _ => null
            };

            if (texto == null || texto.Length < 8 || texto.Length > 14 || !texto.All(char.IsDigit))
                return null;

            return texto;
        }

        private static string? LeerString(JsonElement elemento, string propiedad)
            => elemento.TryGetProperty(propiedad, out var valor) && valor.ValueKind == JsonValueKind.String ? valor.GetString() : null;

        private static decimal? LeerDecimal(JsonElement elemento, string propiedad)
            => elemento.TryGetProperty(propiedad, out var valor) && valor.ValueKind == JsonValueKind.Number ? valor.GetDecimal() : null;
    }
}
