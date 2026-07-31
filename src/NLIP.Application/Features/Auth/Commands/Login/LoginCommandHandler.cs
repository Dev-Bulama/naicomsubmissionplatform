using MediatR;
using Microsoft.EntityFrameworkCore;
using NLIP.Application.Common.Interfaces;
using NLIP.Application.Features.Auth.Dtos;
using NLIP.Domain.Entities.Identity;
using NLIP.Shared.Constants;

namespace NLIP.Application.Features.Auth.Commands.Login;

/// <summary>
/// Enforces account-lockout policy (AccountLockoutThreshold consecutive failures locks the
/// account for AccountLockoutMinutes) and issues a short-lived JWT + rotating refresh token on
/// success. Password complexity is enforced at account-creation/change time (see
/// ChangePasswordCommandValidator), not here.
/// </summary>
public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResultDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly ISettingsService _settings;
    private readonly IDateTimeProvider _dateTime;

    public LoginCommandHandler(
        IApplicationDbContext context, IPasswordHasher passwordHasher, IJwtTokenGenerator tokenGenerator,
        ISettingsService settings, IDateTimeProvider dateTime)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
        _settings = settings;
        _dateTime = dateTime;
    }

    public async Task<AuthResultDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username, cancellationToken);

        if (user is null || !user.IsActive)
            throw new UnauthorizedAccessException("Invalid username or password.");

        if (user.IsLockedOut)
            throw new UnauthorizedAccessException($"Account is locked until {user.LockoutEnd:u}. Contact an administrator.");

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            var threshold = await _settings.GetIntAsync(SettingKeys.AccountLockoutThreshold, 5, cancellationToken);
            var lockoutMinutes = await _settings.GetIntAsync(SettingKeys.AccountLockoutMinutes, 15, cancellationToken);

            user.AccessFailedCount++;
            if (user.AccessFailedCount >= threshold)
            {
                user.LockoutEnd = _dateTime.UtcNow.AddMinutes(lockoutMinutes);
            }

            await _context.SaveChangesAsync(cancellationToken);
            throw new UnauthorizedAccessException("Invalid username or password.");
        }

        user.AccessFailedCount = 0;
        user.LockoutEnd = null;
        user.LastLoginAt = _dateTime.UtcNow;
        user.LastLoginIp = request.IpAddress;

        var roles = await _context.UserRoles
            .Where(ur => ur.UserId == user.Id)
            .Include(ur => ur.Role)
            .Select(ur => ur.Role!.Name)
            .ToListAsync(cancellationToken);

        var permissions = await (
            from ur in _context.UserRoles
            join rp in _context.RolePermissions on ur.RoleId equals rp.RoleId
            join p in _context.Permissions on rp.PermissionId equals p.Id
            where ur.UserId == user.Id
            select p.Name).Distinct().ToListAsync(cancellationToken);

        var (accessToken, expiresAt) = _tokenGenerator.GenerateAccessToken(user, roles, permissions);
        var refreshTokenValue = _tokenGenerator.GenerateRefreshToken();

        _context.RefreshTokens.Add(new NLIP.Domain.Entities.Identity.RefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenValue,
            ExpiresAt = _dateTime.UtcNow.AddDays(7),
            CreatedByIp = request.IpAddress
        });

        await _context.SaveChangesAsync(cancellationToken);

        return new AuthResultDto
        {
            AccessToken = accessToken,
            AccessTokenExpiresAt = expiresAt,
            RefreshToken = refreshTokenValue,
            UserName = user.Username,
            FullName = user.FullName,
            Roles = roles,
            Permissions = permissions
        };
    }
}
