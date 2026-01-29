using System.Text.Json;
using CasePan.Application;
using Microsoft.Extensions.Logging;

namespace CasePan.Infrastructure.External;

public sealed class FileEventPublisher : IEventPublisher
{
    private readonly ILogger<FileEventPublisher> _log;
    private readonly string _filePath;

    public FileEventPublisher(ILogger<FileEventPublisher> log, string filePath)
    {
        _log = log;
        _filePath = filePath;
    }

    public async Task PublishAsync<T>(string eventName, T payload, string correlationId, CancellationToken ct)
    {
        var envelope = new
        {
            eventName,
            correlationId,
            occurredAt = DateTimeOffset.UtcNow,
            payload
        };

        var line = JsonSerializer.Serialize(envelope, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        try
        {
            var dir = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrWhiteSpace(dir))
                Directory.CreateDirectory(dir);

            await File.AppendAllTextAsync(_filePath, line + Environment.NewLine, ct);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Failed to write event to file: {FilePath}", _filePath);
            // não estoura a request por falha de log
        }
    }
}
