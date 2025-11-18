using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.EntityConfigurations;

public class LocalizationResourceEntityConfiguration : IEntityTypeConfiguration<LocalizationResource>
{
    public void Configure(EntityTypeBuilder<LocalizationResource> builder)
    {
        builder.ToTable("LocalizationResources");

        builder.HasKey(r => r.ResourceId);

        builder.Property(r => r.ResourceId)
            .IsRequired();

        builder.Property(r => r.LanguageCode)
            .HasMaxLength(5)
            .IsRequired();

        builder.Property(r => r.ResourceKey)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(r => r.ResourceValue)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(r => r.Category)
            .HasMaxLength(50);

        builder.Property(r => r.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(r => r.CreatedAt)
            .IsRequired();

        builder.Property(r => r.UpdatedAt)
            .IsRequired();

        builder.HasIndex(r => new { r.LanguageCode, r.ResourceKey })
            .IsUnique();

        builder.HasIndex(r => r.LanguageCode);
        builder.HasIndex(r => r.ResourceKey);
        builder.HasIndex(r => r.Category);
        builder.HasIndex(r => r.IsActive);

        builder.HasOne(r => r.Language)
            .WithMany()
            .HasForeignKey(r => r.LanguageCode)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.CreatedByUser)
            .WithMany()
            .HasForeignKey(r => r.CreatedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(r => r.UpdatedByUser)
            .WithMany()
            .HasForeignKey(r => r.UpdatedByUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

