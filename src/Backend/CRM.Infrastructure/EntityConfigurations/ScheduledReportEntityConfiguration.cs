using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.EntityConfigurations
{
    public class ScheduledReportEntityConfiguration : IEntityTypeConfiguration<ScheduledReport>
    {
        public void Configure(EntityTypeBuilder<ScheduledReport> builder)
        {
            builder.ToTable("ScheduledReports");
            builder.HasKey(x => x.ReportId);

            builder.Property(x => x.CreatedByUserId)
                .IsRequired();

            builder.Property(x => x.ReportName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.ReportType)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.ReportConfig)
                .IsRequired()
                .HasColumnType("jsonb");

            builder.Property(x => x.RecurrencePattern)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(x => x.EmailRecipients)
                .IsRequired();

            builder.Property(x => x.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(x => x.LastSentAt)
                .IsRequired(false);

            builder.Property(x => x.NextScheduledAt)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()");

            builder.Property(x => x.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()");

            // Check constraint for RecurrencePattern
            builder.HasCheckConstraint("CK_ScheduledReports_RecurrencePattern",
                "\"RecurrencePattern\" IN ('daily', 'weekly', 'monthly')");

            // Relationships
            builder.HasOne(x => x.CreatedByUser)
                .WithMany()
                .HasForeignKey(x => x.CreatedByUserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(x => x.CreatedByUserId)
                .HasDatabaseName("IX_ScheduledReports_CreatedByUserId");

            builder.HasIndex(x => new { x.IsActive, x.NextScheduledAt })
                .HasDatabaseName("IX_ScheduledReports_IsActive_NextScheduledAt");
        }
    }
}

