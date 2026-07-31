using System.Text.Json.Serialization;

namespace NLIP.Integration.Dtos.IndividualLife;

/// <summary>Wire DTO for the NAICOM Individual Life (Retail Life) Create/Update/Renew endpoints.
/// Field set per README_VERIFY_AGAINST_SPEC.md — confirm against the live spec before go-live.</summary>
public class IndividualLifePolicyRequestDto
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

    [JsonPropertyName("insuredFirstName")]
    public string InsuredFirstName { get; set; } = default!;

    [JsonPropertyName("insuredLastName")]
    public string InsuredLastName { get; set; } = default!;

    [JsonPropertyName("insuredDateOfBirth")]
    public string InsuredDateOfBirth { get; set; } = default!;

    [JsonPropertyName("insuredGender")]
    public string InsuredGender { get; set; } = default!;

    [JsonPropertyName("insuredNationalId")]
    public string? InsuredNationalId { get; set; }

    [JsonPropertyName("insuredBvn")]
    public string? InsuredBvn { get; set; }

    [JsonPropertyName("insuredPhoneNumber")]
    public string InsuredPhoneNumber { get; set; } = default!;

    [JsonPropertyName("insuredEmail")]
    public string? InsuredEmail { get; set; }

    [JsonPropertyName("insuredAddress")]
    public string? InsuredAddress { get; set; }

    [JsonPropertyName("sumAssured")]
    public decimal SumAssured { get; set; }

    [JsonPropertyName("premiumAmount")]
    public decimal PremiumAmount { get; set; }

    [JsonPropertyName("premiumFrequency")]
    public string PremiumFrequency { get; set; } = default!;

    [JsonPropertyName("commencementDate")]
    public string CommencementDate { get; set; } = default!;

    [JsonPropertyName("expiryDate")]
    public string ExpiryDate { get; set; } = default!;

    [JsonPropertyName("beneficiaries")]
    public List<IndividualLifeBeneficiaryDto> Beneficiaries { get; set; } = new();
}

public class IndividualLifeBeneficiaryDto
{
    [JsonPropertyName("fullName")]
    public string FullName { get; set; } = default!;

    [JsonPropertyName("relationship")]
    public string Relationship { get; set; } = default!;

    [JsonPropertyName("dateOfBirth")]
    public string DateOfBirth { get; set; } = default!;

    [JsonPropertyName("sharePercentage")]
    public decimal SharePercentage { get; set; }
}
