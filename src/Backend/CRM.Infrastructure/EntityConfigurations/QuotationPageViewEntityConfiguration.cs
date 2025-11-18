using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CRM.Domain.Entities;

namespace CRM.Infrastructure.EntityConfigurations
{
    public class QuotationPageViewEntityConfiguration : IEntityTypeConfiguration<QuotationPageView>
    {
        public void Configure(EntityTypeBuilder<QuotationPageView> builder)
        {
            builder.ToTable("QuotationPageViews");

            builder.HasKey(x => x.ViewId);

            builder.Property(x => x.ViewId)
                .IsRequired();

            builder.Property(x => x.AccessLinkId)
                .IsRequired();

            builder.Property(x => x.QuotationId)
                .IsRequired();

            builder.Property(x => x.ClientEmail)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(x => x.ViewStartTime)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(x => x.DurationSeconds);

            builder.Property(x => x.IpAddress)
                .HasMaxLength(50);

            builder.Property(x => x.UserAgent)
                .HasMaxLength(500);

            builder.Property(x => x.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Indexes
            builder.HasIndex(x => x.AccessLinkId)
                .HasDatabaseName("IX_QuotationPageViews_AccessLinkId");

            builder.HasIndex(x => x.QuotationId)
                .HasDatabaseName("IX_QuotationPageViews_QuotationId");

            builder.HasIndex(x => x.ClientEmail)
                .HasDatabaseName("IX_QuotationPageViews_ClientEmail");

            builder.HasIndex(x => x.ViewStartTime)
                .HasDatabaseName("IX_QuotationPageViews_ViewStartTime");

            // Foreign keys
            builder.HasOne(x => x.AccessLink)
                .WithMany()
                .HasForeignKey(x => x.AccessLinkId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Quotation)
                .WithMany()
                .HasForeignKey(x => x.QuotationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

