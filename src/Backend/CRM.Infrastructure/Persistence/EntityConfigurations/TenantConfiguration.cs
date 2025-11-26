using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Persistence.EntityConfigurations
{
    public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
    {
        public void Configure(EntityTypeBuilder<Tenant> builder)
        {
            builder.HasKey(t => t.Id);

            builder.Property(t => t.TenantId)
                .IsRequired()
                .HasMaxLength(64);

            builder.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(t => t.IsActive)
                .HasDefaultValue(true);

            builder.Property(t => t.CreatedAt)
                .HasDefaultValueSql("NOW()");

            // Unique index on TenantId for business identifier lookups
            builder.HasIndex(t => t.TenantId)
                .IsUnique()
                .HasDatabaseName("IX_Tenants_TenantId_Unique");

            // Index on IsActive for filtering active tenants
            builder.HasIndex(t => t.IsActive)
                .HasDatabaseName("IX_Tenants_IsActive");

            builder.ToTable("Tenants");
        }
    }
}
