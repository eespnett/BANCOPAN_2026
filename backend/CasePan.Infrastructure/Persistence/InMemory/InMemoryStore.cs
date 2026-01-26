using System.Collections.Concurrent;
using CasePan.Domain;

namespace CasePan.Infrastructure.Persistence.InMemory;

public sealed class InMemoryStore
{
    public ConcurrentDictionary<Guid, Endereco> Enderecos { get; } = new();
    public ConcurrentDictionary<Guid, PessoaFisica> PessoasFisicas { get; } = new();
    public ConcurrentDictionary<Guid, PessoaJuridica> PessoasJuridicas { get; } = new();
}
