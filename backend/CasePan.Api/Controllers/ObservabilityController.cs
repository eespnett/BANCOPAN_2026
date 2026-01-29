using Amazon.SQS;
using Amazon.SQS.Model;
using CasePan.Infrastructure.External.AwsSqs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace CasePan.Api.Controllers;

[ApiController]
[Route("api/observability")]
public class ObservabilityController : ControllerBase
{
    private readonly IAmazonSQS _sqs;
    private readonly AwsSqsOptions _opt;
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _cfg;

    public ObservabilityController(
      IAmazonSQS sqs,
      IOptions<AwsSqsOptions> opt,
      IWebHostEnvironment env,
      IConfiguration cfg
  )
    {
        _sqs = sqs;
        _opt = opt.Value;
        _env = env;
        _cfg = cfg;
    }


    [HttpGet("sqs")]
    public async Task<IActionResult> Sqs(CancellationToken ct)
    {
        var resp = await _sqs.GetQueueAttributesAsync(new GetQueueAttributesRequest
        {
            QueueUrl = _opt.QueueUrl,
            AttributeNames = new List<string>
            {
                "ApproximateNumberOfMessages",
                "ApproximateNumberOfMessagesNotVisible"
            }
        }, ct);

        resp.Attributes.TryGetValue("ApproximateNumberOfMessages", out var n);
        resp.Attributes.TryGetValue("ApproximateNumberOfMessagesNotVisible", out var nv);

        return Ok(new
        {
            approximateNumberOfMessages = n,
            approximateNumberOfMessagesNotVisible = nv,
            queueUrl = _opt.QueueUrl
        });
    }

    [HttpGet("volumetria")]
    public IActionResult Volumetria([FromQuery] int tail = 5000)
    {
        var relPath = _cfg["Events:FilePath"] ?? "logs/events.ndjson";
        var filePath = Path.IsPathRooted(relPath)
           ? relPath
           : Path.Combine(_env.ContentRootPath, relPath);


        if (!System.IO.File.Exists(filePath))
            return Ok(new { total = 0, filePath, byEvent = Array.Empty<object>(), byRoute = Array.Empty<object>() });

        var lines = System.IO.File.ReadLines(filePath).TakeLast(Math.Max(1, tail));

        var items = new List<(string EventName, string Method, string Path, string Outcome)>();

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            using var doc = JsonDocument.Parse(line);
            var root = doc.RootElement;

            var eventName = root.TryGetProperty("eventName", out var en) ? en.GetString() ?? "" : "";
            var outcome = "";
            if (root.TryGetProperty("payload", out var pl) && pl.TryGetProperty("outcome", out var oc))
                outcome = oc.GetString() ?? "";

            var method = "";
            var path = "";
            if (root.TryGetProperty("http", out var http))
            {
                if (http.TryGetProperty("method", out var m)) method = m.GetString() ?? "";
                if (http.TryGetProperty("path", out var p)) path = p.GetString() ?? "";
            }

            items.Add((eventName, method, path, outcome));
        }

        var byEvent = items
            .GroupBy(x => x.EventName)
            .Select(g => new { eventName = g.Key, total = g.Count(), success = g.Count(x => x.Outcome == "success"), failure = g.Count(x => x.Outcome == "failure") })
            .OrderByDescending(x => x.total)
            .ToList();

        var byRoute = items
            .GroupBy(x => $"{x.Method} {x.Path}")
            .Select(g => new { route = g.Key, total = g.Count(), success = g.Count(x => x.Outcome == "success"), failure = g.Count(x => x.Outcome == "failure") })
            .OrderByDescending(x => x.total)
            .ToList();

        return Ok(new { total = items.Count, filePath, byEvent, byRoute });
    }

    // “Sample” (não é peek perfeito, mas pra demo funciona)
    [HttpGet("sqs/sample")]
    public async Task<IActionResult> Sample([FromQuery] int max = 1, CancellationToken ct = default)
    {
        max = Math.Clamp(max, 1, 10);

        var resp = await _sqs.ReceiveMessageAsync(new ReceiveMessageRequest
        {
            QueueUrl = _opt.QueueUrl,
            MaxNumberOfMessages = max,
            WaitTimeSeconds = 0,
            VisibilityTimeout = 0 // devolve rápido
        }, ct);

        return Ok(resp.Messages.Select(m => new {
            m.MessageId,
            m.Body
        }));
    }

    [HttpGet("events-file")]
    public IActionResult EventsFile()
    {
        var configuredPath = Environment.GetEnvironmentVariable("EVENTS_FILEPATH") ?? "logs/events.ndjson";
        var full = Path.IsPathRooted(configuredPath)
            ? configuredPath
            : Path.Combine(_env.ContentRootPath, configuredPath.Replace('/', Path.DirectorySeparatorChar));

        var fi = new FileInfo(full);
        if (!fi.Exists) return Ok(new { file = full, exists = false });

        return Ok(new { file = full, exists = true, sizeBytes = fi.Length, lastWriteUtc = fi.LastWriteTimeUtc });
    }
}
