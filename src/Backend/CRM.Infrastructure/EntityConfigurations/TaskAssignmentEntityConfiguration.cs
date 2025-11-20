using CRM.Domain.UserManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.EntityConfigurations;

public class TaskAssignmentEntityConfiguration : IEntityTypeConfiguration<TaskAssignment>
{
    public void Configure(EntityTypeBuilder<TaskAssignment> builder)
    {
        builder.ToTable("TaskAssignments");
        builder.HasKey(ta => ta.AssignmentId);

        builder.Property(ta => ta.EntityType).IsRequired().HasMaxLength(50);
        builder.Property(ta => ta.Status)
            .HasConversion<int>()
            .IsRequired();
        builder.Property(ta => ta.CreatedAt).IsRequired();
        builder.Property(ta => ta.UpdatedAt).IsRequired();

        builder.HasOne(ta => ta.AssignedToUser)
            .WithMany()
            .HasForeignKey(ta => ta.AssignedToUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ta => ta.AssignedByUser)
            .WithMany()
            .HasForeignKey(ta => ta.AssignedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(ta => new { ta.AssignedToUserId, ta.Status });
        builder.HasIndex(ta => new { ta.EntityType, ta.EntityId });
        builder.HasIndex(ta => ta.DueDate);
        builder.HasIndex(ta => ta.CreatedAt);
    }
}

