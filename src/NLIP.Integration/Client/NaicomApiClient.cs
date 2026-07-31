using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AutoMapper;
using Microsoft.Extensions.Logging;
using NLIP.Application.Common.Interfaces;
using NLIP.Application.Common.Models;
using NLIP.Domain.Entities.Policies;
using NLIP.Domain.Enums;
using NLIP.Integration.Dtos.Common;
using NLIP.Integration.Dtos.GroupLife;
using NLIP.Integration.Dtos.IndividualLife;

namespace NLIP.Integration.Client;

/// <summary>
/// Adapter implementing the Application-layer <see cref="INaicomApiClient"/> port. Resolves
/// Individual Life vs Group Life at runtime from Policy.BusinessType, maps the domain aggregate
/// to the matching NAICOM DTO via AutoMapper, attaches the bearer token, and lets the Polly
/// pipeline (registered on this typed HttpClient) handle retry/circuit-breaker/timeout/bulkhead.
/// Every call is timed and its correlation ID logged by the caller (ProcessSubmissionCommandHandler)
/// into NaicomTransaction; this class does not persist anything itself.
/// </summary>
public class NaicomApiClient : INaicomApiClient
{
    private readonly HttpClient _httpClient;
    private readonly INaicomAuthTokenProvider _tokenProvider;
    private readonly IMapper _mapper;
    private readonly ILogger<NaicomApiClient> _logger;

    public NaicomApiClient(HttpClient httpClient, INaicomAuthTokenProvider tokenProvider, IMapper mapper, ILogger<NaicomApiClient> logger)
    {
        _httpClient = httpClient;
        _tokenProvider = tokenProvider;
        _mapper = mapper;
        _logger = logger;
    }

    public Task<NaicomApiResponse> CreatePolicyAsync(Policy policy, CancellationToken cancellationToken = default) =>
        policy.BusinessType == BusinessType.IndividualLife
            ? SendAsync(HttpMethod.Post, NaicomApiEndpoints.IndividualLifeCreate, _mapper.Map<IndividualLifePolicyRequestDto>(policy), cancellationToken)
            : SendAsync(HttpMethod.Post, NaicomApiEndpoints.GroupLifeCreate, _mapper.Map<GroupLifePolicyRequestDto>(policy), cancellationToken);

    public Task<NaicomApiResponse> UpdatePolicyAsync(Policy policy, CancellationToken cancellationToken = default) =>
        policy.BusinessType == BusinessType.IndividualLife
            ? SendAsync(HttpMethod.Put, Format(NaicomApiEndpoints.IndividualLifeUpdate, policy.NaicomPolicyId), _mapper.Map<IndividualLifePolicyRequestDto>(policy), cancellationToken)
            : SendAsync(HttpMethod.Put, Format(NaicomApiEndpoints.GroupLifeUpdate, policy.NaicomPolicyId), _mapper.Map<GroupLifePolicyRequestDto>(policy), cancellationToken);

    public Task<NaicomApiResponse> RenewPolicyAsync(Policy policy, CancellationToken cancellationToken = default) =>
        policy.BusinessType == BusinessType.IndividualLife
            ? SendAsync(HttpMethod.Put, Format(NaicomApiEndpoints.IndividualLifeRenew, policy.NaicomPolicyId), _mapper.Map<IndividualLifePolicyRequestDto>(policy), cancellationToken)
            : SendAsync(HttpMethod.Put, Format(NaicomApiEndpoints.GroupLifeRenew, policy.NaicomPolicyId), _mapper.Map<GroupLifePolicyRequestDto>(policy), cancellationToken);

    public Task<NaicomApiResponse> TerminatePolicyAsync(Policy policy, string reason, CancellationToken cancellationToken = default) =>
        policy.BusinessType == BusinessType.IndividualLife
            ? SendAsync(HttpMethod.Post, Format(NaicomApiEndpoints.IndividualLifeTerminate, policy.NaicomPolicyId), new { reason }, cancellationToken)
            : SendAsync(HttpMethod.Post, Format(NaicomApiEndpoints.GroupLifeTerminate, policy.NaicomPolicyId), new { reason }, cancellationToken);

    public Task<NaicomApiResponse> QueryPolicyAsync(string naicomPolicyId, BusinessType businessType, CancellationToken cancellationToken = default) =>
        SendAsync<object?>(HttpMethod.Get,
            Format(businessType == BusinessType.IndividualLife ? NaicomApiEndpoints.IndividualLifeQuery : NaicomApiEndpoints.GroupLifeQuery, naicomPolicyId),
            null, cancellationToken);

    public Task<NaicomApiResponse> DeletePolicyAsync(string naicomPolicyId, BusinessType businessType, CancellationToken cancellationToken = default) =>
        SendAsync<object?>(HttpMethod.Delete,
            Format(businessType == BusinessType.IndividualLife ? NaicomApiEndpoints.IndividualLifeDelete : NaicomApiEndpoints.GroupLifeDelete, naicomPolicyId),
            null, cancellationToken);

    public async Task<NaicomHealthStatus> CheckHealthAsync(CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var response = await _httpClient.GetAsync(NaicomApiEndpoints.Health, cancellationToken);
            sw.Stop();
            var isUp = response.IsSuccessStatusCode;
            string? version = null;
            try { version = await GetVersionAsync(cancellationToken); } catch { /* health still reported even if version endpoint fails */ }

            return new NaicomHealthStatus { IsAvailable = isUp, Version = version, ResponseTimeMs = sw.ElapsedMilliseconds };
        }
        catch (Exception ex)
        {
            sw.Stop();
            return new NaicomHealthStatus { IsAvailable = false, ResponseTimeMs = sw.ElapsedMilliseconds, ErrorMessage = ex.Message };
        }
    }

    public async Task<string> GetVersionAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetFromJsonAsync<NaicomVersionResponseDto>(NaicomApiEndpoints.Version, cancellationToken);
        return response?.Version ?? "unknown";
    }

    private static string Format(string template, string? id) => string.Format(template, id);

    private async Task<NaicomApiResponse> SendAsync<TBody>(HttpMethod method, string endpoint, TBody? body, CancellationToken cancellationToken)
    {
        var correlationId = Guid.NewGuid();
        var sw = Stopwatch.StartNew();

        try
        {
            var token = await _tokenProvider.GetTokenAsync(cancellationToken);

            using var httpRequest = new HttpRequestMessage(method, endpoint);
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            httpRequest.Headers.Add("X-Correlation-Id", correlationId.ToString());
            if (body is not null)
                httpRequest.Content = JsonContent.Create(body);

            using var httpResponse = await _httpClient.SendAsync(httpRequest, cancellationToken);
            sw.Stop();

            var rawContent = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

            if (httpResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                // Token likely expired mid-flight despite our cache TTL — invalidate and let the
                // caller's retry (outbox engine) pick up a fresh token on the next attempt.
                await _tokenProvider.InvalidateAsync(cancellationToken);
                return NaicomApiResponse.Failure("NAICOM rejected the access token (401). It has been invalidated for the next attempt.",
                    (int)httpResponse.StatusCode, rawContent, correlationId, sw.ElapsedMilliseconds);
            }

            if (!httpResponse.IsSuccessStatusCode)
            {
                return NaicomApiResponse.Failure($"NAICOM returned HTTP {(int)httpResponse.StatusCode}", (int)httpResponse.StatusCode, rawContent, correlationId, sw.ElapsedMilliseconds);
            }

            string? naicomPolicyId = null;
            try
            {
                var envelope = System.Text.Json.JsonSerializer.Deserialize<NaicomEnvelope<NaicomPolicyIdentifierDto>>(rawContent);
                naicomPolicyId = envelope?.Data?.NaicomPolicyId;
            }
            catch (System.Text.Json.JsonException)
            {
                _logger.LogWarning("NLIP NAICOM response for {Endpoint} did not match the expected envelope shape; raw response preserved in the transaction log.", endpoint);
            }

            return NaicomApiResponse.Success(naicomPolicyId, (int)httpResponse.StatusCode, rawContent, correlationId, sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex, "NLIP NAICOM call to {Endpoint} failed", endpoint);
            return NaicomApiResponse.Failure(ex.Message, null, null, correlationId, sw.ElapsedMilliseconds);
        }
    }
}
