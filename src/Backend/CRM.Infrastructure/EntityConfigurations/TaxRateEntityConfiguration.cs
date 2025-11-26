using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.EntityConfigurations
{
    public class TaxRateEntityConfiguration : IEntityTypeConfiguration<TaxRate>
    {
        public void Configure(EntityTypeBuilder<TaxRate> builder)
        {
            builder.ToTable("TaxRates");
            builder.HasKey(x => x.TaxRateId);

            builder.Property(x => x.Rate)
                .IsRequired()
                .HasColumnType("decimal(5,2)")
                .HasColumnName("TaxRate"); // Map to database column name

            builder.Property(x => x.EffectiveFrom)
                .IsRequired();

            builder.Property(x => x.EffectiveTo);

            builder.Property(x => x.IsExempt)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(x => x.IsZeroRated)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(x => x.TaxComponents)
                .IsRequired()
                .HasColumnType("jsonb")
                .HasDefaultValue("[]");

            builder.Property(x => x.Description)
                .HasMaxLength(500);

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.UpdatedAt)
                .IsRequired();

            // Indexes
            builder.HasIndex(x => x.JurisdictionId);
            builder.HasIndex(x => x.TaxFrameworkId);
            builder.HasIndex(x => x.ProductServiceCategoryId);
            builder.HasIndex(x => new { x.EffectiveFrom, x.EffectiveTo });

            // Composite index for rate lookup query
            builder.HasIndex(x => new { x.JurisdictionId, x.ProductServiceCategoryId, x.EffectiveFrom, x.EffectiveTo })
                .HasDatabaseName("IX_TaxRates_Lookup");

            // Relationships
            builder.HasOne(x => x.Jurisdiction)
                .WithMany()
                .HasForeignKey(x => x.JurisdictionId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(x => x.TaxFramework)
                .WithMany()
                .HasForeignKey(x => x.TaxFrameworkId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.ProductServiceCategory)
                .WithMany()
                .HasForeignKey(x => x.ProductServiceCategoryId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}

