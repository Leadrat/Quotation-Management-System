using CRM.Domain.Entities;
using CRM.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.EntityConfigurations
{
    public class CountryEntityConfiguration : IEntityTypeConfiguration<Country>
    {
        public void Configure(EntityTypeBuilder<Country> builder)
        {
            builder.ToTable("Countries");
            builder.HasKey(x => x.CountryId);

            builder.Property(x => x.CountryName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.CountryCode)
                .IsRequired()
                .HasMaxLength(2);

            builder.Property(x => x.TaxFrameworkType)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(x => x.DefaultCurrency)
                .IsRequired()
                .HasMaxLength(3);

            builder.Property(x => x.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(x => x.IsDefault)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.UpdatedAt)
                .IsRequired();

            builder.Property(x => x.DeletedAt);

            // Indexes
            builder.HasIndex(x => x.CountryCode)
                .IsUnique()
                .HasFilter("\"DeletedAt\" IS NULL");

            builder.HasIndex(x => x.CountryName)
                .IsUnique()
                .HasFilter("\"DeletedAt\" IS NULL");

            builder.HasIndex(x => x.IsActive);

            builder.HasIndex(x => x.IsDefault);

            // Relationships
            builder.HasMany<Jurisdiction>()
                .WithOne()
                .HasForeignKey("CountryId")
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.TaxFramework)
                .WithOne(x => x.Country)
                .HasForeignKey<TaxFramework>(x => x.CountryId)
                .OnDelete(DeleteBehavior.Cascade);

            // Note: Client relationship is configured in ClientEntityConfiguration

            // Query filter for soft delete
            builder.HasQueryFilter(x => x.DeletedAt == null);
        }
    }
}

