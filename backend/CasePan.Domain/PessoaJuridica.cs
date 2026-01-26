namespace CasePan.Domain;

public sealed class PessoaJuridica
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public string RazaoSocial { get; private set; } = "";
    public string Cnpj { get; private set; } = "";

    public Guid EnderecoId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;

    public PessoaJuridica(string razaoSocial, string cnpj, Guid enderecoId)
    {
        Atualizar(razaoSocial, cnpj);
        if (enderecoId == Guid.Empty) throw new DomainException("EndereçoId é obrigatório.");
        EnderecoId = enderecoId;
    }

    private PessoaJuridica() { }

    public void Atualizar(string razaoSocial, string cnpj)
    {
        if (string.IsNullOrWhiteSpace(razaoSocial)) throw new DomainException("Razão social é obrigatória.");
        RazaoSocial = razaoSocial.Trim();

        var digits = OnlyDigits(cnpj);
        if (digits.Length != 14) throw new DomainException("CNPJ inválido. Deve conter 14 dígitos.");
        Cnpj = digits;
    }

    private static string OnlyDigits(string? s)
        => new string((s ?? "").Where(char.IsDigit).ToArray());
}
