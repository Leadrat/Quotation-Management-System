using CRM.Domain.Admin;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.EntityConfigurations;

public class AuditLogEntityConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLog");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.ActionType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.Entity)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.EntityId)
            .IsRequired(false);

        builder.Property(a => a.PerformedBy)
            .IsRequired();

        builder.Property(a => a.IpAddress)
            .HasMaxLength(45); // IPv6 max length

        builder.Property(a => a.Timestamp)
            .IsRequired();

        builder.Property(a => a.Changes)
            .HasColumnType("jsonb")
            .IsRequired(false);

        // Foreign key to Users
        builder.HasOne(a => a.PerformedByUser)
            .WithMany()
            .HasForeignKey(a => a.PerformedBy)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes for performance
        builder.HasIndex(a => a.PerformedBy)
            .HasDatabaseName("IX_AuditLog_PerformedBy");

        builder.HasIndex(a => a.Timestamp)
            .HasDatabaseName("IX_AuditLog_Timestamp");

        builder.HasIndex(a => a.Entity)
            .HasDatabaseName("IX_AuditLog_Entity");

        builder.HasIndex(a => a.ActionType)
            .HasDatabaseName("IX_AuditLog_ActionType");

        builder.HasIndex(a => a.EntityId)
            .HasDatabaseName("IX_AuditLog_EntityId")
            .HasFilter("\"EntityId\" IS NOT NULL");
    }
}

