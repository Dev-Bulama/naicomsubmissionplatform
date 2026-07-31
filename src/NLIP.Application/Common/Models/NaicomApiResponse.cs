namespace NLIP.Application.Common.Models;

/// <summary>Normalized result of any call to the NAICOM Portal, regardless of which Life
/// endpoint (Individual/Group) was invoked. Callers in Application never see raw NAICOM JSON.</summary>
public class NaicomApiResponse
{
    public bool IsSuccessful { get; init; }
    public string? NaicomPolicyId { get; init; }
    public int? HttpStatusCode { get; init; }
    public string? RawResponse { get; init; }
    public string? ErrorMessage { get; init; }
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
    public long DurationMs { get; init; }

    public static NaicomApiResponse Success(string? naicomPolicyId, int httpStatusCode, string? rawResponse, Guid correlationId, long durationMs) => new()
    {
        IsSuccessful = true,
        NaicomPolicyId = naicomPolicyId,
        HttpStatusCode = httpStatusCode,
        RawResponse = rawResponse,
        CorrelationId = correlationId,
        DurationMs = durationMs
    };

    public static NaicomApiResponse Failure(string errorMessage, int? httpStatusCode, string? rawResponse, Guid correlationId, long durationMs) => new()
    {
        IsSuccessful = false,
        ErrorMessage = errorMessage,
        HttpStatusCode = httpStatusCode,
        RawResponse = rawResponse,
        CorrelationId = correlationId,
        DurationMs = durationMs
    };
}

public class NaicomHealthStatus
{
    public bool IsAvailable { get; init; }
    public string? Version { get; init; }
    public long ResponseTimeMs { get; init; }
    public string? ErrorMessage { get; init; }
    public DateTimeOffset CheckedAt { get; init; } = DateTimeOffset.UtcNow;
}
