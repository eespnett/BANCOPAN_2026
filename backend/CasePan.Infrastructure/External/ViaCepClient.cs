using System.Net.Http.Json;
using CasePan.Application;

namespace CasePan.Infrastructure.External;

public sealed class ViaCepClient : IViaCepClient
{
    private readonly HttpClient _http;

    public ViaCepClient(HttpClient http) => _http = http;

    public async Task<ViaCepResult?> ConsultarAsync(string cep, CancellationToken ct)
    {
        var digits = new string((cep ?? "").Where(char.IsDigit).ToArray());
        if (digits.Length != 8) return null;

        var resp = await _http.GetFromJsonAsync<ViaCepResponse>($"/ws/{digits}/json/", ct);
        if (resp is null) return null;

        return new ViaCepResult
        {
            Logradouro = resp.logradouro,
            Bairro = resp.bairro,
            Localidade = resp.localidade,
            Uf = resp.uf,
            Erro = resp.erro
        };
    }

    // DTO com nomes exatamente como o ViaCEP devolve
    private sealed class ViaCepResponse
    {
        public string? logradouro { get; set; }
        public string? bairro { get; set; }
        public string? localidade { get; set; }
        public string? uf { get; set; }
        public bool erro { get; set; }
    }
}
