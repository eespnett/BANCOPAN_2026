namespace CasePan.Domain;

public sealed class PessoaFisica
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public string Nome { get; private set; } = "";
    public string Cpf { get; private set; } = "";

    public Guid EnderecoId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;

    public PessoaFisica(string nome, string cpf, Guid enderecoId)
    {
        Atualizar(nome, cpf);
        if (enderecoId == Guid.Empty) throw new DomainException("EndereçoId é obrigatório.");
        EnderecoId = enderecoId;
    }

    private PessoaFisica() { }

    public void Atualizar(string nome, string cpf)
    {
        if (string.IsNullOrWhiteSpace(nome)) throw new DomainException("Nome é obrigatório.");
        Nome = nome.Trim();

        var digits = OnlyDigits(cpf);
        if (digits.Length != 11) throw new DomainException("CPF inválido. Deve conter 11 dígitos.");
        Cpf = digits;
    }

    private static string OnlyDigits(string? s)
        => new string((s ?? "").Where(char.IsDigit).ToArray());
}
