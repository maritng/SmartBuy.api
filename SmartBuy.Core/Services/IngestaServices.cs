using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using SmartBuy.Core.Common;
using SmartBuy.Core.Common.Responses;
using SmartBuy.Core.Interfaces.Repositories;
using SmartBuy.Core.Interfaces.Services;
using SmartBuy.Core.Models.Ingesta;

namespace SmartBuy.Core.Services
{
    /// <summary>
    /// Orquesta una ingesta de precios: valida el payload, crea la captura,
    /// registra cada ítem (upsert publicación + precio, atómico por ítem) y
    /// cierra la captura con su estado. Si un ítem falla, la captura queda en
    /// 'error' con el detalle: la tabla captura es la bitácora de los bots.
    /// </summary>
    public partial class IngestaServices : IIngestaServices
    {
        private const int MaxItems = 20000;
        private static readonly string[] FuentesValidas = { "web", "mail", "api", "manual" };

        [GeneratedRegex(@"^\d{8,14}$")]
        private static partial Regex EanRegex();

        private readonly IIngestaRepository _ingestaRepository;
        private readonly ICadenaRepository _cadenaRepository;
        private readonly ILogger<IngestaServices> _logger;

        public IngestaServices(
            IIngestaRepository ingestaRepository,
            ICadenaRepository cadenaRepository,
            ILogger<IngestaServices> logger)
        {
            _ingestaRepository = ingestaRepository;
            _cadenaRepository = cadenaRepository;
            _logger = logger;
        }

        public async Task<StandarResponse<IngestaResumen>> RegistrarCapturaAsync(IngestaRequest request, CancellationToken cancellationToken)
        {
            var errores = Validar(request);
            if (errores.Count > 0)
                return Fallo(errores);

            var cadenas = await _cadenaRepository.GetAllCadenasAsync(cancellationToken);
            if (!cadenas.Success)
                return Fallo(cadenas.Errors);
            if (cadenas.Result == null || !cadenas.Result.Any(c => c.Id == request.CadenaId))
                return Fallo(new List<string> { $"La cadena {request.CadenaId} no existe." });

            var captura = await _ingestaRepository.CrearCapturaAsync(request.CadenaId, request.Fuente, cancellationToken);
            if (!captura.Success || captura.Result == null)
                return Fallo(captura.Errors.Count > 0 ? captura.Errors : new List<string> { "No se pudo crear la captura." });

            var capturaId = captura.Result.Id;
            int procesados = 0, matcheadas = 0, pendientes = 0;

            foreach (var item in request.Items)
            {
                // Precio efectivo por unidad comprada: min(lista, oferta directa,
                // promo por cantidad computable). Queda persistido con el precio.
                var precioEfectivo = Math.Min(
                    Math.Min(item.PrecioLista, item.PrecioOferta ?? item.PrecioLista),
                    OfertaCalculator.CalcularEfectivo(item.PrecioLista, item.TipoOferta) ?? item.PrecioLista);

                var resultado = await _ingestaRepository.RegistrarItemAsync(capturaId, request.CadenaId, item, precioEfectivo, cancellationToken);

                if (!resultado.Success || resultado.Result == null)
                {
                    // El detalle completo va al log; a la tabla y al bot solo el
                    // ítem que falló, sin arrastrar mensajes internos del motor.
                    _logger.LogError(
                        "Ingesta: falló el ítem con codigo_externo {CodigoExterno} de la captura {CapturaId}. Errores: {Errores}",
                        item.CodigoExterno, capturaId, string.Join(" | ", resultado.Errors));

                    var detalle = $"Falló el ítem con codigo_externo '{item.CodigoExterno}' ({procesados} ítems procesados antes del error).";
                    await _ingestaRepository.FinalizarCapturaAsync(capturaId, "error", procesados, detalle, cancellationToken);

                    return Fallo(new List<string> { detalle, $"La captura {capturaId} quedó registrada en estado 'error'." });
                }

                procesados++;
                if (resultado.Result.EstadoMatching == "pendiente")
                    pendientes++;
                else if (resultado.Result.EstadoMatching != "descartada")
                    matcheadas++;
            }

            await _ingestaRepository.FinalizarCapturaAsync(capturaId, "ok", procesados, null, cancellationToken);

            _logger.LogInformation(
                "Ingesta ok: captura {CapturaId} de cadena {CadenaId} con {Items} ítems ({Matcheadas} matcheadas, {Pendientes} pendientes).",
                capturaId, request.CadenaId, procesados, matcheadas, pendientes);

            return new StandarResponse<IngestaResumen>
            {
                Success = true,
                Result = new IngestaResumen
                {
                    CapturaId = capturaId,
                    ItemsProcesados = procesados,
                    PublicacionesMatcheadas = matcheadas,
                    PublicacionesPendientes = pendientes
                }
            };
        }

        public async Task<StandarResponse<RecalculoOfertasResumen>> RecalcularOfertasAsync(CancellationToken cancellationToken)
        {
            var resumen = new RecalculoOfertasResumen();

            var basePasada = await _ingestaRepository.RecalcularOfertasBaseAsync(cancellationToken);
            if (!basePasada.Success)
                return new StandarResponse<RecalculoOfertasResumen> { Success = false, Errors = basePasada.Errors };

            resumen.FilasBase = basePasada.Result?.Cantidad ?? 0;

            var tipos = await _ingestaRepository.GetTiposOfertaAsync(cancellationToken);
            if (!tipos.Success)
                return new StandarResponse<RecalculoOfertasResumen> { Success = false, Errors = tipos.Errors };

            foreach (var tipo in tipos.Result ?? new List<TipoOfertaDto>())
            {
                if (string.IsNullOrEmpty(tipo.TipoOferta))
                    continue;

                resumen.TiposRevisados++;

                var factor = OfertaCalculator.CalcularFactor(tipo.TipoOferta);
                if (factor == null)
                    continue; // informativo, no computable: queda como está.

                resumen.TiposComputables++;

                var aplicacion = await _ingestaRepository.RecalcularOfertasPorTipoAsync(tipo.TipoOferta, factor.Value, cancellationToken);
                if (aplicacion.Success)
                    resumen.FilasConPromoAplicada += aplicacion.Result?.Cantidad ?? 0;
            }

            _logger.LogInformation(
                "RecalcularOfertas: {Base} filas base, {Tipos} descriptores ({Computables} computables), {Aplicadas} filas con promo aplicada.",
                resumen.FilasBase, resumen.TiposRevisados, resumen.TiposComputables, resumen.FilasConPromoAplicada);

            return new StandarResponse<RecalculoOfertasResumen> { Success = true, Result = resumen };
        }

        public Task<StandarResponse<List<CapturaListado>>> GetCapturasAsync(int limite, CancellationToken cancellationToken)
            => _ingestaRepository.GetCapturasAsync(Math.Clamp(limite, 1, 200), cancellationToken);

        /// <summary>
        /// Validación server-side completa: el payload viene de bots (o de quien
        /// tenga la API key), nunca se confía en el emisor. Si algo no cierra se
        /// rechaza la ingesta entera: el bot corrige y reenvía, y no quedan
        /// capturas a medias por datos malformados.
        /// </summary>
        private static List<string> Validar(IngestaRequest? request)
        {
            var errores = new List<string>();

            if (request == null)
                return new List<string> { "El cuerpo de la ingesta es obligatorio." };

            if (request.CadenaId <= 0)
                errores.Add("cadenaId es obligatorio y debe ser mayor a cero.");

            if (!FuentesValidas.Contains(request.Fuente))
                errores.Add($"fuente debe ser una de: {string.Join(", ", FuentesValidas)}.");

            if (request.Items == null || request.Items.Count == 0)
            {
                errores.Add("items no puede estar vacío.");
                return errores;
            }

            if (request.Items.Count > MaxItems)
            {
                errores.Add($"items supera el máximo de {MaxItems} por captura; partir el envío.");
                return errores;
            }

            for (var i = 0; i < request.Items.Count; i++)
            {
                var item = request.Items[i];
                var prefijo = $"items[{i}]";

                if (string.IsNullOrWhiteSpace(item.CodigoExterno) || item.CodigoExterno.Length > 128)
                    errores.Add($"{prefijo}.codigoExterno es obligatorio (máximo 128 caracteres).");

                if (string.IsNullOrWhiteSpace(item.NombrePublicado) || item.NombrePublicado.Length > 500)
                    errores.Add($"{prefijo}.nombrePublicado es obligatorio (máximo 500 caracteres).");

                if (item.EanPublicado != null && !EanRegex().IsMatch(item.EanPublicado))
                    errores.Add($"{prefijo}.eanPublicado debe tener entre 8 y 14 dígitos.");

                if (item.Url != null && item.Url.Length > 1000)
                    errores.Add($"{prefijo}.url supera los 1000 caracteres.");

                if (item.TipoOferta != null && item.TipoOferta.Length > 200)
                    errores.Add($"{prefijo}.tipoOferta supera los 200 caracteres.");

                if (item.PrecioLista < 0)
                    errores.Add($"{prefijo}.precioLista no puede ser negativo.");

                if (item.PrecioOferta is < 0)
                    errores.Add($"{prefijo}.precioOferta no puede ser negativo.");

                // Corta temprano para no devolver miles de errores en payloads rotos.
                if (errores.Count >= 50)
                {
                    errores.Add("Se detectaron más errores; se listan solo los primeros 50.");
                    break;
                }
            }

            return errores;
        }

        private static StandarResponse<IngestaResumen> Fallo(List<string> errores)
            => new() { Success = false, Errors = errores };
    }
}
