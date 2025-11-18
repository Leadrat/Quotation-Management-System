using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.EntityConfigurations;

public class SupportedLanguageEntityConfiguration : IEntityTypeConfiguration<SupportedLanguage>
{
    public void Configure(EntityTypeBuilder<SupportedLanguage> builder)
    {
        builder.ToTable("SupportedLanguages");

        builder.HasKey(l => l.LanguageCode);

        builder.Property(l => l.LanguageCode)
            .HasMaxLength(5)
            .IsRequired();

        builder.Property(l => l.DisplayName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(l => l.DisplayNameEn)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(l => l.NativeName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(l => l.IsRTL)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(l => l.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(l => l.FlagIcon)
            .HasMaxLength(50);

        builder.Property(l => l.CreatedAt)
            .IsRequired();

        builder.Property(l => l.UpdatedAt)
            .IsRequired();

        builder.HasIndex(l => l.IsActive);
    }
}

