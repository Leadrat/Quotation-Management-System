using CRM.Domain.Entities;
using CRM.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.EntityConfigurations
{
    public class QuotationTemplateEntityConfiguration : IEntityTypeConfiguration<QuotationTemplate>
    {
        public void Configure(EntityTypeBuilder<QuotationTemplate> builder)
        {
            builder.ToTable("QuotationTemplates");
            builder.HasKey(x => x.TemplateId);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Description)
                .HasMaxLength(255);

            builder.Property(x => x.OwnerUserId)
                .IsRequired();

            builder.Property(x => x.OwnerRole)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("SalesRep");

            builder.Property(x => x.Visibility)
                .IsRequired()
                .HasConversion(
                    v => v.ToString(),
                    v => (TemplateVisibility)Enum.Parse(typeof(TemplateVisibility), v))
                .HasMaxLength(50);

            builder.Property(x => x.IsApproved)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(x => x.Version)
                .IsRequired()
                .HasDefaultValue(1);

            builder.Property(x => x.UsageCount)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.UpdatedAt)
                .IsRequired();

            builder.Property(x => x.DiscountDefault)
                .HasColumnType("decimal(5,2)");

            builder.Property(x => x.Notes)
                .HasMaxLength(2000);

            // File-based template properties
            builder.Property(x => x.TemplateType)
                .HasMaxLength(50);

            builder.Property(x => x.IsFileBased)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(x => x.FileName)
                .HasMaxLength(255);

            builder.Property(x => x.FileUrl)
                .HasColumnType("text");

            builder.Property(x => x.FileSize);

            builder.Property(x => x.MimeType)
                .HasMaxLength(100);

            // Relationships
            builder.HasOne(x => x.OwnerUser)
                .WithMany()
                .HasForeignKey(x => x.OwnerUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ApprovedByUser)
                .WithMany()
                .HasForeignKey(x => x.ApprovedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(x => x.PreviousVersion)
                .WithMany()
                .HasForeignKey(x => x.PreviousVersionId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(x => x.LineItems)
                .WithOne(x => x.Template)
                .HasForeignKey(x => x.TemplateId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(x => x.OwnerUserId);
            builder.HasIndex(x => new { x.OwnerUserId, x.Visibility })
                .HasFilter("[DeletedAt] IS NULL");
            builder.HasIndex(x => new { x.IsApproved, x.Visibility })
                .HasFilter("[DeletedAt] IS NULL");
            builder.HasIndex(x => x.Name)
                .HasFilter("[DeletedAt] IS NULL");
            builder.HasIndex(x => x.UpdatedAt)
                .HasFilter("[DeletedAt] IS NULL");
            builder.HasIndex(x => x.PreviousVersionId)
                .HasFilter("[PreviousVersionId] IS NOT NULL");

            // Unique constraint: Name must be unique per owner per version (excluding deleted)
            builder.HasIndex(x => new { x.Name, x.OwnerUserId, x.Version })
                .IsUnique()
                .HasFilter("[DeletedAt] IS NULL");
        }
    }
}

