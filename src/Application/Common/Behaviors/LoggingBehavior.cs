using Application.Common.Logging;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Common.Behaviors;

/// <summary>
/// Logs the start, completion, and any unhandled exception for every request handled through the
/// MediatR pipeline.
/// </summary>
/// <typeparam name="TRequest">The type of the request being handled.</typeparam>
/// <typeparam name="TResponse">The type of the response returned by the handler.</typeparam>
public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoggingBehavior{TRequest, TResponse}"/> class.
    /// </summary>
    /// <param name="logger">The logger used to record request handling.</param>
    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        _logger.LogHandlingRequest(requestName);

        try
        {
            var response = await next(cancellationToken).ConfigureAwait(false);

            _logger.LogHandledRequest(requestName);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogUnhandledRequestException(ex, requestName);
            throw;
        }
    }
}
