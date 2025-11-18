using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.EntityConfigurations;

public class UserPreferencesEntityConfiguration : IEntityTypeConfiguration<UserPreferences>
{
    public void Configure(EntityTypeBuilder<UserPreferences> builder)
    {
        builder.ToTable("UserPreferences");

        builder.HasKey(u => u.UserId);

        builder.Property(u => u.UserId)
            .IsRequired();

        builder.Property(u => u.LanguageCode)
            .HasMaxLength(5)
            .IsRequired()
            .HasDefaultValue("en");

        builder.Property(u => u.CurrencyCode)
            .HasMaxLength(3);

        builder.Property(u => u.DateFormat)
            .HasMaxLength(20)
            .IsRequired()
            .HasDefaultValue("dd/MM/yyyy");

        builder.Property(u => u.TimeFormat)
            .HasMaxLength(10)
            .IsRequired()
            .HasDefaultValue("24h");

        builder.Property(u => u.NumberFormat)
            .HasMaxLength(50)
            .IsRequired()
            .HasDefaultValue("en-IN");

        builder.Property(u => u.Timezone)
            .HasMaxLength(50);

        builder.Property(u => u.FirstDayOfWeek)
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(u => u.CreatedAt)
            .IsRequired();

        builder.Property(u => u.UpdatedAt)
            .IsRequired();

        builder.HasIndex(u => u.LanguageCode);
        builder.HasIndex(u => u.CurrencyCode);

        builder.HasOne(u => u.User)
            .WithOne()
            .HasForeignKey<UserPreferences>(u => u.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(u => u.Currency)
            .WithMany()
            .HasForeignKey(u => u.CurrencyCode)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(u => u.Language)
            .WithMany()
            .HasForeignKey(u => u.LanguageCode)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

