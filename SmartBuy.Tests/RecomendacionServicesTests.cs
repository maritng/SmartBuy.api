using SmartBuy.Core.Common.Responses;
using SmartBuy.Core.Interfaces.Repositories;
using SmartBuy.Core.Models;
using SmartBuy.Core.Models.Bots;
using SmartBuy.Core.Models.Catalogo;
using SmartBuy.Core.Models.Recomendacion;
using SmartBuy.Core.Services;
using Xunit;

namespace SmartBuy.Tests
{
    /// <summary>
    /// La lógica del reparto óptimo es C# puro sobre la lista que devuelve el
    /// repo, así que se testea con fakes en memoria (sin base, sin mocks).
    /// Contrato del repo que los fakes respetan: las opciones de cada producto
    /// vienen ordenadas por precio efectivo ascendente.
    /// </summary>
    public class RecomendacionServicesTests
    {
        private const long Carrefour = 2;
        private const long Coto = 3;
        private const long Dia = 4;

        // ---- Fakes en memoria ----

        private sealed class FakeRecomendacionRepository : IRecomendacionRepository
        {
            public List<PrecioProductoCadena> Precios { get; init; } = new();
            public IReadOnlyCollection<long>? UltimoFiltroCadenas { get; private set; }

            public Task<StandarResponse<List<PrecioProductoCadena>>> GetPreciosParaListaAsync(
                IEnumerable<long> productoIds, IReadOnlyCollection<long>? cadenasIds, CancellationToken cancellationToken)
            {
                UltimoFiltroCadenas = cadenasIds;

                var ids = productoIds.ToHashSet();
                var filas = Precios
                    .Where(p => ids.Contains(p.ProductoId))
                    .Where(p => cadenasIds == null || cadenasIds.Contains(p.CadenaId))
                    .OrderBy(p => p.ProductoId).ThenBy(p => p.PrecioEfectivo)
                    .ToList();

                return Task.FromResult(new StandarResponse<List<PrecioProductoCadena>> { Success = true, Result = filas });
            }
        }

        private sealed class FakeProductoRepository : IProductoRepository
        {
            public Dictionary<long, ProductoDetalle> Productos { get; init; } = new();

            public Task<StandarResponse<List<ProductoDetalle>>> GetProductoByIdAsync(long id, CancellationToken ct)
            {
                var lista = Productos.TryGetValue(id, out var detalle)
                    ? new List<ProductoDetalle> { detalle }
                    : new List<ProductoDetalle>();

                return Task.FromResult(new StandarResponse<List<ProductoDetalle>> { Success = true, Result = lista });
            }

            // El servicio de recomendación no usa el resto del contrato.
            public Task<StandarResponse<List<ProductoListado>>> GetAllProductosAsync(string? filtro, int limit, int offset, CancellationToken ct) => throw new NotImplementedException();
            public Task<StandarResponse<IdDto>> CrearProductoAsync(GuardarProductoRequest producto, CancellationToken ct) => throw new NotImplementedException();
            public Task<StandarResponse<IdDto>> ActualizarProductoAsync(GuardarProductoRequest producto, CancellationToken ct) => throw new NotImplementedException();
            public Task<StandarResponse<IdDto>> EliminarProductoAsync(long id, CancellationToken ct) => throw new NotImplementedException();
            public Task<StandarResponse<CantidadDto>> GenerarDesdePendientesAsync(int minCadenas, CancellationToken ct) => throw new NotImplementedException();
            public Task<StandarResponse<List<Marca>>> GetAllMarcasAsync(CancellationToken ct) => throw new NotImplementedException();
            public Task<StandarResponse<IdDto>> CrearMarcaAsync(string nombre, CancellationToken ct) => throw new NotImplementedException();
            public Task<StandarResponse<List<CategoriaNodo>>> GetAllCategoriasAsync(CancellationToken ct) => throw new NotImplementedException();
            public Task<StandarResponse<List<ProductoSinContenido>>> GetProductosSinContenidoAsync(CancellationToken ct) => throw new NotImplementedException();
            public Task<StandarResponse<List<Core.Models.Historico.HistoricoPrecioPunto>>> GetHistoricoProductoAsync(long productoId, int dias, CancellationToken ct) => throw new NotImplementedException();
            public Task<StandarResponse<IdDto>> ActualizarContenidoAsync(long id, decimal valor, string unidad, CancellationToken ct) => throw new NotImplementedException();
        }

        private sealed class FakeCadenaRepository : ICadenaRepository
        {
            public List<Cadena> Cadenas { get; init; } = new()
            {
                new Cadena { Id = Carrefour, Nombre = "Carrefour" },
                new Cadena { Id = Coto, Nombre = "Coto" },
                new Cadena { Id = Dia, Nombre = "Día" }
            };

            public Task<StandarResponse<List<Cadena>>> GetAllCadenasAsync(CancellationToken ct)
                => Task.FromResult(new StandarResponse<List<Cadena>> { Success = true, Result = Cadenas });
        }

        private static PrecioProductoCadena Precio(
            long productoId, string producto, long cadenaId, string cadena, decimal efectivo,
            decimal? contenidoValor = null, string? contenidoUnidad = null)
            => new()
            {
                ProductoId = productoId,
                Producto = producto,
                CadenaId = cadenaId,
                Cadena = cadena,
                NombrePublicado = producto,
                CodigoExterno = $"SKU-{cadenaId}-{productoId}",
                Fecha = DateOnly.FromDateTime(DateTime.Today),
                PrecioLista = efectivo,
                PrecioEfectivo = efectivo,
                ContenidoValor = contenidoValor,
                ContenidoUnidad = contenidoUnidad
            };

        /// <summary>Config de bots como la real: Carrefour y Día vtex, Coto tipo propio.</summary>
        private static BotsConfiguration ConfigBots() => new()
        {
            Cadenas =
            {
                new BotCadenaConfiguration { CadenaId = Carrefour, Nombre = "Carrefour", Tipo = "vtex", BaseUrl = "https://www.carrefour.com.ar" },
                new BotCadenaConfiguration { CadenaId = Coto, Nombre = "Coto", Tipo = "coto", BaseUrl = "https://ac.cnstrc.com" },
                new BotCadenaConfiguration { CadenaId = Dia, Nombre = "Día", Tipo = "vtex", BaseUrl = "https://diaonline.supermercadosdia.com.ar" }
            }
        };

        private static RecomendacionServices Armar(FakeRecomendacionRepository repo, FakeProductoRepository? productos = null)
            => new(repo, productos ?? new FakeProductoRepository(), new FakeCadenaRepository(), ConfigBots());

        private static ListaCompraRequest Pedido(params (long productoId, int cantidad)[] items)
            => new() { Items = items.Select(i => new ListaCompraItem { ProductoId = i.productoId, Cantidad = i.cantidad }).ToList() };

        // ---- Reparto óptimo ----

        [Fact]
        public async Task Cada_producto_va_a_su_cadena_mas_barata()
        {
            var repo = new FakeRecomendacionRepository
            {
                Precios =
                {
                    Precio(1, "Coca-Cola", Carrefour, "Carrefour", 5000m),
                    Precio(1, "Coca-Cola", Dia, "Día", 3835m),
                    Precio(2, "Yerba", Carrefour, "Carrefour", 2000m),
                    Precio(2, "Yerba", Dia, "Día", 2600m)
                }
            };

            var resultado = await Armar(repo).ResolverListaAsync(Pedido((1, 1), (2, 1)), CancellationToken.None);

            Assert.True(resultado.Success);
            var items = resultado.Result!.Items;
            Assert.Equal(Dia, items.Single(i => i.ProductoId == 1).CadenaId);
            Assert.Equal(Carrefour, items.Single(i => i.ProductoId == 2).CadenaId);
            Assert.Equal(3835m + 2000m, resultado.Result.Totales.TotalOptimizado);
            Assert.Equal(2, resultado.Result.Totales.CadenasInvolucradas);
        }

        [Fact]
        public async Task Las_cantidades_multiplican_subtotales_y_totales()
        {
            var repo = new FakeRecomendacionRepository
            {
                Precios = { Precio(1, "Coca-Cola", Dia, "Día", 1000m) }
            };

            var resultado = await Armar(repo).ResolverListaAsync(Pedido((1, 3)), CancellationToken.None);

            Assert.True(resultado.Success);
            Assert.Equal(3000m, resultado.Result!.Items.Single().Subtotal);
            Assert.Equal(3000m, resultado.Result.Totales.TotalOptimizado);
        }

        // ---- Mejor cadena única y ahorro ----

        [Fact]
        public async Task La_mejor_cadena_unica_solo_compite_si_tiene_todos_los_productos()
        {
            var repo = new FakeRecomendacionRepository
            {
                Precios =
                {
                    // Coto es baratísimo en el producto 1 pero no tiene el 2:
                    // no puede ser la "mejor cadena única".
                    Precio(1, "Coca-Cola", Coto, "Coto", 100m),
                    Precio(1, "Coca-Cola", Carrefour, "Carrefour", 5000m),
                    Precio(1, "Coca-Cola", Dia, "Día", 4000m),
                    Precio(2, "Yerba", Carrefour, "Carrefour", 2000m),
                    Precio(2, "Yerba", Dia, "Día", 2500m)
                }
            };

            var resultado = await Armar(repo).ResolverListaAsync(Pedido((1, 1), (2, 1)), CancellationToken.None);

            Assert.True(resultado.Success);
            var totales = resultado.Result!.Totales;

            // Entre las completas: Carrefour 7000 vs Día 6500 -> Día.
            Assert.NotNull(totales.MejorCadenaUnica);
            Assert.Equal(Dia, totales.MejorCadenaUnica!.CadenaId);
            Assert.Equal(6500m, totales.MejorCadenaUnica.Total);

            // Reparto óptimo: 100 (Coto) + 2000 (Carrefour) = 2100. Ahorro: 6500 - 2100.
            Assert.Equal(2100m, totales.TotalOptimizado);
            Assert.Equal(4400m, totales.Ahorro);
        }

        [Fact]
        public async Task Sin_cadena_que_tenga_todo_no_hay_mejor_unica_ni_ahorro()
        {
            var repo = new FakeRecomendacionRepository
            {
                Precios =
                {
                    Precio(1, "Coca-Cola", Coto, "Coto", 1000m),
                    Precio(2, "Yerba", Dia, "Día", 2000m)
                }
            };

            var resultado = await Armar(repo).ResolverListaAsync(Pedido((1, 1), (2, 1)), CancellationToken.None);

            Assert.True(resultado.Success);
            Assert.Null(resultado.Result!.Totales.MejorCadenaUnica);
            Assert.Null(resultado.Result.Totales.Ahorro);
        }

        // ---- Precio por unidad ----

        [Fact]
        public async Task El_precio_por_unidad_convierte_ml_a_litro_y_g_a_kilo()
        {
            var repo = new FakeRecomendacionRepository
            {
                Precios =
                {
                    Precio(1, "Cerveza 473cc", Dia, "Día", 1000m, contenidoValor: 500m, contenidoUnidad: "ml"),
                    Precio(2, "Queso 300g", Dia, "Día", 3000m, contenidoValor: 300m, contenidoUnidad: "g")
                }
            };

            var resultado = await Armar(repo).ResolverListaAsync(Pedido((1, 1), (2, 1)), CancellationToken.None);

            Assert.True(resultado.Success);
            var cerveza = resultado.Result!.Items.Single(i => i.ProductoId == 1);
            var queso = resultado.Result.Items.Single(i => i.ProductoId == 2);

            Assert.Equal(2000m, cerveza.PrecioPorUnidad); // 1000 / 0.5 L
            Assert.Equal("L", cerveza.UnidadBase);
            Assert.Equal(10000m, queso.PrecioPorUnidad);  // 3000 / 0.3 kg
            Assert.Equal("kg", queso.UnidadBase);
        }

        [Fact]
        public async Task Caso_real_coto_coca_de_2_25_litros()
        {
            // Validación de oro de la etapa 2a: $4845 la botella de 2,25 L
            // da $2153.33 el litro (el mismo valor que muestra el sitio de Coto).
            var repo = new FakeRecomendacionRepository
            {
                Precios = { Precio(1, "Coca-Cola 2,25L", Coto, "Coto", 4845m, contenidoValor: 2.25m, contenidoUnidad: "L") }
            };

            var resultado = await Armar(repo).ResolverListaAsync(Pedido((1, 1)), CancellationToken.None);

            Assert.Equal(2153.33m, resultado.Result!.Items.Single().PrecioPorUnidad);
        }

        [Fact]
        public async Task Sin_contenido_no_hay_precio_por_unidad()
        {
            var repo = new FakeRecomendacionRepository
            {
                Precios = { Precio(1, "Coca-Cola", Dia, "Día", 1000m) }
            };

            var resultado = await Armar(repo).ResolverListaAsync(Pedido((1, 1)), CancellationToken.None);

            Assert.Null(resultado.Result!.Items.Single().PrecioPorUnidad);
            Assert.Null(resultado.Result.Items.Single().UnidadBase);
        }

        // ---- No disponibles y errores ----

        [Fact]
        public async Task Producto_activo_sin_precio_capturado_cae_en_no_disponibles()
        {
            var repo = new FakeRecomendacionRepository
            {
                Precios = { Precio(1, "Coca-Cola", Dia, "Día", 1000m) }
            };
            var productos = new FakeProductoRepository
            {
                Productos = { [99] = new ProductoDetalle { Id = 99, Nombre = "Yerba nueva", Activo = true } }
            };

            var resultado = await Armar(repo, productos).ResolverListaAsync(Pedido((1, 1), (99, 1)), CancellationToken.None);

            Assert.True(resultado.Success);
            var noDisponible = Assert.Single(resultado.Result!.NoDisponibles);
            Assert.Equal(99, noDisponible.ProductoId);
            Assert.Equal("Yerba nueva", noDisponible.Producto);
        }

        [Fact]
        public async Task Producto_inexistente_o_de_baja_es_error()
        {
            var repo = new FakeRecomendacionRepository();

            var resultado = await Armar(repo).ResolverListaAsync(Pedido((12345, 1)), CancellationToken.None);

            Assert.False(resultado.Success);
            Assert.Contains(resultado.Errors, e => e.Contains("12345"));
        }

        [Fact]
        public async Task Filtro_de_cadenas_inexistentes_es_error_claro()
        {
            var repo = new FakeRecomendacionRepository
            {
                Precios = { Precio(1, "Coca-Cola", Dia, "Día", 1000m) }
            };
            var request = Pedido((1, 1));
            request.CadenasIds = new List<long> { Dia, 999 };

            var resultado = await Armar(repo).ResolverListaAsync(request, CancellationToken.None);

            Assert.False(resultado.Success);
            Assert.Contains(resultado.Errors, e => e.Contains("999"));
        }

        [Fact]
        public async Task El_filtro_de_cadenas_llega_al_repositorio()
        {
            var repo = new FakeRecomendacionRepository
            {
                Precios =
                {
                    Precio(1, "Coca-Cola", Coto, "Coto", 100m),
                    Precio(1, "Coca-Cola", Dia, "Día", 1000m)
                }
            };
            var request = Pedido((1, 1));
            request.CadenasIds = new List<long> { Dia };

            var resultado = await Armar(repo).ResolverListaAsync(request, CancellationToken.None);

            Assert.True(resultado.Success);
            Assert.Equal(new[] { Dia }, repo.UltimoFiltroCadenas!);
            // Con Coto excluido, gana Día aunque sea más caro.
            Assert.Equal(Dia, resultado.Result!.Items.Single().CadenaId);
        }

        // ---- Deep links de carrito ----

        [Fact]
        public async Task Arma_un_carrito_vtex_por_cadena_del_reparto_con_skus_y_cantidades()
        {
            var repo = new FakeRecomendacionRepository
            {
                Precios =
                {
                    Precio(1, "Coca-Cola", Dia, "Día", 1000m),
                    Precio(2, "Yerba", Dia, "Día", 2000m),
                    Precio(3, "Fideos", Carrefour, "Carrefour", 500m)
                }
            };

            var resultado = await Armar(repo).ResolverListaAsync(Pedido((1, 2), (2, 1), (3, 1)), CancellationToken.None);

            Assert.True(resultado.Success);
            var carritos = resultado.Result!.Carritos;
            Assert.Equal(2, carritos.Count);

            var dia = carritos.Single(c => c.CadenaId == Dia);
            Assert.StartsWith("https://diaonline.supermercadosdia.com.ar/checkout/cart/add?", dia.Url);
            Assert.Contains($"sku=SKU-{Dia}-1&qty=2&seller=1", dia.Url);
            Assert.Contains($"sku=SKU-{Dia}-2&qty=1&seller=1", dia.Url);

            var carrefour = carritos.Single(c => c.CadenaId == Carrefour);
            Assert.Contains($"sku=SKU-{Carrefour}-3&qty=1&seller=1", carrefour.Url);
        }

        [Fact]
        public async Task Una_cadena_sin_soporte_de_carrito_no_genera_boton()
        {
            var repo = new FakeRecomendacionRepository
            {
                Precios = { Precio(1, "Coca-Cola", Coto, "Coto", 1000m) }
            };

            var resultado = await Armar(repo).ResolverListaAsync(Pedido((1, 1)), CancellationToken.None);

            Assert.True(resultado.Success);
            Assert.Empty(resultado.Result!.Carritos);
            // Pero el ítem viaja igual, con su SKU informativo.
            Assert.Equal($"SKU-{Coto}-1", resultado.Result.Items.Single().CodigoExterno);
        }

        // ---- Validaciones del request ----

        [Fact]
        public async Task Lista_vacia_es_error()
        {
            var resultado = await Armar(new FakeRecomendacionRepository())
                .ResolverListaAsync(new ListaCompraRequest(), CancellationToken.None);

            Assert.False(resultado.Success);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(1000)]
        public async Task Cantidad_fuera_de_rango_es_error(int cantidad)
        {
            var resultado = await Armar(new FakeRecomendacionRepository())
                .ResolverListaAsync(Pedido((1, cantidad)), CancellationToken.None);

            Assert.False(resultado.Success);
        }

        [Fact]
        public async Task Productos_repetidos_en_la_lista_es_error()
        {
            var resultado = await Armar(new FakeRecomendacionRepository())
                .ResolverListaAsync(Pedido((1, 1), (1, 2)), CancellationToken.None);

            Assert.False(resultado.Success);
            Assert.Contains(resultado.Errors, e => e.Contains("repetidos", StringComparison.OrdinalIgnoreCase));
        }
    }
}
