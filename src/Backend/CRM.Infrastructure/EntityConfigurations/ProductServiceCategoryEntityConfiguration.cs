using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.EntityConfigurations
{
    public class ProductServiceCategoryEntityConfiguration : IEntityTypeConfiguration<ProductServiceCategory>
    {
        public void Configure(EntityTypeBuilder<ProductServiceCategory> builder)
        {
            builder.ToTable("ProductServiceCategories");
            builder.HasKey(x => x.CategoryId);

            builder.Property(x => x.CategoryName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.CategoryCode)
                .HasMaxLength(20);

            builder.Property(x => x.Description)
                .HasMaxLength(500);

            builder.Property(x => x.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.UpdatedAt)
                .IsRequired();

            builder.Property(x => x.DeletedAt);

            // Indexes
            builder.HasIndex(x => x.CategoryName)
                .IsUnique()
                .HasFilter("\"DeletedAt\" IS NULL");

            builder.HasIndex(x => x.CategoryCode)
                .IsUnique()
                .HasFilter("\"CategoryCode\" IS NOT NULL AND \"DeletedAt\" IS NULL");

            builder.HasIndex(x => x.IsActive);

            // Query filter for soft delete
            builder.HasQueryFilter(x => x.DeletedAt == null);
        }
    }
}

