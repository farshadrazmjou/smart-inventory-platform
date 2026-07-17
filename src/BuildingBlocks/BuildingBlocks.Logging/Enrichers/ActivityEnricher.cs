using System.Diagnostics;
using Serilog.Core;
using Serilog.Events;
using BuildingBlocks.Logging.Constants;

namespace BuildingBlocks.Logging.Enrichers;

public sealed class ActivityEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var activity=Activity.Current;
        if(activity is null)
            return;
        
        logEvent.AddPropertyIfAbsent(
            property: propertyFactory.CreateProperty(
                name: LogPropertyNames.TraceId,
                value: activity.TraceId.ToString()));
        
        logEvent.AddPropertyIfAbsent(
            property: propertyFactory.CreateProperty(
                name: LogPropertyNames.SpanId,
                value: activity.SpanId.ToString()));
    }
}