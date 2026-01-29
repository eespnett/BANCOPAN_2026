namespace CasePan.Infrastructure.External.AwsSqs;

public sealed class AwsSqsOptions
{
    public bool Enabled { get; set; } = false;

    // QueueUrl do SQS (AWS real) OU LocalStack
    public string? QueueUrl { get; set; }

    // Para LocalStack (sem custo): "http://localhost:4566"
    public string? ServiceUrl { get; set; }

    public string Region { get; set; } = "us-east-1";
}
