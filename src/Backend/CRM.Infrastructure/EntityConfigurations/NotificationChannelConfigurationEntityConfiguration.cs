using CRM.Domain.Entities;
using CRM.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.EntityConfigurations;

public class NotificationChannelConfigurationEntityConfiguration : IEntityTypeConfiguration<NotificationChannelConfiguration>
{
    public void Configure(EntityTypeBuilder<NotificationChannelConfiguration> builder)
    {
        builder.ToTable("NotificationChannelConfigurations");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();
            
        builder.Property(x => x.Channel)
            .HasConversion<int>()
            .IsRequired();
            
        builder.Property(x => x.IsEnabled)
            .IsRequired()
            .HasDefaultValue(true);
            
        builder.Property(x => x.Configuration)
            .HasColumnType("jsonb")
            .IsRequired();
            
        builder.Property(x => x.MaxRetryAttempts)
            .IsRequired()
            .HasDefaultValue(3);
            
        builder.Property(x => x.RetryDelay)
            .IsRequired()
            .HasDefaultValue(TimeSpan.FromMinutes(5));
            
        builder.Property(x => x.CreatedAt)
            .IsRequired();
            
        builder.Property(x => x.UpdatedAt)
            .IsRequired();
        
        // Indexes
        builder.HasIndex(x => x.Channel)
            .IsUnique();
        builder.HasIndex(x => x.IsEnabled);
    }
}