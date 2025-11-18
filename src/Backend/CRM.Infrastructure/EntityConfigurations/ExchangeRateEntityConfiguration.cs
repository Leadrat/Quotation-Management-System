using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.EntityConfigurations;

public class ExchangeRateEntityConfiguration : IEntityTypeConfiguration<ExchangeRate>
{
    public void Configure(EntityTypeBuilder<ExchangeRate> builder)
    {
        builder.ToTable("ExchangeRates");

        builder.HasKey(e => e.ExchangeRateId);

        builder.Property(e => e.ExchangeRateId)
            .IsRequired();

        builder.Property(e => e.FromCurrencyCode)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(e => e.ToCurrencyCode)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(e => e.Rate)
            .HasPrecision(18, 6)
            .IsRequired();

        builder.Property(e => e.EffectiveDate)
            .IsRequired();

        builder.Property(e => e.ExpiryDate)
            .IsRequired(false);

        builder.Property(e => e.Source)
            .HasMaxLength(50);

        builder.Property(e => e.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.UpdatedAt)
            .IsRequired();

        builder.HasIndex(e => new { e.FromCurrencyCode, e.ToCurrencyCode, e.EffectiveDate })
            .IsUnique();

        builder.HasIndex(e => e.FromCurrencyCode);
        builder.HasIndex(e => e.ToCurrencyCode);
        builder.HasIndex(e => e.EffectiveDate);
        builder.HasIndex(e => e.IsActive);

        builder.HasOne(e => e.FromCurrency)
            .WithMany()
            .HasForeignKey(e => e.FromCurrencyCode)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ToCurrency)
            .WithMany()
            .HasForeignKey(e => e.ToCurrencyCode)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.CreatedByUser)
            .WithMany()
            .HasForeignKey(e => e.CreatedByUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

