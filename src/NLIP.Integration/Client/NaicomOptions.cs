namespace NLIP.Integration.Client;

/// <summary>Bound at startup from configuration/environment for the initial connection, then
/// refreshed from the SystemSettings table (ISettingsService) so admins can change it at runtime
/// without redeploying — see the Settings module.</summary>
public class NaicomOptions
{
    public const string SectionName = "Naicom";

    public string BaseUrl { get; set; } = "https://portal.naicom.gov.ng/";
    public string Sid { get; set; } = string.Empty;
    public string Secret { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 30;
    public int RetryCount { get; set; } = 3;
    public int CircuitBreakerFailureThreshold { get; set; } = 5;
    public int CircuitBreakerBreakSeconds { get; set; } = 30;
    public int BulkheadMaxParallelization { get; set; } = 10;
    public int BulkheadMaxQueuedActions { get; set; } = 20;
}
