using System.Text.Json.Serialization;

namespace NLIP.Integration.Dtos.GroupLife;

/// <summary>Wire DTO for the NAICOM Group Life Create/Update/Renew endpoints. Field set per
/// README_VERIFY_AGAINST_SPEC.md — confirm against the live spec before go-live.</summary>
public class GroupLifePolicyRequestDto
{
    [JsonPropertyName("policyNo")]
    public string PolicyNo { get; set; } = default!;

    [JsonPropertyName("naicomPolicyId")]
    public string? NaicomPolicyId { get; set; }

    [JsonPropertyName("productCode")]
    public string ProductCode { get; set; } = default!;

    [JsonPropertyName("branchCode")]
    public string BranchCode { get; set; } = default!;

    [JsonPropertyName("agentCode")]
    public string? AgentCode { get; set; }

    [JsonPropertyName("employerName")]
    public string EmployerName { get; set; } = default!;

    [JsonPropertyName("employerRcNumber")]
    public string? EmployerRcNumber { get; set; }

    [JsonPropertyName("employerAddress")]
    public string? EmployerAddress { get; set; }

    [JsonPropertyName("employerTin")]
    public string? EmployerTin { get; set; }

    [JsonPropertyName("totalSumAssured")]
    public decimal TotalSumAssured { get; set; }

    [JsonPropertyName("premiumAmount")]
    public decimal PremiumAmount { get; set; }

    [JsonPropertyName("premiumFrequency")]
    public string PremiumFrequency { get; set; } = default!;

    [JsonPropertyName("commencementDate")]
    public string CommencementDate { get; set; } = default!;

    [JsonPropertyName("expiryDate")]
    public string ExpiryDate { get; set; } = default!;

    [JsonPropertyName("numberOfLives")]
    public int NumberOfLives { get; set; }

    [JsonPropertyName("members")]
    public List<GroupLifeMemberDto> Members { get; set; } = new();
}

public class GroupLifeMemberDto
{
    [JsonPropertyName("staffId")]
    public string StaffId { get; set; } = default!;

    [JsonPropertyName("fullName")]
    public string FullName { get; set; } = default!;

    [JsonPropertyName("dateOfBirth")]
    public string DateOfBirth { get; set; } = default!;

    [JsonPropertyName("gender")]
    public string Gender { get; set; } = default!;

    [JsonPropertyName("sumAssured")]
    public decimal SumAssured { get; set; }
}
