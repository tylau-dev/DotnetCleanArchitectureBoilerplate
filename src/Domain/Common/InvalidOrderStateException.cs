namespace Domain.Common;

/// <summary>
/// Thrown when an operation is attempted against an order while it is in a status that does not permit it.
/// </summary>
public sealed class InvalidOrderStateException : DomainException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidOrderStateException"/> class.
    /// </summary>
    /// <param name="message">A message describing why the order's current status does not permit the operation.</param>
    public InvalidOrderStateException(string message)
        : base(message)
    {
    }
}
