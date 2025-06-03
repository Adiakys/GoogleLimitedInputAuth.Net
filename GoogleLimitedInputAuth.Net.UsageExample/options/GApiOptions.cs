namespace GoogleLimitedInputAuth.Net.UsageExample.options;

public sealed class GApiOptions
{
    public const string SectionName = "google";

    public string ApplicationName { get; init; } = string.Empty;
    public string ClientId { get; init; } = string.Empty;
    public string ClientSecret { get; init; } = string.Empty;
}