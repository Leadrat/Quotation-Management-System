using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.EntityConfigurations
{
    public class AnalyticsMetricsSnapshotEntityConfiguration : IEntityTypeConfiguration<AnalyticsMetricsSnapshot>
    {
        public void Configure(EntityTypeBuilder<AnalyticsMetricsSnapshot> builder)
        {
            builder.ToTable("AnalyticsMetricsSnapshot");
            builder.HasKey(x => x.SnapshotId);

            builder.Property(x => x.MetricType)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.UserId)
                .IsRequired(false);

            builder.Property(x => x.MetricData)
                .IsRequired()
                .HasColumnType("jsonb");

            builder.Property(x => x.CalculatedAt)
                .IsRequired();

            builder.Property(x => x.PeriodDate)
                .IsRequired()
                .HasColumnType("date");

            builder.Property(x => x.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()");

            // Relationships
            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(x => new { x.MetricType, x.PeriodDate })
                .HasDatabaseName("IX_AnalyticsMetricsSnapshot_MetricType_PeriodDate");

            builder.HasIndex(x => x.UserId)
                .HasDatabaseName("IX_AnalyticsMetricsSnapshot_UserId");

            builder.HasIndex(x => x.CalculatedAt)
                .HasDatabaseName("IX_AnalyticsMetricsSnapshot_CalculatedAt");
        }
    }
}

