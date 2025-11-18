using CRM.Domain.Admin;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.EntityConfigurations;

public class DataRetentionPolicyEntityConfiguration : IEntityTypeConfiguration<DataRetentionPolicy>
{
    public void Configure(EntityTypeBuilder<DataRetentionPolicy> builder)
    {
        builder.ToTable("DataRetentionPolicy");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.EntityType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.RetentionPeriodMonths)
            .IsRequired();

        builder.Property(p => p.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(p => p.AutoPurgeEnabled)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .IsRequired();

        builder.Property(p => p.CreatedBy)
            .IsRequired();

        builder.Property(p => p.UpdatedBy)
            .IsRequired(false);

        // Foreign keys
        builder.HasOne(p => p.CreatedByUser)
            .WithMany()
            .HasForeignKey(p => p.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.UpdatedByUser)
            .WithMany()
            .HasForeignKey(p => p.UpdatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        // Unique constraint: one policy per entity type
        builder.HasIndex(p => p.EntityType)
            .IsUnique()
            .HasDatabaseName("IX_DataRetentionPolicy_EntityType");

        builder.HasIndex(p => p.IsActive)
            .HasDatabaseName("IX_DataRetentionPolicy_IsActive");
    }
}

