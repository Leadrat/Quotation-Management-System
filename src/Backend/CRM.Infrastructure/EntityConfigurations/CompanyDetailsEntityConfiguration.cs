using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.EntityConfigurations
{
    public class CompanyDetailsEntityConfiguration : IEntityTypeConfiguration<CompanyDetails>
    {
        public void Configure(EntityTypeBuilder<CompanyDetails> builder)
        {
            builder.ToTable("CompanyDetails");
            builder.HasKey(c => c.CompanyDetailsId);
            
            builder.Property(c => c.CompanyDetailsId)
                .HasDefaultValue(new Guid("00000000-0000-0000-0000-000000000001"));
            
            builder.Property(c => c.PanNumber).HasMaxLength(10);
            builder.Property(c => c.TanNumber).HasMaxLength(10);
            builder.Property(c => c.GstNumber).HasMaxLength(15);
            builder.Property(c => c.CompanyName).HasMaxLength(255);
            builder.Property(c => c.City).HasMaxLength(100);
            builder.Property(c => c.State).HasMaxLength(100);
            builder.Property(c => c.PostalCode).HasMaxLength(20);
            builder.Property(c => c.Country).HasMaxLength(100);
            builder.Property(c => c.CountryId); // Foreign key to Countries table
            builder.Property(c => c.ContactEmail).HasMaxLength(255);
            builder.Property(c => c.ContactPhone).HasMaxLength(20);
            builder.Property(c => c.Website).HasMaxLength(255);
            builder.Property(c => c.LogoUrl).HasMaxLength(500);
            
            // JSONB column for country-specific identifier values
            builder.Property(c => c.IdentifierValues)
                .HasColumnType("jsonb");
            
            // JSONB column for country-specific basic company details
            builder.Property(c => c.CountryDetails)
                .HasColumnType("jsonb");
            
            // GIN index for efficient JSONB queries
            builder.HasIndex(c => c.IdentifierValues)
                .HasMethod("gin")
                .HasDatabaseName("IX_CompanyDetails_IdentifierValues");
            
            // GIN index for country details JSONB queries
            builder.HasIndex(c => c.CountryDetails)
                .HasMethod("gin")
                .HasDatabaseName("IX_CompanyDetails_CountryDetails");
            
            builder.HasOne(c => c.UpdatedByUser)
                .WithMany()
                .HasForeignKey(c => c.UpdatedBy)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.HasMany(c => c.BankDetails)
                .WithOne(b => b.CompanyDetails)
                .HasForeignKey(b => b.CompanyDetailsId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Foreign key to Countries table
            builder.HasOne(c => c.CountryNavigation)
                .WithMany()
                .HasForeignKey(c => c.CountryId)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.HasIndex(c => c.UpdatedAt);
            builder.HasIndex(c => c.CountryId); // Index for CountryId
        }
    }
}

