using CRM.Domain.Entities;
using CRM.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.EntityConfigurations
{
    public class AdjustmentEntityConfiguration : IEntityTypeConfiguration<Adjustment>
    {
        public void Configure(EntityTypeBuilder<Adjustment> builder)
        {
            builder.ToTable("Adjustments");
            builder.HasKey(x => x.AdjustmentId);

            builder.Property(x => x.QuotationId)
                .IsRequired();

            builder.Property(x => x.AdjustmentType)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(x => x.OriginalAmount)
                .IsRequired()
                .HasColumnType("decimal(12,2)");

            builder.Property(x => x.AdjustedAmount)
                .IsRequired()
                .HasColumnType("decimal(12,2)");

            builder.Property(x => x.Reason)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(x => x.RequestedByUserId)
                .IsRequired();

            builder.Property(x => x.ApprovedByUserId)
                .IsRequired(false);

            builder.Property(x => x.Status)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(x => x.ApprovalLevel)
                .HasMaxLength(50);

            builder.Property(x => x.RequestDate)
                .IsRequired()
                .HasDefaultValueSql("NOW()");

            builder.Property(x => x.ApprovalDate)
                .IsRequired(false);

            builder.Property(x => x.AppliedDate)
                .IsRequired(false);

            builder.Property(x => x.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()");

            builder.Property(x => x.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()");

            // Check constraints
            builder.HasCheckConstraint("CK_Adjustments_Type",
                "\"AdjustmentType\" IN ('DISCOUNT_CHANGE', 'AMOUNT_CORRECTION', 'TAX_CORRECTION')");
            builder.HasCheckConstraint("CK_Adjustments_Status",
                "\"Status\" IN ('PENDING', 'APPROVED', 'REJECTED', 'APPLIED')");
            builder.HasCheckConstraint("CK_Adjustments_Amount", "\"AdjustedAmount\" > 0");

            // Relationships
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

            // Indexes
            builder.HasIndex(x => x.QuotationId)
                .HasDatabaseName("IX_Adjustments_QuotationId");

            builder.HasIndex(x => x.Status)
                .HasDatabaseName("IX_Adjustments_Status");

            builder.HasIndex(x => x.RequestDate)
                .HasDatabaseName("IX_Adjustments_RequestDate");

            builder.HasIndex(x => x.RequestedByUserId)
                .HasDatabaseName("IX_Adjustments_RequestedByUserId");
        }
    }
}

