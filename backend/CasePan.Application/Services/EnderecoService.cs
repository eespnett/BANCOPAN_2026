using CasePan.Domain;

namespace CasePan.Application.Services;

public sealed class EnderecoService
{
    private readonly IEnderecoRepository _repo;
    private readonly IViaCepClient _viaCep;

    public EnderecoService(IEnderecoRepository repo, IViaCepClient viaCep)
    {
        _repo = repo;
        _viaCep = viaCep;
    }

    // (Opcional) alias para quem estava chamando "LookupCepAsync"
    public Task<ViaCepResult?> LookupCepAsync(string cep, CancellationToken ct) => ConsultarCepAsync(cep, ct);

    public async Task<ViaCepResult?> ConsultarCepAsync(string cep, CancellationToken ct)
    {
        // Para teste e para UX melhor: CEP inválido => null (não exception)
        if (!string.IsNullOrEmpty(OnlyDigits(cep)))
            return null;

        var res = await _viaCep.ConsultarAsync(cep, ct);
        if (res is null || res.Erro)
            return null;

        return res;
    }



    public async Task<Guid> CriarAsync(
        string cep,
        string logradouro,
        string numero,
        string? complemento,
        string bairro,
        string cidade,
        string uf,
        CancellationToken ct)
    {
        cep = OnlyDigits(cep);
        if (cep.Length != 8) throw new InvalidOperationException("CEP inválido. Informe 8 dígitos.");

        var end = new Endereco(
            cep: cep,
            logradouro: logradouro ?? "",
            numero: numero ?? "",
            complemento: complemento,
            bairro: bairro ?? "",
            cidade: cidade ?? "",
            uf: uf ?? ""
        );

        await _repo.AddAsync(end, ct);
        return end.Id;
    }

    public async Task<Guid> CriarPorCepAsync(string cep, string numero, string? complemento, CancellationToken ct)
    {
        cep = OnlyDigits(cep);
        if (cep.Length != 8) throw new InvalidOperationException("CEP inválido. Informe 8 dígitos.");

        var via = await _viaCep.ConsultarAsync(cep, ct);
        if (via is null || via.Erro) throw new InvalidOperationException("CEP não encontrado no ViaCEP.");

        var end = new Endereco(
            cep: cep,
            logradouro: via.Logradouro ?? "",
            numero: numero ?? "",
            complemento: complemento,
            bairro: via.Bairro ?? "",
            cidade: via.Localidade ?? "",
            uf: via.Uf ?? ""
        );

        await _repo.AddAsync(end, ct);
        return end.Id;
    }

    public Task<Endereco?> ObterAsync(Guid id, CancellationToken ct) => _repo.GetAsync(id, ct);

    public Task<List<Endereco>> ListarAsync(CancellationToken ct) => _repo.ListAsync(ct);

    public async Task AtualizarAsync(
        Guid id,
        string logradouro,
        string numero,
        string? complemento,
        string bairro,
        string cidade,
        string uf,
        CancellationToken ct)
    {
        var end = await _repo.GetAsync(id, ct) ?? throw new KeyNotFoundException("Endereço não encontrado.");
        end.Atualizar(
            logradouro: logradouro ?? "",
            numero: numero ?? "",
            complemento: complemento,
            bairro: bairro ?? "",
            cidade: cidade ?? "",
            uf: uf ?? ""
        );
        await _repo.UpdateAsync(end, ct);
    }

    public async Task RemoverAsync(Guid id, CancellationToken ct)
    {
        var end = await _repo.GetAsync(id, ct) ?? throw new KeyNotFoundException("Endereço não encontrado.");
        await _repo.DeleteAsync(end, ct);
    }

    private static string OnlyDigits(string? value)
        => new string((value ?? "").Where(char.IsDigit).ToArray());
}
