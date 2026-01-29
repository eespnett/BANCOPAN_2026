namespace CasePan.Api.Observability;

public sealed class ControllerEventsOptions
{
    public bool Enabled { get; set; } = true;

    /// <summary>file | sqs | none</summary>
    public string Publisher { get; set; } = "file";

    public string LogDirectory { get; set; } = "logs";
    public string FileName { get; set; } = "controller-events.jsonl";

    /// <summary>Obrigatório quando Publisher = "sqs"</summary>
    public string? SqsQueueUrl { get; set; }
    // "file" (default) ou "sqs"
    public string Mode { get; set; } = "file";

    

    // Mode=sqs
    public string? QueueUrl { get; set; }

    // Se a fila for FIFO (.fifo)
    public string FifoMessageGroupId { get; set; } = "casepan";
}
