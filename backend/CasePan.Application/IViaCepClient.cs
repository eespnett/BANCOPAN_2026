namespace CasePan.Application;

public interface IViaCepClient
{
    Task<ViaCepResult?> ConsultarAsync(string cep, CancellationToken ct);
}

public sealed class ViaCepResult
{
    

    public string? Logradouro { get; set; }
    public string? Bairro { get; set; }
    public string? Localidade { get; set; }
    public string? Uf { get; set; } 
    public bool Erro { get; set; }
}
