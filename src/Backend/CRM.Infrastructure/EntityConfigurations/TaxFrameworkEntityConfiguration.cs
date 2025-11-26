using CRM.Domain.Entities;
using CRM.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.EntityConfigurations
{
    public class TaxFrameworkEntityConfiguration : IEntityTypeConfiguration<TaxFramework>
    {
        public void Configure(EntityTypeBuilder<TaxFramework> builder)
        {
            builder.ToTable("TaxFrameworks");
            builder.HasKey(x => x.TaxFrameworkId);

            builder.Property(x => x.CountryId)
                .IsRequired();

            builder.Property(x => x.FrameworkName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.FrameworkType)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(x => x.Description);

            builder.Property(x => x.TaxComponents)
                .IsRequired()
                .HasColumnType("jsonb");

            builder.Property(x => x.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.UpdatedAt)
                .IsRequired();

            builder.Property(x => x.DeletedAt);

            // Indexes
            builder.HasIndex(x => x.CountryId)
                .IsUnique()
                .HasFilter("\"DeletedAt\" IS NULL");

            builder.HasIndex(x => x.FrameworkType);

            builder.HasIndex(x => x.IsActive);

            // GIN index for JSONB column
            builder.HasIndex(x => x.TaxComponents)
                .HasMethod("gin");

            // Relationships
            builder.HasOne(x => x.Country)
                .WithOne(x => x.TaxFramework)
                .HasForeignKey<TaxFramework>(x => x.CountryId)
                .OnDelete(DeleteBehavior.Cascade);

            // TaxRates relationship
            builder.HasMany<TaxRate>()
                .WithOne()
                .HasForeignKey("TaxFrameworkId")
                .OnDelete(DeleteBehavior.Cascade);

            // Query filter for soft delete
            builder.HasQueryFilter(x => x.DeletedAt == null);
        }
    }
}

