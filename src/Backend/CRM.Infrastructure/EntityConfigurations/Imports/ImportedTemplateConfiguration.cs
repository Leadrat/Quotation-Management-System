using CRM.Domain.Imports;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.EntityConfigurations.Imports;

public class ImportedTemplateConfiguration : IEntityTypeConfiguration<ImportedTemplate>
{
    public void Configure(EntityTypeBuilder<ImportedTemplate> builder)
    {
        builder.ToTable("ImportedTemplates");
        builder.HasKey(x => x.ImportedTemplateId);
        builder.Property(x => x.Name).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Type).HasMaxLength(64).IsRequired();
        builder.Property(x => x.ContentRef).HasMaxLength(1024).IsRequired();
        builder.Property(x => x.Version).IsRequired();
        builder.Property(x => x.CreatedBy).HasMaxLength(128).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();
    }
}
