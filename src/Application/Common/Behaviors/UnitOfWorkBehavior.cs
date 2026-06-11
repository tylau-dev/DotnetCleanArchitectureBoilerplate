using Application.Common.Messaging;
using Domain.Common;
using MediatR;

namespace Application.Common.Behaviors;

/// <summary>
/// Persists pending changes via <see cref="IUnitOfWork"/> after a command handler completes
/// successfully. Queries are passed through without triggering a save.
/// </summary>
/// <typeparam name="TRequest">The type of the request being handled.</typeparam>
/// <typeparam name="TResponse">The type of the response returned by the handler.</typeparam>
public sealed class UnitOfWorkBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IUnitOfWork unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="UnitOfWorkBehavior{TRequest, TResponse}"/> class.
    /// </summary>
    /// <param name="unitOfWork">The unit of work used to persist pending changes.</param>
    public UnitOfWorkBehavior(IUnitOfWork unitOfWork)
    {
        this.unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var response = await next(cancellationToken).ConfigureAwait(false);

        if (request is ICommand or ICommand<TResponse>)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        return response;
    }
}
