using AutoMapper;
using NLIP.Domain.Entities.Policies;
using NLIP.Integration.Dtos.GroupLife;
using NLIP.Integration.Dtos.IndividualLife;

namespace NLIP.Integration.Mapping;

/// <summary>
/// The single place that turns a Domain Policy aggregate into NAICOM wire DTOs. No other class
/// in this solution builds NAICOM JSON by hand — NaicomApiClient calls IMapper.Map, never string
/// concatenation or manual JObject construction ("never hardcode JSON" requirement).
/// </summary>
public class PolicyToNaicomMappingProfile : Profile
{
    private const string DateFormat = "yyyy-MM-dd";

    public PolicyToNaicomMappingProfile()
    {
        CreateMap<Policy, IndividualLifePolicyRequestDto>()
            .ForMember(d => d.PolicyNo, o => o.MapFrom(s => s.PolicyNumber))
            .ForMember(d => d.NaicomPolicyId, o => o.MapFrom(s => s.NaicomPolicyId))
            .ForMember(d => d.ProductCode, o => o.MapFrom(s => s.Product!.Code))
            .ForMember(d => d.BranchCode, o => o.MapFrom(s => s.Branch!.Code))
            .ForMember(d => d.AgentCode, o => o.MapFrom(s => s.Agent != null ? s.Agent.Code : null))
            .ForMember(d => d.InsuredFirstName, o => o.MapFrom(s => s.Customer!.FirstName))
            .ForMember(d => d.InsuredLastName, o => o.MapFrom(s => s.Customer!.LastName))
            .ForMember(d => d.InsuredDateOfBirth, o => o.MapFrom(s => s.Customer!.DateOfBirth.ToString(DateFormat)))
            .ForMember(d => d.InsuredGender, o => o.MapFrom(s => s.Customer!.Gender))
            .ForMember(d => d.InsuredNationalId, o => o.MapFrom(s => s.Customer!.NationalIdNumber))
            .ForMember(d => d.InsuredBvn, o => o.MapFrom(s => s.Customer!.Bvn))
            .ForMember(d => d.InsuredPhoneNumber, o => o.MapFrom(s => s.Customer!.PhoneNumber))
            .ForMember(d => d.InsuredEmail, o => o.MapFrom(s => s.Customer!.Email))
            .ForMember(d => d.InsuredAddress, o => o.MapFrom(s => s.Customer!.Address))
            .ForMember(d => d.CommencementDate, o => o.MapFrom(s => s.CoverageStartDate.ToString(DateFormat)))
            .ForMember(d => d.ExpiryDate, o => o.MapFrom(s => s.CoverageEndDate.ToString(DateFormat)))
            .ForMember(d => d.Beneficiaries, o => o.MapFrom(s => s.Beneficiaries));

        CreateMap<PolicyBeneficiary, IndividualLifeBeneficiaryDto>()
            .ForMember(d => d.DateOfBirth, o => o.MapFrom(s => s.DateOfBirth.ToString(DateFormat)));

        CreateMap<Policy, GroupLifePolicyRequestDto>()
            .ForMember(d => d.PolicyNo, o => o.MapFrom(s => s.PolicyNumber))
            .ForMember(d => d.NaicomPolicyId, o => o.MapFrom(s => s.NaicomPolicyId))
            .ForMember(d => d.ProductCode, o => o.MapFrom(s => s.Product!.Code))
            .ForMember(d => d.BranchCode, o => o.MapFrom(s => s.Branch!.Code))
            .ForMember(d => d.AgentCode, o => o.MapFrom(s => s.Agent != null ? s.Agent.Code : null))
            .ForMember(d => d.EmployerName, o => o.MapFrom(s => s.Employer!.Name))
            .ForMember(d => d.EmployerRcNumber, o => o.MapFrom(s => s.Employer!.RcNumber))
            .ForMember(d => d.EmployerAddress, o => o.MapFrom(s => s.Employer!.Address))
            .ForMember(d => d.EmployerTin, o => o.MapFrom(s => s.Employer!.TaxIdentificationNumber))
            .ForMember(d => d.TotalSumAssured, o => o.MapFrom(s => s.SumAssured))
            .ForMember(d => d.CommencementDate, o => o.MapFrom(s => s.CoverageStartDate.ToString(DateFormat)))
            .ForMember(d => d.ExpiryDate, o => o.MapFrom(s => s.CoverageEndDate.ToString(DateFormat)))
            .ForMember(d => d.NumberOfLives, o => o.MapFrom(s => s.GroupMembers.Count))
            .ForMember(d => d.Members, o => o.MapFrom(s => s.GroupMembers));

        CreateMap<GroupMember, GroupLifeMemberDto>()
            .ForMember(d => d.StaffId, o => o.MapFrom(s => s.EmployeeId))
            .ForMember(d => d.DateOfBirth, o => o.MapFrom(s => s.DateOfBirth.ToString(DateFormat)));
    }
}
