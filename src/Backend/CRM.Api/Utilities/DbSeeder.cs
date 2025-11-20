using System;
using System.Linq;
using System.Text.Json;
using CRM.Infrastructure.Persistence;
using CRM.Domain.Entities;
using CRM.Shared.Constants;
using CRM.Shared.Helpers;
using CRM.Domain.Enums;

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

            // Seed Admin user (password: Admin@123 to match migration)
            var adminEmail = "admin@crm.com";
            var adminPassword = "Admin@123";
            var admin = db.Users.FirstOrDefault(u => u.Email == adminEmail);
            if (admin == null)
            {
                admin = new User
                {
                    UserId = Guid.NewGuid(),
                    Email = adminEmail,
                    PasswordHash = PasswordHelper.HashPassword(adminPassword),
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
                Console.WriteLine($"[Seeder] Created admin user: {adminEmail}");
                Console.WriteLine($"[Seeder] Admin login credentials - Email: {adminEmail}, Password: {adminPassword}");
            }
            else
            {
                // Always ensure admin password is correct and user is active
                // In development, always reset password to ensure it works
                var needsUpdate = false;
                
                // Always reset password to ensure it's correct (useful for development)
                // Verify current password first
                var currentPasswordWorks = false;
                try
                {
                    if (!string.IsNullOrEmpty(admin.PasswordHash) && admin.PasswordHash.Length >= 60)
                    {
                        currentPasswordWorks = PasswordHelper.VerifyPassword(adminPassword, admin.PasswordHash);
                    }
                }
                catch
                {
                    currentPasswordWorks = false;
                }
                
                // Reset password if it doesn't work OR if we're in development (always ensure it's correct)
                if (!currentPasswordWorks)
                {
                    admin.PasswordHash = PasswordHelper.HashPassword(adminPassword);
                    needsUpdate = true;
                    Console.WriteLine($"[Seeder] Resetting admin password for: {adminEmail} (password: {adminPassword})");
                }
                
                // Ensure admin is active and has correct role
                if (!admin.IsActive || admin.RoleId != RoleIds.Admin)
                {
                    admin.IsActive = true;
                    admin.RoleId = RoleIds.Admin;
                    needsUpdate = true;
                    Console.WriteLine($"[Seeder] Fixing admin user status/role for: {adminEmail}");
                }
                
                if (needsUpdate)
                {
                    admin.UpdatedAt = now;
                    db.SaveChanges();
                    Console.WriteLine($"[Seeder] Updated admin user: {adminEmail}");
                }
                else
                {
                    Console.WriteLine($"[Seeder] Admin user verified: {adminEmail}");
                }
                
                // Always print admin credentials for development convenience
                Console.WriteLine($"[Seeder] Admin login credentials - Email: {adminEmail}, Password: {adminPassword}");
            }

            // Seed Countries
            SeedCountries(db, now);

            // Seed Identifier Types, Bank Field Types, and Country Configurations
            SeedCompanyIdentifiersAndBankDetails(db, now);
        }

        private static void SeedCountries(AppDbContext db, DateTime now)
        {
            // Only seed if we have less than 50 countries (assuming we should have ~195)
            var existingCount = db.Countries.Count();
            if (existingCount >= 50)
            {
                Console.WriteLine($"[Seeder] Countries already seeded ({existingCount} countries found). Skipping.");
                return;
            }

            Console.WriteLine($"[Seeder] Seeding countries... ({existingCount} existing)");

            var countries = new[]
            {
                new { Name = "Afghanistan", Code = "AF", Currency = "AFN", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Albania", Code = "AL", Currency = "ALL", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Algeria", Code = "DZ", Currency = "DZD", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Andorra", Code = "AD", Currency = "EUR", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Angola", Code = "AO", Currency = "AOA", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Antigua and Barbuda", Code = "AG", Currency = "XCD", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Argentina", Code = "AR", Currency = "ARS", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Armenia", Code = "AM", Currency = "AMD", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Australia", Code = "AU", Currency = "AUD", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Austria", Code = "AT", Currency = "EUR", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Azerbaijan", Code = "AZ", Currency = "AZN", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Bahamas", Code = "BS", Currency = "BSD", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Bahrain", Code = "BH", Currency = "BHD", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Bangladesh", Code = "BD", Currency = "BDT", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Barbados", Code = "BB", Currency = "BBD", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Belarus", Code = "BY", Currency = "BYN", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Belgium", Code = "BE", Currency = "EUR", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Belize", Code = "BZ", Currency = "BZD", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Benin", Code = "BJ", Currency = "XOF", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Bhutan", Code = "BT", Currency = "BTN", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Bolivia", Code = "BO", Currency = "BOB", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Bosnia and Herzegovina", Code = "BA", Currency = "BAM", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Botswana", Code = "BW", Currency = "BWP", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Brazil", Code = "BR", Currency = "BRL", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Brunei", Code = "BN", Currency = "BND", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Bulgaria", Code = "BG", Currency = "BGN", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Burkina Faso", Code = "BF", Currency = "XOF", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Burundi", Code = "BI", Currency = "BIF", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Cabo Verde", Code = "CV", Currency = "CVE", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Cambodia", Code = "KH", Currency = "KHR", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Cameroon", Code = "CM", Currency = "XAF", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Canada", Code = "CA", Currency = "CAD", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Central African Republic", Code = "CF", Currency = "XAF", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Chad", Code = "TD", Currency = "XAF", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Chile", Code = "CL", Currency = "CLP", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "China", Code = "CN", Currency = "CNY", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Colombia", Code = "CO", Currency = "COP", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Comoros", Code = "KM", Currency = "KMF", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Congo", Code = "CG", Currency = "XAF", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Costa Rica", Code = "CR", Currency = "CRC", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Croatia", Code = "HR", Currency = "EUR", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Cuba", Code = "CU", Currency = "CUP", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Cyprus", Code = "CY", Currency = "EUR", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Czech Republic", Code = "CZ", Currency = "CZK", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Denmark", Code = "DK", Currency = "DKK", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Djibouti", Code = "DJ", Currency = "DJF", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Dominica", Code = "DM", Currency = "XCD", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Dominican Republic", Code = "DO", Currency = "DOP", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Ecuador", Code = "EC", Currency = "USD", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Egypt", Code = "EG", Currency = "EGP", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "El Salvador", Code = "SV", Currency = "USD", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Equatorial Guinea", Code = "GQ", Currency = "XAF", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Eritrea", Code = "ER", Currency = "ERN", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Estonia", Code = "EE", Currency = "EUR", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Eswatini", Code = "SZ", Currency = "SZL", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Ethiopia", Code = "ET", Currency = "ETB", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Fiji", Code = "FJ", Currency = "FJD", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Finland", Code = "FI", Currency = "EUR", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "France", Code = "FR", Currency = "EUR", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Gabon", Code = "GA", Currency = "XAF", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Gambia", Code = "GM", Currency = "GMD", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Georgia", Code = "GE", Currency = "GEL", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Germany", Code = "DE", Currency = "EUR", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Ghana", Code = "GH", Currency = "GHS", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Greece", Code = "GR", Currency = "EUR", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Grenada", Code = "GD", Currency = "XCD", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Guatemala", Code = "GT", Currency = "GTQ", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Guinea", Code = "GN", Currency = "GNF", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Guinea-Bissau", Code = "GW", Currency = "XOF", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Guyana", Code = "GY", Currency = "GYD", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Haiti", Code = "HT", Currency = "HTG", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Honduras", Code = "HN", Currency = "HNL", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Hungary", Code = "HU", Currency = "HUF", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Iceland", Code = "IS", Currency = "ISK", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "India", Code = "IN", Currency = "INR", TaxFramework = TaxFrameworkType.GST },
                new { Name = "Indonesia", Code = "ID", Currency = "IDR", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Iran", Code = "IR", Currency = "IRR", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Iraq", Code = "IQ", Currency = "IQD", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Ireland", Code = "IE", Currency = "EUR", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Israel", Code = "IL", Currency = "ILS", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Italy", Code = "IT", Currency = "EUR", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Jamaica", Code = "JM", Currency = "JMD", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Japan", Code = "JP", Currency = "JPY", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Jordan", Code = "JO", Currency = "JOD", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Kazakhstan", Code = "KZ", Currency = "KZT", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Kenya", Code = "KE", Currency = "KES", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Kiribati", Code = "KI", Currency = "AUD", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Kosovo", Code = "XK", Currency = "EUR", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Kuwait", Code = "KW", Currency = "KWD", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Kyrgyzstan", Code = "KG", Currency = "KGS", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Laos", Code = "LA", Currency = "LAK", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Latvia", Code = "LV", Currency = "EUR", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Lebanon", Code = "LB", Currency = "LBP", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Lesotho", Code = "LS", Currency = "LSL", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Liberia", Code = "LR", Currency = "LRD", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Libya", Code = "LY", Currency = "LYD", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Liechtenstein", Code = "LI", Currency = "CHF", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Lithuania", Code = "LT", Currency = "EUR", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Luxembourg", Code = "LU", Currency = "EUR", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Madagascar", Code = "MG", Currency = "MGA", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Malawi", Code = "MW", Currency = "MWK", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Malaysia", Code = "MY", Currency = "MYR", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Maldives", Code = "MV", Currency = "MVR", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Mali", Code = "ML", Currency = "XOF", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Malta", Code = "MT", Currency = "EUR", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Marshall Islands", Code = "MH", Currency = "USD", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Mauritania", Code = "MR", Currency = "MRU", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Mauritius", Code = "MU", Currency = "MUR", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Mexico", Code = "MX", Currency = "MXN", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Micronesia", Code = "FM", Currency = "USD", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Moldova", Code = "MD", Currency = "MDL", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Monaco", Code = "MC", Currency = "EUR", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Mongolia", Code = "MN", Currency = "MNT", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Montenegro", Code = "ME", Currency = "EUR", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Morocco", Code = "MA", Currency = "MAD", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Mozambique", Code = "MZ", Currency = "MZN", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Myanmar", Code = "MM", Currency = "MMK", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Namibia", Code = "NA", Currency = "NAD", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Nauru", Code = "NR", Currency = "AUD", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Nepal", Code = "NP", Currency = "NPR", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Netherlands", Code = "NL", Currency = "EUR", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "New Zealand", Code = "NZ", Currency = "NZD", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Nicaragua", Code = "NI", Currency = "NIO", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Niger", Code = "NE", Currency = "XOF", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Nigeria", Code = "NG", Currency = "NGN", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "North Korea", Code = "KP", Currency = "KPW", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "North Macedonia", Code = "MK", Currency = "MKD", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Norway", Code = "NO", Currency = "NOK", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Oman", Code = "OM", Currency = "OMR", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Pakistan", Code = "PK", Currency = "PKR", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Palau", Code = "PW", Currency = "USD", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Palestine", Code = "PS", Currency = "ILS", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Panama", Code = "PA", Currency = "PAB", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Papua New Guinea", Code = "PG", Currency = "PGK", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Paraguay", Code = "PY", Currency = "PYG", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Peru", Code = "PE", Currency = "PEN", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Philippines", Code = "PH", Currency = "PHP", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Poland", Code = "PL", Currency = "PLN", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Portugal", Code = "PT", Currency = "EUR", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Qatar", Code = "QA", Currency = "QAR", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Romania", Code = "RO", Currency = "RON", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Russia", Code = "RU", Currency = "RUB", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Rwanda", Code = "RW", Currency = "RWF", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Saint Kitts and Nevis", Code = "KN", Currency = "XCD", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Saint Lucia", Code = "LC", Currency = "XCD", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Saint Vincent and the Grenadines", Code = "VC", Currency = "XCD", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Samoa", Code = "WS", Currency = "WST", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "San Marino", Code = "SM", Currency = "EUR", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Sao Tome and Principe", Code = "ST", Currency = "STN", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Saudi Arabia", Code = "SA", Currency = "SAR", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Senegal", Code = "SN", Currency = "XOF", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Serbia", Code = "RS", Currency = "RSD", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Seychelles", Code = "SC", Currency = "SCR", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Sierra Leone", Code = "SL", Currency = "SLE", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Singapore", Code = "SG", Currency = "SGD", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Slovakia", Code = "SK", Currency = "EUR", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Slovenia", Code = "SI", Currency = "EUR", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Solomon Islands", Code = "SB", Currency = "SBD", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Somalia", Code = "SO", Currency = "SOS", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "South Africa", Code = "ZA", Currency = "ZAR", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "South Korea", Code = "KR", Currency = "KRW", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "South Sudan", Code = "SS", Currency = "SSP", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Spain", Code = "ES", Currency = "EUR", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Sri Lanka", Code = "LK", Currency = "LKR", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Sudan", Code = "SD", Currency = "SDG", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Suriname", Code = "SR", Currency = "SRD", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Sweden", Code = "SE", Currency = "SEK", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Switzerland", Code = "CH", Currency = "CHF", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Syria", Code = "SY", Currency = "SYP", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Taiwan", Code = "TW", Currency = "TWD", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Tajikistan", Code = "TJ", Currency = "TJS", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Tanzania", Code = "TZ", Currency = "TZS", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Thailand", Code = "TH", Currency = "THB", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Timor-Leste", Code = "TL", Currency = "USD", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Togo", Code = "TG", Currency = "XOF", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Tonga", Code = "TO", Currency = "TOP", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Trinidad and Tobago", Code = "TT", Currency = "TTD", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Tunisia", Code = "TN", Currency = "TND", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Turkey", Code = "TR", Currency = "TRY", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Turkmenistan", Code = "TM", Currency = "TMT", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Tuvalu", Code = "TV", Currency = "AUD", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Uganda", Code = "UG", Currency = "UGX", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Ukraine", Code = "UA", Currency = "UAH", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "United Arab Emirates", Code = "AE", Currency = "AED", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "United Kingdom", Code = "GB", Currency = "GBP", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "United States", Code = "US", Currency = "USD", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Uruguay", Code = "UY", Currency = "UYU", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Uzbekistan", Code = "UZ", Currency = "UZS", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Vanuatu", Code = "VU", Currency = "VUV", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Vatican City", Code = "VA", Currency = "EUR", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Venezuela", Code = "VE", Currency = "VES", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Vietnam", Code = "VN", Currency = "VND", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Yemen", Code = "YE", Currency = "YER", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Zambia", Code = "ZM", Currency = "ZMW", TaxFramework = TaxFrameworkType.VAT },
                new { Name = "Zimbabwe", Code = "ZW", Currency = "ZWL", TaxFramework = TaxFrameworkType.VAT }
            };

            var added = 0;
            var skipped = 0;
            var nowOffset = DateTimeOffset.UtcNow;

            foreach (var country in countries)
            {
                // Check if country already exists by code
                if (!db.Countries.Any(c => c.CountryCode == country.Code))
                {
                    var countryEntity = new Country
                    {
                        CountryId = Guid.NewGuid(),
                        CountryName = country.Name,
                        CountryCode = country.Code,
                        TaxFrameworkType = country.TaxFramework,
                        DefaultCurrency = country.Currency,
                        IsActive = true,
                        IsDefault = country.Code == "IN", // Set India as default
                        CreatedAt = nowOffset,
                        UpdatedAt = nowOffset
                    };
                    db.Countries.Add(countryEntity);
                    added++;
                }
                else
                {
                    skipped++;
                }
            }

            if (added > 0)
            {
                db.SaveChanges();
                Console.WriteLine($"[Seeder] Seeded {added} countries ({skipped} already existed)");
            }
            else if (skipped > 0)
            {
                Console.WriteLine($"[Seeder] All {skipped} countries already exist. Skipping seed.");
            }
        }

        private static void SeedCompanyIdentifiersAndBankDetails(AppDbContext db, DateTime now)
        {
            var nowOffset = DateTimeOffset.UtcNow;

            // Seed Identifier Types (Master Data)
            var identifierTypes = new[]
            {
                // India
                new { Name = "PAN", DisplayName = "PAN Number", Description = "Permanent Account Number" },
                new { Name = "TAN", DisplayName = "TAN Number", Description = "Tax Deduction and Collection Account Number" },
                new { Name = "GSTIN", DisplayName = "GSTIN", Description = "Goods and Services Tax Identification Number" },
                new { Name = "CIN", DisplayName = "CIN", Description = "Corporate Identification Number" },
                new { Name = "DIN", DisplayName = "DIN", Description = "Director Identification Number" },
                new { Name = "UIN", DisplayName = "UIN", Description = "Unique Identification Number" },
                new { Name = "CRN", DisplayName = "CRN", Description = "Company Registration Number" },
                // USA
                new { Name = "EIN", DisplayName = "EIN", Description = "Employer Identification Number" },
                new { Name = "DUNS", DisplayName = "DUNS Number", Description = "Data Universal Numbering System" },
                // UK
                new { Name = "COMPANIES_HOUSE", DisplayName = "Companies House Number", Description = "UK Companies House Registration Number" },
                new { Name = "VAT_UK", DisplayName = "VAT Number", Description = "UK Value Added Tax Number" },
                new { Name = "UTR", DisplayName = "UTR", Description = "Unique Taxpayer Reference" },
                // UAE
                new { Name = "TRADE_LICENSE", DisplayName = "Trade License Number", Description = "UAE Trade License Number" },
                new { Name = "VAT_UAE", DisplayName = "VAT Number", Description = "UAE Value Added Tax Number" },
                // Singapore
                new { Name = "UEN", DisplayName = "UEN", Description = "Unique Entity Number" },
                // Australia
                new { Name = "ABN", DisplayName = "ABN", Description = "Australian Business Number" },
                new { Name = "ACN", DisplayName = "ACN", Description = "Australian Company Number" },
                // General
                new { Name = "VAT", DisplayName = "VAT Number", Description = "Value Added Tax Number" },
                new { Name = "TAX_ID", DisplayName = "Tax ID", Description = "Tax Identification Number" },
                new { Name = "REGISTRATION_NUMBER", DisplayName = "Registration Number", Description = "Business Registration Number" }
            };

            var identifierTypeDict = new Dictionary<string, Guid>();
            foreach (var it in identifierTypes)
            {
                var existing = db.IdentifierTypes.FirstOrDefault(x => x.Name == it.Name);
                if (existing == null)
                {
                    var identifierType = new IdentifierType
                    {
                        IdentifierTypeId = Guid.NewGuid(),
                        Name = it.Name,
                        DisplayName = it.DisplayName,
                        Description = it.Description,
                        IsActive = true,
                        CreatedAt = nowOffset,
                        UpdatedAt = nowOffset
                    };
                    db.IdentifierTypes.Add(identifierType);
                    identifierTypeDict[it.Name] = identifierType.IdentifierTypeId;
                }
                else
                {
                    identifierTypeDict[it.Name] = existing.IdentifierTypeId;
                }
            }

            // Seed Bank Field Types (Master Data)
            var bankFieldTypes = new[]
            {
                new { Name = "IFSC", DisplayName = "IFSC Code", Description = "Indian Financial System Code" },
                new { Name = "IBAN", DisplayName = "IBAN", Description = "International Bank Account Number" },
                new { Name = "SWIFT", DisplayName = "SWIFT Code", Description = "Society for Worldwide Interbank Financial Telecommunication Code" },
                new { Name = "ROUTING_NUMBER", DisplayName = "Routing Number", Description = "Bank Routing Number (US/Canada)" },
                new { Name = "BSB", DisplayName = "BSB Code", Description = "Bank State Branch Code (Australia)" },
                new { Name = "SORT_CODE", DisplayName = "Sort Code", Description = "UK Bank Sort Code" },
                new { Name = "BRANCH_CODE", DisplayName = "Branch Code", Description = "Bank Branch Code" },
                new { Name = "ACCOUNT_NUMBER", DisplayName = "Account Number", Description = "Bank Account Number" },
                new { Name = "BANK_NAME", DisplayName = "Bank Name", Description = "Name of the Bank" },
                new { Name = "BRANCH_NAME", DisplayName = "Branch Name", Description = "Name of the Branch" }
            };

            var bankFieldTypeDict = new Dictionary<string, Guid>();
            foreach (var bt in bankFieldTypes)
            {
                var existing = db.BankFieldTypes.FirstOrDefault(x => x.Name == bt.Name);
                if (existing == null)
                {
                    var bankFieldType = new BankFieldType
                    {
                        BankFieldTypeId = Guid.NewGuid(),
                        Name = bt.Name,
                        DisplayName = bt.DisplayName,
                        Description = bt.Description,
                        IsActive = true,
                        CreatedAt = nowOffset,
                        UpdatedAt = nowOffset
                    };
                    db.BankFieldTypes.Add(bankFieldType);
                    bankFieldTypeDict[bt.Name] = bankFieldType.BankFieldTypeId;
                }
                else
                {
                    bankFieldTypeDict[bt.Name] = existing.BankFieldTypeId;
                }
            }

            db.SaveChanges();

            // Get countries
            var india = db.Countries.FirstOrDefault(c => c.CountryCode == "IN");
            var usa = db.Countries.FirstOrDefault(c => c.CountryCode == "US");
            var uk = db.Countries.FirstOrDefault(c => c.CountryCode == "GB");
            var uae = db.Countries.FirstOrDefault(c => c.CountryCode == "AE");

            // Configure India - Company Identifiers
            if (india != null && !db.CountryIdentifierConfigurations.Any(c => c.CountryId == india.CountryId))
            {
                var indianIdentifiers = new[]
                {
                    new { TypeName = "PAN", IsRequired = true, ValidationRegex = @"^[A-Z]{5}[0-9]{4}[A-Z]{1}$", MinLength = (int?)10, MaxLength = (int?)10, DisplayOrder = 1, HelpText = "10 character alphanumeric code (e.g., ABCDE1234F)" },
                    new { TypeName = "GSTIN", IsRequired = true, ValidationRegex = @"^[0-9]{2}[A-Z]{5}[0-9]{4}[A-Z]{1}[1-9A-Z]{1}Z[0-9A-Z]{1}$", MinLength = (int?)15, MaxLength = (int?)15, DisplayOrder = 2, HelpText = "15 character GST identification number" },
                    new { TypeName = "CIN", IsRequired = false, ValidationRegex = @"^[A-Z]{1}[0-9]{5}[A-Z]{2}[0-9]{4}[A-Z]{3}[0-9]{6}$", MinLength = (int?)21, MaxLength = (int?)21, DisplayOrder = 3, HelpText = "21 character Corporate Identification Number" },
                    new { TypeName = "TAN", IsRequired = false, ValidationRegex = @"^[A-Z]{4}[0-9]{5}[A-Z]{1}$", MinLength = (int?)10, MaxLength = (int?)10, DisplayOrder = 4, HelpText = "10 character Tax Deduction Account Number (e.g., ABCD12345E)" },
                    new { TypeName = "CRN", IsRequired = false, ValidationRegex = (string?)null, MinLength = (int?)null, MaxLength = (int?)null, DisplayOrder = 5, HelpText = "Company Registration Number" },
                    new { TypeName = "DIN", IsRequired = false, ValidationRegex = @"^[0-9]{8}$", MinLength = (int?)8, MaxLength = (int?)8, DisplayOrder = 6, HelpText = "8 digit Director Identification Number" },
                    new { TypeName = "UIN", IsRequired = false, ValidationRegex = (string?)null, MinLength = (int?)null, MaxLength = (int?)null, DisplayOrder = 7, HelpText = "Unique Identification Number" }
                };

                foreach (var id in indianIdentifiers)
                {
                    if (identifierTypeDict.ContainsKey(id.TypeName))
                    {
                        db.CountryIdentifierConfigurations.Add(new CountryIdentifierConfiguration
                        {
                            ConfigurationId = Guid.NewGuid(),
                            CountryId = india.CountryId,
                            IdentifierTypeId = identifierTypeDict[id.TypeName],
                            IsRequired = id.IsRequired,
                            ValidationRegex = id.ValidationRegex,
                            MinLength = id.MinLength,
                            MaxLength = id.MaxLength,
                            DisplayOrder = id.DisplayOrder,
                            HelpText = id.HelpText,
                            IsActive = true,
                            CreatedAt = nowOffset,
                            UpdatedAt = nowOffset
                        });
                    }
                }
            }

            // Configure India - Bank Details
            if (india != null && !db.CountryBankFieldConfigurations.Any(c => c.CountryId == india.CountryId))
            {
                var indianBankFields = new[]
                {
                    new { TypeName = "BANK_NAME", IsRequired = true, ValidationRegex = (string?)null, MinLength = (int?)null, MaxLength = (int?)200, DisplayOrder = 1, HelpText = "Name of the bank" },
                    new { TypeName = "ACCOUNT_NUMBER", IsRequired = true, ValidationRegex = @"^[0-9]{9,18}$", MinLength = (int?)9, MaxLength = (int?)18, DisplayOrder = 2, HelpText = "9-18 digit account number" },
                    new { TypeName = "IFSC", IsRequired = true, ValidationRegex = @"^[A-Z]{4}0[A-Z0-9]{6}$", MinLength = (int?)11, MaxLength = (int?)11, DisplayOrder = 3, HelpText = "11 character IFSC code (e.g., SBIN0001234)" },
                    new { TypeName = "BRANCH_NAME", IsRequired = false, ValidationRegex = (string?)null, MinLength = (int?)null, MaxLength = (int?)200, DisplayOrder = 4, HelpText = "Branch name" },
                    new { TypeName = "SWIFT", IsRequired = false, ValidationRegex = @"^[A-Z]{4}[A-Z]{2}[A-Z0-9]{2}([A-Z0-9]{3})?$", MinLength = (int?)8, MaxLength = (int?)11, DisplayOrder = 5, HelpText = "8-11 character SWIFT code for international transfers" }
                };

                foreach (var bf in indianBankFields)
                {
                    if (bankFieldTypeDict.ContainsKey(bf.TypeName))
                    {
                        db.CountryBankFieldConfigurations.Add(new CountryBankFieldConfiguration
                        {
                            ConfigurationId = Guid.NewGuid(),
                            CountryId = india.CountryId,
                            BankFieldTypeId = bankFieldTypeDict[bf.TypeName],
                            IsRequired = bf.IsRequired,
                            ValidationRegex = bf.ValidationRegex,
                            MinLength = bf.MinLength,
                            MaxLength = bf.MaxLength,
                            DisplayOrder = bf.DisplayOrder,
                            HelpText = bf.HelpText,
                            IsActive = true,
                            CreatedAt = nowOffset,
                            UpdatedAt = nowOffset
                        });
                    }
                }
            }

            // Configure USA - Company Identifiers
            if (usa != null && !db.CountryIdentifierConfigurations.Any(c => c.CountryId == usa.CountryId))
            {
                var usIdentifiers = new[]
                {
                    new { TypeName = "EIN", IsRequired = true, ValidationRegex = @"^[0-9]{2}-[0-9]{7}$", MinLength = (int?)10, MaxLength = (int?)10, DisplayOrder = 1, HelpText = "Format: XX-XXXXXXX" },
                    new { TypeName = "DUNS", IsRequired = false, ValidationRegex = @"^[0-9]{9}$", MinLength = (int?)9, MaxLength = (int?)9, DisplayOrder = 2, HelpText = "9 digit DUNS number" }
                };

                foreach (var id in usIdentifiers)
                {
                    if (identifierTypeDict.ContainsKey(id.TypeName))
                    {
                        db.CountryIdentifierConfigurations.Add(new CountryIdentifierConfiguration
                        {
                            ConfigurationId = Guid.NewGuid(),
                            CountryId = usa.CountryId,
                            IdentifierTypeId = identifierTypeDict[id.TypeName],
                            IsRequired = id.IsRequired,
                            ValidationRegex = id.ValidationRegex,
                            MinLength = id.MinLength,
                            MaxLength = id.MaxLength,
                            DisplayOrder = id.DisplayOrder,
                            HelpText = id.HelpText,
                            IsActive = true,
                            CreatedAt = nowOffset,
                            UpdatedAt = nowOffset
                        });
                    }
                }
            }

            // Configure USA - Bank Details
            if (usa != null && !db.CountryBankFieldConfigurations.Any(c => c.CountryId == usa.CountryId))
            {
                var usBankFields = new[]
                {
                    new { TypeName = "BANK_NAME", IsRequired = true, ValidationRegex = (string?)null, MinLength = (int?)null, MaxLength = (int?)200, DisplayOrder = 1, HelpText = "Name of the bank" },
                    new { TypeName = "ACCOUNT_NUMBER", IsRequired = true, ValidationRegex = @"^[0-9]{8,17}$", MinLength = (int?)8, MaxLength = (int?)17, DisplayOrder = 2, HelpText = "8-17 digit account number" },
                    new { TypeName = "ROUTING_NUMBER", IsRequired = true, ValidationRegex = @"^[0-9]{9}$", MinLength = (int?)9, MaxLength = (int?)9, DisplayOrder = 3, HelpText = "9 digit routing number" },
                    new { TypeName = "SWIFT", IsRequired = false, ValidationRegex = @"^[A-Z]{4}[A-Z]{2}[A-Z0-9]{2}([A-Z0-9]{3})?$", MinLength = (int?)8, MaxLength = (int?)11, DisplayOrder = 4, HelpText = "SWIFT code for international transfers" }
                };

                foreach (var bf in usBankFields)
                {
                    if (bankFieldTypeDict.ContainsKey(bf.TypeName))
                    {
                        db.CountryBankFieldConfigurations.Add(new CountryBankFieldConfiguration
                        {
                            ConfigurationId = Guid.NewGuid(),
                            CountryId = usa.CountryId,
                            BankFieldTypeId = bankFieldTypeDict[bf.TypeName],
                            IsRequired = bf.IsRequired,
                            ValidationRegex = bf.ValidationRegex,
                            MinLength = bf.MinLength,
                            MaxLength = bf.MaxLength,
                            DisplayOrder = bf.DisplayOrder,
                            HelpText = bf.HelpText,
                            IsActive = true,
                            CreatedAt = nowOffset,
                            UpdatedAt = nowOffset
                        });
                    }
                }
            }

            // Configure UK - Company Identifiers
            if (uk != null && !db.CountryIdentifierConfigurations.Any(c => c.CountryId == uk.CountryId))
            {
                var ukIdentifiers = new[]
                {
                    new { TypeName = "COMPANIES_HOUSE", IsRequired = true, ValidationRegex = @"^[0-9]{6,8}$", MinLength = (int?)6, MaxLength = (int?)8, DisplayOrder = 1, HelpText = "6-8 digit Companies House number" },
                    new { TypeName = "VAT_UK", IsRequired = false, ValidationRegex = @"^GB[0-9]{9}$|^GB[0-9]{12}$", MinLength = (int?)11, MaxLength = (int?)14, DisplayOrder = 2, HelpText = "UK VAT number (GB followed by 9 or 12 digits)" },
                    new { TypeName = "UTR", IsRequired = false, ValidationRegex = @"^[0-9]{10}$", MinLength = (int?)10, MaxLength = (int?)10, DisplayOrder = 3, HelpText = "10 digit Unique Taxpayer Reference" }
                };

                foreach (var id in ukIdentifiers)
                {
                    if (identifierTypeDict.ContainsKey(id.TypeName))
                    {
                        db.CountryIdentifierConfigurations.Add(new CountryIdentifierConfiguration
                        {
                            ConfigurationId = Guid.NewGuid(),
                            CountryId = uk.CountryId,
                            IdentifierTypeId = identifierTypeDict[id.TypeName],
                            IsRequired = id.IsRequired,
                            ValidationRegex = id.ValidationRegex,
                            MinLength = id.MinLength,
                            MaxLength = id.MaxLength,
                            DisplayOrder = id.DisplayOrder,
                            HelpText = id.HelpText,
                            IsActive = true,
                            CreatedAt = nowOffset,
                            UpdatedAt = nowOffset
                        });
                    }
                }
            }

            // Configure UK - Bank Details
            if (uk != null && !db.CountryBankFieldConfigurations.Any(c => c.CountryId == uk.CountryId))
            {
                var ukBankFields = new[]
                {
                    new { TypeName = "BANK_NAME", IsRequired = true, ValidationRegex = (string?)null, MinLength = (int?)null, MaxLength = (int?)200, DisplayOrder = 1, HelpText = "Name of the bank" },
                    new { TypeName = "ACCOUNT_NUMBER", IsRequired = true, ValidationRegex = @"^[0-9]{8}$", MinLength = (int?)8, MaxLength = (int?)8, DisplayOrder = 2, HelpText = "8 digit account number" },
                    new { TypeName = "SORT_CODE", IsRequired = true, ValidationRegex = @"^[0-9]{6}$", MinLength = (int?)6, MaxLength = (int?)6, DisplayOrder = 3, HelpText = "6 digit sort code" },
                    new { TypeName = "IBAN", IsRequired = false, ValidationRegex = @"^GB[0-9]{2}[A-Z]{4}[0-9]{14}$", MinLength = (int?)22, MaxLength = (int?)22, DisplayOrder = 4, HelpText = "22 character UK IBAN" },
                    new { TypeName = "SWIFT", IsRequired = false, ValidationRegex = @"^[A-Z]{4}[A-Z]{2}[A-Z0-9]{2}([A-Z0-9]{3})?$", MinLength = (int?)8, MaxLength = (int?)11, DisplayOrder = 5, HelpText = "SWIFT code" }
                };

                foreach (var bf in ukBankFields)
                {
                    if (bankFieldTypeDict.ContainsKey(bf.TypeName))
                    {
                        db.CountryBankFieldConfigurations.Add(new CountryBankFieldConfiguration
                        {
                            ConfigurationId = Guid.NewGuid(),
                            CountryId = uk.CountryId,
                            BankFieldTypeId = bankFieldTypeDict[bf.TypeName],
                            IsRequired = bf.IsRequired,
                            ValidationRegex = bf.ValidationRegex,
                            MinLength = bf.MinLength,
                            MaxLength = bf.MaxLength,
                            DisplayOrder = bf.DisplayOrder,
                            HelpText = bf.HelpText,
                            IsActive = true,
                            CreatedAt = nowOffset,
                            UpdatedAt = nowOffset
                        });
                    }
                }
            }

            // Configure UAE - Company Identifiers
            if (uae != null && !db.CountryIdentifierConfigurations.Any(c => c.CountryId == uae.CountryId))
            {
                var uaeIdentifiers = new[]
                {
                    new { TypeName = "TRADE_LICENSE", IsRequired = true, ValidationRegex = (string?)null, MinLength = (int?)null, MaxLength = (int?)50, DisplayOrder = 1, HelpText = "Trade License Number" },
                    new { TypeName = "VAT_UAE", IsRequired = false, ValidationRegex = @"^[0-9]{15}$", MinLength = (int?)15, MaxLength = (int?)15, DisplayOrder = 2, HelpText = "15 digit UAE VAT number" }
                };

                foreach (var id in uaeIdentifiers)
                {
                    if (identifierTypeDict.ContainsKey(id.TypeName))
                    {
                        db.CountryIdentifierConfigurations.Add(new CountryIdentifierConfiguration
                        {
                            ConfigurationId = Guid.NewGuid(),
                            CountryId = uae.CountryId,
                            IdentifierTypeId = identifierTypeDict[id.TypeName],
                            IsRequired = id.IsRequired,
                            ValidationRegex = id.ValidationRegex,
                            MinLength = id.MinLength,
                            MaxLength = id.MaxLength,
                            DisplayOrder = id.DisplayOrder,
                            HelpText = id.HelpText,
                            IsActive = true,
                            CreatedAt = nowOffset,
                            UpdatedAt = nowOffset
                        });
                    }
                }
            }

            // Configure UAE - Bank Details
            if (uae != null && !db.CountryBankFieldConfigurations.Any(c => c.CountryId == uae.CountryId))
            {
                var uaeBankFields = new[]
                {
                    new { TypeName = "BANK_NAME", IsRequired = true, ValidationRegex = (string?)null, MinLength = (int?)null, MaxLength = (int?)200, DisplayOrder = 1, HelpText = "Name of the bank" },
                    new { TypeName = "ACCOUNT_NUMBER", IsRequired = true, ValidationRegex = @"^[0-9]{9,18}$", MinLength = (int?)9, MaxLength = (int?)18, DisplayOrder = 2, HelpText = "Account number" },
                    new { TypeName = "IBAN", IsRequired = true, ValidationRegex = @"^AE[0-9]{2}[0-9]{3}[0-9]{16}$", MinLength = (int?)23, MaxLength = (int?)23, DisplayOrder = 3, HelpText = "23 character UAE IBAN" },
                    new { TypeName = "SWIFT", IsRequired = true, ValidationRegex = @"^[A-Z]{4}[A-Z]{2}[A-Z0-9]{2}([A-Z0-9]{3})?$", MinLength = (int?)8, MaxLength = (int?)11, DisplayOrder = 4, HelpText = "SWIFT code" }
                };

                foreach (var bf in uaeBankFields)
                {
                    if (bankFieldTypeDict.ContainsKey(bf.TypeName))
                    {
                        db.CountryBankFieldConfigurations.Add(new CountryBankFieldConfiguration
                        {
                            ConfigurationId = Guid.NewGuid(),
                            CountryId = uae.CountryId,
                            BankFieldTypeId = bankFieldTypeDict[bf.TypeName],
                            IsRequired = bf.IsRequired,
                            ValidationRegex = bf.ValidationRegex,
                            MinLength = bf.MinLength,
                            MaxLength = bf.MaxLength,
                            DisplayOrder = bf.DisplayOrder,
                            HelpText = bf.HelpText,
                            IsActive = true,
                            CreatedAt = nowOffset,
                            UpdatedAt = nowOffset
                        });
                    }
                }
            }

            // Configure all other countries with generic/default fields
            ConfigureAllCountries(db, identifierTypeDict, bankFieldTypeDict, nowOffset);

            db.SaveChanges();
            Console.WriteLine($"[Seeder] Seeded company identifier types, bank field types, and country configurations for all countries");
        }

        private static void ConfigureAllCountries(AppDbContext db, Dictionary<string, Guid> identifierTypeDict, Dictionary<string, Guid> bankFieldTypeDict, DateTimeOffset nowOffset)
        {
            // Get all countries that don't have configurations yet
            var allCountries = db.Countries.ToList();
            var configuredCountryIds = db.CountryIdentifierConfigurations.Select(c => c.CountryId).Distinct().ToHashSet();
            var countriesToConfigure = allCountries.Where(c => !configuredCountryIds.Contains(c.CountryId)).ToList();

            Console.WriteLine($"[Seeder] Configuring {countriesToConfigure.Count} countries with default/generic fields...");

            // Country-specific identifier configurations (by country code)
            var countryIdentifierMap = new Dictionary<string, List<(string TypeName, bool IsRequired, string? ValidationRegex, int? MinLength, int? MaxLength, int DisplayOrder, string? HelpText)>>();

            // India (already configured above, but keep for reference)
            // USA (already configured)
            // UK (already configured)
            // UAE (already configured)

            // EU Countries (VAT + IBAN)
            var euCountries = new[] { "AT", "BE", "BG", "HR", "CY", "CZ", "DK", "EE", "FI", "FR", "DE", "GR", "HU", "IE", "IT", "LV", "LT", "LU", "MT", "NL", "PL", "PT", "RO", "SK", "SI", "ES", "SE", "XK", "AD", "MC", "SM", "VA" };
            foreach (var code in euCountries)
            {
                if (code != "GB") // UK is already configured
                {
                    countryIdentifierMap[code] = new List<(string, bool, string?, int?, int?, int, string?)>
                    {
                        ("REGISTRATION_NUMBER", true, null, null, 50, 1, "Business Registration Number"),
                        ("VAT", false, @"^[A-Z]{2}[A-Z0-9]{2,15}$", 4, 20, 2, "VAT number (2 letter country code + alphanumeric)")
                    };
                }
            }

            // Australia & New Zealand
            countryIdentifierMap["AU"] = new List<(string, bool, string?, int?, int?, int, string?)>
            {
                ("ABN", true, @"^[0-9]{11}$", 11, 11, 1, "11 digit Australian Business Number"),
                ("ACN", false, @"^[0-9]{9}$", 9, 9, 2, "9 digit Australian Company Number"),
                ("VAT", false, null, null, 20, 3, "GST/VAT number if applicable")
            };

            countryIdentifierMap["NZ"] = new List<(string, bool, string?, int?, int?, int, string?)>
            {
                ("REGISTRATION_NUMBER", true, null, null, 20, 1, "New Zealand Business Number (NZBN)"),
                ("VAT", false, null, null, 20, 2, "GST number if applicable")
            };

            // Canada
            countryIdentifierMap["CA"] = new List<(string, bool, string?, int?, int?, int, string?)>
            {
                ("REGISTRATION_NUMBER", true, null, null, 20, 1, "Business Registration Number"),
                ("TAX_ID", false, @"^[0-9]{9}$", 9, 9, 2, "9 digit Business Number (BN)")
            };

            // Singapore
            countryIdentifierMap["SG"] = new List<(string, bool, string?, int?, int?, int, string?)>
            {
                ("UEN", true, @"^[0-9]{8}[A-Z]$|^[STFG][0-9]{2}[A-Z]{3}[0-9]{4}[A-Z]$", 9, 10, 1, "9-10 character Unique Entity Number"),
                ("VAT", false, @"^[0-9]{8}[A-Z]$", 9, 9, 2, "GST registration number")
            };

            // Japan
            countryIdentifierMap["JP"] = new List<(string, bool, string?, int?, int?, int, string?)>
            {
                ("REGISTRATION_NUMBER", true, @"^[0-9]{13}$", 13, 13, 1, "13 digit Corporate Number"),
                ("TAX_ID", false, @"^[0-9]{13}$", 13, 13, 2, "Tax ID number")
            };

            // South Korea
            countryIdentifierMap["KR"] = new List<(string, bool, string?, int?, int?, int, string?)>
            {
                ("REGISTRATION_NUMBER", true, @"^[0-9]{6}-[0-9]{7}$", 14, 14, 1, "Business Registration Number (format: XXXXXX-XXXXXXX)"),
                ("TAX_ID", false, @"^[0-9]{10}$", 10, 10, 2, "10 digit Corporate Tax Number")
            };

            // China
            countryIdentifierMap["CN"] = new List<(string, bool, string?, int?, int?, int, string?)>
            {
                ("REGISTRATION_NUMBER", true, @"^[0-9A-Z]{15}$|^[0-9A-Z]{18}$", 15, 18, 1, "15 or 18 character Unified Social Credit Code"),
                ("TAX_ID", false, @"^[0-9]{15}$|^[0-9]{18}$", 15, 18, 2, "Tax Registration Number")
            };

            // Hong Kong
            countryIdentifierMap["HK"] = new List<(string, bool, string?, int?, int?, int, string?)>
            {
                ("REGISTRATION_NUMBER", true, @"^[0-9]{7,8}$|^[0-9]{8}$", 7, 8, 1, "7-8 digit Company Registration Number"),
                ("TAX_ID", false, null, null, 20, 2, "Tax ID if applicable")
            };

            // Malaysia
            countryIdentifierMap["MY"] = new List<(string, bool, string?, int?, int?, int, string?)>
            {
                ("REGISTRATION_NUMBER", true, @"^[0-9]{6,12}[A-Z]?$", 6, 12, 1, "6-12 digit Company Registration Number"),
                ("TAX_ID", false, @"^[0-9]{6,12}[A-Z]?$", 6, 12, 2, "Tax ID number")
            };

            // Thailand
            countryIdentifierMap["TH"] = new List<(string, bool, string?, int?, int?, int, string?)>
            {
                ("REGISTRATION_NUMBER", true, @"^[0-9]{13}$", 13, 13, 1, "13 digit Company Registration Number"),
                ("TAX_ID", false, @"^[0-9]{13}$", 13, 13, 2, "Tax ID number")
            };

            // Indonesia
            countryIdentifierMap["ID"] = new List<(string, bool, string?, int?, int?, int, string?)>
            {
                ("REGISTRATION_NUMBER", true, null, null, 50, 1, "Company Registration Number (NIB - Nomor Induk Berusaha)"),
                ("TAX_ID", false, @"^[0-9]{15}$", 15, 15, 2, "15 digit Tax ID (NPWP)")
            };

            // Philippines
            countryIdentifierMap["PH"] = new List<(string, bool, string?, int?, int?, int, string?)>
            {
                ("REGISTRATION_NUMBER", true, null, null, 50, 1, "Company Registration Number (SEC)"),
                ("TAX_ID", false, @"^[0-9]{3}-[0-9]{3}-[0-9]{3}-[0-9]{3}$", 13, 13, 2, "Tax Identification Number (TIN)")
            };

            // Vietnam
            countryIdentifierMap["VN"] = new List<(string, bool, string?, int?, int?, int, string?)>
            {
                ("REGISTRATION_NUMBER", true, @"^[0-9]{10}$|^[0-9]{13}$", 10, 13, 1, "10 or 13 digit Enterprise Registration Number"),
                ("TAX_ID", false, @"^[0-9]{10}$|^[0-9]{13}$", 10, 13, 2, "Tax Code")
            };

            // Saudi Arabia
            countryIdentifierMap["SA"] = new List<(string, bool, string?, int?, int?, int, string?)>
            {
                ("REGISTRATION_NUMBER", true, @"^[0-9]{10}$|^[0-9]{15}$", 10, 15, 1, "Commercial Registration Number"),
                ("VAT_UAE", false, @"^[0-9]{15}$", 15, 15, 2, "15 digit VAT number")
            };

            // Qatar
            countryIdentifierMap["QA"] = new List<(string, bool, string?, int?, int?, int, string?)>
            {
                ("REGISTRATION_NUMBER", true, null, null, 50, 1, "Commercial Registration Number"),
                ("VAT_UAE", false, @"^[0-9]{15}$", 15, 15, 2, "VAT number if applicable")
            };

            // Bahrain
            countryIdentifierMap["BH"] = new List<(string, bool, string?, int?, int?, int, string?)>
            {
                ("REGISTRATION_NUMBER", true, null, null, 50, 1, "Commercial Registration Number"),
                ("VAT_UAE", false, @"^[0-9]{15}$", 15, 15, 2, "VAT number if applicable")
            };

            // Kuwait
            countryIdentifierMap["KW"] = new List<(string, bool, string?, int?, int?, int, string?)>
            {
                ("REGISTRATION_NUMBER", true, null, null, 50, 1, "Commercial Registration Number"),
                ("VAT_UAE", false, null, null, 20, 2, "VAT number if applicable")
            };

            // Oman
            countryIdentifierMap["OM"] = new List<(string, bool, string?, int?, int?, int, string?)>
            {
                ("REGISTRATION_NUMBER", true, null, null, 50, 1, "Commercial Registration Number"),
                ("VAT_UAE", false, @"^[0-9]{15}$", 15, 15, 2, "VAT number if applicable")
            };

            // Brazil
            countryIdentifierMap["BR"] = new List<(string, bool, string?, int?, int?, int, string?)>
            {
                ("REGISTRATION_NUMBER", true, @"^[0-9]{2}\.?[0-9]{3}\.?[0-9]{3}\/?[0-9]{4}-?[0-9]{2}$", 14, 18, 1, "CNPJ (format: XX.XXX.XXX/XXXX-XX)"),
                ("TAX_ID", false, @"^[0-9]{11}$", 11, 11, 2, "CPF (individual tax ID) or CNPJ")
            };

            // Mexico
            countryIdentifierMap["MX"] = new List<(string, bool, string?, int?, int?, int, string?)>
            {
                ("REGISTRATION_NUMBER", true, null, null, 50, 1, "RFC (Registro Federal de Contribuyentes)"),
                ("TAX_ID", false, @"^[A-Z]{3,4}[0-9]{6}[A-Z0-9]{3}$", 12, 13, 2, "RFC format: XXXX######XXX")
            };

            // Argentina
            countryIdentifierMap["AR"] = new List<(string, bool, string?, int?, int?, int, string?)>
            {
                ("REGISTRATION_NUMBER", true, @"^[0-9]{2}-[0-9]{8}-[0-9]$", 13, 13, 1, "CUIT (format: XX-XXXXXXX-X)"),
                ("TAX_ID", false, @"^[0-9]{2}-[0-9]{8}-[0-9]$", 13, 13, 2, "Tax ID (CUIT)")
            };

            // Chile
            countryIdentifierMap["CL"] = new List<(string, bool, string?, int?, int?, int, string?)>
            {
                ("REGISTRATION_NUMBER", true, @"^[0-9]{8,9}-[0-9K]$", 10, 11, 1, "RUT (format: XXXXXXXX-X)"),
                ("TAX_ID", false, @"^[0-9]{8,9}-[0-9K]$", 10, 11, 2, "RUT (Tax ID)")
            };

            // South Africa
            countryIdentifierMap["ZA"] = new List<(string, bool, string?, int?, int?, int, string?)>
            {
                ("REGISTRATION_NUMBER", true, @"^[0-9]{4}/[0-9]{6}/[0-9]{2}$|^[0-9]{10,11}$", 10, 14, 1, "Company Registration Number"),
                ("TAX_ID", false, @"^[0-9]{10}$", 10, 10, 2, "10 digit Tax ID (Income Tax Number)")
            };

            // Nigeria
            countryIdentifierMap["NG"] = new List<(string, bool, string?, int?, int?, int, string?)>
            {
                ("REGISTRATION_NUMBER", true, @"^RC[0-9]{6,8}$", 8, 10, 1, "RC number (format: RCXXXXXX)"),
                ("TAX_ID", false, null, null, 20, 2, "Tax ID (TIN)")
            };

            // Kenya
            countryIdentifierMap["KE"] = new List<(string, bool, string?, int?, int?, int, string?)>
            {
                ("REGISTRATION_NUMBER", true, null, null, 20, 1, "Company Registration Number (CRN)"),
                ("TAX_ID", false, @"^[P][0-9]{9}[A-Z]$", 11, 11, 2, "KRA PIN (format: PXXXXXXXXX)")
            };

            // Pakistan
            countryIdentifierMap["PK"] = new List<(string, bool, string?, int?, int?, int, string?)>
            {
                ("REGISTRATION_NUMBER", true, null, null, 20, 1, "Company Registration Number"),
                ("TAX_ID", false, @"^[0-9]{7}-[0-9]$", 9, 9, 2, "NTN (format: XXXXXXX-X)")
            };

            // Bangladesh
            countryIdentifierMap["BD"] = new List<(string, bool, string?, int?, int?, int, string?)>
            {
                ("REGISTRATION_NUMBER", true, null, null, 20, 1, "Company Registration Number"),
                ("TAX_ID", false, @"^[0-9]{9,12}$", 9, 12, 2, "Tax ID (TIN)")
            };

            // Sri Lanka
            countryIdentifierMap["LK"] = new List<(string, bool, string?, int?, int?, int, string?)>
            {
                ("REGISTRATION_NUMBER", true, null, null, 20, 1, "Company Registration Number"),
                ("TAX_ID", false, @"^[0-9]{10,12}$", 10, 12, 2, "VAT/Tax Registration Number")
            };

            // Egypt
            countryIdentifierMap["EG"] = new List<(string, bool, string?, int?, int?, int, string?)>
            {
                ("REGISTRATION_NUMBER", true, @"^[0-9]{9}$", 9, 9, 1, "9 digit Commercial Registration Number"),
                ("TAX_ID", false, @"^[0-9]{9}$|^[0-9]{14}$", 9, 14, 2, "Tax ID number")
            };

            // Israel
            countryIdentifierMap["IL"] = new List<(string, bool, string?, int?, int?, int, string?)>
            {
                ("REGISTRATION_NUMBER", true, @"^[0-9]{9}$", 9, 9, 1, "9 digit Company Number"),
                ("TAX_ID", false, @"^[0-9]{9}$", 9, 9, 2, "Tax ID (format: 9 digits)")
            };

            // Turkey
            countryIdentifierMap["TR"] = new List<(string, bool, string?, int?, int?, int, string?)>
            {
                ("REGISTRATION_NUMBER", true, @"^[0-9]{10}$", 10, 10, 1, "10 digit Tax Office Registration Number"),
                ("TAX_ID", false, @"^[0-9]{10}$", 10, 10, 2, "Tax ID (Vergi No)")
            };

            // Russia
            countryIdentifierMap["RU"] = new List<(string, bool, string?, int?, int?, int, string?)>
            {
                ("REGISTRATION_NUMBER", true, @"^[0-9]{10}$|^[0-9]{13}$", 10, 13, 1, "INN (10 or 13 digits)"),
                ("TAX_ID", false, @"^[0-9]{10}$|^[0-9]{13}$", 10, 13, 2, "Tax ID (INN)")
            };

            // Switzerland
            countryIdentifierMap["CH"] = new List<(string, bool, string?, int?, int?, int, string?)>
            {
                ("REGISTRATION_NUMBER", true, @"^CHE-[0-9]{3}\.[0-9]{3}\.[0-9]{3}$|^[0-9]{6}\.[0-9]{3}$", 9, 15, 1, "CHE-XXX.XXX.XXX or XXXXXX.XXX"),
                ("VAT", false, @"^CHE-[0-9]{3}\.[0-9]{3}\.[0-9]{3}$", 13, 13, 2, "VAT number")
            };

            // Now configure each country with identifiers
            foreach (var country in countriesToConfigure)
            {
                if (countryIdentifierMap.ContainsKey(country.CountryCode))
                {
                    var identifiers = countryIdentifierMap[country.CountryCode];
                    foreach (var id in identifiers)
                    {
                        if (identifierTypeDict.ContainsKey(id.TypeName))
                        {
                            db.CountryIdentifierConfigurations.Add(new CountryIdentifierConfiguration
                            {
                                ConfigurationId = Guid.NewGuid(),
                                CountryId = country.CountryId,
                                IdentifierTypeId = identifierTypeDict[id.TypeName],
                                IsRequired = id.IsRequired,
                                ValidationRegex = id.ValidationRegex,
                                MinLength = id.MinLength,
                                MaxLength = id.MaxLength,
                                DisplayOrder = id.DisplayOrder,
                                HelpText = id.HelpText,
                                IsActive = true,
                                CreatedAt = nowOffset,
                                UpdatedAt = nowOffset
                            });
                        }
                    }
                }
                else
                {
                    // Generic fallback for countries without specific requirements
                    db.CountryIdentifierConfigurations.Add(new CountryIdentifierConfiguration
                    {
                        ConfigurationId = Guid.NewGuid(),
                        CountryId = country.CountryId,
                        IdentifierTypeId = identifierTypeDict["REGISTRATION_NUMBER"],
                        IsRequired = true,
                        ValidationRegex = null,
                        MinLength = null,
                        MaxLength = 50,
                        DisplayOrder = 1,
                        HelpText = "Business Registration Number",
                        IsActive = true,
                        CreatedAt = nowOffset,
                        UpdatedAt = nowOffset
                    });

                    // Add VAT if country uses VAT
                    if (country.TaxFrameworkType == TaxFrameworkType.VAT)
                    {
                        db.CountryIdentifierConfigurations.Add(new CountryIdentifierConfiguration
                        {
                            ConfigurationId = Guid.NewGuid(),
                            CountryId = country.CountryId,
                            IdentifierTypeId = identifierTypeDict["VAT"],
                            IsRequired = false,
                            ValidationRegex = null,
                            MinLength = null,
                            MaxLength = 20,
                            DisplayOrder = 2,
                            HelpText = "VAT number if applicable",
                            IsActive = true,
                            CreatedAt = nowOffset,
                            UpdatedAt = nowOffset
                        });
                    }
                }
            }

            // Configure Bank Details for all countries
            var configuredBankCountryIds = db.CountryBankFieldConfigurations.Select(c => c.CountryId).Distinct().ToHashSet();
            var countriesToConfigureBank = allCountries.Where(c => !configuredBankCountryIds.Contains(c.CountryId)).ToList();

            foreach (var country in countriesToConfigureBank)
            {
                var bankFields = new List<(string TypeName, bool IsRequired, string? ValidationRegex, int? MinLength, int? MaxLength, int DisplayOrder, string? HelpText)>();

                // Determine bank fields based on country
                switch (country.CountryCode)
                {
                    case "IN":
                        // Already configured above
                        break;
                    case "US":
                    case "CA":
                        // USA and Canada use Routing Number
                        bankFields.Add(("BANK_NAME", true, null, null, 200, 1, "Name of the bank"));
                        bankFields.Add(("ACCOUNT_NUMBER", true, @"^[0-9]{8,17}$", 8, 17, 2, "Account number"));
                        bankFields.Add(("ROUTING_NUMBER", true, @"^[0-9]{9}$", 9, 9, 3, "9 digit routing number"));
                        bankFields.Add(("SWIFT", false, @"^[A-Z]{4}[A-Z]{2}[A-Z0-9]{2}([A-Z0-9]{3})?$", 8, 11, 4, "SWIFT code for international transfers"));
                        break;
                    case "GB":
                        // Already configured above
                        break;
                    case "AE":
                    case "SA":
                    case "QA":
                    case "BH":
                    case "KW":
                    case "OM":
                        // Gulf countries use IBAN + SWIFT
                        bankFields.Add(("BANK_NAME", true, null, null, 200, 1, "Name of the bank"));
                        bankFields.Add(("ACCOUNT_NUMBER", true, @"^[0-9]{9,18}$", 9, 18, 2, "Account number"));
                        bankFields.Add(("IBAN", true, @"^[A-Z]{2}[0-9]{2}[0-9]{4,30}$", 15, 34, 3, "IBAN"));
                        bankFields.Add(("SWIFT", true, @"^[A-Z]{4}[A-Z]{2}[A-Z0-9]{2}([A-Z0-9]{3})?$", 8, 11, 4, "SWIFT code"));
                        break;
                    case "AU":
                        // Australia uses BSB
                        bankFields.Add(("BANK_NAME", true, null, null, 200, 1, "Name of the bank"));
                        bankFields.Add(("ACCOUNT_NUMBER", true, @"^[0-9]{6,10}$", 6, 10, 2, "Account number"));
                        bankFields.Add(("BSB", true, @"^[0-9]{6}$", 6, 6, 3, "6 digit BSB code"));
                        bankFields.Add(("SWIFT", false, @"^[A-Z]{4}[A-Z]{2}[A-Z0-9]{2}([A-Z0-9]{3})?$", 8, 11, 4, "SWIFT code"));
                        break;
                    case "NZ":
                        // New Zealand
                        bankFields.Add(("BANK_NAME", true, null, null, 200, 1, "Name of the bank"));
                        bankFields.Add(("ACCOUNT_NUMBER", true, null, null, 20, 2, "Account number"));
                        bankFields.Add(("BRANCH_CODE", true, @"^[0-9]{4}$", 4, 4, 3, "4 digit branch code"));
                        bankFields.Add(("SWIFT", false, @"^[A-Z]{4}[A-Z]{2}[A-Z0-9]{2}([A-Z0-9]{3})?$", 8, 11, 4, "SWIFT code"));
                        break;
                    default:
                        // Generic/Default: Use IBAN for most countries (EU, etc.) or SWIFT + Account Number
                        if (euCountries.Contains(country.CountryCode) || 
                            new[] { "CH", "NO", "IS", "LI", "ME", "RS", "AL", "BA", "MK", "MD", "UA", "BY" }.Contains(country.CountryCode))
                        {
                            // European countries: IBAN + SWIFT
                            bankFields.Add(("BANK_NAME", true, null, null, 200, 1, "Name of the bank"));
                            bankFields.Add(("ACCOUNT_NUMBER", false, null, null, 20, 2, "Account number (local format)"));
                            bankFields.Add(("IBAN", true, @"^[A-Z]{2}[0-9]{2}[0-9A-Z]{4,30}$", 15, 34, 3, "IBAN"));
                            bankFields.Add(("SWIFT", true, @"^[A-Z]{4}[A-Z]{2}[A-Z0-9]{2}([A-Z0-9]{3})?$", 8, 11, 4, "SWIFT code"));
                        }
                        else
                        {
                            // Generic: Bank Name + Account Number + SWIFT
                            bankFields.Add(("BANK_NAME", true, null, null, 200, 1, "Name of the bank"));
                            bankFields.Add(("ACCOUNT_NUMBER", true, @"^[0-9]{6,20}$", 6, 20, 2, "Account number"));
                            bankFields.Add(("SWIFT", false, @"^[A-Z]{4}[A-Z]{2}[A-Z0-9]{2}([A-Z0-9]{3})?$", 8, 11, 3, "SWIFT code for international transfers"));
                            bankFields.Add(("IBAN", false, @"^[A-Z]{2}[0-9]{2}[0-9A-Z]{4,30}$", 15, 34, 4, "IBAN if available"));
                        }
                        break;
                }

                // Add bank field configurations
                foreach (var bf in bankFields)
                {
                    if (bankFieldTypeDict.ContainsKey(bf.TypeName))
                    {
                        db.CountryBankFieldConfigurations.Add(new CountryBankFieldConfiguration
                        {
                            ConfigurationId = Guid.NewGuid(),
                            CountryId = country.CountryId,
                            BankFieldTypeId = bankFieldTypeDict[bf.TypeName],
                            IsRequired = bf.IsRequired,
                            ValidationRegex = bf.ValidationRegex,
                            MinLength = bf.MinLength,
                            MaxLength = bf.MaxLength,
                            DisplayOrder = bf.DisplayOrder,
                            HelpText = bf.HelpText,
                            IsActive = true,
                            CreatedAt = nowOffset,
                            UpdatedAt = nowOffset
                        });
                    }
                }
            }

            Console.WriteLine($"[Seeder] Configured {countriesToConfigure.Count} countries with identifiers and {countriesToConfigureBank.Count} countries with bank details");
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
