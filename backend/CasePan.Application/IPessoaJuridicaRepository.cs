using CasePan.Domain;

namespace CasePan.Application;

public interface IPessoaJuridicaRepository
{
    Task<PessoaJuridica?> GetAsync(Guid id, CancellationToken ct);
    Task<List<PessoaJuridica>> ListAsync(CancellationToken ct);
    Task AddAsync(PessoaJuridica entity, CancellationToken ct);
    Task UpdateAsync(PessoaJuridica entity, CancellationToken ct);
    Task DeleteAsync(PessoaJuridica entity, CancellationToken ct);
}
