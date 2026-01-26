namespace CasePan.Domain;

/// <summary>
/// Exceção de domínio para violações de regras de negócio.
/// </summary>
public sealed class DomainException : Exception
{
    public DomainException(string message) : base(message) { }

    public DomainException(string message, Exception? innerException)
        : base(message, innerException) { }
}
