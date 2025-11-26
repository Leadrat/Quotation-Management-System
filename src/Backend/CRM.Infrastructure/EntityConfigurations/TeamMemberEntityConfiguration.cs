using CRM.Domain.UserManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.EntityConfigurations;

public class TeamMemberEntityConfiguration : IEntityTypeConfiguration<TeamMember>
{
    public void Configure(EntityTypeBuilder<TeamMember> builder)
    {
        builder.ToTable("TeamMembers");
        builder.HasKey(tm => tm.TeamMemberId);

        builder.Property(tm => tm.Role).IsRequired().HasMaxLength(50);
        builder.Property(tm => tm.JoinedAt).IsRequired();

        builder.HasOne(tm => tm.Team)
            .WithMany(t => t.Members)
            .HasForeignKey(tm => tm.TeamId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(tm => tm.User)
            .WithMany()
            .HasForeignKey(tm => tm.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(tm => new { tm.TeamId, tm.UserId }).IsUnique();
        builder.HasIndex(tm => tm.TeamId);
        builder.HasIndex(tm => tm.UserId);
    }
}

