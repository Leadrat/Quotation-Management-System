using CRM.Domain.Admin;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.EntityConfigurations;

public class NotificationSettingsEntityConfiguration : IEntityTypeConfiguration<NotificationSettings>
{
    public void Configure(EntityTypeBuilder<NotificationSettings> builder)
    {
        builder.ToTable("NotificationSettings");
        builder.HasKey(n => n.Id);

        builder.Property(n => n.BannerMessage)
            .HasColumnType("text");

        builder.Property(n => n.BannerType)
            .HasMaxLength(20); // "info", "warning", "error"

        builder.Property(n => n.IsVisible)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(n => n.UpdatedAt)
            .IsRequired();

        builder.Property(n => n.UpdatedBy)
            .IsRequired();

        // Foreign key
        builder.HasOne(n => n.UpdatedByUser)
            .WithMany()
            .HasForeignKey(n => n.UpdatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        // Single row constraint
        builder.HasIndex(n => n.Id)
            .IsUnique();
    }
}

