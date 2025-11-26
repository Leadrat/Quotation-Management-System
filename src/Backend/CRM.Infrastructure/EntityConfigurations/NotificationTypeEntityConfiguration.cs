using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.EntityConfigurations;

public class NotificationTypeEntityConfiguration : IEntityTypeConfiguration<NotificationType>
{
    public void Configure(EntityTypeBuilder<NotificationType> builder)
    {
        builder.ToTable("NotificationTypes");
        
        builder.HasKey(nt => nt.NotificationTypeId);
        
        builder.Property(nt => nt.NotificationTypeId)
            .HasDefaultValueSql("gen_random_uuid()");
            
        builder.Property(nt => nt.TypeName)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(nt => nt.Description)
            .HasMaxLength(1000);
            
        builder.Property(nt => nt.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");
            
        builder.Property(nt => nt.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");
            
        // Unique constraint
        builder.HasIndex(nt => nt.TypeName)
            .IsUnique()
            .HasDatabaseName("UQ_NotificationTypes_TypeName");
    }
}