using NLIP.Domain.Entities.Identity;

namespace NLIP.Application.Common.Interfaces;

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}

public interface IJwtTokenGenerator
{
    (string Token, DateTimeOffset ExpiresAt) GenerateAccessToken(User user, IReadOnlyList<string> roles, IReadOnlyList<string> permissions);
    string GenerateRefreshToken();
}
