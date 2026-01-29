using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CasePan.Application;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CasePan.Tests.Integration.Api;

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Força ambiente de teste (não carrega appsettings.Development.json)
        builder.UseEnvironment("Testing");

        // E garante que publisher não escreve em arquivo durante testes
        builder.ConfigureAppConfiguration((_, cfg) =>
        {
            cfg.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Events:Publisher"] = "noop",
                ["Observability:ControllerEvents:Mode"] = "noop"
            });
        });

        builder.ConfigureServices(services =>
        {
            // Override do ViaCep para não depender de internet
            services.AddSingleton<IViaCepClient, TestViaCepClient>();
        });
    }

    private sealed class TestViaCepClient : IViaCepClient
    {
        public Task<ViaCepResult?> ConsultarAsync(string cep, CancellationToken ct)
        {
            var digits = OnlyDigits(cep);

            // Simula "cep inexistente"
            if (digits == "00000000")
                return Task.FromResult<ViaCepResult?>(new ViaCepResult {   Erro = true });

            return Task.FromResult<ViaCepResult?>(new ViaCepResult
            {
                
                Logradouro = "Praça da Sé",
                
                Bairro = "Sé",
                Localidade = "São Paulo",
                Uf = "SP",
                Erro = false
            });
        }

        private static string OnlyDigits(string? s)
            => new string((s ?? "").Where(char.IsDigit).ToArray());
    }
}
