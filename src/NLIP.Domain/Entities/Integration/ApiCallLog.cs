using NLIP.Domain.Common;

namespace NLIP.Domain.Entities.Integration;

/// <summary>Full request/response capture for every outbound HTTP call (NAICOM or otherwise),
/// consolidating what the spec calls "APIRequests" + "APIResponses" into a single 1:1 row since
/// every request in this platform gets exactly one logged response (or timeout/error).</summary>
public class ApiCallLog : BaseEntity
{
    public Guid CorrelationId { get; set; }
    public string Endpoint { get; set; } = default!;
    public string HttpMethod { get; set; } = default!;
    public string? RequestHeaders { get; set; }
    public string? RequestPayload { get; set; }
    public string? ResponseHeaders { get; set; }
    public string? ResponsePayload { get; set; }
    public int? StatusCode { get; set; }
    public long DurationMs { get; set; }
    public bool IsSuccessful { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
