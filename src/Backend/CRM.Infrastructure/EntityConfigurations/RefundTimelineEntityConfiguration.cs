using CRM.Domain.Entities;
using CRM.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.EntityConfigurations
{
    public class RefundTimelineEntityConfiguration : IEntityTypeConfiguration<RefundTimeline>
    {
        public void Configure(EntityTypeBuilder<RefundTimeline> builder)
        {
            builder.ToTable("RefundTimeline");
            builder.HasKey(x => x.TimelineId);

            builder.Property(x => x.RefundId)
                .IsRequired();

            builder.Property(x => x.EventType)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(x => x.ActedByUserId)
                .IsRequired();

            builder.Property(x => x.Comments)
                .HasColumnType("text");

            builder.Property(x => x.EventDate)
                .IsRequired()
                .HasDefaultValueSql("NOW()");

            builder.Property(x => x.IpAddress)
                .HasMaxLength(50);

            // Check constraint
            builder.HasCheckConstraint("CK_RefundTimeline_EventType",
                "\"EventType\" IN ('REQUESTED', 'APPROVED', 'REJECTED', 'PROCESSING', 'COMPLETED', 'FAILED', 'REVERSED')");

            // Relationships
            builder.HasOne(x => x.Refund)
                .WithMany(r => r.Timeline)
                .HasForeignKey(x => x.RefundId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.ActedByUser)
                .WithMany()
                .HasForeignKey(x => x.ActedByUserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(x => x.RefundId)
                .HasDatabaseName("IX_RefundTimeline_RefundId");

            builder.HasIndex(x => x.EventDate)
                .HasDatabaseName("IX_RefundTimeline_EventDate");

            builder.HasIndex(x => x.ActedByUserId)
                .HasDatabaseName("IX_RefundTimeline_ActedByUserId");
        }
    }
}

