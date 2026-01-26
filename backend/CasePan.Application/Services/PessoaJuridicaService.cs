using CasePan.Domain;

namespace CasePan.Application.Services;

public sealed class PessoaJuridicaService
{
    private readonly IPessoaJuridicaRepository _pjRepo;
    private readonly IEnderecoRepository _endRepo;
    private readonly IViaCepClient _viaCep;

    public PessoaJuridicaService(IPessoaJuridicaRepository pjRepo, IEnderecoRepository endRepo, IViaCepClient viaCep)
    {
        _pjRepo = pjRepo;
        _endRepo = endRepo;
        _viaCep = viaCep;
    }

    public async Task<Guid> CriarAsync(string razaoSocial, string cnpj, string cep, string numero, string? complemento, CancellationToken ct)
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


        var pj = new PessoaJuridica(razaoSocial, cnpj, end.Id);

        await _pjRepo.AddAsync(pj, ct);

        return pj.Id;
    }

    public Task<PessoaJuridica?> ObterAsync(Guid id, CancellationToken ct) => _pjRepo.GetAsync(id, ct);
    public Task<List<PessoaJuridica>> ListarAsync(CancellationToken ct) => _pjRepo.ListAsync(ct);

    public async Task AtualizarAsync(Guid id, string razaoSocial, string cnpj, CancellationToken ct)
    {
        var pj = await _pjRepo.GetAsync(id, ct) ?? throw new KeyNotFoundException("Pessoa jurídica não encontrada.");
        pj.Atualizar(razaoSocial, cnpj);
        await _pjRepo.UpdateAsync(pj, ct);
    }

    public async Task RemoverAsync(Guid id, CancellationToken ct)
    {
        var pj = await _pjRepo.GetAsync(id, ct) ?? throw new KeyNotFoundException("Pessoa jurídica não encontrada.");
        await _pjRepo.DeleteAsync(pj, ct);
    }
}
