using FluentAssertions;
using NLIP.Infrastructure.Security;
using Xunit;

namespace NLIP.UnitTests.Infrastructure;

public class PasswordHasherTests
{
    private readonly PasswordHasher _hasher = new();

    [Fact]
    public void Hash_ThenVerify_WithCorrectPassword_Succeeds()
    {
        var hash = _hasher.Hash("Sup3r$ecretPassw0rd!");

        _hasher.Verify("Sup3r$ecretPassw0rd!", hash).Should().BeTrue();
    }

    [Fact]
    public void Verify_WithWrongPassword_Fails()
    {
        var hash = _hasher.Hash("Sup3r$ecretPassw0rd!");

        _hasher.Verify("wrong-password", hash).Should().BeFalse();
    }

    [Fact]
    public void Hash_ProducesDifferentHashesForSamePassword_DueToSalting()
    {
        var hash1 = _hasher.Hash("Sup3r$ecretPassw0rd!");
        var hash2 = _hasher.Hash("Sup3r$ecretPassw0rd!");

        hash1.Should().NotBe(hash2);
    }
}
