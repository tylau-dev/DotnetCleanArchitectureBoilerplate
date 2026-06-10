using MediatR;

namespace Application.Common.Messaging;

/// <summary>
/// Marker interface for commands that do not return a value.
/// </summary>
public interface ICommand : IRequest
{
}
