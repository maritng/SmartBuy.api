using SmartBuy.Api.Workers;
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
            return services;
        }

        /// <summary>Servicios de negocio (capa Core).</summary>
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<ICadenaServices, CadenaServices>();
            return services;
        }

        /// <summary>Repositorios (capa Data).</summary>
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<ICadenaRepository, CadenaRepository>();
            return services;
        }

        /// <summary>Procesos de fondo: orquestador de capturas diarias.</summary>
        public static IServiceCollection AddWorkers(this IServiceCollection services)
        {
            services.AddHostedService<OrquestadorCapturasWorker>();
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
