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

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.UpdatedAt)
                .IsRequired();

            // Relationships
            builder.HasOne(x => x.Quotation)
                .WithMany(x => x.LineItems)
                .HasForeignKey(x => x.QuotationId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(x => x.QuotationId);

            builder.HasIndex(x => new { x.QuotationId, x.SequenceNumber });
        }
    }
}

