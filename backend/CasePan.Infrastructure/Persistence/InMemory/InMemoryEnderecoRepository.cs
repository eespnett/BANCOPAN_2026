using CasePan.Application;
using CasePan.Domain;

namespace CasePan.Infrastructure.Persistence.InMemory;

public sealed class InMemoryEnderecoRepository : IEnderecoRepository
{
    private readonly InMemoryStore _store;

    public InMemoryEnderecoRepository(InMemoryStore store) => _store = store;

    public Task AddAsync(Endereco entity, CancellationToken ct)
    {
        _store.Enderecos[entity.Id] = entity;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Endereco entity, CancellationToken ct)
    {
        _store.Enderecos.TryRemove(entity.Id, out _);
        return Task.CompletedTask;
    }

    public Task<Endereco?> GetAsync(Guid id, CancellationToken ct)
    {
        _store.Enderecos.TryGetValue(id, out var e);
        return Task.FromResult(e);
    }

    public Task<List<Endereco>> ListAsync(CancellationToken ct)
        => Task.FromResult(_store.Enderecos.Values.OrderBy(x => x.Cep).ToList());

    public Task UpdateAsync(Endereco entity, CancellationToken ct)
    {
        _store.Enderecos[entity.Id] = entity;
        return Task.CompletedTask;
    }
}
