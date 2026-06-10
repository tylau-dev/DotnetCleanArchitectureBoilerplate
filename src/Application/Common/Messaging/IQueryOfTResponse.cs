using MediatR;

namespace Application.Common.Messaging;

/// <summary>
/// Marker interface for queries that return a response of type <typeparamref name="TResponse"/>.
/// </summary>
/// <typeparam name="TResponse">The type of the data returned by the query.</typeparam>
public interface IQuery<out TResponse> : IRequest<TResponse>
{
}
