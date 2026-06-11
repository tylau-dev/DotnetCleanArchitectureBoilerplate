namespace Application.Common.Exceptions;

/// <summary>
/// Thrown by a command handler when an entity required to fulfill the request cannot be found.
/// </summary>
public sealed class NotFoundException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundException"/> class.
    /// </summary>
    /// <param name="entityName">The name of the entity type that could not be found.</param>
    /// <param name="key">The identifier that was used to look up the entity.</param>
    public NotFoundException(string entityName, object key)
        : base($"Entity \"{entityName}\" ({key}) was not found.")
    {
    }
}
