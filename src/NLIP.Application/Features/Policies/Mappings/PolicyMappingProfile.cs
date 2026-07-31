using AutoMapper;
using NLIP.Application.Features.Policies.Dtos;
using NLIP.Domain.Entities.Policies;

namespace NLIP.Application.Features.Policies.Mappings;

public class PolicyMappingProfile : Profile
{
    public PolicyMappingProfile()
    {
        CreateMap<Policy, PolicySummaryDto>()
            .ForMember(d => d.CustomerOrEmployerName, o => o.MapFrom(s =>
                s.Customer != null ? s.Customer.FirstName + " " + s.Customer.LastName : s.Employer != null ? s.Employer.Name : null))
            .ForMember(d => d.BranchName, o => o.MapFrom(s => s.Branch != null ? s.Branch.Name : null))
            .ForMember(d => d.AgentName, o => o.MapFrom(s => s.Agent != null ? s.Agent.FullName : null));

        CreateMap<Policy, PolicyDetailDto>()
            .IncludeBase<Policy, PolicySummaryDto>()
            .ForMember(d => d.Beneficiaries, o => o.MapFrom(s => s.Beneficiaries))
            .ForMember(d => d.GroupMembers, o => o.MapFrom(s => s.GroupMembers))
            .ForMember(d => d.History, o => o.MapFrom(s => s.History))
            .ForMember(d => d.Transactions, o => o.Ignore())
            .ForMember(d => d.SubmissionTimeline, o => o.Ignore());

        CreateMap<PolicyBeneficiary, BeneficiaryDto>();
        CreateMap<GroupMember, GroupMemberDto>();
        CreateMap<PolicyHistory, PolicyHistoryDto>();
        CreateMap<Domain.Entities.Integration.NaicomTransaction, NaicomTransactionDto>();
        CreateMap<Domain.Entities.Integration.SubmissionQueue, SubmissionQueueDto>();
    }
}
