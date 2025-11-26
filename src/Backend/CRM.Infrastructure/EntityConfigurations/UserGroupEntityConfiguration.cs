using CRM.Domain.UserManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.EntityConfigurations;

public class UserGroupEntityConfiguration : IEntityTypeConfiguration<UserGroup>
{
    public void Configure(EntityTypeBuilder<UserGroup> builder)
    {
        builder.ToTable("UserGroups");
        builder.HasKey(ug => ug.GroupId);

        builder.Property(ug => ug.Name).IsRequired().HasMaxLength(255);
        builder.Property(ug => ug.Description).HasMaxLength(1000);
        builder.Property(ug => ug.Permissions)
            .IsRequired()
            .HasColumnType("jsonb")
            .HasDefaultValue("[]");
        builder.Property(ug => ug.CreatedAt).IsRequired();
        builder.Property(ug => ug.UpdatedAt).IsRequired();

        builder.HasOne(ug => ug.CreatedByUser)
            .WithMany()
            .HasForeignKey(ug => ug.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(ug => ug.CreatedByUserId);
        builder.HasIndex(ug => ug.CreatedAt);
    }
}

