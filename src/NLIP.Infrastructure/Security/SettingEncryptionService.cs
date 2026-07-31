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

/// <summary>
/// AES-256-GCM encryption for secret SystemSettings values (NAICOM Secret, SMTP password, etc.)
/// before they hit the database, so a DB-only compromise doesn't leak credentials.
///
/// Deliberately does NOT fall back to a randomly generated key when Encryption:Key is unset: a
/// per-process random key would silently make every previously encrypted setting undecryptable
/// after the next restart — effectively locking the app out of its own NAICOM credentials. Instead
/// this fails fast, but only at the point a secret is actually encrypted/decrypted, so the app
/// still boots and runs normally (dev/test/CI) right up until something genuinely needs the key.
/// </summary>
public class SettingEncryptionService
{
    private readonly byte[]? _key;

    public SettingEncryptionService(IOptions<EncryptionOptions> options)
    {
        var configured = options.Value.Key;
        _key = string.IsNullOrWhiteSpace(configured) ? null : Convert.FromBase64String(configured);
    }

    public string Encrypt(string plainText)
    {
        var key = RequireKey();
        var nonce = RandomNumberGenerator.GetBytes(12);
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = new byte[plainBytes.Length];
        var tag = new byte[16];

        using var aes = new AesGcm(key, tag.Length);
        aes.Encrypt(nonce, plainBytes, cipherBytes, tag);

        return Convert.ToBase64String(nonce) + "." + Convert.ToBase64String(tag) + "." + Convert.ToBase64String(cipherBytes);
    }

    public string Decrypt(string cipherText)
    {
        var key = RequireKey();
        var parts = cipherText.Split('.');
        if (parts.Length != 3) throw new FormatException("Invalid encrypted setting format.");

        var nonce = Convert.FromBase64String(parts[0]);
        var tag = Convert.FromBase64String(parts[1]);
        var cipherBytes = Convert.FromBase64String(parts[2]);
        var plainBytes = new byte[cipherBytes.Length];

        using var aes = new AesGcm(key, tag.Length);
        aes.Decrypt(nonce, cipherBytes, tag, plainBytes);

        return Encoding.UTF8.GetString(plainBytes);
    }

    private byte[] RequireKey() => _key
        ?? throw new InvalidOperationException(
            "Encryption:Key is not configured. Set a base64-encoded 32-byte AES key via environment " +
            "variable or Key Vault (generate one with: openssl rand -base64 32) before storing or " +
            "reading any secret SystemSettings value (e.g. Naicom:Secret).");
}
