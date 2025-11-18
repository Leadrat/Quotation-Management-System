using CRM.Domain.Admin;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.EntityConfigurations;

public class SystemSettingsEntityConfiguration : IEntityTypeConfiguration<SystemSettings>
{
    public void Configure(EntityTypeBuilder<SystemSettings> builder)
    {
        builder.ToTable("SystemSettings");
        builder.HasKey(s => s.Key);

        builder.Property(s => s.Key)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(s => s.Value)
            .IsRequired()
            .HasColumnType("jsonb");

        builder.Property(s => s.LastModifiedAt)
            .IsRequired();

        builder.Property(s => s.LastModifiedBy)
            .IsRequired();

        // Foreign key to Users
        builder.HasOne(s => s.LastModifiedByUser)
            .WithMany()
            .HasForeignKey(s => s.LastModifiedBy)
            .OnDelete(DeleteBehavior.Restrict);

        // Index for audit queries
        builder.HasIndex(s => s.LastModifiedAt)
            .HasDatabaseName("IX_SystemSettings_LastModifiedAt");
    }
}

