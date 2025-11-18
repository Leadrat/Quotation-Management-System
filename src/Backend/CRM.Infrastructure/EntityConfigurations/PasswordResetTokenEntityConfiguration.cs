using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.EntityConfigurations
{
    public class PasswordResetTokenEntityConfiguration : IEntityTypeConfiguration<PasswordResetToken>
    {
        public void Configure(EntityTypeBuilder<PasswordResetToken> builder)
        {
            builder.ToTable("PasswordResetTokens");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.TokenHash)
                   .IsRequired();

            builder.Property(x => x.ExpiresAt)
                   .IsRequired();

            builder.Property(x => x.CreatedAt)
                   .IsRequired();

            builder.HasOne(x => x.User)
                   .WithMany()
                   .HasForeignKey(x => x.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Only one active token per user (UsedAt is null)
            builder.HasIndex(x => new { x.UserId, x.UsedAt })
                   .HasDatabaseName("IX_PasswordResetToken_User_Active");

            builder.HasIndex(x => x.ExpiresAt)
                   .HasDatabaseName("IX_PasswordResetToken_ExpiresAt");
        }
    }
}
