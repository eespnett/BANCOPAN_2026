using CasePan.Domain;

namespace CasePan.Application.Services;

public sealed class PessoaFisicaService
{
    private readonly IPessoaFisicaRepository _pfRepo;
    private readonly IEnderecoRepository _endRepo;
    private readonly IViaCepClient _viaCep;

    public PessoaFisicaService(IPessoaFisicaRepository pfRepo, IEnderecoRepository endRepo, IViaCepClient viaCep)
    {
        _pfRepo = pfRepo;
        _endRepo = endRepo;
        _viaCep = viaCep;
    }

    public async Task<Guid> CriarAsync(string nome, string cpf, string cep, string numero, string? complemento, CancellationToken ct)
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
        await _endRepo.AddAsync(end, ct);

        var pf = new PessoaFisica(nome, cpf, end.Id);

        await _pfRepo.AddAsync(pf, ct);

        return pf.Id;
    }

    public Task<PessoaFisica?> ObterAsync(Guid id, CancellationToken ct) => _pfRepo.GetAsync(id, ct);
    public Task<List<PessoaFisica>> ListarAsync(CancellationToken ct) => _pfRepo.ListAsync(ct);

    public async Task AtualizarAsync(Guid id, string nome, string cpf, CancellationToken ct)
    {
        var pf = await _pfRepo.GetAsync(id, ct) ?? throw new KeyNotFoundException("Pessoa física não encontrada.");
        pf.Atualizar(nome, cpf);
        await _pfRepo.UpdateAsync(pf, ct);
    }

    public async Task RemoverAsync(Guid id, CancellationToken ct)
    {
        var pf = await _pfRepo.GetAsync(id, ct) ?? throw new KeyNotFoundException("Pessoa física não encontrada.");
        await _pfRepo.DeleteAsync(pf, ct);
    }
}
