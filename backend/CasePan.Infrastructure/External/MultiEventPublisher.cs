using CasePan.Application;
using Microsoft.Extensions.Logging;

namespace CasePan.Infrastructure.External;

public sealed class MultiEventPublisher : IEventPublisher
{
    private readonly ILogger<MultiEventPublisher> _logger;
    private readonly IReadOnlyList<IEventPublisher> _publishers;

    public MultiEventPublisher(ILogger<MultiEventPublisher> logger, IReadOnlyList<IEventPublisher> publishers)
    {
        _logger = logger;
        _publishers = publishers;
    }

    public async Task PublishAsync<T>(string eventName, T payload, string correlationId, CancellationToken ct = default)
    {
        foreach (var p in _publishers)
        {
            try
            {
                await p.PublishAsync(eventName, payload, correlationId, ct);
            }
            catch (Exception ex)
            {
                // Mantém o sistema funcionando mesmo se 1 sink falhar (demo/produção).
                _logger.LogError(ex, "Falha ao publicar evento {EventName} em {Publisher}", eventName, p.GetType().Name);
            }
        }
    }
}
