using Microsoft.Extensions.Logging;

namespace Application.Common.Logging;

/// <summary>
/// Extension methods that prefix log messages with the project's structured numeric log identifiers.
/// </summary>
/// <remarks>
/// Log identifiers follow the format <c>[LogLevel][Layer][Incremental]</c>, e.g. <c>2201</c> = Information /
/// Application / 01. Layers: 1 = API, 2 = Application, 3 = Domain, 4 = Infrastructure, 5 = Cross-cutting.
/// </remarks>
public static class LoggingExtensions
{
    /// <summary>Logs an informational message prefixed with the given structured log identifier.</summary>
    public static void LogInformationWithId(this ILogger logger, int logId, string message, params object?[] args)
        => logger.LogInformation($"[{logId}] {message}", args);

    /// <summary>Logs a warning message prefixed with the given structured log identifier.</summary>
    public static void LogWarningWithId(this ILogger logger, int logId, string message, params object?[] args)
        => logger.LogWarning($"[{logId}] {message}", args);

    /// <summary>Logs an error message and exception prefixed with the given structured log identifier.</summary>
    public static void LogErrorWithId(this ILogger logger, int logId, Exception exception, string message, params object?[] args)
        => logger.LogError(exception, $"[{logId}] {message}", args);

    /// <summary>Logs an error message prefixed with the given structured log identifier.</summary>
    public static void LogErrorWithId(this ILogger logger, int logId, string message, params object?[] args)
        => logger.LogError($"[{logId}] {message}", args);

    /// <summary>Logs a debug message prefixed with the given structured log identifier.</summary>
    public static void LogDebugWithId(this ILogger logger, int logId, string message, params object?[] args)
        => logger.LogDebug($"[{logId}] {message}", args);

    /// <summary>Logs that the MediatR pipeline has started handling a request.</summary>
    public static void LogHandlingRequest(this ILogger logger, string requestName)
        => logger.LogInformationWithId(2201, "Handling {RequestName}", requestName);

    /// <summary>Logs that the MediatR pipeline has finished handling a request successfully.</summary>
    public static void LogHandledRequest(this ILogger logger, string requestName)
        => logger.LogInformationWithId(2202, "Handled {RequestName}", requestName);

    /// <summary>Logs that an unhandled exception was thrown while a request was being handled.</summary>
    public static void LogUnhandledRequestException(this ILogger logger, Exception exception, string requestName)
        => logger.LogErrorWithId(2203, exception, "Unhandled exception while processing {RequestName}", requestName);
}
