using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.EntityConfigurations;

public class UserEntityConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(u => u.UserId);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnType("citext");

        builder.Property(u => u.PasswordHash).IsRequired().HasMaxLength(255);
        builder.Property(u => u.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(u => u.LastName).IsRequired().HasMaxLength(100);
        builder.Property(u => u.Mobile).HasMaxLength(20);
        builder.Property(u => u.PhoneCode).HasMaxLength(5);
        builder.Property(u => u.IsActive).IsRequired();
        builder.Property(u => u.CreatedAt).IsRequired();
        builder.Property(u => u.UpdatedAt).IsRequired();

        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasIndex(u => u.RoleId);
        builder.HasIndex(u => u.ReportingManagerId);
        builder.HasIndex(u => u.IsActive);
        builder.HasIndex(u => u.CreatedAt);
        builder.HasIndex(u => u.UpdatedAt);
        builder.HasIndex(u => u.DeletedAt);

        builder.HasOne(u => u.Role)
            .WithMany()
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(u => u.ReportingManager)
            .WithMany(m => m.DirectReports)
            .HasForeignKey(u => u.ReportingManagerId)
            .OnDelete(DeleteBehavior.SetNull);

        // Enhanced profile properties
        builder.Property(u => u.AvatarUrl).HasMaxLength(500);
        builder.Property(u => u.Bio).HasMaxLength(500);
        builder.Property(u => u.LinkedInUrl).HasMaxLength(255);
        builder.Property(u => u.TwitterUrl).HasMaxLength(255);
        builder.Property(u => u.Skills).HasColumnType("jsonb");
        builder.Property(u => u.OutOfOfficeStatus).IsRequired().HasDefaultValue(false);
        builder.Property(u => u.OutOfOfficeMessage).HasMaxLength(1000);
        builder.Property(u => u.PresenceStatus)
            .HasConversion<int>()
            .IsRequired();

        builder.HasOne(u => u.DelegateUser)
            .WithMany(d => d.DelegatedToMe)
            .HasForeignKey(u => u.DelegateUserId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes for new properties
        builder.HasIndex(u => u.DelegateUserId);
        builder.HasIndex(u => u.LastSeenAt);
        builder.HasIndex(u => u.PresenceStatus);
    }
}
