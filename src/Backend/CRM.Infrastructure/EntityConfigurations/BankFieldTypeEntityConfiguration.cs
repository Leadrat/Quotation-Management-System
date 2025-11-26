using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.EntityConfigurations
{
    public class BankFieldTypeEntityConfiguration : IEntityTypeConfiguration<BankFieldType>
    {
        public void Configure(EntityTypeBuilder<BankFieldType> builder)
        {
            builder.ToTable("BankFieldTypes");
            builder.HasKey(b => b.BankFieldTypeId);
            
            builder.Property(b => b.Name)
                .HasMaxLength(50)
                .IsRequired();
            
            builder.Property(b => b.DisplayName)
                .HasMaxLength(100)
                .IsRequired();
            
            builder.Property(b => b.Description)
                .HasColumnType("text");
            
            builder.Property(b => b.IsActive)
                .IsRequired()
                .HasDefaultValue(true);
            
            builder.Property(b => b.CreatedAt)
                .IsRequired();
            
            builder.Property(b => b.UpdatedAt)
                .IsRequired();
            
            // Unique constraint on Name (case-insensitive comparison handled at application level)
            builder.HasIndex(b => b.Name)
                .IsUnique()
                .HasFilter("[DeletedAt] IS NULL");
            
            // Index for active bank field types
            builder.HasIndex(b => b.IsActive)
                .HasFilter("[DeletedAt] IS NULL");
            
            // Relationships
            builder.HasMany(b => b.CountryBankFieldConfigurations)
                .WithOne(c => c.BankFieldType)
                .HasForeignKey(c => c.BankFieldTypeId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

