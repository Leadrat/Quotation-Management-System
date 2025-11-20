using CRM.Domain.UserManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.EntityConfigurations;

public class TeamEntityConfiguration : IEntityTypeConfiguration<Team>
{
    public void Configure(EntityTypeBuilder<Team> builder)
    {
        builder.ToTable("Teams");
        builder.HasKey(t => t.TeamId);

        builder.Property(t => t.Name).IsRequired().HasMaxLength(255);
        builder.Property(t => t.Description).HasMaxLength(1000);
        builder.Property(t => t.IsActive).IsRequired();
        builder.Property(t => t.CreatedAt).IsRequired();
        builder.Property(t => t.UpdatedAt).IsRequired();

        builder.HasOne(t => t.TeamLead)
            .WithMany()
            .HasForeignKey(t => t.TeamLeadUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.ParentTeam)
            .WithMany(p => p.ChildTeams)
            .HasForeignKey(t => t.ParentTeamId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(t => t.TeamLeadUserId);
        builder.HasIndex(t => t.ParentTeamId);
        builder.HasIndex(t => t.CompanyId);
        builder.HasIndex(t => t.IsActive);
        builder.HasIndex(t => new { t.CompanyId, t.Name }).IsUnique();
    }
}

