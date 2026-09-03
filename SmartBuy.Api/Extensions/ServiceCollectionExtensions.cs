using SmartBuy.Api.Auth;
using SmartBuy.Api.Filters;
using SmartBuy.Api.Workers;
using SmartBuy.Core.Models.Bots;
using SmartBuy.Core.Services.Bots;
using SmartBuy.Core.Interfaces.Repositories;
using SmartBuy.Core.Interfaces.Services;
using SmartBuy.Core.Services;
using SmartBuy.Data.OrionCatalog;
using SmartBuy.Data.Repositories;
using Orion.Domain.Interfaces;
using Orion.Infrastructure.DependencyInjection;
using Orion.Infrastructure.Persistence.Connection;

namespace SmartBuy.Api.Extensions
{
    /// <summary>
    /// Registro de dependencias de la API, agrupado por responsabilidad.
    /// Al agregar un servicio/repositorio nuevo, sumarlo al método de su capa
    /// (no en Program.cs) para minimizar conflictos de merge.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        public const string CorsPolicy = "AllowAngularLocalhost";

        /// <summary>Orion y fábrica de conexiones.</summary>
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOrion(configuration, typeof(SmartBuyOrionCatalog).Assembly);
            services.AddSingleton<IDbConnectionFactory, OrionDbConnectionFactory>();
            services.AddScoped<ApiKeyAuthFilter>();
            services.Configure<BotsConfiguration>(configuration.GetSection("Bots"));

            // El POCO directo, para servicios de Core que necesitan la config de
            // cadenas (deep links de carrito) sin depender de IOptions.
            services.AddSingleton(sp =>
                sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<BotsConfiguration>>().Value);

            return services;
        }

        /// <summary>Servicios de negocio (capa Core).</summary>
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<ICadenaServices, CadenaServices>();
            services.AddScoped<IIngestaServices, IngestaServices>();
            services.AddScoped<IProductoServices, ProductoServices>();
            services.AddScoped<IPublicacionServices, PublicacionServices>();
            services.AddScoped<IRecomendacionServices, RecomendacionServices>();
            services.AddScoped<IAuthServices, AuthServices>();
            services.AddScoped<IListaServices, ListaServices>();
            services.AddScoped<ITendenciaServices, TendenciaServices>();
            services.AddScoped<JwtTokenService>();
            return services;
        }

        /// <summary>Repositorios (capa Data).</summary>
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<ICadenaRepository, CadenaRepository>();
            services.AddScoped<IIngestaRepository, IngestaRepository>();
            services.AddScoped<IProductoRepository, ProductoRepository>();
            services.AddScoped<IPublicacionRepository, PublicacionRepository>();
            services.AddScoped<IRecomendacionRepository, RecomendacionRepository>();
            services.AddScoped<IUsuarioRepository, UsuarioRepository>();
            services.AddScoped<IListaRepository, ListaRepository>();
            services.AddScoped<ITendenciaRepository, TendenciaRepository>();
            return services;
        }

        /// <summary>Procesos de fondo: orquestador de capturas diarias y sus bots.</summary>
        public static IServiceCollection AddWorkers(this IServiceCollection services)
        {
            services.AddHostedService<OrquestadorCapturasWorker>();

            // HttpClient con identidad de navegador: la API pública de VTEX es la
            // misma que usa el sitio y espera un User-Agent normal.
            services.AddHttpClient<VtexCapturaBot>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(30);
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
                client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
            });
            services.AddTransient<ICapturaBot>(sp => sp.GetRequiredService<VtexCapturaBot>());

            services.AddHttpClient<CotoCapturaBot>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(30);
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
                client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
            });
            services.AddTransient<ICapturaBot>(sp => sp.GetRequiredService<CotoCapturaBot>());

            return services;
        }

        /// <summary>Política CORS para el frontend Angular de SmartBuy.</summary>
        public static IServiceCollection AddCorsPolicies(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(CorsPolicy, policy =>
                {
                    policy
                        .WithOrigins("http://localhost:4200")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });
            return services;
        }
    }
}
