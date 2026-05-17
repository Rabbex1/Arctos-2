using Microsoft.Extensions.Logging;

namespace Arctos.Shell.Logging
{
    public sealed class LogEntry(
        DateTimeOffset timestamp,
        LogLevel level,
        string category,
        string message,
        EventId eventId,
        Exception? exception = null,
        string? scope = null)
    {
        public DateTimeOffset Timestamp { get; } = timestamp;

        public LogLevel Level { get; } = level;

        public string Category { get; } = category;

        public string Message { get; } = message;

    public EventId EventId { get; } = eventId;

    public Exception? Exception { get; } = exception;

    public string? Scope { get; } = scope;

    public string Event => string.IsNullOrWhiteSpace(EventId.Name)
        ? EventId.Id.ToString()
        : EventId.Name!;

    public string? ExceptionText => Exception?.ToString();
}
}
