using CasePan.Application;
using CasePan.Domain;

namespace CasePan.Infrastructure.Persistence.InMemory;

public sealed class InMemoryPessoaFisicaRepository : IPessoaFisicaRepository
{
    private readonly InMemoryStore _store;

    public InMemoryPessoaFisicaRepository(InMemoryStore store) => _store = store;

    public Task AddAsync(PessoaFisica entity, CancellationToken ct)
    {
        _store.PessoasFisicas[entity.Id] = entity;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(PessoaFisica entity, CancellationToken ct)
    {
        _store.PessoasFisicas.TryRemove(entity.Id, out _);
        return Task.CompletedTask;
    }

    public Task<PessoaFisica?> GetAsync(Guid id, CancellationToken ct)
    {
        _store.PessoasFisicas.TryGetValue(id, out var pf);
        return Task.FromResult(pf);
    }

    public Task<List<PessoaFisica>> ListAsync(CancellationToken ct)
        => Task.FromResult(_store.PessoasFisicas.Values.OrderBy(x => x.Nome).ToList());

    public Task UpdateAsync(PessoaFisica entity, CancellationToken ct)
    {
        _store.PessoasFisicas[entity.Id] = entity;
        return Task.CompletedTask;
    }
}
