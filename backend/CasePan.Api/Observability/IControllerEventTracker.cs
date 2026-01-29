using Microsoft.AspNetCore.Http;

namespace CasePan.Api.Observability;

public interface IControllerEventTracker
{
    Task<string> TrackSuccessAsync(
          HttpContext httpContext,
          string eventName,
          string userMessage,
          object? payload,
          CancellationToken ct);

    Task<string> TrackFailureAsync(
        HttpContext httpContext,
        string eventName,
        string userMessage,
        object? payload,
        Exception exception,
        CancellationToken ct);

    Task<string> TrackAsync(
     HttpContext httpContext,
     string eventName,
     string userMessage,
     object? payload,
     string outcome,              // "success" | "not_found" | "failure"
     CancellationToken ct,
     Exception? exception = null);

}
