using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.EntityConfigurations
{
    public class ClientEntityConfiguration : IEntityTypeConfiguration<Client>
    {
        public void Configure(EntityTypeBuilder<Client> builder)
        {
            builder.ToTable("Clients");
            builder.HasKey(x => x.ClientId);

            builder.Property(x => x.CompanyName).IsRequired().HasMaxLength(255);
            builder.Property(x => x.ContactName).HasMaxLength(255);
            builder.Property(x => x.Email).IsRequired().HasMaxLength(255);
            builder.Property(x => x.Mobile).IsRequired().HasMaxLength(20);
            builder.Property(x => x.PhoneCode).HasMaxLength(5);
            builder.Property(x => x.Gstin).HasMaxLength(15);
            builder.Property(x => x.StateCode).HasMaxLength(2);
            builder.Property(x => x.City).HasMaxLength(100);
            builder.Property(x => x.State).HasMaxLength(100);
            builder.Property(x => x.PinCode).HasMaxLength(10);

            builder.HasOne(x => x.CreatedByUser)
                   .WithMany()
                   .HasForeignKey(x => x.CreatedByUserId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Conventional indexes (the partial unique index on lower(email) will be added in migration)
            builder.HasIndex(x => x.Gstin);
            builder.HasIndex(x => x.CreatedAt);
            builder.HasIndex(x => x.UpdatedAt);
            builder.HasIndex(x => x.DeletedAt);
            builder.HasIndex(x => new { x.CreatedByUserId, x.DeletedAt });
        }
    }
}
