using CRM.Domain.Admin;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.EntityConfigurations;

public class CustomBrandingEntityConfiguration : IEntityTypeConfiguration<CustomBranding>
{
    public void Configure(EntityTypeBuilder<CustomBranding> builder)
    {
        builder.ToTable("CustomBranding");
        builder.HasKey(b => b.Id);

        builder.Property(b => b.LogoUrl)
            .HasMaxLength(500);

        builder.Property(b => b.PrimaryColor)
            .HasMaxLength(7); // #RRGGBB format

        builder.Property(b => b.SecondaryColor)
            .HasMaxLength(7);

        builder.Property(b => b.AccentColor)
            .HasMaxLength(7);

        builder.Property(b => b.FooterHtml)
            .HasColumnType("text");

        builder.Property(b => b.UpdatedAt)
            .IsRequired();

        builder.Property(b => b.UpdatedBy)
            .IsRequired();

        // Foreign key
        builder.HasOne(b => b.UpdatedByUser)
            .WithMany()
            .HasForeignKey(b => b.UpdatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        // Single row constraint - only one branding configuration
        builder.HasIndex(b => b.Id)
            .IsUnique();
    }
}

