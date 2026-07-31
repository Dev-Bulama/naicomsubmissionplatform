using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using NLIP.Web.Models;

namespace NLIP.Web.Services;

/// <summary>
/// Holds the signed-in user's JWT/refresh token/claims for the lifetime of the Blazor Server
/// circuit and persists them in ProtectedSessionStorage (browser sessionStorage, encrypted by
/// the server's data protection keys) so a page refresh doesn't force a re-login mid-session —
/// but a closed tab does, which is the session-timeout behavior the security spec asks for.
/// </summary>
public class AuthSessionService
{
    private const string StorageKey = "nlip.session";
    private readonly ProtectedSessionStorage _storage;

    public AuthResult? Current { get; private set; }

    public event Action? Changed;

    public AuthSessionService(ProtectedSessionStorage storage) => _storage = storage;

    public async Task<AuthResult?> RestoreAsync()
    {
        try
        {
            var result = await _storage.GetAsync<AuthResult>(StorageKey);
            if (result.Success && result.Value is not null && result.Value.AccessTokenExpiresAt > DateTimeOffset.UtcNow)
            {
                Current = result.Value;
                Changed?.Invoke();
                return Current;
            }
        }
        catch (InvalidOperationException)
        {
            // Prerendering — storage isn't available yet; caller retries after first render.
        }
        return null;
    }

    public async Task SetAsync(AuthResult auth)
    {
        Current = auth;
        await _storage.SetAsync(StorageKey, auth);
        Changed?.Invoke();
    }

    public async Task ClearAsync()
    {
        Current = null;
        await _storage.DeleteAsync(StorageKey);
        Changed?.Invoke();
    }

    public bool HasPermission(string permission) => Current?.Permissions.Contains(permission) ?? false;
}
