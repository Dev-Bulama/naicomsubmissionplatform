using System.Text.Json.Serialization;

namespace NLIP.Integration.Dtos.Common;

/// <summary>Returned by Create/Update/Renew (and read by Query) — the NAICOM-issued unique
/// policy identifier this platform persists as Policy.NaicomPolicyId.</summary>
public class NaicomPolicyIdentifierDto
{
    [JsonPropertyName("naicomPolicyId")]
    public string? NaicomPolicyId { get; set; }

    [JsonPropertyName("policyNo")]
    public string? PolicyNo { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }
}

public class NaicomHealthResponseDto
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = default!;

    [JsonPropertyName("timestamp")]
    public DateTimeOffset Timestamp { get; set; }
}

public class NaicomVersionResponseDto
{
    [JsonPropertyName("version")]
    public string Version { get; set; } = default!;
}
