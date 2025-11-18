using CRM.Domain.Entities;
using CRM.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.EntityConfigurations
{
    public class RefundEntityConfiguration : IEntityTypeConfiguration<Refund>
    {
        public void Configure(EntityTypeBuilder<Refund> builder)
        {
            builder.ToTable("Refunds");
            builder.HasKey(x => x.RefundId);

            builder.Property(x => x.PaymentId)
                .IsRequired();

            builder.Property(x => x.QuotationId)
                .IsRequired();

            builder.Property(x => x.RefundAmount)
                .IsRequired()
                .HasColumnType("decimal(12,2)");

            builder.Property(x => x.RefundReason)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(x => x.RefundReasonCode)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(x => x.RequestedByUserId)
                .IsRequired();

            builder.Property(x => x.ApprovedByUserId)
                .IsRequired(false);

            builder.Property(x => x.RefundStatus)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(x => x.PaymentGatewayReference)
                .HasMaxLength(255);

            builder.Property(x => x.ApprovalLevel)
                .HasMaxLength(50);

            builder.Property(x => x.Comments)
                .HasColumnType("text");

            builder.Property(x => x.FailureReason)
                .HasColumnType("text");

            builder.Property(x => x.RequestDate)
                .IsRequired()
                .HasDefaultValueSql("NOW()");

            builder.Property(x => x.ApprovalDate)
                .IsRequired(false);

            builder.Property(x => x.CompletedDate)
                .IsRequired(false);

            builder.Property(x => x.ReversedDate)
                .IsRequired(false);

            builder.Property(x => x.ReversedReason)
                .HasMaxLength(500);

            builder.Property(x => x.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()");

            builder.Property(x => x.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()");

            // Check constraints
            builder.HasCheckConstraint("CK_Refunds_Amount", "\"RefundAmount\" > 0");
            builder.HasCheckConstraint("CK_Refunds_Status",
                "\"RefundStatus\" IN ('Pending', 'Approved', 'Processing', 'Completed', 'Failed', 'Reversed')");
            builder.HasCheckConstraint("CK_Refunds_ReasonCode",
                "\"RefundReasonCode\" IN ('CLIENT_REQUEST', 'ERROR', 'DISCOUNT_ADJUSTMENT', 'CANCELLATION', 'DUPLICATE_PAYMENT', 'OTHER')");

            // Relationships
            builder.HasOne(x => x.Payment)
                .WithMany()
                .HasForeignKey(x => x.PaymentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Quotation)
                .WithMany()
                .HasForeignKey(x => x.QuotationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.RequestedByUser)
                .WithMany()
                .HasForeignKey(x => x.RequestedByUserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.ApprovedByUser)
                .WithMany()
                .HasForeignKey(x => x.ApprovedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(x => x.Timeline)
                .WithOne(t => t.Refund)
                .HasForeignKey(t => t.RefundId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(x => x.PaymentId)
                .HasDatabaseName("IX_Refunds_PaymentId");

            builder.HasIndex(x => x.QuotationId)
                .HasDatabaseName("IX_Refunds_QuotationId");

            builder.HasIndex(x => x.RequestedByUserId)
                .HasDatabaseName("IX_Refunds_RequestedByUserId");

            builder.HasIndex(x => x.RefundStatus)
                .HasDatabaseName("IX_Refunds_Status");

            builder.HasIndex(x => x.RequestDate)
                .HasDatabaseName("IX_Refunds_RequestDate");

            builder.HasIndex(x => x.ApprovedByUserId)
                .HasDatabaseName("IX_Refunds_ApprovedByUserId");
        }
    }
}

