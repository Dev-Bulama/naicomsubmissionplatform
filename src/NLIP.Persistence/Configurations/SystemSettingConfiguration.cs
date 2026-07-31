using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NLIP.Domain.Entities.Common;

namespace NLIP.Persistence.Configurations;

public class SystemSettingConfiguration : IEntityTypeConfiguration<SystemSetting>
{
    public void Configure(EntityTypeBuilder<SystemSetting> builder)
    {
        builder.ToTable("SystemSettings");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Key).IsRequired().HasMaxLength(200);
        builder.HasIndex(s => s.Key).IsUnique();
    }
}

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");
        builder.HasKey(n => n.Id);
        builder.HasIndex(n => n.UserId);
        builder.HasIndex(n => n.CreatedAt);
    }
}

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");
        builder.HasKey(a => a.Id);
        builder.HasIndex(a => a.Timestamp);
        builder.HasIndex(a => new { a.EntityName, a.EntityId });
    }
}

public class ActivityLogConfiguration : IEntityTypeConfiguration<ActivityLog>
{
    public void Configure(EntityTypeBuilder<ActivityLog> builder)
    {
        builder.ToTable("ActivityLogs");
        builder.HasKey(a => a.Id);
        builder.HasIndex(a => a.Timestamp);
    }
}

public class ErrorLogConfiguration : IEntityTypeConfiguration<ErrorLog>
{
    public void Configure(EntityTypeBuilder<ErrorLog> builder)
    {
        builder.ToTable("ErrorLogs");
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => e.OccurredAt);
    }
}
