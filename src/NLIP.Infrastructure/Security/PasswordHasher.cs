using NLIP.Application.Common.Interfaces;

namespace NLIP.Infrastructure.Security;

/// <summary>BCrypt work factor 12 — deliberately expensive to slow down brute-force/credential-
/// stuffing attempts against the login endpoint (paired with the account-lockout policy in LoginCommandHandler).</summary>
public class PasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12;

    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);

    public bool Verify(string password, string hash) => BCrypt.Net.BCrypt.Verify(password, hash);
}
