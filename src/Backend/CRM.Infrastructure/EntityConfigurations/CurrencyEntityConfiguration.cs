using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.EntityConfigurations;

public class CurrencyEntityConfiguration : IEntityTypeConfiguration<Currency>
{
    public void Configure(EntityTypeBuilder<Currency> builder)
    {
        builder.ToTable("Currencies");

        builder.HasKey(c => c.CurrencyCode);

        builder.Property(c => c.CurrencyCode)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(c => c.DisplayName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.Symbol)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(c => c.DecimalPlaces)
            .IsRequired()
            .HasDefaultValue(2);

        builder.Property(c => c.IsDefault)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(c => c.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .IsRequired();

        builder.HasIndex(c => c.IsDefault)
            .HasFilter("\"IsDefault\" = true");

        builder.HasIndex(c => c.IsActive);

        builder.HasOne(c => c.CreatedByUser)
            .WithMany()
            .HasForeignKey(c => c.CreatedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(c => c.UpdatedByUser)
            .WithMany()
            .HasForeignKey(c => c.UpdatedByUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

