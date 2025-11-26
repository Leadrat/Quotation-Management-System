using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.EntityConfigurations
{
    public class IdentifierTypeEntityConfiguration : IEntityTypeConfiguration<IdentifierType>
    {
        public void Configure(EntityTypeBuilder<IdentifierType> builder)
        {
            builder.ToTable("IdentifierTypes");
            builder.HasKey(i => i.IdentifierTypeId);
            
            builder.Property(i => i.Name)
                .HasMaxLength(50)
                .IsRequired();
            
            builder.Property(i => i.DisplayName)
                .HasMaxLength(100)
                .IsRequired();
            
            builder.Property(i => i.Description)
                .HasColumnType("text");
            
            builder.Property(i => i.IsActive)
                .IsRequired()
                .HasDefaultValue(true);
            
            builder.Property(i => i.CreatedAt)
                .IsRequired();
            
            builder.Property(i => i.UpdatedAt)
                .IsRequired();
            
            // Unique constraint on Name (case-insensitive comparison handled at application level)
            builder.HasIndex(i => i.Name)
                .IsUnique()
                .HasFilter("[DeletedAt] IS NULL");
            
            // Index for active identifier types
            builder.HasIndex(i => i.IsActive)
                .HasFilter("[DeletedAt] IS NULL");
            
            // Relationships
            builder.HasMany(i => i.CountryIdentifierConfigurations)
                .WithOne(c => c.IdentifierType)
                .HasForeignKey(c => c.IdentifierTypeId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

