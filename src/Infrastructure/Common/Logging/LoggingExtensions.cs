using Microsoft.Extensions.Logging;

namespace Infrastructure.Common.Logging;

/// <summary>
/// Extension methods that prefix log messages with the project's structured numeric log identifiers.
/// </summary>
/// <remarks>
/// Log identifiers follow the format <c>[LogLevel][Layer][Incremental]</c>, e.g. <c>4401</c> = Information /
/// Infrastructure / 01. Layers: 1 = API, 2 = Application, 3 = Domain, 4 = Infrastructure, 5 = Cross-cutting.
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

    /// <summary>Logs that a domain event was appended to the event store.</summary>
    public static void LogAppendedDomainEvent(this ILogger logger, string eventType, Guid streamId)
        => logger.LogInformationWithId(4401, "Appended {EventType} to event store stream {StreamId}", eventType, streamId);
}
