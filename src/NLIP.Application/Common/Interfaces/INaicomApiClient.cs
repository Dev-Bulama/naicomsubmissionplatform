using NLIP.Application.Common.Models;
using NLIP.Domain.Entities.Policies;
using NLIP.Domain.Enums;

namespace NLIP.Application.Common.Interfaces;

/// <summary>
/// Port to the NAICOM Portal API, implemented by NLIP.Integration. Application code never
/// builds or parses NAICOM JSON directly — it hands over the domain Policy aggregate and gets
/// back a normalized <see cref="NaicomApiResponse"/>. The implementation decides internally
/// whether to call the Individual Life or Group Life endpoint based on Policy.BusinessType,
/// handles authentication/token refresh, and applies the Polly resilience pipeline.
/// </summary>
public interface INaicomApiClient
{
    Task<NaicomApiResponse> CreatePolicyAsync(Policy policy, CancellationToken cancellationToken = default);
    Task<NaicomApiResponse> UpdatePolicyAsync(Policy policy, CancellationToken cancellationToken = default);
    Task<NaicomApiResponse> RenewPolicyAsync(Policy policy, CancellationToken cancellationToken = default);
    Task<NaicomApiResponse> TerminatePolicyAsync(Policy policy, string reason, CancellationToken cancellationToken = default);
    Task<NaicomApiResponse> QueryPolicyAsync(string naicomPolicyId, BusinessType businessType, CancellationToken cancellationToken = default);
    Task<NaicomApiResponse> DeletePolicyAsync(string naicomPolicyId, BusinessType businessType, CancellationToken cancellationToken = default);
    Task<NaicomHealthStatus> CheckHealthAsync(CancellationToken cancellationToken = default);
    Task<string> GetVersionAsync(CancellationToken cancellationToken = default);
}
