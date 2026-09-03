using SmartBuy.Core.Common;
using SmartBuy.Core.Common.Responses;
using SmartBuy.Core.Interfaces.Repositories;
using SmartBuy.Core.Interfaces.Services;
using SmartBuy.Core.Models.Historico;

namespace SmartBuy.Core.Services
{
    /// <summary>
    /// Tendencias de precios por categoría de captura: el SQL agrega los
    /// eslabones diarios (canasta común) y acá se encadenan a índice base 100.
    /// </summary>
    public class TendenciaServices : ITendenciaServices
    {
        private readonly ITendenciaRepository _tendenciaRepository;

        public TendenciaServices(ITendenciaRepository tendenciaRepository)
        {
            _tendenciaRepository = tendenciaRepository;
        }

        public async Task<StandarResponse<EvolucionCategorias>> GetEvolucionCategoriasAsync(int? dias, CancellationToken cancellationToken)
        {
            var ventana = Math.Clamp(dias ?? 90, 7, 365);

            var eslabones = await _tendenciaRepository.GetEvolucionCategoriasAsync(ventana, cancellationToken);
            if (!eslabones.Success)
                return new StandarResponse<EvolucionCategorias> { Success = false, Errors = eslabones.Errors, Execution = eslabones.Execution };

            return new StandarResponse<EvolucionCategorias>
            {
                Success = true,
                Result = new EvolucionCategorias
                {
                    Dias = ventana,
                    Series = IndiceCategoria.Calcular(eslabones.Result ?? new List<EslabonCategoriaFila>(), ventana)
                }
            };
        }
    }
}
