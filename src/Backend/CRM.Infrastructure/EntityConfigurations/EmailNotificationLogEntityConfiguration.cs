using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.EntityConfigurations
{
    public class EmailNotificationLogEntityConfiguration : IEntityTypeConfiguration<EmailNotificationLog>
    {
        public void Configure(EntityTypeBuilder<EmailNotificationLog> builder)
        {
            builder.ToTable("EmailNotificationLog");
            builder.HasKey(x => x.LogId);

            builder.Property(x => x.NotificationId);

            builder.Property(x => x.RecipientEmail)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(x => x.EventType)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.Subject)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(x => x.SentAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(x => x.DeliveredAt);

            builder.Property(x => x.Status)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("SENT");

            builder.Property(x => x.ErrorMsg)
                .HasColumnType("TEXT");

            builder.Property(x => x.RetryCount)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(x => x.LastRetryAt);

            // Relationships
            builder.HasOne(x => x.Notification)
                .WithMany()
                .HasForeignKey(x => x.NotificationId)
                .OnDelete(DeleteBehavior.SetNull);

            // Indexes
            builder.HasIndex(x => x.NotificationId);
            builder.HasIndex(x => x.RecipientEmail);
            builder.HasIndex(x => x.EventType);
            builder.HasIndex(x => x.Status);
            builder.HasIndex(x => x.SentAt)
                .IsDescending();
            builder.HasIndex(x => x.Status)
                .HasFilter("\"Status\" IN ('FAILED', 'BOUNCED')");
        }
    }
}

