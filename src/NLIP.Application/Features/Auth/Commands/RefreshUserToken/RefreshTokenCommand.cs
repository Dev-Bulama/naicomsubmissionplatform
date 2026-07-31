using MediatR;
using Microsoft.EntityFrameworkCore;
using NLIP.Application.Common.Interfaces;
using NLIP.Application.Features.Auth.Dtos;
using NLIP.Domain.Entities.Identity;

namespace NLIP.Application.Features.Auth.Commands.RefreshUserToken;

public record RefreshTokenCommand(string RefreshToken, string? IpAddress) : IRequest<AuthResultDto>;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResultDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly IDateTimeProvider _dateTime;

    public RefreshTokenCommandHandler(IApplicationDbContext context, IJwtTokenGenerator tokenGenerator, IDateTimeProvider dateTime)
    {
        _context = context;
        _tokenGenerator = tokenGenerator;
        _dateTime = dateTime;
    }

    public async Task<AuthResultDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var existing = await _context.RefreshTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == request.RefreshToken, cancellationToken);

        if (existing is null || !existing.IsActive || existing.User is null)
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");

        var roles = await _context.UserRoles
            .Where(ur => ur.UserId == existing.UserId)
            .Include(ur => ur.Role)
            .Select(ur => ur.Role!.Name)
            .ToListAsync(cancellationToken);

        var permissions = await (
            from ur in _context.UserRoles
            join rp in _context.RolePermissions on ur.RoleId equals rp.RoleId
            join p in _context.Permissions on rp.PermissionId equals p.Id
            where ur.UserId == existing.UserId
            select p.Name).Distinct().ToListAsync(cancellationToken);

        var (accessToken, expiresAt) = _tokenGenerator.GenerateAccessToken(existing.User, roles, permissions);
        var newRefreshToken = _tokenGenerator.GenerateRefreshToken();

        existing.RevokedAt = _dateTime.UtcNow;
        existing.ReplacedByToken = newRefreshToken;

        _context.RefreshTokens.Add(new NLIP.Domain.Entities.Identity.RefreshToken
        {
            UserId = existing.UserId,
            Token = newRefreshToken,
            ExpiresAt = _dateTime.UtcNow.AddDays(7),
            CreatedByIp = request.IpAddress
        });

        await _context.SaveChangesAsync(cancellationToken);

        return new AuthResultDto
        {
            AccessToken = accessToken,
            AccessTokenExpiresAt = expiresAt,
            RefreshToken = newRefreshToken,
            UserName = existing.User.Username,
            FullName = existing.User.FullName,
            Roles = roles,
            Permissions = permissions
        };
    }
}
