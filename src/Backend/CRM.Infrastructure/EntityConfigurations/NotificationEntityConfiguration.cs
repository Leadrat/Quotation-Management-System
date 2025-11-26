using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.EntityConfigurations;

public class NotificationEntityConfiguration : IEntityTypeConfiguration<UserNotification>
{
    public void Configure(EntityTypeBuilder<UserNotification> builder)
    {
        builder.ToTable("Notifications");
        
        builder.HasKey(n => n.NotificationId);
        
        builder.Property(n => n.NotificationId)
            .HasDefaultValueSql("gen_random_uuid()");
            
        builder.Property(n => n.UserId)
            .IsRequired();
            
        builder.Property(n => n.RecipientUserId)
            .IsRequired();
            
        builder.Property(n => n.NotificationTypeId)
            .IsRequired();
            
        builder.Property(n => n.EventType)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(n => n.Title)
            .IsRequired()
            .HasMaxLength(255);
            
        builder.Property(n => n.Message)
            .IsRequired()
            .HasMaxLength(10000);
            
        builder.Property(n => n.RelatedEntityType)
            .HasMaxLength(100);
            
        builder.Property(n => n.IsRead)
            .IsRequired()
            .HasDefaultValue(false);
            
        builder.Property(n => n.IsArchived)
            .IsRequired()
            .HasDefaultValue(false);
            
        builder.Property(n => n.SentVia)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(n => n.DeliveredChannels)
            .HasMaxLength(500);
            
        builder.Property(n => n.DeliveryStatus)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("PENDING");
            
        builder.Property(n => n.Meta)
            .HasMaxLength(4000);
            
        builder.Property(n => n.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");
            
        builder.Property(n => n.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");
            
        // Relationships
        builder.HasOne(n => n.User)
            .WithMany()
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne(n => n.NotificationType)
            .WithMany(nt => nt.Notifications)
            .HasForeignKey(n => n.NotificationTypeId)
            .OnDelete(DeleteBehavior.Restrict);
            
        // Indexes
        builder.HasIndex(n => n.UserId)
            .HasDatabaseName("IX_Notifications_UserId");
            
        builder.HasIndex(n => n.RecipientUserId)
            .HasDatabaseName("IX_Notifications_RecipientUserId");
            
        builder.HasIndex(n => n.NotificationTypeId)
            .HasDatabaseName("IX_Notifications_NotificationTypeId");
            
        builder.HasIndex(n => n.CreatedAt)
            .HasDatabaseName("IX_Notifications_CreatedAt");
            
        builder.HasIndex(n => n.IsRead)
            .HasDatabaseName("IX_Notifications_IsRead");
            
        builder.HasIndex(n => n.IsArchived)
            .HasDatabaseName("IX_Notifications_IsArchived");
            
        builder.HasIndex(n => new { n.UserId, n.IsRead })
            .HasDatabaseName("IX_Notifications_UserId_IsRead");
            
        builder.HasIndex(n => new { n.RecipientUserId, n.IsRead })
            .HasDatabaseName("IX_Notifications_RecipientUserId_IsRead");
            
        builder.HasIndex(n => new { n.RecipientUserId, n.IsArchived })
            .HasDatabaseName("IX_Notifications_RecipientUserId_IsArchived");
            
        builder.HasIndex(n => new { n.UserId, n.CreatedAt })
            .HasDatabaseName("IX_Notifications_UserId_CreatedAt");
            
        builder.HasIndex(n => n.RelatedEntityId)
            .HasDatabaseName("IX_Notifications_RelatedEntityId");
    }
}