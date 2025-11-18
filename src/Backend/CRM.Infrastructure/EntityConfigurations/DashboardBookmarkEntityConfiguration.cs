using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.EntityConfigurations
{
    public class DashboardBookmarkEntityConfiguration : IEntityTypeConfiguration<DashboardBookmark>
    {
        public void Configure(EntityTypeBuilder<DashboardBookmark> builder)
        {
            builder.ToTable("DashboardBookmarks");
            builder.HasKey(x => x.BookmarkId);

            builder.Property(x => x.UserId)
                .IsRequired();

            builder.Property(x => x.DashboardName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.DashboardConfig)
                .IsRequired()
                .HasColumnType("jsonb");

            builder.Property(x => x.IsDefault)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(x => x.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()");

            builder.Property(x => x.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()");

            // Relationships
            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(x => x.UserId)
                .HasDatabaseName("IX_DashboardBookmarks_UserId");

            // Unique index for default bookmark per user
            builder.HasIndex(x => new { x.UserId, x.IsDefault })
                .HasDatabaseName("IX_DashboardBookmarks_UserId_IsDefault")
                .IsUnique()
                .HasFilter("\"IsDefault\" = true");
        }
    }
}

