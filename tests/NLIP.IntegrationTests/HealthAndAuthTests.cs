using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace NLIP.IntegrationTests;

public class HealthAndAuthTests : IClassFixture<NlipWebApplicationFactory>
{
    private readonly HttpClient _client;

    public HealthAndAuthTests(NlipWebApplicationFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task HealthCheck_ReturnsHealthy()
    {
        var response = await _client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithoutToken_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/dashboard/stats");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithUnknownUser_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new { username = "does-not-exist", password = "whatever" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
