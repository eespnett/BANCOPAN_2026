namespace CasePan.Application;

public sealed class NoOpEventPublisher : IEventPublisher
{
    public Task PublishAsync<T>(string eventName, T payload, string correlationId, CancellationToken ct)
        => Task.CompletedTask;
}
