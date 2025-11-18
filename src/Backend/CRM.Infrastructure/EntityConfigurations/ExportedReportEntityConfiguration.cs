using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.EntityConfigurations
{
    public class ExportedReportEntityConfiguration : IEntityTypeConfiguration<ExportedReport>
    {
        public void Configure(EntityTypeBuilder<ExportedReport> builder)
        {
            builder.ToTable("ExportedReports");
            builder.HasKey(x => x.ExportId);

            builder.Property(x => x.CreatedByUserId)
                .IsRequired();

            builder.Property(x => x.ReportType)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.ExportFormat)
                .IsRequired()
                .HasMaxLength(10);

            builder.Property(x => x.FilePath)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(x => x.FileSize)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()");

            // Check constraint for ExportFormat
            builder.HasCheckConstraint("CK_ExportedReports_ExportFormat",
                "\"ExportFormat\" IN ('pdf', 'excel', 'csv')");

            // Relationships
            builder.HasOne(x => x.CreatedByUser)
                .WithMany()
                .HasForeignKey(x => x.CreatedByUserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(x => x.CreatedByUserId)
                .HasDatabaseName("IX_ExportedReports_CreatedByUserId");

            builder.HasIndex(x => x.CreatedAt)
                .HasDatabaseName("IX_ExportedReports_CreatedAt");
        }
    }
}

