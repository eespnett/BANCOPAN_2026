using CasePan.Application;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;

namespace CasePan.Api.Observability;

public sealed class ControllerEventTracker : IControllerEventTracker
{
    private readonly IEventPublisher _events;
    private readonly ILogger<ControllerEventTracker> _logger;

    public ControllerEventTracker(IEventPublisher events, ILogger<ControllerEventTracker> logger)
    {
        _events = events;
        _logger = logger;
    }

    public Task<string> TrackSuccessAsync(
        HttpContext httpContext,
        string eventName,
        string userMessage,
        object? payload,
        CancellationToken ct)
        => TrackAsync(httpContext, eventName, userMessage, payload, outcome: "success", ct);

    public Task<string> TrackFailureAsync(
        HttpContext httpContext,
        string eventName,
        string userMessage,
        object? payload,
        Exception exception,
        CancellationToken ct)
        => TrackAsync(httpContext, eventName, userMessage, payload, outcome: "failure", ct, exception);

    public async Task<string> TrackAsync(
        HttpContext httpContext,
        string eventName,
        string userMessage,
        object? payload,
        string outcome,
        CancellationToken ct,
        Exception? exception = null)
    {
        var correlationId = GetCorrelationId(httpContext);

        // O FileEventPublisher já coloca eventName/correlationId/occurredAt no envelope.
        // Aqui vai o "payload" do evento com contexto de controller.
        var controllerPayload = new
        {
            outcome,
            userMessage,
            http = new
            {
                method = httpContext.Request.Method,
                path = httpContext.Request.Path.Value,
                traceId = Activity.Current?.TraceId.ToString()
            },
            payload,
            error = exception is null ? null : new
            {
                type = exception.GetType().FullName,
                message = exception.Message
            }
        };

        try
        {
            await _events.PublishAsync(eventName, controllerPayload, correlationId, ct);
        }
        catch (Exception ex)
        {
            // tracker nunca deve derrubar a API
            _logger.LogError(ex,
                "Falha ao publicar evento. eventName={EventName} correlationId={CorrelationId}",
                eventName, correlationId);
        }

        return correlationId;
    }

    private static string GetCorrelationId(HttpContext ctx)
    {
        if (ctx.Request.Headers.TryGetValue("X-Correlation-Id", out var h) && !string.IsNullOrWhiteSpace(h))
            return h.ToString();

        if (ctx.Items.TryGetValue("CorrelationId", out var v) && v is string s && !string.IsNullOrWhiteSpace(s))
            return s;

        return Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
    }
}
