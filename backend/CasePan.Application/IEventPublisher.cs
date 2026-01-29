namespace CasePan.Application;

public interface IEventPublisher
{
    Task PublishAsync<T>(string eventName, T payload, string correlationId, CancellationToken ct = default);
}
