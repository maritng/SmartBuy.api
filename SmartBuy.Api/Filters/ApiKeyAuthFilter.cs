using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SmartBuy.Api.Filters
{
    /// <summary>
    /// Autenticación por API key para los endpoints de ingesta (bots).
    /// Las claves viven en configuración bajo Ingesta:ApiKeys como pares
    /// nombre-de-bot -> clave; en entornos reales se inyectan por variables de
    /// entorno (Ingesta__ApiKeys__nombre), nunca commiteadas.
    /// La comparación es en tiempo constante y la clave recibida jamás se loguea.
    /// </summary>
    public class ApiKeyAuthFilter : IAsyncActionFilter
    {
        public const string HeaderName = "X-Api-Key";
        public const string BotItemKey = "IngestaBot";

        private readonly IConfiguration _configuration;
        private readonly ILogger<ApiKeyAuthFilter> _logger;

        public ApiKeyAuthFilter(IConfiguration configuration, ILogger<ApiKeyAuthFilter> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var http = context.HttpContext;

            if (!http.Request.Headers.TryGetValue(HeaderName, out var apiKey) || string.IsNullOrWhiteSpace(apiKey))
            {
                _logger.LogWarning("Ingesta rechazada: falta {Header} (origen {Ip}).", HeaderName, http.Connection.RemoteIpAddress);
                context.Result = NoAutorizado();
                return;
            }

            var bot = ResolverBot(apiKey.ToString());

            if (bot == null)
            {
                _logger.LogWarning("Ingesta rechazada: API key inválida (origen {Ip}).", http.Connection.RemoteIpAddress);
                context.Result = NoAutorizado();
                return;
            }

            // Deja el actor disponible para logs/auditoría del resto del pipeline.
            http.Items[BotItemKey] = bot;
            _logger.LogInformation("Ingesta autorizada para el bot {Bot}.", bot);

            await next();
        }

        private string? ResolverBot(string apiKey)
        {
            var providedBytes = Encoding.UTF8.GetBytes(apiKey);

            foreach (var entry in _configuration.GetSection("Ingesta:ApiKeys").GetChildren())
            {
                if (string.IsNullOrEmpty(entry.Value))
                    continue;

                var configuredBytes = Encoding.UTF8.GetBytes(entry.Value);

                if (providedBytes.Length == configuredBytes.Length
                    && CryptographicOperations.FixedTimeEquals(providedBytes, configuredBytes))
                {
                    return entry.Key;
                }
            }

            return null;
        }

        private static UnauthorizedObjectResult NoAutorizado()
            => new(new { success = false, errors = new[] { "API key inválida o ausente." } });
    }
}
