using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.EntityConfigurations
{
    public class QuotationTemplateLineItemEntityConfiguration : IEntityTypeConfiguration<QuotationTemplateLineItem>
    {
        public void Configure(EntityTypeBuilder<QuotationTemplateLineItem> builder)
        {
            builder.ToTable("QuotationTemplateLineItems");
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

            // Relationships
            builder.HasOne(x => x.Template)
                .WithMany(x => x.LineItems)
                .HasForeignKey(x => x.TemplateId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(x => x.TemplateId);
            builder.HasIndex(x => new { x.TemplateId, x.SequenceNumber });
        }
    }
}

