using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace NLIP.Infrastructure.Security;

public class EncryptionOptions
{
    public const string SectionName = "Encryption";

    /// <summary>Base64-encoded 256-bit key. Must come from environment variable / Azure Key
    /// Vault in every real deployment (Key Vault-ready per the security requirements) — never
    /// checked into source control.</summary>
    public string Key { get; set; } = string.Empty;
}

/// <summary>AES-256-GCM encryption for secret SystemSettings values (NAICOM Secret, SMTP
/// password, etc.) before they hit the database, so a DB-only compromise doesn't leak credentials.</summary>
public class SettingEncryptionService
{
    private readonly byte[] _key;

    public SettingEncryptionService(IOptions<EncryptionOptions> options)
    {
        var configured = options.Value.Key;
        _key = string.IsNullOrWhiteSpace(configured)
            ? RandomNumberGenerator.GetBytes(32) // dev-only fallback; production must set Encryption:Key
            : Convert.FromBase64String(configured);
    }

    public string Encrypt(string plainText)
    {
        var nonce = RandomNumberGenerator.GetBytes(12);
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = new byte[plainBytes.Length];
        var tag = new byte[16];

        using var aes = new AesGcm(_key, tag.Length);
        aes.Encrypt(nonce, plainBytes, cipherBytes, tag);

        return Convert.ToBase64String(nonce) + "." + Convert.ToBase64String(tag) + "." + Convert.ToBase64String(cipherBytes);
    }

    public string Decrypt(string cipherText)
    {
        var parts = cipherText.Split('.');
        if (parts.Length != 3) throw new FormatException("Invalid encrypted setting format.");

        var nonce = Convert.FromBase64String(parts[0]);
        var tag = Convert.FromBase64String(parts[1]);
        var cipherBytes = Convert.FromBase64String(parts[2]);
        var plainBytes = new byte[cipherBytes.Length];

        using var aes = new AesGcm(_key, tag.Length);
        aes.Decrypt(nonce, cipherBytes, tag, plainBytes);

        return Encoding.UTF8.GetString(plainBytes);
    }
}
