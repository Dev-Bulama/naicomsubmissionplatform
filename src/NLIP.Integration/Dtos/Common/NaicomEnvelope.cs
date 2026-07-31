using System.Text.Json.Serialization;

namespace NLIP.Integration.Dtos.Common;

/// <summary>Generic response envelope assumed for every NAICOM Portal endpoint. See
/// README_VERIFY_AGAINST_SPEC.md — confirm the actual field names/casing against the live spec.</summary>
public class NaicomEnvelope<T>
{
    [JsonPropertyName("statusCode")]
    public string? StatusCode { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("data")]
    public T? Data { get; set; }

    [JsonIgnore]
    public bool IsSuccess => string.Equals(StatusCode, "00", StringComparison.OrdinalIgnoreCase)
        || string.Equals(StatusCode, "success", StringComparison.OrdinalIgnoreCase);
}
