using CasePan.Application;
using CasePan.Application.Services;
using CasePan.Infrastructure.External;
using CasePan.Infrastructure.Persistence.InMemory;

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

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();
app.Run();

public partial class Program { }
