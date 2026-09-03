using SmartBuy.Core.Common;
using SmartBuy.Core.Common.Responses;
using SmartBuy.Core.Interfaces.Repositories;
using SmartBuy.Core.Interfaces.Services;
using SmartBuy.Core.Models.Bots;
using SmartBuy.Core.Models.Recomendacion;

namespace SmartBuy.Core.Services
{
    /// <summary>
    /// La consulta estrella: resuelve la lista de compras contra los últimos
    /// precios. El repo trae el mejor precio por producto x cadena; acá se
    /// elige el mínimo por producto (reparto óptimo) y se calcula cuánto se
    /// ahorra vs. comprar todo en la mejor cadena única. Lógica de C# puro
    /// sobre la lista del repo: testeable sin base.
    /// </summary>
    public class RecomendacionServices : IRecomendacionServices
    {
        private const int MaxItems = 100;
        private const int MaxCantidad = 999;

        private const int MaxCadenas = 20;

        private readonly IRecomendacionRepository _recomendacionRepository;
        private readonly IProductoRepository _productoRepository;
        private readonly ICadenaRepository _cadenaRepository;
        private readonly BotsConfiguration _botsConfiguration;

        public RecomendacionServices(
            IRecomendacionRepository recomendacionRepository,
            IProductoRepository productoRepository,
            ICadenaRepository cadenaRepository,
            BotsConfiguration botsConfiguration)
        {
            _recomendacionRepository = recomendacionRepository;
            _productoRepository = productoRepository;
            _cadenaRepository = cadenaRepository;
            _botsConfiguration = botsConfiguration;
        }

        public async Task<StandarResponse<ListaCompraResumen>> ResolverListaAsync(ListaCompraRequest request, CancellationToken cancellationToken)
        {
            var errores = Validar(request);
            if (errores.Count > 0)
                return Fallo(errores);

            var cantidades = request.Items.ToDictionary(i => i.ProductoId, i => i.Cantidad);

            // Universo de cadenas accesibles: si viene el filtro, validar que
            // todas existan (una cadena inexistente es error claro, no una
            // respuesta vacía silenciosa).
            var cadenasFiltro = request.CadenasIds is { Count: > 0 } ? request.CadenasIds.Distinct().ToList() : null;

            if (cadenasFiltro != null)
            {
                var cadenas = await _cadenaRepository.GetAllCadenasAsync(cancellationToken);
                if (!cadenas.Success)
                    return new StandarResponse<ListaCompraResumen> { Success = false, Errors = cadenas.Errors };

                var inexistentes = cadenasFiltro.Where(id => cadenas.Result == null || !cadenas.Result.Any(c => c.Id == id)).ToList();
                if (inexistentes.Count > 0)
                    return Fallo(new List<string> { $"Cadenas inexistentes: {string.Join(", ", inexistentes)}." });
            }

            var precios = await _recomendacionRepository.GetPreciosParaListaAsync(cantidades.Keys, cadenasFiltro, cancellationToken);
            if (!precios.Success)
                return new StandarResponse<ListaCompraResumen> { Success = false, Errors = precios.Errors, Execution = precios.Execution };

            var porProducto = (precios.Result ?? new List<PrecioProductoCadena>())
                .GroupBy(p => p.ProductoId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var resumen = new ListaCompraResumen();

            // Reparto óptimo: por producto, la fila más barata (el repo ya ordena
            // por precio_efectivo, el First es el mínimo).
            foreach (var (productoId, opciones) in porProducto)
            {
                var mejor = opciones.First();
                var cantidad = cantidades[productoId];

                var porUnidad = PrecioPorUnidad(mejor.PrecioEfectivo, mejor.ContenidoValor, mejor.ContenidoUnidad);

                resumen.Items.Add(new RecomendacionItem
                {
                    ProductoId = productoId,
                    Producto = mejor.Producto,
                    Cantidad = cantidad,
                    CadenaId = mejor.CadenaId,
                    Cadena = mejor.Cadena,
                    NombrePublicado = mejor.NombrePublicado,
                    CodigoExterno = mejor.CodigoExterno,
                    Url = mejor.Url,
                    PrecioUnitario = mejor.PrecioEfectivo,
                    TipoOferta = mejor.TipoOferta,
                    FechaPrecio = mejor.Fecha,
                    PrecioPorUnidad = porUnidad?.Precio,
                    UnidadBase = porUnidad?.Unidad,
                    Subtotal = mejor.PrecioEfectivo * cantidad,
                    CadenasComparadas = opciones.Count
                });
            }

            // Productos pedidos que no aparecieron: o no tienen precio capturado
            // (van a NoDisponibles con su nombre) o no existen/están de baja (error).
            foreach (var productoId in cantidades.Keys.Where(id => !porProducto.ContainsKey(id)))
            {
                var producto = await _productoRepository.GetProductoByIdAsync(productoId, cancellationToken);
                var detalle = producto.Result?.FirstOrDefault();

                if (detalle == null || detalle.Id <= 0 || !detalle.Activo)
                    return Fallo(new List<string> { $"El producto {productoId} no existe o está dado de baja." });

                resumen.NoDisponibles.Add(new ProductoNoDisponible { ProductoId = productoId, Producto = detalle.Nombre });
            }

            resumen.Totales = CalcularTotales(resumen.Items, porProducto, cantidades);
            resumen.Carritos = ArmarCarritos(resumen.Items);

            return new StandarResponse<ListaCompraResumen> { Success = true, Result = resumen };
        }

        /// <summary>
        /// Un deep link de carrito por cadena del reparto, solo para plataformas
        /// que soportan carga por URL (VTEX). Cadena sin soporte = sin botón.
        /// </summary>
        private List<CarritoCadena> ArmarCarritos(List<RecomendacionItem> items)
        {
            var carritos = new List<CarritoCadena>();

            foreach (var grupo in items.GroupBy(i => new { i.CadenaId, i.Cadena }))
            {
                var config = _botsConfiguration.Cadenas.FirstOrDefault(c => c.CadenaId == grupo.Key.CadenaId);
                if (config == null || !string.Equals(config.Tipo, "vtex", StringComparison.OrdinalIgnoreCase))
                    continue;

                var url = CarritoLinkBuilder.ArmarVtex(
                    config.BaseUrl,
                    grupo.Select(i => (i.CodigoExterno, i.Cantidad)).ToList());

                if (url != null)
                    carritos.Add(new CarritoCadena { CadenaId = grupo.Key.CadenaId, Cadena = grupo.Key.Cadena, Url = url });
            }

            return carritos.OrderBy(c => c.Cadena).ToList();
        }

        private static RecomendacionTotales CalcularTotales(
            List<RecomendacionItem> items,
            Dictionary<long, List<PrecioProductoCadena>> porProducto,
            Dictionary<long, int> cantidades)
        {
            var totales = new RecomendacionTotales
            {
                TotalOptimizado = items.Sum(i => i.Subtotal),
                CadenasInvolucradas = items.Select(i => i.CadenaId).Distinct().Count()
            };

            if (items.Count == 0)
                return totales;

            // Mejor cadena única: entre las cadenas que tienen TODOS los productos
            // disponibles, la de menor total. Es la vara honesta del ahorro:
            // "¿cuánto gano repartiendo vs. ir a un solo súper?".
            var mejorUnica = porProducto.Values
                .SelectMany(opciones => opciones)
                .GroupBy(p => new { p.CadenaId, p.Cadena })
                .Where(g => g.Select(p => p.ProductoId).Distinct().Count() == porProducto.Count)
                .Select(g => new MejorCadenaUnica
                {
                    CadenaId = g.Key.CadenaId,
                    Cadena = g.Key.Cadena,
                    Total = g.Sum(p => p.PrecioEfectivo * cantidades[p.ProductoId])
                })
                .OrderBy(c => c.Total)
                .FirstOrDefault();

            if (mejorUnica != null)
            {
                totales.MejorCadenaUnica = mejorUnica;
                totales.Ahorro = mejorUnica.Total - totales.TotalOptimizado;
            }

            return totales;
        }

        /// <summary>
        /// Normaliza a unidad base ($/L, $/kg, $/un) para comparar presentaciones
        /// distintas a ojo: ml y g se convierten a L y kg.
        /// </summary>
        private static (decimal Precio, string Unidad)? PrecioPorUnidad(decimal precioEfectivo, decimal? valor, string? unidad)
        {
            if (valor is null or <= 0 || string.IsNullOrEmpty(unidad))
                return null;

            (decimal precio, string unidadBase)? resultado = unidad switch
            {
                "L" => (precioEfectivo / valor.Value, "L"),
                "ml" => (precioEfectivo / (valor.Value / 1000m), "L"),
                "kg" => (precioEfectivo / valor.Value, "kg"),
                "g" => (precioEfectivo / (valor.Value / 1000m), "kg"),
                "un" => (precioEfectivo / valor.Value, "un"),
                _ => null
            };

            return resultado == null ? null : (Math.Round(resultado.Value.precio, 2), resultado.Value.unidadBase);
        }

        private static List<string> Validar(ListaCompraRequest? request)
        {
            var errores = new List<string>();

            if (request == null || request.Items == null || request.Items.Count == 0)
                return new List<string> { "La lista debe tener al menos un producto." };

            if (request.Items.Count > MaxItems)
                errores.Add($"La lista supera el máximo de {MaxItems} productos.");

            if (request.Items.Any(i => i.ProductoId <= 0))
                errores.Add("Todos los ítems deben tener productoId mayor a cero.");

            if (request.Items.Any(i => i.Cantidad <= 0 || i.Cantidad > MaxCantidad))
                errores.Add($"cantidad debe estar entre 1 y {MaxCantidad}.");

            var repetidos = request.Items.GroupBy(i => i.ProductoId).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            if (repetidos.Count > 0)
                errores.Add($"Productos repetidos en la lista: {string.Join(", ", repetidos)}.");

            if (request.CadenasIds is { Count: > 0 })
            {
                if (request.CadenasIds.Count > MaxCadenas)
                    errores.Add($"cadenasIds supera el máximo de {MaxCadenas}.");

                if (request.CadenasIds.Any(id => id <= 0))
                    errores.Add("Todos los cadenasIds deben ser mayores a cero.");
            }

            return errores;
        }

        private static StandarResponse<ListaCompraResumen> Fallo(List<string> errores)
            => new() { Success = false, Errors = errores };
    }
}
