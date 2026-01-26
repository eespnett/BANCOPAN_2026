using CasePan.Application;
using CasePan.Domain;

namespace CasePan.Infrastructure.Persistence.InMemory;

public sealed class InMemoryPessoaJuridicaRepository : IPessoaJuridicaRepository
{
    private readonly InMemoryStore _store;

    public InMemoryPessoaJuridicaRepository(InMemoryStore store) => _store = store;

    public Task AddAsync(PessoaJuridica entity, CancellationToken ct)
    {
        _store.PessoasJuridicas[entity.Id] = entity;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(PessoaJuridica entity, CancellationToken ct)
    {
        _store.PessoasJuridicas.TryRemove(entity.Id, out _);
        return Task.CompletedTask;
    }

    public Task<PessoaJuridica?> GetAsync(Guid id, CancellationToken ct)
    {
        _store.PessoasJuridicas.TryGetValue(id, out var pj);
        return Task.FromResult(pj);
    }

    public Task<List<PessoaJuridica>> ListAsync(CancellationToken ct)
        => Task.FromResult(_store.PessoasJuridicas.Values.OrderBy(x => x.RazaoSocial).ToList());

    public Task UpdateAsync(PessoaJuridica entity, CancellationToken ct)
    {
        _store.PessoasJuridicas[entity.Id] = entity;
        return Task.CompletedTask;
    }
}
