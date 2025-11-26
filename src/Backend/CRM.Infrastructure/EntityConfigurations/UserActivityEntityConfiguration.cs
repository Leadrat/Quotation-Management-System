using CRM.Domain.UserManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.EntityConfigurations;

public class UserActivityEntityConfiguration : IEntityTypeConfiguration<UserActivity>
{
    public void Configure(EntityTypeBuilder<UserActivity> builder)
    {
        builder.ToTable("UserActivities");
        builder.HasKey(ua => ua.ActivityId);

        builder.Property(ua => ua.ActionType).IsRequired().HasMaxLength(100);
        builder.Property(ua => ua.EntityType).HasMaxLength(50);
        builder.Property(ua => ua.IpAddress).IsRequired().HasMaxLength(45);
        builder.Property(ua => ua.UserAgent).HasColumnType("text");
        builder.Property(ua => ua.Timestamp).IsRequired();

        builder.HasOne(ua => ua.User)
            .WithMany()
            .HasForeignKey(ua => ua.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(ua => new { ua.UserId, ua.Timestamp });
        builder.HasIndex(ua => new { ua.ActionType, ua.Timestamp });
        builder.HasIndex(ua => ua.EntityType);
        builder.HasIndex(ua => ua.Timestamp);
    }
}

