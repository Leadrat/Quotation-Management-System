using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.EntityConfigurations;

public class RoleEntityConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> b)
    {
        b.ToTable("Roles");
        b.HasKey(r => r.RoleId);
        b.Property(r => r.RoleName).HasMaxLength(100).HasColumnType("citext").IsRequired();
        b.Property(r => r.Description).HasMaxLength(500);
        b.Property(r => r.Permissions)
            .IsRequired()
            .HasColumnType("jsonb")
            .HasDefaultValue("[]");
        b.Property(r => r.IsBuiltIn)
            .IsRequired()
            .HasDefaultValue(false);
        b.Property(r => r.IsActive).HasDefaultValue(true).IsRequired();
        b.Property(r => r.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        b.Property(r => r.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

        b.HasIndex(r => r.RoleName).IsUnique().HasDatabaseName("UX_Roles_RoleName");
        b.HasIndex(r => r.IsActive).HasDatabaseName("IX_Roles_IsActive");
        b.HasIndex(r => r.IsBuiltIn).HasDatabaseName("IX_Roles_IsBuiltIn");

        // Relationships are configured via UserRoleEntityConfiguration (many-to-many)
    }
}
