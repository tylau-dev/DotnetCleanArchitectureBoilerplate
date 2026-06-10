using MediatR;

namespace Application.Common.Messaging;

/// <summary>
/// Marker interface for commands that return a response of type <typeparamref name="TResponse"/>.
/// </summary>
/// <typeparam name="TResponse">The type of the response returned after handling the command.</typeparam>
public interface ICommand<out TResponse> : IRequest<TResponse>
{
}
