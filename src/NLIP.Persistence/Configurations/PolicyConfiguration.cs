using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NLIP.Domain.Entities.Policies;

namespace NLIP.Persistence.Configurations;

public class PolicyConfiguration : IEntityTypeConfiguration<Policy>
{
    public void Configure(EntityTypeBuilder<Policy> builder)
    {
        builder.ToTable("Policies");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.PolicyNumber).IsRequired().HasMaxLength(50);
        builder.Property(p => p.CorePolicyId).IsRequired().HasMaxLength(100);
        builder.Property(p => p.NaicomPolicyId).HasMaxLength(100);
        builder.Property(p => p.PremiumFrequency).HasMaxLength(20);

        builder.HasIndex(p => p.PolicyNumber).IsUnique();
        builder.HasIndex(p => p.CorePolicyId).IsUnique();
        builder.HasIndex(p => p.NaicomPolicyId);
        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.BusinessType);

        builder.Property(p => p.RowVersion).IsRowVersion();

        builder.HasOne(p => p.Product).WithMany().HasForeignKey(p => p.ProductId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(p => p.Branch).WithMany().HasForeignKey(p => p.BranchId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(p => p.Agent).WithMany().HasForeignKey(p => p.AgentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(p => p.Customer).WithMany().HasForeignKey(p => p.CustomerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(p => p.Employer).WithMany().HasForeignKey(p => p.EmployerId).OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Beneficiaries)
            .WithOne(b => b.Policy)
            .HasForeignKey(b => b.PolicyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.GroupMembers)
            .WithOne(m => m.Policy)
            .HasForeignKey(m => m.PolicyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.History)
            .WithOne(h => h.Policy)
            .HasForeignKey(h => h.PolicyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(p => p.Beneficiaries).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(p => p.GroupMembers).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(p => p.History).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(p => p.DomainEvents);
    }
}
