namespace CasePan.Domain;

public sealed class Endereco
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public string Cep { get; private set; } = "";
    public string Logradouro { get; private set; } = "";
    public string Numero { get; private set; } = "";
    public string? Complemento { get; private set; }
    public string Bairro { get; private set; } = "";
    public string Cidade { get; private set; } = "";
    public string Uf { get; private set; } = "";

    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;

    // Construtor “rico” (valida regras de domínio)
    public Endereco(string cep, string logradouro, string numero, string? complemento, string bairro, string cidade, string uf)
    {
        SetCep(cep);
        SetTextoObrigatorio(logradouro, "Logradouro");
        SetTextoObrigatorio(numero, "Número");
        SetTextoObrigatorio(bairro, "Bairro");
        SetTextoObrigatorio(cidade, "Cidade");
        SetUf(uf);

        Logradouro = logradouro.Trim();
        Numero = numero.Trim();
        Complemento = string.IsNullOrWhiteSpace(complemento) ? null : complemento.Trim();
        Bairro = bairro.Trim();
        Cidade = cidade.Trim();
        Uf = uf.Trim().ToUpperInvariant();
    }

    // EF/serialização
    private Endereco() { }

    public void Atualizar(string logradouro, string numero, string? complemento, string bairro, string cidade, string uf)
    {
        SetTextoObrigatorio(logradouro, "Logradouro");
        SetTextoObrigatorio(numero, "Número");
        SetTextoObrigatorio(bairro, "Bairro");
        SetTextoObrigatorio(cidade, "Cidade");
        SetUf(uf);

        Logradouro = logradouro.Trim();
        Numero = numero.Trim();
        Complemento = string.IsNullOrWhiteSpace(complemento) ? null : complemento.Trim();
        Bairro = bairro.Trim();
        Cidade = cidade.Trim();
        Uf = uf.Trim().ToUpperInvariant();
    }

    private void SetCep(string cep)
    {
        var digits = OnlyDigits(cep);
        if (digits.Length != 8) throw new DomainException("CEP inválido. Deve conter 8 dígitos.");
        Cep = digits;
    }

    private void SetUf(string uf)
    {
        uf = (uf ?? "").Trim();
        if (uf.Length != 2) throw new DomainException("UF inválida. Deve conter 2 caracteres.");
        Uf = uf.ToUpperInvariant();
    }

    private static void SetTextoObrigatorio(string? value, string campo)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new DomainException($"{campo} é obrigatório.");
    }

    private static string OnlyDigits(string? s)
        => new string((s ?? "").Where(char.IsDigit).ToArray());
}
