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

    public async Task<Guid> CriarPorCepAsync(string cep, string numero, string? complemento, CancellationToken ct)
    {
        var via = await _viaCep.ConsultarAsync(cep, ct);
        if (via is null || via.Erro) throw new InvalidOperationException("CEP não encontrado no ViaCEP.");

        var end = new Endereco(
            cep: cep,
            logradouro: via.Logradouro ?? "",
            numero: numero,
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

    public async Task AtualizarAsync(Guid id, string logradouro, string numero, string? complemento, string bairro, string cidade, string uf, CancellationToken ct)
    {
        var end = await _repo.GetAsync(id, ct) ?? throw new KeyNotFoundException("Endereço não encontrado.");
        end.Atualizar(logradouro, numero, complemento, bairro, cidade, uf);
        await _repo.UpdateAsync(end, ct);
    }

    public async Task RemoverAsync(Guid id, CancellationToken ct)
    {
        var end = await _repo.GetAsync(id, ct) ?? throw new KeyNotFoundException("Endereço não encontrado.");
        await _repo.DeleteAsync(end, ct);
    }
}
