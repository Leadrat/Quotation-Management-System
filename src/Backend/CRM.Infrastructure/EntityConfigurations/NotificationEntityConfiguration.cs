using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.EntityConfigurations
{
    public class NotificationEntityConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.ToTable("Notifications");
            builder.HasKey(x => x.NotificationId);

            builder.Property(x => x.RecipientUserId)
                .IsRequired();

            builder.Property(x => x.RelatedEntityType)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.RelatedEntityId)
                .IsRequired();

            builder.Property(x => x.EventType)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.Message)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(x => x.IsRead)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(x => x.IsArchived)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(x => x.DeliveredChannels)
                .HasMaxLength(255);

            builder.Property(x => x.DeliveryStatus)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("SENT");

            builder.Property(x => x.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(x => x.ReadAt);

            builder.Property(x => x.ArchivedAt);

            builder.Property(x => x.Meta)
                .HasColumnType("jsonb");

            // Relationships
            builder.HasOne(x => x.RecipientUser)
                .WithMany()
                .HasForeignKey(x => x.RecipientUserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(x => x.RecipientUserId);
            builder.HasIndex(x => x.IsRead);
            builder.HasIndex(x => x.IsArchived);
            builder.HasIndex(x => new { x.RelatedEntityType, x.RelatedEntityId });
            builder.HasIndex(x => x.DeliveryStatus);
            builder.HasIndex(x => x.CreatedAt)
                .IsDescending();
            builder.HasIndex(x => new { x.RecipientUserId, x.IsRead })
                .HasFilter("\"IsRead\" = false");
        }
    }
}

