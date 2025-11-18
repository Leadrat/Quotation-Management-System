using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CRM.Domain.Entities;

namespace CRM.Infrastructure.EntityConfigurations
{
    public class ClientPortalOtpEntityConfiguration : IEntityTypeConfiguration<ClientPortalOtp>
    {
        public void Configure(EntityTypeBuilder<ClientPortalOtp> builder)
        {
            builder.ToTable("ClientPortalOtps");

            builder.HasKey(x => x.OtpId);

            builder.Property(x => x.OtpId)
                .IsRequired();

            builder.Property(x => x.AccessLinkId)
                .IsRequired();

            builder.Property(x => x.ClientEmail)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(x => x.OtpCode)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(x => x.ExpiresAt)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(x => x.IsUsed)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(x => x.Attempts)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(x => x.IpAddress)
                .HasMaxLength(50);

            // Indexes
            builder.HasIndex(x => x.AccessLinkId)
                .HasDatabaseName("IX_ClientPortalOtps_AccessLinkId");

            builder.HasIndex(x => x.ClientEmail)
                .HasDatabaseName("IX_ClientPortalOtps_ClientEmail");

            builder.HasIndex(x => new { x.AccessLinkId, x.IsUsed, x.ExpiresAt })
                .HasDatabaseName("IX_ClientPortalOtps_AccessLinkId_IsUsed_ExpiresAt");

            // Foreign key
            builder.HasOne(x => x.AccessLink)
                .WithMany()
                .HasForeignKey(x => x.AccessLinkId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

