using System;
using CRM.Domain.Entities;
using CRM.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.EntityConfigurations
{
    public class QuotationEntityConfiguration : IEntityTypeConfiguration<Quotation>
    {
        public void Configure(EntityTypeBuilder<Quotation> builder)
        {
            builder.ToTable("Quotations");
            builder.HasKey(x => x.QuotationId);

            builder.Property(x => x.QuotationNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.Status)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(x => x.QuotationDate)
                .IsRequired();

            builder.Property(x => x.ValidUntil)
                .IsRequired();

            builder.Property(x => x.SubTotal)
                .IsRequired()
                .HasColumnType("decimal(12,2)")
                .HasDefaultValue(0m);

            builder.Property(x => x.DiscountAmount)
                .IsRequired()
                .HasColumnType("decimal(12,2)")
                .HasDefaultValue(0m);

            builder.Property(x => x.DiscountPercentage)
                .IsRequired()
                .HasColumnType("decimal(5,2)")
                .HasDefaultValue(0m);

            builder.Property(x => x.TaxAmount)
                .IsRequired()
                .HasColumnType("decimal(12,2)")
                .HasDefaultValue(0m);

            builder.Property(x => x.CgstAmount)
                .HasColumnType("decimal(12,2)");

            builder.Property(x => x.SgstAmount)
                .HasColumnType("decimal(12,2)");

            builder.Property(x => x.IgstAmount)
                .HasColumnType("decimal(12,2)");

            builder.Property(x => x.TaxCountryId);
            builder.Property(x => x.TaxJurisdictionId);
            builder.Property(x => x.TaxFrameworkId);
            builder.Property(x => x.TaxBreakdown)
                .HasColumnType("jsonb");

            builder.Property(x => x.CompanyDetailsSnapshot)
                .HasColumnType("jsonb");

            builder.Property(x => x.TotalAmount)
                .IsRequired()
                .HasColumnType("decimal(12,2)");

            builder.Property(x => x.Notes)
                .HasMaxLength(2000);

            builder.Property(x => x.IsPendingApproval)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(x => x.PendingApprovalId);

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.UpdatedAt)
                .IsRequired();

            // Relationships
            builder.HasOne(x => x.Client)
                .WithMany()
                .HasForeignKey(x => x.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.CreatedByUser)
                .WithMany()
                .HasForeignKey(x => x.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.LineItems)
                .WithOne(x => x.Quotation)
                .HasForeignKey(x => x.QuotationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.PendingApproval)
                .WithOne()
                .HasForeignKey<Quotation>(x => x.PendingApprovalId)
                .OnDelete(DeleteBehavior.SetNull);

            // Indexes
            builder.HasIndex(x => x.QuotationNumber)
                .IsUnique();

            builder.HasIndex(x => x.ClientId);

            builder.HasIndex(x => x.CreatedByUserId);

            builder.HasIndex(x => x.Status);

            builder.HasIndex(x => x.QuotationDate);

            builder.HasIndex(x => x.ValidUntil);

            builder.HasIndex(x => x.CreatedAt);

            builder.HasIndex(x => new { x.ClientId, x.Status });

            builder.HasIndex(x => new { x.CreatedByUserId, x.Status, x.CreatedAt });

            builder.HasIndex(x => x.IsPendingApproval);

            builder.HasIndex(x => x.PendingApprovalId)
                .HasFilter("\"PendingApprovalId\" IS NOT NULL");

            // Tax management relationships
            builder.HasOne<Country>()
                .WithMany()
                .HasForeignKey(x => x.TaxCountryId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne<Jurisdiction>()
                .WithMany()
                .HasForeignKey(x => x.TaxJurisdictionId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne<TaxFramework>()
                .WithMany()
                .HasForeignKey(x => x.TaxFrameworkId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(x => x.TaxCountryId);
            builder.HasIndex(x => x.TaxJurisdictionId);
            builder.HasIndex(x => x.TaxFrameworkId);
        }
    }
}

