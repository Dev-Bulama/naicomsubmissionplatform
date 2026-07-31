using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NLIP.Domain.Entities.Integration;

namespace NLIP.Persistence.Configurations;

public class SubmissionQueueConfiguration : IEntityTypeConfiguration<SubmissionQueue>
{
    public void Configure(EntityTypeBuilder<SubmissionQueue> builder)
    {
        builder.ToTable("SubmissionQueue");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Payload).IsRequired().HasColumnType("nvarchar(max)");
        builder.HasIndex(s => new { s.Status, s.NextAttemptAt });
        builder.HasIndex(s => s.PolicyId);
        builder.HasOne<Domain.Entities.Policies.Policy>().WithMany().HasForeignKey(s => s.PolicyId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class NaicomTransactionConfiguration : IEntityTypeConfiguration<NaicomTransaction>
{
    public void Configure(EntityTypeBuilder<NaicomTransaction> builder)
    {
        builder.ToTable("NaicomTransactions");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.RequestPayload).IsRequired().HasColumnType("nvarchar(max)");
        builder.Property(t => t.ResponsePayload).HasColumnType("nvarchar(max)");
        builder.HasIndex(t => t.PolicyId);
        builder.HasIndex(t => t.AttemptedAt);
        builder.HasOne<Domain.Entities.Policies.Policy>().WithMany().HasForeignKey(t => t.PolicyId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class ApiCallLogConfiguration : IEntityTypeConfiguration<ApiCallLog>
{
    public void Configure(EntityTypeBuilder<ApiCallLog> builder)
    {
        builder.ToTable("ApiCallLogs");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.RequestPayload).HasColumnType("nvarchar(max)");
        builder.Property(a => a.ResponsePayload).HasColumnType("nvarchar(max)");
        builder.HasIndex(a => a.CorrelationId);
        builder.HasIndex(a => a.CreatedAt);
    }
}
