using System;
using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.EntityConfigurations
{
    public class SuspiciousActivityFlagEntityConfiguration : IEntityTypeConfiguration<SuspiciousActivityFlag>
    {
        public void Configure(EntityTypeBuilder<SuspiciousActivityFlag> builder)
        {
            builder.ToTable("SuspiciousActivityFlags");
            builder.HasKey(x => x.FlagId);

            builder.Property(x => x.Score).IsRequired();
            builder.Property(x => x.Status)
                   .HasMaxLength(32)
                   .HasDefaultValue("OPEN")
                   .IsRequired();

            builder.Property(x => x.Reasons)
                   .HasColumnType("text[]");

            builder.Property(x => x.Metadata)
                   .HasColumnType("jsonb")
                   .HasDefaultValue("{}");

            builder.Property(x => x.DetectedAt).IsRequired();

            builder.HasOne(x => x.History)
                   .WithMany(h => h.Flags)
                   .HasForeignKey(x => x.HistoryId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => new { x.Status, x.DetectedAt }).HasDatabaseName("IX_SuspiciousActivityFlags_Status_DetectedAt");
            builder.HasIndex(x => new { x.ClientId, x.DetectedAt }).HasDatabaseName("IX_SuspiciousActivityFlags_Client_DetectedAt");
        }
    }
}

