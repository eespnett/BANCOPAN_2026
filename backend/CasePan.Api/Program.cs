using Amazon;
using Amazon.SQS;
using CasePan.Api.Middleware;
using CasePan.Api.Observability;
using CasePan.Application;
using CasePan.Application.Services;
using CasePan.Infrastructure.External;
using CasePan.Infrastructure.External.AwsSqs;
using CasePan.Infrastructure.Persistence.InMemory;
using CasePan.Api.Observability;
using Amazon.Runtime; // <-- adicione esse using


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// InMemory
builder.Services.AddSingleton<InMemoryStore>();
builder.Services.AddSingleton<IEnderecoRepository, InMemoryEnderecoRepository>();
builder.Services.AddSingleton<IPessoaFisicaRepository, InMemoryPessoaFisicaRepository>();
builder.Services.AddSingleton<IPessoaJuridicaRepository, InMemoryPessoaJuridicaRepository>();

// Services
builder.Services.AddScoped<EnderecoService>();
builder.Services.AddScoped<PessoaFisicaService>();
builder.Services.AddScoped<PessoaJuridicaService>();
builder.Services.AddSwaggerGen(c =>
{
    // Evita conflito de schema quando existem tipos com mesmo nome (CreateRequest/UpdateRequest)
    c.CustomSchemaIds(t => (t.FullName ?? t.Name).Replace("+", "."));
});

// ViaCEP
builder.Services.AddHttpClient<IViaCepClient, ViaCepClient>(c =>
{
    c.BaseAddress = new Uri("https://viacep.com.br");
    c.Timeout = TimeSpan.FromSeconds(10);
});


builder.Services.Configure<AwsSqsOptions>(builder.Configuration.GetSection("Aws:Sqs"));

builder.Services.AddSingleton<IAmazonSQS>(sp =>
{
    var cfgRoot = sp.GetRequiredService<IConfiguration>();

    var serviceUrl = cfgRoot["Aws:Sqs:ServiceUrl"];
    var region = cfgRoot["Aws:Sqs:Region"] ?? "us-east-1";

    var cfg = new AmazonSQSConfig
    {
        RegionEndpoint = RegionEndpoint.GetBySystemName(region)
    };

    // LocalStack: sem conta AWS
    if (!string.IsNullOrWhiteSpace(serviceUrl))
    {
        cfg.ServiceURL = serviceUrl;
        return new AmazonSQSClient(new BasicAWSCredentials("test", "test"), cfg);
    }

    return new AmazonSQSClient(cfg);
});
builder.Services.Configure<ControllerEventsOptions>(
    builder.Configuration.GetSection("ControllerEvents"));


builder.Services.Configure<ControllerEventsOptions>(
    builder.Configuration.GetSection("Observability:ControllerEvents"));

builder.Services.AddScoped<IControllerEventTracker, ControllerEventTracker>();

builder.Services.AddSingleton<IEventPublisher>(sp =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    var env = sp.GetRequiredService<IHostEnvironment>();

    var raw = (cfg["Events:Publisher"] ?? "noop").Trim();

    var modes = raw
        .Split(new[] { '+', ',', ';', '|', ' ' }, StringSplitOptions.RemoveEmptyEntries)
        .Select(x => x.Trim().ToLowerInvariant())
        .ToHashSet();

    var publishers = new List<IEventPublisher>();

    if (modes.Contains("file"))
    {
        var configuredPath = cfg["Events:FilePath"] ?? "logs/events.ndjson";
        var filePath = Path.IsPathRooted(configuredPath)
            ? configuredPath
            : Path.Combine(env.ContentRootPath, configuredPath.Replace('/', Path.DirectorySeparatorChar));

        publishers.Add(new CasePan.Infrastructure.External.FileEventPublisher(
            sp.GetRequiredService<ILogger<CasePan.Infrastructure.External.FileEventPublisher>>(),
            filePath
        ));
    }

    if (modes.Contains("sqs"))
    {
        publishers.Add(new CasePan.Infrastructure.External.AwsSqs.SqsEventPublisher(
            sp.GetRequiredService<IAmazonSQS>(),
            sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<CasePan.Infrastructure.External.AwsSqs.AwsSqsOptions>>()
        ));
    }

    return publishers.Count switch
    {
        0 => new CasePan.Application.NoOpEventPublisher(),
        1 => publishers[0],
        _ => new CasePan.Infrastructure.External.MultiEventPublisher(
            sp.GetRequiredService<ILogger<CasePan.Infrastructure.External.MultiEventPublisher>>(),
            publishers
        )
    };
});


builder.Services.AddSingleton<IAmazonSQS>(sp =>
{
    var opt = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<AwsSqsOptions>>().Value;

    var cfg = new AmazonSQSConfig
    {
        RegionEndpoint = RegionEndpoint.GetBySystemName(opt.Region)
    };

    // LocalStack: ServiceUrl aponta para localhost e evita custo
    if (!string.IsNullOrWhiteSpace(opt.ServiceUrl))
    {
        cfg.ServiceURL = opt.ServiceUrl;

        // Credenciais fake (não precisa conta AWS)
        var creds = new BasicAWSCredentials("test", "test");
        return new AmazonSQSClient(creds, cfg);
    }

    // Se um dia apontar para AWS real, aí sim precisará credencial real
    return new AmazonSQSClient(cfg);
});


//configure AWS - E configuração SQS - file log
builder.Services.AddSingleton<IEventPublisher>(sp =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    var env = sp.GetRequiredService<IHostEnvironment>();

    var mode = (cfg["Events:Publisher"] ?? "noop").Trim().ToLowerInvariant();

    // força o arquivo a ficar no: CasePan.Api\logs\event.ndjson
    var configuredPath = cfg["Events:FilePath"] ?? "logs/event.ndjson";
    var filePath = Path.IsPathRooted(configuredPath)
        ? configuredPath
        : Path.Combine(env.ContentRootPath, configuredPath.Replace('/', Path.DirectorySeparatorChar));

    return mode switch
    {
        "file" => new CasePan.Infrastructure.External.FileEventPublisher(
            sp.GetRequiredService<ILogger<CasePan.Infrastructure.External.FileEventPublisher>>(),
            filePath
        ),

        // (opcional) se quiser SQS depois:
        "sqs" => new CasePan.Infrastructure.External.AwsSqs.SqsEventPublisher(
            sp.GetRequiredService<IAmazonSQS>(),
            sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<CasePan.Infrastructure.External.AwsSqs.AwsSqsOptions>>()
        ),

        _ => new CasePan.Application.NoOpEventPublisher()
    };
});





var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseMiddleware<CasePan.Api.Middleware.CorrelationIdMiddleware>();

app.MapControllers();
app.Run();

public partial class Program { }
