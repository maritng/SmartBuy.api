using SmartBuy.Core.Common.Mappers;
using SmartBuy.Core.Common.Responses;
using Orion.Application.Abstractions;
using Orion.Domain.Orion;

namespace SmartBuy.Data.Repositories
{
    /// <summary>
    /// Base de los repositorios Orion: ejecuta la acción del catálogo y mapea la
    /// respuesta cruda a StandarResponse tipado, de modo que las interfaces de
    /// Core no expongan OrionResponse.
    /// </summary>
    public abstract class OrionRepositoryBase
    {
        private readonly IOrionGateway _orion;

        protected OrionRepositoryBase(IOrionGateway orion)
        {
            _orion = orion;
        }

        /// <summary>
        /// Pasar data null cuando la acción no recibe parámetros: el request se arma
        /// sin Data.
        /// </summary>
        protected async Task<StandarResponse<T>> ExecuteAsync<T>(string action, object? data, CancellationToken cancellationToken)
        {
            var request = new OrionRequest { Action = action };

            if (data != null)
                request.Data = data;

            var response = await _orion.ExecuteAsync(request, cancellationToken);

            return response.ToStandard<T>();
        }
    }
}
