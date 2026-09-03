using Microsoft.Extensions.Options;
using SmartBuy.Core.Common;
using SmartBuy.Core.Interfaces.Repositories;
using SmartBuy.Core.Interfaces.Services;
using SmartBuy.Core.Models.Bots;

namespace SmartBuy.Api.Workers
{
    /// <summary>
    /// Orquestador de capturas de precios: corre dentro del propio monolito
    /// (BackgroundService, sin dependencias externas) y viaja con el contenedor.
    /// En cada tick revisa, por cadena habilitada, si ya existe una captura 'ok'
    /// de la VENTANA vigente (horarios configurados en Bots:HorariosCaptura,
    /// hora argentina); si no, dispara el bot de su plataforma. Si a futuro
    /// hacen falta reintentos sofisticados o dashboard, el reemplazo natural de
    /// esta clase es Hangfire sobre el mismo Postgres.
    /// </summary>
    public class OrquestadorCapturasWorker : BackgroundService
    {
        // Cada cuánto se despierta a revisar. El control de "ya corrí en esta
        // ventana" es contra la tabla captura, no contra este timer: el tick
        // corto solo hace que la corrida arranque cerca del horario programado.
        private static readonly TimeSpan Intervalo = TimeSpan.FromMinutes(15);

        // Una captura en_proceso más vieja que esto es un bot que murió a mitad
        // de camino: el auto-saneo la cierra como error antes de cada revisión.
        private const int HorasMaximasCaptura = 2;

        private readonly ILogger<OrquestadorCapturasWorker> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IOptions<BotsConfiguration> _botsConfiguration;

        public OrquestadorCapturasWorker(
            ILogger<OrquestadorCapturasWorker> logger,
            IServiceScopeFactory scopeFactory,
            IOptions<BotsConfiguration> botsConfiguration)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _botsConfiguration = botsConfiguration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "Orquestador de capturas iniciado. Horarios (hora argentina): {Horarios}; revisión cada {Intervalo}.",
                string.Join(", ", _botsConfiguration.Value.HorariosCaptura), Intervalo);

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

        /// <summary>Auto-saneo: cierra capturas huérfanas de corridas que murieron (crash, kill, corte).</summary>
        private async Task CerrarCapturasAbandonadasAsync(CancellationToken cancellationToken)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var ingestaRepository = scope.ServiceProvider.GetRequiredService<IIngestaRepository>();

                var cierre = await ingestaRepository.CerrarCapturasAbandonadasAsync(HorasMaximasCaptura, cancellationToken);

                if (cierre.Success && cierre.Result is { Cantidad: > 0 })
                    _logger.LogWarning("Auto-saneo: {Cantidad} capturas abandonadas (en_proceso > {Horas}h) cerradas como error.",
                        cierre.Result.Cantidad, HorasMaximasCaptura);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en el auto-saneo de capturas abandonadas.");
            }
        }

        private async Task RevisarCapturasPendientesAsync(CancellationToken cancellationToken)
        {
            await CerrarCapturasAbandonadasAsync(cancellationToken);

            var cadenas = _botsConfiguration.Value.Cadenas.Where(c => c.Habilitado).ToList();

            if (cadenas.Count == 0)
            {
                _logger.LogDebug("Sin bots habilitados en configuración.");
                return;
            }

            var inicioVentana = VentanaCaptura.InicioVentanaActualUtc(
                DateTimeOffset.UtcNow, _botsConfiguration.Value.HorariosCaptura);

            _logger.LogDebug("Ventana de captura vigente desde {InicioArt} (hora argentina).",
                inicioVentana.ToOffset(VentanaCaptura.OffsetArgentina));

            foreach (var config in cadenas)
            {
                // Scope por cadena: servicios scoped frescos y un fallo aislado.
                using var scope = _scopeFactory.CreateScope();

                try
                {
                    var ingestaRepository = scope.ServiceProvider.GetRequiredService<IIngestaRepository>();
                    var capturaVentana = await ingestaRepository.GetCapturaOkDesdeAsync(config.CadenaId, inicioVentana, cancellationToken);

                    if (capturaVentana.Success && capturaVentana.Result is { Id: > 0 })
                        continue; // ya corrió bien en esta ventana

                    var bot = scope.ServiceProvider.GetServices<ICapturaBot>()
                        .FirstOrDefault(b => b.Tipo.Equals(config.Tipo, StringComparison.OrdinalIgnoreCase));

                    if (bot == null)
                    {
                        _logger.LogWarning("No hay bot para la plataforma '{Tipo}' (cadena {Cadena}).", config.Tipo, config.Nombre);
                        continue;
                    }

                    _logger.LogInformation("Disparando bot {Tipo} para {Cadena}.", config.Tipo, config.Nombre);

                    var resultado = await bot.EjecutarAsync(config, cancellationToken);

                    if (resultado.Success && resultado.Result != null)
                        _logger.LogInformation(
                            "Bot {Cadena} ok: captura {CapturaId}, {Items} ítems ({Matcheadas} matcheadas, {Pendientes} pendientes).",
                            config.Nombre, resultado.Result.CapturaId, resultado.Result.ItemsProcesados,
                            resultado.Result.PublicacionesMatcheadas, resultado.Result.PublicacionesPendientes);
                    else
                        _logger.LogError("Bot {Cadena} falló: {Errores}", config.Nombre, string.Join(" | ", resultado.Errors));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Bot de {Cadena}: error no controlado; sigue la próxima cadena.", config.Nombre);
                }
            }
        }
    }
}
