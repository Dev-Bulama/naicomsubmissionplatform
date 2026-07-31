using System.Text.Json.Serialization;

namespace NLIP.Integration.Dtos.Auth;

/// <summary>Credentials configured by an administrator in Settings (SettingKeys.NaicomSid /
/// NaicomSecret) and exchanged for a bearer token at the NAICOM auth endpoint.</summary>
public class NaicomTokenRequestDto
{
    [JsonPropertyName("sid")]
    public string Sid { get; set; } = default!;

    [JsonPropertyName("secret")]
    public string Secret { get; set; } = default!;
}

public class NaicomTokenResponseDto
{
    [JsonPropertyName("accessToken")]
    public string AccessToken { get; set; } = default!;

    [JsonPropertyName("expiresInSeconds")]
    public int ExpiresInSeconds { get; set; } = 3600;

    [JsonPropertyName("tokenType")]
    public string TokenType { get; set; } = "Bearer";
}
