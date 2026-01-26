using CasePan.Application;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace CasePan.Tests.Integration.Api;

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Troca o ViaCEP real por Fake (evita dependência de rede)
            services.RemoveAll(typeof(IViaCepClient));
            services.AddSingleton<IViaCepClient>(new FakeViaCepClient());
        });
    }
}

internal sealed class FakeViaCepClient : IViaCepClient
{
    public Task<ViaCepResult?> ConsultarAsync(string cep, CancellationToken ct)
    {
        var digits = new string((cep ?? "").Where(char.IsDigit).ToArray());
        if (digits.Length != 8) return Task.FromResult<ViaCepResult?>(null);

        // Use este CEP para simular "não encontrado"
        if (digits == "00000000")
            return Task.FromResult<ViaCepResult?>(new ViaCepResult { Erro = true });

        return Task.FromResult<ViaCepResult?>(new ViaCepResult
        {
            Logradouro = "Rua Teste",
            Bairro = "Centro",
            Localidade = "São Paulo",
            Uf = "SP",
            Erro = false
        });
    }
}
