using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.EntityConfigurations
{
    public class QuotationLineItemEntityConfiguration : IEntityTypeConfiguration<QuotationLineItem>
    {
        public void Configure(EntityTypeBuilder<QuotationLineItem> builder)
        {
            builder.ToTable("QuotationLineItems");
            builder.HasKey(x => x.LineItemId);

            builder.Property(x => x.SequenceNumber)
                .IsRequired();

            builder.Property(x => x.ItemName)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(x => x.Description)
                .HasMaxLength(1000);

            builder.Property(x => x.Quantity)
                .IsRequired()
                .HasColumnType("decimal(10,2)");

            builder.Property(x => x.UnitRate)
                .IsRequired()
                .HasColumnType("decimal(12,2)");

            builder.Property(x => x.Amount)
                .IsRequired()
                .HasColumnType("decimal(12,2)");

            builder.Property(x => x.ProductServiceCategoryId);

            // Product catalog integration fields
            builder.Property(x => x.ProductId);
            builder.Property(x => x.BillingCycle)
                .HasConversion<int>();
            builder.Property(x => x.Hours)
                .HasColumnType("decimal(10,2)");
            builder.Property(x => x.OriginalProductPrice)
                .HasColumnType("decimal(18,2)");
            builder.Property(x => x.DiscountAmount)
                .HasColumnType("decimal(18,2)");
            builder.Property(x => x.TaxCategoryId);

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.UpdatedAt)
                .IsRequired();

            // Relationships
            builder.HasOne(x => x.Quotation)
                .WithMany(x => x.LineItems)
                .HasForeignKey(x => x.QuotationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne<ProductServiceCategory>()
                .WithMany()
                .HasForeignKey(x => x.ProductServiceCategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(x => x.TaxCategory)
                .WithMany()
                .HasForeignKey(x => x.TaxCategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            // Indexes
            builder.HasIndex(x => x.QuotationId);
            builder.HasIndex(x => x.ProductServiceCategoryId);
            builder.HasIndex(x => x.ProductId);
            builder.HasIndex(x => x.TaxCategoryId);

            builder.HasIndex(x => new { x.QuotationId, x.SequenceNumber });
        }
    }
}

