using System;
using System.Linq;
using System.Text.Json;
using CRM.Infrastructure.Persistence;
using CRM.Domain.Entities;
using CRM.Shared.Constants;
using CRM.Shared.Helpers;

namespace CRM.Api.Utilities
{
    internal static class DbSeeder
    {
        public static void Seed(AppDbContext db)
        {
            var now = DateTime.UtcNow;

            // Seed built-in roles
            if (!db.Roles.Any(r => r.RoleId == RoleIds.Admin))
            {
                db.Roles.Add(new Role { RoleId = RoleIds.Admin, RoleName = RoleConstants.Admin, IsActive = true, CreatedAt = now, UpdatedAt = now });
            }
            if (!db.Roles.Any(r => r.RoleId == RoleIds.Manager))
            {
                db.Roles.Add(new Role { RoleId = RoleIds.Manager, RoleName = RoleConstants.Manager, IsActive = true, CreatedAt = now, UpdatedAt = now });
            }
            if (!db.Roles.Any(r => r.RoleId == RoleIds.SalesRep))
            {
                db.Roles.Add(new Role { RoleId = RoleIds.SalesRep, RoleName = RoleConstants.SalesRep, IsActive = true, CreatedAt = now, UpdatedAt = now });
            }
            if (!db.Roles.Any(r => r.RoleId == RoleIds.Client))
            {
                db.Roles.Add(new Role { RoleId = RoleIds.Client, RoleName = RoleConstants.Client, IsActive = true, CreatedAt = now, UpdatedAt = now });
            }

            db.SaveChanges();

            // Seed Admin user
            var adminEmail = "admin@crm.com";
            var admin = db.Users.FirstOrDefault(u => u.Email == adminEmail);
            if (admin == null)
            {
                admin = new User
                {
                    UserId = Guid.NewGuid(),
                    Email = adminEmail,
                    PasswordHash = PasswordHelper.HashPassword("Admin@12345"),
                    FirstName = "Admin",
                    LastName = "User",
                    IsActive = true,
                    RoleId = RoleIds.Admin,
                    ReportingManagerId = null,
                    CreatedAt = now,
                    UpdatedAt = now
                };
                db.Users.Add(admin);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Optional helper invoked manually (e.g., from quickstart) to populate history demo data.
        /// </summary>
        public static void SeedHistoryDemo(AppDbContext db, Guid? specificClientId = null)
        {
            var client = specificClientId.HasValue
                ? db.Clients.FirstOrDefault(c => c.ClientId == specificClientId.Value)
                : db.Clients.FirstOrDefault();
            if (client == null) return;

            var admin = db.Users.FirstOrDefault(u => u.RoleId == RoleIds.Admin);
            if (admin == null) return;

            if (db.ClientHistories.Any(h => h.ClientId == client.ClientId))
            {
                return;
            }

            var now = DateTimeOffset.UtcNow;
            var metadata = JsonSerializer.Serialize(new
            {
                IpAddress = "127.0.0.1",
                UserAgent = "seed-script",
                IsAutomation = true,
                RequestId = Guid.NewGuid().ToString("N"),
                Origin = "Seeder"
            });

            var createdHistory = new ClientHistory
            {
                HistoryId = Guid.NewGuid(),
                ClientId = client.ClientId,
                ActorUserId = admin.UserId,
                ActionType = "CREATED",
                ChangedFields = new() { "CompanyName", "Email" },
                AfterSnapshot = JsonSerializer.Serialize(new { client.CompanyName, client.Email }),
                Metadata = metadata,
                CreatedAt = now.AddMinutes(-30)
            };

            var updatedHistory = new ClientHistory
            {
                HistoryId = Guid.NewGuid(),
                ClientId = client.ClientId,
                ActorUserId = admin.UserId,
                ActionType = "UPDATED",
                ChangedFields = new() { "City", "State" },
                BeforeSnapshot = JsonSerializer.Serialize(new { client.City, client.State }),
                AfterSnapshot = JsonSerializer.Serialize(new { City = "Mumbai", State = "Maharashtra" }),
                Metadata = metadata,
                CreatedAt = now.AddMinutes(-5)
            };

            var flag = new SuspiciousActivityFlag
            {
                FlagId = Guid.NewGuid(),
                HistoryId = updatedHistory.HistoryId,
                ClientId = client.ClientId,
                Score = 8,
                Reasons = new() { "RAPID_CHANGES" },
                Metadata = metadata,
                DetectedAt = now.AddMinutes(-4),
                Status = "OPEN"
            };

            db.ClientHistories.AddRange(createdHistory, updatedHistory);
            db.SuspiciousActivityFlags.Add(flag);
            db.SaveChanges();
        }

        /// <summary>
        /// Optional helper invoked manually to populate quotation demo data.
        /// </summary>
        public static void SeedQuotationDemo(AppDbContext db, Guid? specificClientId = null, Guid? specificUserId = null)
        {
            var client = specificClientId.HasValue
                ? db.Clients.FirstOrDefault(c => c.ClientId == specificClientId.Value)
                : db.Clients.FirstOrDefault();
            if (client == null) return;

            var user = specificUserId.HasValue
                ? db.Users.FirstOrDefault(u => u.UserId == specificUserId.Value)
                : db.Users.FirstOrDefault(u => u.RoleId == RoleIds.SalesRep || u.RoleId == RoleIds.Admin);
            if (user == null) return;

            if (db.Quotations.Any(q => q.ClientId == client.ClientId))
            {
                return; // Already seeded
            }

            var now = DateTimeOffset.UtcNow;
            var quotationDate = DateTime.Today;
            var validUntil = quotationDate.AddDays(30);

            var quotation = new Quotation
            {
                QuotationId = Guid.NewGuid(),
                ClientId = client.ClientId,
                CreatedByUserId = user.UserId,
                QuotationNumber = $"QT-{quotationDate.Year}-{DateTime.Now:MMdd}0001",
                Status = Domain.Enums.QuotationStatus.Draft,
                QuotationDate = quotationDate,
                ValidUntil = validUntil,
                SubTotal = 50000.00m,
                DiscountAmount = 5000.00m,
                DiscountPercentage = 10.00m,
                TaxAmount = 8100.00m,
                CgstAmount = 4050.00m,
                SgstAmount = 4050.00m,
                IgstAmount = 0m,
                TotalAmount = 53100.00m,
                Notes = "Payment due within 30 days. Prices include delivery.",
                CreatedAt = now,
                UpdatedAt = now
            };

            var lineItem1 = new QuotationLineItem
            {
                LineItemId = Guid.NewGuid(),
                QuotationId = quotation.QuotationId,
                SequenceNumber = 1,
                ItemName = "Cloud Storage 1TB/month",
                Description = "Monthly cloud storage subscription with backup",
                Quantity = 10m,
                UnitRate = 5000.00m,
                Amount = 50000.00m,
                CreatedAt = now,
                UpdatedAt = now
            };

            db.Quotations.Add(quotation);
            db.QuotationLineItems.Add(lineItem1);
            db.SaveChanges();
        }
    }
}
