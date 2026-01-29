using Microsoft.Extensions.Primitives;

namespace CasePan.Api.Middleware;

public sealed class CorrelationIdMiddleware
{
    private const string Header = "X-Correlation-Id";
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next) => _next = next;

    public async Task Invoke(HttpContext ctx)
    {
        string cid = ctx.Request.Headers.TryGetValue(Header, out StringValues v) && !StringValues.IsNullOrEmpty(v)
            ? v.ToString()
            : Guid.NewGuid().ToString("N")[..12];

        ctx.Items[Header] = cid;
        ctx.Response.Headers[Header] = cid;

        await _next(ctx);
    }

    public static string Get(HttpContext ctx)
        => (ctx.Items[Header] as string) ?? ctx.TraceIdentifier;
}
