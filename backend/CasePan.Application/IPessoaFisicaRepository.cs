using CasePan.Domain;

namespace CasePan.Application;

public interface IPessoaFisicaRepository
{
    Task<PessoaFisica?> GetAsync(Guid id, CancellationToken ct);
    Task<List<PessoaFisica>> ListAsync(CancellationToken ct);
    Task AddAsync(PessoaFisica entity, CancellationToken ct);
    Task UpdateAsync(PessoaFisica entity, CancellationToken ct);
    Task DeleteAsync(PessoaFisica entity, CancellationToken ct);
}
