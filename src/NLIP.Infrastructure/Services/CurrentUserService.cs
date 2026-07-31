using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using NLIP.Application.Common.Interfaces;

namespace NLIP.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public Guid? UserId
    {
        get
        {
            var sub = User?.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);
            return Guid.TryParse(sub, out var id) ? id : null;
        }
    }

    public string? UserName => User?.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.UniqueName);

    public string? IpAddress => _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

    public IReadOnlyList<string> Roles => User?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList() ?? new List<string>();

    public IReadOnlyList<string> Permissions => User?.FindAll("permission").Select(c => c.Value).ToList() ?? new List<string>();

    public bool HasPermission(string permission) => Permissions.Contains(permission);
}
