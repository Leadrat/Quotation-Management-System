using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.EntityConfigurations
{
    public class NotificationPreferenceEntityConfiguration : IEntityTypeConfiguration<NotificationPreference>
    {
        public void Configure(EntityTypeBuilder<NotificationPreference> builder)
        {
            builder.ToTable("NotificationPreferences");
            builder.HasKey(x => x.UserId);

            builder.Property(x => x.PreferenceData)
                .IsRequired()
                .HasColumnType("jsonb")
                .HasDefaultValue("{}");

            builder.Property(x => x.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(x => x.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Relationships
            builder.HasOne(x => x.User)
                .WithOne()
                .HasForeignKey<NotificationPreference>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

