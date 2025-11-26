using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.EntityConfigurations
{
    public class TemplatePlaceholderEntityConfiguration : IEntityTypeConfiguration<TemplatePlaceholder>
    {
        public void Configure(EntityTypeBuilder<TemplatePlaceholder> builder)
        {
            builder.ToTable("TemplatePlaceholders");
            builder.HasKey(x => x.PlaceholderId);

            builder.Property(x => x.TemplateId)
                .IsRequired();

            builder.Property(x => x.PlaceholderName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.PlaceholderType)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.OriginalText)
                .HasColumnType("text");

            builder.Property(x => x.IsManuallyAdded)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.UpdatedAt)
                .IsRequired();

            // Relationships
            builder.HasOne(x => x.Template)
                .WithMany(x => x.Placeholders)
                .HasForeignKey(x => x.TemplateId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(x => x.TemplateId)
                .HasDatabaseName("IX_TemplatePlaceholders_TemplateId");

            builder.HasIndex(x => new { x.TemplateId, x.PlaceholderType })
                .HasDatabaseName("IX_TemplatePlaceholders_TemplateId_Type");

            builder.HasIndex(x => new { x.TemplateId, x.PlaceholderName })
                .IsUnique()
                .HasDatabaseName("IX_TemplatePlaceholders_TemplateId_PlaceholderName");
        }
    }
}

