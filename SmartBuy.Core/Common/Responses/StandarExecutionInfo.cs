namespace SmartBuy.Core.Common.Responses
{
    /// <summary>Trazabilidad de la ejecución Orion: acción, executor y query resueltos.</summary>
    public class StandarExecutionInfo
    {
        public string? Action { get; set; }

        public string? Executor { get; set; }

        public string? Query { get; set; }
    }
}
