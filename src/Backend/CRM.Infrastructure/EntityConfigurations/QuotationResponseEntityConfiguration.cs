using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CRM.Domain.Entities;

namespace CRM.Infrastructure.EntityConfigurations
{
    public class QuotationResponseEntityConfiguration : IEntityTypeConfiguration<QuotationResponse>
    {
        public void Configure(EntityTypeBuilder<QuotationResponse> builder)
        {
            builder.ToTable("QuotationResponses");

            builder.HasKey(x => x.ResponseId);

            builder.Property(x => x.ResponseId)
                .ValueGeneratedOnAdd();

            builder.Property(x => x.QuotationId)
                .IsRequired();

            builder.Property(x => x.ResponseType)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.ClientEmail)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(x => x.ClientName)
                .HasMaxLength(255);

            builder.Property(x => x.ResponseMessage)
                .HasMaxLength(2000);

            builder.Property(x => x.ResponseDate)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(x => x.IpAddress)
                .HasMaxLength(50);

            builder.Property(x => x.UserAgent);

            builder.Property(x => x.NotifiedAdminAt);

            // Indexes
            builder.HasIndex(x => x.QuotationId)
                .IsUnique()
                .HasDatabaseName("IX_QuotationResponses_QuotationId");

            builder.HasIndex(x => x.ClientEmail)
                .HasDatabaseName("IX_QuotationResponses_ClientEmail");

            builder.HasIndex(x => x.ResponseType)
                .HasDatabaseName("IX_QuotationResponses_ResponseType");

            // Foreign keys
            builder.HasOne(x => x.Quotation)
                .WithMany()
                .HasForeignKey(x => x.QuotationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

