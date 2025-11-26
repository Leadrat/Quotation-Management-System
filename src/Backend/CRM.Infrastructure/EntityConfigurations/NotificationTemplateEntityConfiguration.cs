using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace CRM.Infrastructure.EntityConfigurations;

public class NotificationTemplateEntityConfiguration : IEntityTypeConfiguration<NotificationTemplate>
{
    public void Configure(EntityTypeBuilder<NotificationTemplate> builder)
    {
        builder.ToTable("NotificationTemplates");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TemplateKey)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.EventType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Channel)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.Subject)
            .HasMaxLength(500);

        builder.Property(x => x.BodyTemplate)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.RequiredVariables)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>())
            .HasColumnType("nvarchar(max)");

        // Indexes
        builder.HasIndex(x => new { x.TemplateKey, x.Channel })
            .IsUnique()
            .HasDatabaseName("IX_NotificationTemplates_TemplateKey_Channel");

        builder.HasIndex(x => x.EventType)
            .HasDatabaseName("IX_NotificationTemplates_EventType");

        builder.HasIndex(x => x.IsActive)
            .HasDatabaseName("IX_NotificationTemplates_IsActive");

        // Relationships
        builder.HasMany(x => x.DispatchAttempts)
            .WithOne()
            .HasForeignKey("NotificationTemplateId")
            .OnDelete(DeleteBehavior.Restrict);
    }
}