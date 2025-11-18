using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.EntityConfigurations
{
    public class EmailVerificationTokenEntityConfiguration : IEntityTypeConfiguration<EmailVerificationToken>
    {
        public void Configure(EntityTypeBuilder<EmailVerificationToken> builder)
        {
            builder.ToTable("EmailVerificationTokens");
            builder.HasKey(e => e.TokenId);

            builder.Property(e => e.TokenId)
                .ValueGeneratedNever();

            builder.Property(e => e.TokenHash)
                .IsRequired()
                .HasMaxLength(256);

            builder.HasIndex(e => e.UserId);

            builder.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
