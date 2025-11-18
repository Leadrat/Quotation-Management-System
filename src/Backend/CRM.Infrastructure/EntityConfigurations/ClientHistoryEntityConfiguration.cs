using System;
using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.EntityConfigurations
{
    public class ClientHistoryEntityConfiguration : IEntityTypeConfiguration<ClientHistory>
    {
        public void Configure(EntityTypeBuilder<ClientHistory> builder)
        {
            builder.ToTable("ClientHistories");
            builder.HasKey(x => x.HistoryId);

            builder.Property(x => x.ActionType)
                   .HasMaxLength(50)
                   .IsRequired();

            builder.Property(x => x.ChangedFields)
                   .HasColumnType("text[]");

            builder.Property(x => x.BeforeSnapshot).HasColumnType("jsonb");
            builder.Property(x => x.AfterSnapshot).HasColumnType("jsonb");
            builder.Property(x => x.Metadata)
                   .HasColumnType("jsonb")
                   .HasDefaultValue("{}");

            builder.Property(x => x.Reason).HasMaxLength(500);
            builder.Property(x => x.SuspicionScore).HasDefaultValue((short)0);
            builder.Property(x => x.CreatedAt).IsRequired();

            builder.HasOne(x => x.Client)
                   .WithMany()
                   .HasForeignKey(x => x.ClientId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ActorUser)
                   .WithMany()
                   .HasForeignKey(x => x.ActorUserId)
                   .OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(x => new { x.ClientId, x.CreatedAt }).HasDatabaseName("IX_ClientHistories_ClientId_CreatedAt");
            builder.HasIndex(x => new { x.ActorUserId, x.CreatedAt }).HasDatabaseName("IX_ClientHistories_Actor_CreatedAt");
        }
    }
}

