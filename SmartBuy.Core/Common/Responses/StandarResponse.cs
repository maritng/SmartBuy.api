namespace SmartBuy.Core.Common.Responses
{
    /// <summary>
    /// Respuesta estándar de servicios y repositorios: resultado tipado + errores +
    /// información de ejecución Orion. Las capas superiores nunca ven OrionResponse.
    /// </summary>
    public class StandarResponse<T>
    {
        public bool Success { get; set; }

        public T? Result { get; set; }

        public List<string> Errors { get; set; } = new List<string>();

        public StandarExecutionInfo Execution { get; set; } = new StandarExecutionInfo();
    }
}
