using CRM.Domain.Entities;
using CRM.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.EntityConfigurations
{
    public class DiscountApprovalEntityConfiguration : IEntityTypeConfiguration<DiscountApproval>
    {
        public void Configure(EntityTypeBuilder<DiscountApproval> builder)
        {
            builder.ToTable("DiscountApprovals");
            builder.HasKey(x => x.ApprovalId);

            builder.Property(x => x.QuotationId)
                .IsRequired();

            builder.Property(x => x.RequestedByUserId)
                .IsRequired();

            builder.Property(x => x.ApproverUserId);

            builder.Property(x => x.Status)
                .IsRequired()
                .HasConversion(
                    v => v.ToString(),
                    v => (ApprovalStatus)Enum.Parse(typeof(ApprovalStatus), v))
                .HasMaxLength(50);

            builder.Property(x => x.RequestDate)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(x => x.ApprovalDate);

            builder.Property(x => x.RejectionDate);

            builder.Property(x => x.CurrentDiscountPercentage)
                .IsRequired()
                .HasColumnType("decimal(5,2)");

            builder.Property(x => x.Threshold)
                .IsRequired()
                .HasColumnType("decimal(5,2)");

            builder.Property(x => x.ApprovalLevel)
                .IsRequired()
                .HasConversion(
                    v => v.ToString(),
                    v => (ApprovalLevel)Enum.Parse(typeof(ApprovalLevel), v))
                .HasMaxLength(50);

            builder.Property(x => x.Reason)
                .IsRequired()
                .HasColumnType("TEXT");

            builder.Property(x => x.Comments)
                .HasColumnType("TEXT");

            builder.Property(x => x.EscalatedToAdmin)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(x => x.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(x => x.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Relationships
            builder.HasOne(x => x.Quotation)
                .WithMany()
                .HasForeignKey(x => x.QuotationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.RequestedByUser)
                .WithMany()
                .HasForeignKey(x => x.RequestedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ApproverUser)
                .WithMany()
                .HasForeignKey(x => x.ApproverUserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Indexes
            builder.HasIndex(x => new { x.ApproverUserId, x.Status })
                .HasFilter("\"ApproverUserId\" IS NOT NULL");

            builder.HasIndex(x => x.QuotationId);

            builder.HasIndex(x => x.RequestedByUserId);

            builder.HasIndex(x => x.CurrentDiscountPercentage);

            builder.HasIndex(x => x.Status);

            builder.HasIndex(x => new { x.CreatedAt, x.Status });
        }
    }
}

