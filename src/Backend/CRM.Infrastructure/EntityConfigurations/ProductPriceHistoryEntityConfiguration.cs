using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.EntityConfigurations
{
    public class ProductPriceHistoryEntityConfiguration : IEntityTypeConfiguration<ProductPriceHistory>
    {
        public void Configure(EntityTypeBuilder<ProductPriceHistory> builder)
        {
            builder.ToTable("ProductPriceHistory");
            builder.HasKey(x => x.PriceHistoryId);

            builder.Property(x => x.ProductId)
                .IsRequired();

            builder.Property(x => x.PriceType)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(x => x.OldPriceValue)
                .HasColumnType("decimal(18,2)");

            builder.Property(x => x.NewPriceValue)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(x => x.EffectiveFrom)
                .IsRequired()
                .HasColumnType("date");

            builder.Property(x => x.EffectiveTo)
                .HasColumnType("date");

            builder.Property(x => x.ChangedByUserId)
                .IsRequired();

            builder.Property(x => x.ChangedAt)
                .IsRequired();

            builder.Property(x => x.ChangeReason)
                .HasMaxLength(500);

            // Relationships
            builder.HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.ChangedByUser)
                .WithMany()
                .HasForeignKey(x => x.ChangedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(x => x.ProductId);
            builder.HasIndex(x => x.EffectiveFrom);
            builder.HasIndex(x => x.EffectiveTo);
            builder.HasIndex(x => new { x.ProductId, x.EffectiveFrom });
        }
    }
}

