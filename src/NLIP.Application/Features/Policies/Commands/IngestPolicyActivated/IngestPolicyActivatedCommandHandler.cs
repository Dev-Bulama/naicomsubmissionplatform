using MediatR;
using Microsoft.EntityFrameworkCore;
using NLIP.Application.Common.Exceptions;
using NLIP.Application.Common.Interfaces;
using NLIP.Domain.Entities.MasterData;
using NLIP.Domain.Entities.Policies;
using NLIP.Domain.Enums;

namespace NLIP.Application.Features.Policies.Commands.IngestPolicyActivated;

public class IngestPolicyActivatedCommandHandler : IRequestHandler<IngestPolicyActivatedCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public IngestPolicyActivatedCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Guid> Handle(IngestPolicyActivatedCommand request, CancellationToken cancellationToken)
    {
        var existing = await _context.Policies
            .FirstOrDefaultAsync(p => p.CorePolicyId == request.CorePolicyId, cancellationToken);
        if (existing is not null)
            return existing.Id;

        var branch = await _context.Branches.FirstOrDefaultAsync(b => b.Code == request.BranchCode, cancellationToken)
            ?? throw new NotFoundException(nameof(Branch), request.BranchCode);
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Code == request.ProductCode, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), request.ProductCode);
        Agent? agent = null;
        if (!string.IsNullOrWhiteSpace(request.AgentCode))
        {
            agent = await _context.Agents.FirstOrDefaultAsync(a => a.Code == request.AgentCode, cancellationToken)
                ?? throw new NotFoundException(nameof(Agent), request.AgentCode);
        }

        Guid? customerId = null;
        Guid? employerId = null;

        if (request.BusinessType == BusinessType.IndividualLife)
        {
            if (request.Customer is null)
                throw new ValidationException(new[] { new FluentValidation.Results.ValidationFailure(nameof(request.Customer), "Customer data is required for Individual Life policies.") });

            var customer = new Customer
            {
                FirstName = request.Customer.FirstName,
                LastName = request.Customer.LastName,
                MiddleName = request.Customer.MiddleName,
                DateOfBirth = request.Customer.DateOfBirth,
                Gender = request.Customer.Gender,
                NationalIdNumber = request.Customer.NationalIdNumber,
                Bvn = request.Customer.Bvn,
                PhoneNumber = request.Customer.PhoneNumber,
                Email = request.Customer.Email,
                Address = request.Customer.Address
            };
            _context.Customers.Add(customer);
            customerId = customer.Id;
        }
        else
        {
            if (request.Employer is null)
                throw new ValidationException(new[] { new FluentValidation.Results.ValidationFailure(nameof(request.Employer), "Employer data is required for Group Life policies.") });

            var employer = new Employer
            {
                Name = request.Employer.Name,
                RcNumber = request.Employer.RcNumber,
                Address = request.Employer.Address,
                ContactPerson = request.Employer.ContactPerson,
                ContactEmail = request.Employer.ContactEmail,
                ContactPhone = request.Employer.ContactPhone,
                TaxIdentificationNumber = request.Employer.TaxIdentificationNumber
            };
            _context.Employers.Add(employer);
            employerId = employer.Id;
        }

        var policy = Policy.CreateDraft(
            request.PolicyNumber, request.CorePolicyId, request.BusinessType, product.Id, branch.Id,
            request.SumAssured, request.PremiumAmount, request.PremiumFrequency,
            request.CoverageStartDate, request.CoverageEndDate, agent?.Id, customerId, employerId);

        foreach (var b in request.Beneficiaries)
            policy.AddBeneficiary(b.FullName, b.Relationship, b.DateOfBirth, b.SharePercentage, b.PhoneNumber, b.Email);

        foreach (var m in request.GroupMembers)
            policy.AddGroupMember(m.EmployeeId, m.FullName, m.DateOfBirth, m.Gender, m.SumAssured, m.Designation, m.DateJoined);

        policy.Activate();

        _context.Policies.Add(policy);
        await _context.SaveChangesAsync(cancellationToken);

        return policy.Id;
    }
}
