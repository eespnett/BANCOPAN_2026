using CasePan.Domain;

namespace CasePan.Application;

public interface IEnderecoRepository
{
    Task<Endereco?> GetAsync(Guid id, CancellationToken ct);
    Task<List<Endereco>> ListAsync(CancellationToken ct);
    Task AddAsync(Endereco entity, CancellationToken ct);
    Task UpdateAsync(Endereco entity, CancellationToken ct);
    Task DeleteAsync(Endereco entity, CancellationToken ct);
}
