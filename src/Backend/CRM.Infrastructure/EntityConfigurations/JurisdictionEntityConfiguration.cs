using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.EntityConfigurations
{
    public class JurisdictionEntityConfiguration : IEntityTypeConfiguration<Jurisdiction>
    {
        public void Configure(EntityTypeBuilder<Jurisdiction> builder)
        {
            builder.ToTable("Jurisdictions");
            builder.HasKey(x => x.JurisdictionId);

            builder.Property(x => x.JurisdictionName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.JurisdictionCode)
                .HasMaxLength(20);

            builder.Property(x => x.JurisdictionType)
                .HasMaxLength(20);

            builder.Property(x => x.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.UpdatedAt)
                .IsRequired();

            builder.Property(x => x.DeletedAt);

            // Indexes
            builder.HasIndex(x => x.CountryId);
            builder.HasIndex(x => x.ParentJurisdictionId);
            builder.HasIndex(x => x.IsActive);

            // Composite unique index for jurisdiction code within parent
            builder.HasIndex(x => new { x.CountryId, x.ParentJurisdictionId, x.JurisdictionCode })
                .IsUnique()
                .HasFilter("\"JurisdictionCode\" IS NOT NULL AND \"DeletedAt\" IS NULL");

            // Relationships
            builder.HasOne(x => x.Country)
                .WithMany()
                .HasForeignKey(x => x.CountryId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.ParentJurisdiction)
                .WithMany(x => x.ChildJurisdictions)
                .HasForeignKey(x => x.ParentJurisdictionId)
                .OnDelete(DeleteBehavior.SetNull);

            // Query filter for soft delete
            builder.HasQueryFilter(x => x.DeletedAt == null);
        }
    }
}

