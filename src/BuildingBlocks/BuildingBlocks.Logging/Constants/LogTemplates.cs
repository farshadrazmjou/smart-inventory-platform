namespace BuildingBlocks.Logging.Constants;

public static class LogTemplates
{
    public const string Console =
        "[{Timestamp:HH:mm:ss} {Level:u3}] " +
        "[ServiceName: {ServiceName}] " +
        "[ServiceVersion: {ServiceVersion}] " +
        "[CorrelationId:{CorrelationId}] " +
        "[Trace:{TraceId}] " +
        "[Span:{SpanId}] " +
        "{Message:lj}{NewLine}{Exception}";

    public const string File =
        "{Timestamp:yyyy-MM-dd HH:mm:ss} " +
        "[{Level:u3}] " +
        "[ServiceName: {ServiceName}] " +
        "[ServiceVersion: {ServiceVersion}] " +
        "[CorrelationId:{CorrelationId}] " +
        "[Trace:{TraceId}] " +
        "[Span:{SpanId}] " +
        "{Message:lj}{NewLine}{Exception}";
}