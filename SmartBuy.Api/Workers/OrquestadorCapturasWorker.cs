namespace SmartBuy.Api.Workers
{
    /// <summary>
    /// Orquestador de capturas de precios: corre dentro del propio monolito
    /// (BackgroundService, sin dependencias externas) y viaja con el contenedor.
    /// Se despierta periódicamente y dispara las capturas diarias pendientes por
    /// cadena. Si a futuro hacen falta reintentos sofisticados o dashboard, el
    /// reemplazo natural de esta clase es Hangfire sobre el mismo Postgres.
    /// </summary>
    public class OrquestadorCapturasWorker : BackgroundService
    {
        // Cada cuánto se despierta a revisar si hay capturas pendientes. El control
        // de "ya corrí hoy" es contra la tabla captura, no contra este timer.
        private static readonly TimeSpan Intervalo = TimeSpan.FromHours(1);

        private readonly ILogger<OrquestadorCapturasWorker> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public OrquestadorCapturasWorker(ILogger<OrquestadorCapturasWorker> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Orquestador de capturas iniciado (intervalo de revisión: {Intervalo}).", Intervalo);

            using var timer = new PeriodicTimer(Intervalo);

            do
            {
                try
                {
                    await RevisarCapturasPendientesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    // Un fallo de una revisión nunca debe tirar el worker: se loguea
                    // y se reintenta en el próximo tick.
                    _logger.LogError(ex, "Error revisando capturas pendientes.");
                }
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }

        private Task RevisarCapturasPendientesAsync(CancellationToken cancellationToken)
        {
            // TODO (bots de captura): resolver por cadena si ya existe una captura
            // 'ok' del día en la tabla captura y, si no, disparar el bot que
            // corresponda vía un ICapturaService del Core. Los bots llegan con la
            // etapa 2 de la hoja de ruta; por ahora el worker solo deja constancia
            // de que está vivo.
            _logger.LogDebug("Sin bots de captura configurados todavía.");
            return Task.CompletedTask;
        }
    }
}
