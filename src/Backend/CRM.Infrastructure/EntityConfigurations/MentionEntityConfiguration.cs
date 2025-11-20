using CRM.Domain.UserManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.EntityConfigurations;

public class MentionEntityConfiguration : IEntityTypeConfiguration<Mention>
{
    public void Configure(EntityTypeBuilder<Mention> builder)
    {
        builder.ToTable("Mentions");
        builder.HasKey(m => m.MentionId);

        builder.Property(m => m.EntityType).IsRequired().HasMaxLength(50);
        builder.Property(m => m.IsRead).IsRequired();
        builder.Property(m => m.CreatedAt).IsRequired();

        builder.HasOne(m => m.MentionedUser)
            .WithMany()
            .HasForeignKey(m => m.MentionedUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.MentionedByUser)
            .WithMany()
            .HasForeignKey(m => m.MentionedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(m => new { m.MentionedUserId, m.IsRead });
        builder.HasIndex(m => new { m.EntityType, m.EntityId });
        builder.HasIndex(m => m.CreatedAt);
    }
}

