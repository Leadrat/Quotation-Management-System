using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CRM.Domain.Entities;

namespace CRM.Infrastructure.EntityConfigurations
{
    public class QuotationAccessLinkEntityConfiguration : IEntityTypeConfiguration<QuotationAccessLink>
    {
        public void Configure(EntityTypeBuilder<QuotationAccessLink> builder)
        {
            builder.ToTable("QuotationAccessLinks");

            builder.HasKey(x => x.AccessLinkId);

            builder.Property(x => x.AccessLinkId)
                .ValueGeneratedOnAdd();

            builder.Property(x => x.QuotationId)
                .IsRequired();

            builder.Property(x => x.ClientEmail)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(x => x.AccessToken)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(x => x.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(x => x.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(x => x.ExpiresAt);

            builder.Property(x => x.SentAt);

            builder.Property(x => x.FirstViewedAt);

            builder.Property(x => x.LastViewedAt);

            builder.Property(x => x.ViewCount)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(x => x.IpAddress)
                .HasMaxLength(50);

            // Indexes
            builder.HasIndex(x => x.AccessToken)
                .IsUnique()
                .HasDatabaseName("IX_QuotationAccessLinks_AccessToken");

            builder.HasIndex(x => x.ClientEmail)
                .HasDatabaseName("IX_QuotationAccessLinks_ClientEmail");

            builder.HasIndex(x => x.QuotationId)
                .HasDatabaseName("IX_QuotationAccessLinks_QuotationId");

            builder.HasIndex(x => new { x.QuotationId, x.IsActive })
                .HasDatabaseName("IX_QuotationAccessLinks_QuotationId_IsActive");

            // Foreign keys
            builder.HasOne(x => x.Quotation)
                .WithMany()
                .HasForeignKey(x => x.QuotationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

