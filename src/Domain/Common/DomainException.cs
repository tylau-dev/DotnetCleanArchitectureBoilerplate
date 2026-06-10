namespace Domain.Common;

/// <summary>
/// Base type for exceptions that represent violations of domain invariants or business rules.
/// </summary>
public abstract class DomainException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DomainException"/> class.
    /// </summary>
    /// <param name="message">A message describing the domain rule violation.</param>
    protected DomainException(string message)
        : base(message)
    {
    }
}
