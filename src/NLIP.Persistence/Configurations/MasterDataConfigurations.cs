using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NLIP.Domain.Entities.MasterData;

namespace NLIP.Persistence.Configurations;

public class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.ToTable("Branches");
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Code).IsRequired().HasMaxLength(20);
        builder.HasIndex(b => b.Code).IsUnique();
    }
}

public class AgentConfiguration : IEntityTypeConfiguration<Agent>
{
    public void Configure(EntityTypeBuilder<Agent> builder)
    {
        builder.ToTable("Agents");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Code).IsRequired().HasMaxLength(20);
        builder.HasIndex(a => a.Code).IsUnique();
        builder.HasOne(a => a.Branch).WithMany().HasForeignKey(a => a.BranchId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class EmployerConfiguration : IEntityTypeConfiguration<Employer>
{
    public void Configure(EntityTypeBuilder<Employer> builder)
    {
        builder.ToTable("Employers");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
    }
}

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(c => c.LastName).IsRequired().HasMaxLength(100);
        builder.Ignore(c => c.FullName);
    }
}

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Code).IsRequired().HasMaxLength(20);
        builder.HasIndex(p => p.Code).IsUnique();
    }
}
