namespace NLIP.Application.Features.Auth.Dtos;

public class AuthResultDto
{
    public string AccessToken { get; set; } = default!;
    public DateTimeOffset AccessTokenExpiresAt { get; set; }
    public string RefreshToken { get; set; } = default!;
    public string UserName { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public List<string> Roles { get; set; } = new();
    public List<string> Permissions { get; set; } = new();
}
