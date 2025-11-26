using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.EntityConfigurations
{
    public class BankDetailsEntityConfiguration : IEntityTypeConfiguration<BankDetails>
    {
        public void Configure(EntityTypeBuilder<BankDetails> builder)
        {
            builder.ToTable("BankDetails");
            builder.HasKey(b => b.BankDetailsId);
            
            builder.Property(b => b.Country).HasMaxLength(50).IsRequired(); // Keep for backward compatibility
            builder.Property(b => b.CountryId); // New field for FK to Countries table (nullable during transition)
            builder.Property(b => b.AccountNumber).HasMaxLength(50).IsRequired();
            builder.Property(b => b.IfscCode).HasMaxLength(11);
            builder.Property(b => b.Iban).HasMaxLength(34);
            builder.Property(b => b.SwiftCode).HasMaxLength(11);
            builder.Property(b => b.BankName).HasMaxLength(255).IsRequired();
            builder.Property(b => b.BranchName).HasMaxLength(255);
            
            // JSONB column for country-specific bank field values
            builder.Property(b => b.FieldValues)
                .HasColumnType("jsonb");
            
            // GIN index for efficient JSONB queries
            builder.HasIndex(b => b.FieldValues)
                .HasMethod("gin")
                .HasDatabaseName("IX_BankDetails_FieldValues");
            
            builder.HasOne(b => b.CompanyDetails)
                .WithMany(c => c.BankDetails)
                .HasForeignKey(b => b.CompanyDetailsId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // FK to Countries table (optional during transition)
            builder.HasOne(b => b.CountryNavigation)
                .WithMany()
                .HasForeignKey(b => b.CountryId)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.HasOne(b => b.UpdatedByUser)
                .WithMany()
                .HasForeignKey(b => b.UpdatedBy)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.HasIndex(b => b.CompanyDetailsId);
            builder.HasIndex(b => b.CountryId);
            // Unique constraint: one bank detail per company/country (using Country string for now)
            builder.HasAlternateKey(b => new { b.CompanyDetailsId, b.Country });
        }
    }
}

