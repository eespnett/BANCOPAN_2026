using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;
using CasePan.Application;
using Microsoft.Extensions.Options;

namespace CasePan.Infrastructure.External.AwsSqs;

public sealed class SqsEventPublisher : IEventPublisher
{
    private readonly IAmazonSQS _sqs;
    private readonly AwsSqsOptions _opt;

    public SqsEventPublisher(IAmazonSQS sqs, IOptions<AwsSqsOptions> opt)
    {
        _sqs = sqs;
        _opt = opt.Value;
    }

    public async Task PublishAsync<T>(string eventName, T payload, string correlationId, CancellationToken ct)
    {
        if (!_opt.Enabled) return;
        if (string.IsNullOrWhiteSpace(_opt.QueueUrl)) return;

        var envelope = new
        {
            eventName,
            correlationId,
            occurredAt = DateTimeOffset.UtcNow,
            payload
        };

        var body = JsonSerializer.Serialize(envelope, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var req = new SendMessageRequest
        {
            QueueUrl = _opt.QueueUrl,
            MessageBody = body,
            MessageAttributes = new Dictionary<string, MessageAttributeValue>
            {
                ["eventName"] = new() { DataType = "String", StringValue = eventName },
                ["correlationId"] = new() { DataType = "String", StringValue = correlationId }
            }
        };

        await _sqs.SendMessageAsync(req, ct);
    }
}
