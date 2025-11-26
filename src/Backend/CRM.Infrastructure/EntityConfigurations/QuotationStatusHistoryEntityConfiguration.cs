using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CRM.Domain.Entities;

namespace CRM.Infrastructure.EntityConfigurations
{
    public class QuotationStatusHistoryEntityConfiguration : IEntityTypeConfiguration<QuotationStatusHistory>
    {
        public void Configure(EntityTypeBuilder<QuotationStatusHistory> builder)
        {
            builder.ToTable("QuotationStatusHistory");

            builder.HasKey(x => x.HistoryId);

            builder.Property(x => x.HistoryId)
                .ValueGeneratedOnAdd();

            builder.Property(x => x.QuotationId)
                .IsRequired();

            builder.Property(x => x.PreviousStatus)
                .HasMaxLength(50);

            builder.Property(x => x.NewStatus)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.ChangedByUserId);

            builder.Property(x => x.Reason)
                .HasMaxLength(500);

            builder.Property(x => x.ChangedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(x => x.IpAddress)
                .HasMaxLength(50);

            // Indexes
            builder.HasIndex(x => x.QuotationId)
                .HasDatabaseName("IX_QuotationStatusHistory_QuotationId");

            builder.HasIndex(x => x.ChangedByUserId)
                .HasDatabaseName("IX_QuotationStatusHistory_ChangedByUserId");

            builder.HasIndex(x => new { x.QuotationId, x.ChangedAt })
                .HasDatabaseName("IX_QuotationStatusHistory_QuotationId_ChangedAt");

            // Foreign keys
            builder.HasOne(x => x.Quotation)
                .WithMany()
                .HasForeignKey(x => x.QuotationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ChangedByUser)
                .WithMany()
                .HasForeignKey(x => x.ChangedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

