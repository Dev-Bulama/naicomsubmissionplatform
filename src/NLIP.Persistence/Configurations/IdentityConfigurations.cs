using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NLIP.Domain.Entities.Identity;

namespace NLIP.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Username).IsRequired().HasMaxLength(100);
        builder.Property(u => u.Email).IsRequired().HasMaxLength(256);
        builder.Property(u => u.PasswordHash).IsRequired();
        builder.HasIndex(u => u.Username).IsUnique();
        builder.HasIndex(u => u.Email).IsUnique();
        builder.Navigation(u => u.UserRoles).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Name).IsRequired().HasMaxLength(100);
        builder.HasIndex(r => r.Name).IsUnique();
        builder.Navigation(r => r.RolePermissions).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permissions");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(100);
        builder.HasIndex(p => p.Name).IsUnique();
    }
}

public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("RolePermissions");
        builder.HasKey(rp => rp.Id);
        builder.HasIndex(rp => new { rp.RoleId, rp.PermissionId }).IsUnique();
        builder.HasOne(rp => rp.Role).WithMany(r => r.RolePermissions).HasForeignKey(rp => rp.RoleId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(rp => rp.Permission).WithMany().HasForeignKey(rp => rp.PermissionId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("UserRoles");
        builder.HasKey(ur => ur.Id);
        builder.HasIndex(ur => new { ur.UserId, ur.RoleId }).IsUnique();
        builder.HasOne(ur => ur.User).WithMany(u => u.UserRoles).HasForeignKey(ur => ur.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(ur => ur.Role).WithMany().HasForeignKey(ur => ur.RoleId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Token).IsRequired().HasMaxLength(200);
        builder.HasIndex(t => t.Token).IsUnique();
        builder.HasOne(t => t.User).WithMany().HasForeignKey(t => t.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}
