namespace BuildingBlocks.Logging.Options;

public sealed class LoggingOptions
{
    public bool EnableConsole { get; set; } = true;
    public bool EnableFile { get; set; } = true;
    public string MinimumLevel { get; set; } = "Information";
}