using CRM.Domain.Entities;
using CRM.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.EntityConfigurations;

public class NotificationDispatchAttemptEntityConfiguration : IEntityTypeConfiguration<NotificationDispatchAttempt>
{
    public void Configure(EntityTypeBuilder<NotificationDispatchAttempt> builder)
    {
        builder.ToTable("NotificationDispatchAttempts");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();
            
        builder.Property(x => x.NotificationId)
            .IsRequired();
            
        builder.Property(x => x.Channel)
            .HasConversion<int>()
            .IsRequired();
            
        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();
            
        builder.Property(x => x.AttemptedAt)
            .IsRequired();
            
        builder.Property(x => x.CompletedAt)
            .IsRequired(false);
            
        builder.Property(x => x.DeliveredAt)
            .IsRequired(false);
            
        builder.Property(x => x.ErrorMessage)
            .HasMaxLength(2000)
            .IsRequired(false);
            
        builder.Property(x => x.ErrorDetails)
            .HasMaxLength(4000)
            .IsRequired(false);
            
        builder.Property(x => x.ExternalReference)
            .HasMaxLength(500)
            .IsRequired(false);
            
        builder.Property(x => x.ExternalId)
            .HasMaxLength(500)
            .IsRequired(false);
            
        builder.Property(x => x.RetryCount)
            .IsRequired()
            .HasDefaultValue(0);
            
        builder.Property(x => x.AttemptNumber)
            .IsRequired()
            .HasDefaultValue(1);
            
        builder.Property(x => x.NextRetryAt)
            .IsRequired(false);
            
        builder.Property(x => x.NotificationTemplateId)
            .IsRequired(false);
        
        // Relationships
        builder.HasOne(x => x.Notification)
            .WithMany()
            .HasForeignKey(x => x.NotificationId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(x => x.NotificationTemplate)
            .WithMany(t => t.DispatchAttempts)
            .HasForeignKey(x => x.NotificationTemplateId)
            .OnDelete(DeleteBehavior.SetNull);
        
        // Indexes
        builder.HasIndex(x => x.NotificationId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.Channel);
        builder.HasIndex(x => x.AttemptedAt);
        builder.HasIndex(x => x.DeliveredAt);
        builder.HasIndex(x => x.AttemptNumber);
        builder.HasIndex(x => x.NotificationTemplateId);
        builder.HasIndex(x => new { x.Status, x.NextRetryAt });
    }
}