using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.EntityConfigurations
{
    public class ProductEntityConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("Products");
            builder.HasKey(x => x.ProductId);

            builder.Property(x => x.ProductName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.ProductType)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(x => x.Description)
                .HasMaxLength(2000);

            builder.Property(x => x.CategoryId);

            builder.Property(x => x.BasePricePerUserPerMonth)
                .HasColumnType("decimal(18,2)");

            builder.Property(x => x.BillingCycleMultipliers)
                .HasColumnType("jsonb");

            builder.Property(x => x.AddOnPricing)
                .HasColumnType("jsonb");

            builder.Property(x => x.CustomDevelopmentPricing)
                .HasColumnType("jsonb");

            builder.Property(x => x.Currency)
                .IsRequired()
                .HasMaxLength(3)
                .HasDefaultValue("USD");

            builder.Property(x => x.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.UpdatedAt)
                .IsRequired();

            // Relationships
            builder.HasOne(x => x.Category)
                .WithMany()
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(x => x.CreatedByUser)
                .WithMany()
                .HasForeignKey(x => x.CreatedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(x => x.UpdatedByUser)
                .WithMany()
                .HasForeignKey(x => x.UpdatedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Indexes
            builder.HasIndex(x => x.ProductType);
            builder.HasIndex(x => x.CategoryId);
            builder.HasIndex(x => x.IsActive);
            builder.HasIndex(x => x.ProductName);
            builder.HasIndex(x => x.CreatedAt);

            // Check constraints (will be added in migration)
        }
    }
}

