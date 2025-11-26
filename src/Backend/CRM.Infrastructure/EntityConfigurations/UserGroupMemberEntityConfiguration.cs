using CRM.Domain.UserManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.EntityConfigurations;

public class UserGroupMemberEntityConfiguration : IEntityTypeConfiguration<UserGroupMember>
{
    public void Configure(EntityTypeBuilder<UserGroupMember> builder)
    {
        builder.ToTable("UserGroupMembers");
        builder.HasKey(ugm => ugm.GroupMemberId);

        builder.Property(ugm => ugm.AddedAt).IsRequired();

        builder.HasOne(ugm => ugm.UserGroup)
            .WithMany(ug => ug.Members)
            .HasForeignKey(ugm => ugm.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ugm => ugm.User)
            .WithMany()
            .HasForeignKey(ugm => ugm.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(ugm => new { ugm.GroupId, ugm.UserId }).IsUnique();
        builder.HasIndex(ugm => ugm.GroupId);
        builder.HasIndex(ugm => ugm.UserId);
    }
}

