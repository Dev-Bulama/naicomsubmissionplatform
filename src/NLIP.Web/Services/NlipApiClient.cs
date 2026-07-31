using System.Net.Http.Headers;
using System.Net.Http.Json;
using NLIP.Web.Models;

namespace NLIP.Web.Services;

/// <summary>Typed HttpClient calling NLIP.API. Attaches the current user's JWT (from
/// AuthSessionService) to every request except login itself.</summary>
public class NlipApiClient
{
    private readonly HttpClient _httpClient;
    private readonly AuthSessionService _session;

    public NlipApiClient(HttpClient httpClient, AuthSessionService session)
    {
        _httpClient = httpClient;
        _session = session;
    }

    private void AttachToken()
    {
        if (_session.Current is not null)
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _session.Current.AccessToken);
    }

    public async Task<AuthResult?> LoginAsync(string username, string password)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/login", new { username, password });
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<AuthResult>();
    }

    public async Task<DashboardStats?> GetDashboardStatsAsync()
    {
        AttachToken();
        return await _httpClient.GetFromJsonAsync<DashboardStats>("api/dashboard/stats");
    }

    public async Task<PaginatedList<PolicySummary>?> SearchPoliciesAsync(string queryString)
    {
        AttachToken();
        return await _httpClient.GetFromJsonAsync<PaginatedList<PolicySummary>>($"api/policies?{queryString}");
    }

    public async Task<PolicyDetail?> GetPolicyAsync(Guid id)
    {
        AttachToken();
        return await _httpClient.GetFromJsonAsync<PolicyDetail>($"api/policies/{id}");
    }

    public async Task<PaginatedList<SyncMonitorItem>?> GetSyncMonitorAsync(string? status, int pageNumber = 1, int pageSize = 50)
    {
        AttachToken();
        var qs = $"pageNumber={pageNumber}&pageSize={pageSize}" + (string.IsNullOrEmpty(status) ? "" : $"&status={status}");
        return await _httpClient.GetFromJsonAsync<PaginatedList<SyncMonitorItem>>($"api/syncmonitor?{qs}");
    }

    public async Task<bool> RetrySubmissionAsync(Guid submissionId)
    {
        AttachToken();
        var response = await _httpClient.PostAsync($"api/policies/submissions/{submissionId}/retry", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<List<SettingItem>?> GetSettingsAsync()
    {
        AttachToken();
        return await _httpClient.GetFromJsonAsync<List<SettingItem>>("api/settings");
    }

    public async Task<bool> UpdateSettingAsync(string key, string value, bool isSecret)
    {
        AttachToken();
        var response = await _httpClient.PutAsJsonAsync($"api/settings/{key}", new { value, isSecret });
        return response.IsSuccessStatusCode;
    }
}
