using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.EntityConfigurations
{
    public class CountryIdentifierConfigurationEntityConfiguration : IEntityTypeConfiguration<CountryIdentifierConfiguration>
    {
        public void Configure(EntityTypeBuilder<CountryIdentifierConfiguration> builder)
        {
            builder.ToTable("CountryIdentifierConfigurations");
            builder.HasKey(c => c.ConfigurationId);
            
            builder.Property(c => c.CountryId)
                .IsRequired();
            
            builder.Property(c => c.IdentifierTypeId)
                .IsRequired();
            
            builder.Property(c => c.IsRequired)
                .IsRequired()
                .HasDefaultValue(false);
            
            builder.Property(c => c.ValidationRegex)
                .HasMaxLength(500);
            
            builder.Property(c => c.DisplayName)
                .HasMaxLength(100);
            
            builder.Property(c => c.HelpText)
                .HasColumnType("text");
            
            builder.Property(c => c.DisplayOrder)
                .IsRequired()
                .HasDefaultValue(0);
            
            builder.Property(c => c.IsActive)
                .IsRequired()
                .HasDefaultValue(true);
            
            builder.Property(c => c.CreatedAt)
                .IsRequired();
            
            builder.Property(c => c.UpdatedAt)
                .IsRequired();
            
            // Unique constraint: one configuration per country/identifier type
            builder.HasIndex(c => new { c.CountryId, c.IdentifierTypeId })
                .IsUnique()
                .HasFilter("[DeletedAt] IS NULL");
            
            // Index for country-specific queries
            builder.HasIndex(c => c.CountryId);
            
            // Index for active configurations
            builder.HasIndex(c => c.IsActive)
                .HasFilter("[DeletedAt] IS NULL");
            
            // Composite index for active configurations per country
            builder.HasIndex(c => new { c.CountryId, c.IsActive })
                .HasFilter("[DeletedAt] IS NULL");
            
            // Relationships
            builder.HasOne(c => c.Country)
                .WithMany()
                .HasForeignKey(c => c.CountryId)
                .OnDelete(DeleteBehavior.Cascade);
            
            builder.HasOne(c => c.IdentifierType)
                .WithMany(i => i.CountryIdentifierConfigurations)
                .HasForeignKey(c => c.IdentifierTypeId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

