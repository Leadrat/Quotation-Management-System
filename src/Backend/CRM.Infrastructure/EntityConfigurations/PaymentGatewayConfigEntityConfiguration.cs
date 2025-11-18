using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.EntityConfigurations
{
    public class PaymentGatewayConfigEntityConfiguration : IEntityTypeConfiguration<PaymentGatewayConfig>
    {
        public void Configure(EntityTypeBuilder<PaymentGatewayConfig> builder)
        {
            builder.ToTable("PaymentGatewayConfigs");
            builder.HasKey(x => x.ConfigId);

            builder.Property(x => x.CompanyId);

            builder.Property(x => x.GatewayName)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.ApiKey)
                .IsRequired();

            builder.Property(x => x.ApiSecret)
                .IsRequired();

            builder.Property(x => x.WebhookSecret);

            builder.Property(x => x.Enabled)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(x => x.IsTestMode)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(x => x.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(x => x.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(x => x.CreatedByUserId);

            builder.Property(x => x.Metadata)
                .HasColumnType("jsonb");

            // Relationships
            builder.HasOne(x => x.CreatedByUser)
                .WithMany()
                .HasForeignKey(x => x.CreatedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Indexes
            builder.HasIndex(x => x.CompanyId);
            builder.HasIndex(x => x.GatewayName);
            builder.HasIndex(x => x.Enabled);
            builder.HasIndex(x => new { x.CompanyId, x.GatewayName })
                .IsUnique();
        }
    }
}

