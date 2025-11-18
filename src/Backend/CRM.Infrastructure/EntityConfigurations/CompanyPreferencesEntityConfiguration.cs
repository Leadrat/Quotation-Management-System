using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.EntityConfigurations;

public class CompanyPreferencesEntityConfiguration : IEntityTypeConfiguration<CompanyPreferences>
{
    public void Configure(EntityTypeBuilder<CompanyPreferences> builder)
    {
        builder.ToTable("CompanyPreferences");

        builder.HasKey(c => c.CompanyId);

        builder.Property(c => c.CompanyId)
            .IsRequired();

        builder.Property(c => c.DefaultLanguageCode)
            .HasMaxLength(5)
            .IsRequired()
            .HasDefaultValue("en");

        builder.Property(c => c.DefaultCurrencyCode)
            .HasMaxLength(3)
            .IsRequired()
            .HasDefaultValue("INR");

        builder.Property(c => c.DateFormat)
            .HasMaxLength(20)
            .IsRequired()
            .HasDefaultValue("dd/MM/yyyy");

        builder.Property(c => c.TimeFormat)
            .HasMaxLength(10)
            .IsRequired()
            .HasDefaultValue("24h");

        builder.Property(c => c.NumberFormat)
            .HasMaxLength(50)
            .IsRequired()
            .HasDefaultValue("en-IN");

        builder.Property(c => c.Timezone)
            .HasMaxLength(50);

        builder.Property(c => c.FirstDayOfWeek)
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .IsRequired();

        builder.HasIndex(c => c.DefaultCurrencyCode);

        // Note: Company entity not yet created - relationship will be added when Company entity exists
        // For now, CompanyId is just a Guid property without a foreign key constraint

        builder.HasOne(c => c.DefaultCurrency)
            .WithMany()
            .HasForeignKey(c => c.DefaultCurrencyCode)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.DefaultLanguage)
            .WithMany()
            .HasForeignKey(c => c.DefaultLanguageCode)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.UpdatedByUser)
            .WithMany()
            .HasForeignKey(c => c.UpdatedByUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

