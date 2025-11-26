using CRM.Domain.Imports;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.EntityConfigurations.Imports;

public class ImportSessionConfiguration : IEntityTypeConfiguration<ImportSession>
{
    public void Configure(EntityTypeBuilder<ImportSession> builder)
    {
        builder.ToTable("ImportSessions");
        builder.HasKey(x => x.ImportSessionId);
        builder.Property(x => x.SourceType).HasMaxLength(16).IsRequired();
        builder.Property(x => x.SourceFileRef).HasMaxLength(1024).IsRequired();
        builder.Property(x => x.Status).HasMaxLength(32).IsRequired();
        builder.Property(x => x.SuggestedMappingsJson).HasColumnType("jsonb");
        builder.Property(x => x.ConfirmedMappingsJson).HasColumnType("jsonb");
        builder.Property(x => x.CreatedBy).HasMaxLength(128).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();
    }
}
