using CRM.Domain.Admin;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.EntityConfigurations;

public class IntegrationKeyEntityConfiguration : IEntityTypeConfiguration<IntegrationKey>
{
    public void Configure(EntityTypeBuilder<IntegrationKey> builder)
    {
        builder.ToTable("IntegrationKeys");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.KeyName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(i => i.KeyValueEncrypted)
            .IsRequired()
            .HasMaxLength(2000); // Encrypted values can be longer

        builder.Property(i => i.Provider)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(i => i.CreatedAt)
            .IsRequired();

        builder.Property(i => i.UpdatedAt)
            .IsRequired();

        builder.Property(i => i.LastUsedAt)
            .IsRequired(false);

        builder.Property(i => i.CreatedBy)
            .IsRequired();

        builder.Property(i => i.UpdatedBy)
            .IsRequired(false);

        // Foreign keys
        builder.HasOne(i => i.CreatedByUser)
            .WithMany()
            .HasForeignKey(i => i.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.UpdatedByUser)
            .WithMany()
            .HasForeignKey(i => i.UpdatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(i => i.Provider)
            .HasDatabaseName("IX_IntegrationKeys_Provider");

        builder.HasIndex(i => i.KeyName)
            .HasDatabaseName("IX_IntegrationKeys_KeyName");

        builder.HasIndex(i => i.CreatedAt)
            .HasDatabaseName("IX_IntegrationKeys_CreatedAt");
    }
}

