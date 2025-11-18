using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.EntityConfigurations
{
    public class PaymentEntityConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.ToTable("Payments");
            builder.HasKey(x => x.PaymentId);

            builder.Property(x => x.QuotationId)
                .IsRequired();

            builder.Property(x => x.PaymentGateway)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.PaymentReference)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(x => x.AmountPaid)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(x => x.Currency)
                .IsRequired()
                .HasMaxLength(3)
                .HasDefaultValue("INR");

            builder.Property(x => x.PaymentStatus)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(x => x.PaymentDate);

            builder.Property(x => x.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(x => x.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(x => x.FailureReason);

            builder.Property(x => x.IsRefundable)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(x => x.RefundAmount)
                .HasColumnType("decimal(18,2)");

            builder.Property(x => x.RefundReason);

            builder.Property(x => x.RefundDate);

            builder.Property(x => x.Metadata)
                .HasColumnType("jsonb");

            // Relationships
            builder.HasOne(x => x.Quotation)
                .WithMany()
                .HasForeignKey(x => x.QuotationId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(x => x.QuotationId);
            builder.HasIndex(x => x.PaymentReference)
                .IsUnique();
            builder.HasIndex(x => x.PaymentStatus);
            builder.HasIndex(x => x.PaymentDate);
        }
    }
}

