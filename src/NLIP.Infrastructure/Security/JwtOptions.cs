namespace NLIP.Infrastructure.Security;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "NLIP";
    public string Audience { get; set; } = "NLIP.Clients";

    /// <summary>Must be provided via environment variable / Key Vault in every real environment —
    /// see NLIP.Shared docs and appsettings.json comment. Never commit a real value.</summary>
    public string SigningKey { get; set; } = string.Empty;

    public int AccessTokenMinutes { get; set; } = 30;
}
