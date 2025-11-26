using CRM.Domain.Entities;
using CRM.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.EntityConfigurations
{
    public class TaxCalculationLogEntityConfiguration : IEntityTypeConfiguration<TaxCalculationLog>
    {
        public void Configure(EntityTypeBuilder<TaxCalculationLog> builder)
        {
            builder.ToTable("TaxCalculationLogs");
            builder.HasKey(x => x.LogId);

            builder.Property(x => x.ActionType)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(x => x.CalculationDetails)
                .IsRequired()
                .HasColumnType("jsonb")
                .HasDefaultValue("{}");

            builder.Property(x => x.ChangedAt)
                .IsRequired();

            // Indexes
            builder.HasIndex(x => x.QuotationId);
            builder.HasIndex(x => x.ChangedAt);
            builder.HasIndex(x => x.ActionType);
            builder.HasIndex(x => new { x.CountryId, x.JurisdictionId });
            builder.HasIndex(x => new { x.ChangedAt, x.ActionType })
                .HasDatabaseName("IX_TaxCalculationLogs_ChangedAt_ActionType");

            // Relationships
            builder.HasOne(x => x.Quotation)
                .WithMany()
                .HasForeignKey(x => x.QuotationId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(x => x.Country)
                .WithMany()
                .HasForeignKey(x => x.CountryId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(x => x.Jurisdiction)
                .WithMany()
                .HasForeignKey(x => x.JurisdictionId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(x => x.ChangedByUser)
                .WithMany()
                .HasForeignKey(x => x.ChangedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}

