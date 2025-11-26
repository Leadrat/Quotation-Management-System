using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;
using CRM.Domain.Enums;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    public partial class SeedTaxManagementInitialData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var now = DateTimeOffset.UtcNow;
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            // Predefined GUIDs for consistency
            var indiaCountryId = Guid.Parse("A1B2C3D4-E5F6-47G8-H9I0-J1K2L3M4N5O6");
            var uaeCountryId = Guid.Parse("B2C3D4E5-F6G7-48H9-I0J1-K2L3M4N5O6P7");
            
            var indiaFrameworkId = Guid.Parse("E5F6G7H8-I9J0-51K1-L2M3-N4O5P6Q7R8S9");
            var uaeFrameworkId = Guid.Parse("F6G7H8I9-J0K1-52L2-M3N4-O5P6Q7R8S9T0");
            
            var maharashtraJurisdictionId = Guid.Parse("C3D4E5F6-G7H8-49I0-J1K2-L3M4N5O6P7Q8");
            var karnatakaJurisdictionId = Guid.Parse("C4D5E6F7-H8I9-50J1-K2L3-M5N6O7P8Q9R0");
            var dubaiJurisdictionId = Guid.Parse("D4E5F6G7-H8I9-50J0-K1L2-M3N4O5P6Q7R8");
            var abuDhabiJurisdictionId = Guid.Parse("D5E6F7G8-I9J0-51K1-L2M3-N4O5P6Q7R8S9");
            
            var servicesCategoryId = Guid.Parse("I9J0K1L2-M3N4-55O5-P6Q7-R8S9T0U1V2W3");
            var productsCategoryId = Guid.Parse("J0K1L2M3-N4O5-56P6-Q7R8-S9T0U1V2W3X4");
            var softwareCategoryId = Guid.Parse("K1L2M3N4-O5P6-57Q7-R8S9-T0U1V2W3X4Y5");

            // Seed Countries
            var nowStr = now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            migrationBuilder.Sql($@"
                INSERT INTO ""Countries"" (""CountryId"", ""CountryName"", ""CountryCode"", ""TaxFrameworkType"", ""DefaultCurrency"", ""IsActive"", ""IsDefault"", ""CreatedAt"", ""UpdatedAt"", ""DeletedAt"")
                VALUES 
                    ('{indiaCountryId}', 'India', 'IN', {(int)TaxFrameworkType.GST}, 'INR', true, true, '{nowStr}', '{nowStr}', NULL),
                    ('{uaeCountryId}', 'United Arab Emirates', 'AE', {(int)TaxFrameworkType.VAT}, 'AED', true, false, '{nowStr}', '{nowStr}', NULL)
                ON CONFLICT (""CountryId"") DO NOTHING;
            ");

            // Seed Tax Frameworks
            var indiaTaxComponents = JsonSerializer.Serialize(new[]
            {
                new { name = "CGST", code = "CGST", isCentrallyGoverned = false, description = "Central Goods and Services Tax" },
                new { name = "SGST", code = "SGST", isCentrallyGoverned = false, description = "State Goods and Services Tax" },
                new { name = "IGST", code = "IGST", isCentrallyGoverned = true, description = "Integrated Goods and Services Tax" }
            });

            var uaeTaxComponents = JsonSerializer.Serialize(new[]
            {
                new { name = "VAT", code = "VAT", isCentrallyGoverned = true, description = "Value Added Tax" }
            });

            migrationBuilder.Sql($@"
                INSERT INTO ""TaxFrameworks"" (""TaxFrameworkId"", ""CountryId"", ""FrameworkName"", ""FrameworkType"", ""Description"", ""TaxComponents"", ""IsActive"", ""CreatedAt"", ""UpdatedAt"", ""DeletedAt"")
                VALUES 
                    ('{indiaFrameworkId}', '{indiaCountryId}', 'Goods and Services Tax', {(int)TaxFrameworkType.GST}, 'GST framework for India with CGST, SGST, and IGST components', '{indiaTaxComponents.Replace("'", "''")}', true, '{nowStr}', '{nowStr}', NULL),
                    ('{uaeFrameworkId}', '{uaeCountryId}', 'Value Added Tax', {(int)TaxFrameworkType.VAT}, 'VAT framework for UAE', '{uaeTaxComponents.Replace("'", "''")}', true, '{nowStr}', '{nowStr}', NULL)
                ON CONFLICT (""TaxFrameworkId"") DO NOTHING;
            ");

            // Seed Jurisdictions
            migrationBuilder.Sql($@"
                INSERT INTO ""Jurisdictions"" (""JurisdictionId"", ""CountryId"", ""ParentJurisdictionId"", ""JurisdictionName"", ""JurisdictionCode"", ""JurisdictionType"", ""IsActive"", ""CreatedAt"", ""UpdatedAt"", ""DeletedAt"")
                VALUES 
                    ('{maharashtraJurisdictionId}', '{indiaCountryId}', NULL, 'Maharashtra', '27', 'State', true, '{nowStr}', '{nowStr}', NULL),
                    ('{karnatakaJurisdictionId}', '{indiaCountryId}', NULL, 'Karnataka', '29', 'State', true, '{nowStr}', '{nowStr}', NULL),
                    ('{dubaiJurisdictionId}', '{uaeCountryId}', NULL, 'Dubai', 'DXB', 'Emirate', true, '{nowStr}', '{nowStr}', NULL),
                    ('{abuDhabiJurisdictionId}', '{uaeCountryId}', NULL, 'Abu Dhabi', 'AUH', 'Emirate', true, '{nowStr}', '{nowStr}', NULL)
                ON CONFLICT (""JurisdictionId"") DO NOTHING;
            ");

            // Seed Product/Service Categories
            migrationBuilder.Sql($@"
                INSERT INTO ""ProductServiceCategories"" (""CategoryId"", ""CategoryName"", ""CategoryCode"", ""Description"", ""IsActive"", ""CreatedAt"", ""UpdatedAt"", ""DeletedAt"")
                VALUES 
                    ('{servicesCategoryId}', 'Services', 'SRV', 'Professional services', true, '{nowStr}', '{nowStr}', NULL),
                    ('{productsCategoryId}', 'Products', 'PROD', 'Physical products', true, '{nowStr}', '{nowStr}', NULL),
                    ('{softwareCategoryId}', 'Software', 'SW', 'Software products and licenses', true, '{nowStr}', '{nowStr}', NULL)
                ON CONFLICT (""CategoryId"") DO NOTHING;
            ");

            // Seed Tax Rates for India (Maharashtra - 18% GST split as 9% CGST + 9% SGST)
            var maharashtraGstComponents = JsonSerializer.Serialize(new[]
            {
                new { component = "CGST", rate = 9.0m },
                new { component = "SGST", rate = 9.0m }
            });

            var maharashtraGstRateId = Guid.NewGuid();
            var todayStr = today.ToString("yyyy-MM-dd");
            migrationBuilder.Sql($@"
                INSERT INTO ""TaxRates"" (""TaxRateId"", ""JurisdictionId"", ""TaxFrameworkId"", ""ProductServiceCategoryId"", ""TaxRate"", ""EffectiveFrom"", ""EffectiveTo"", ""IsExempt"", ""IsZeroRated"", ""TaxComponents"", ""Description"", ""CreatedAt"", ""UpdatedAt"")
                VALUES 
                    ('{maharashtraGstRateId}', '{maharashtraJurisdictionId}', '{indiaFrameworkId}', NULL, 18.00, '{todayStr}', NULL, false, false, '{maharashtraGstComponents.Replace("'", "''")}', 'Standard GST rate for Maharashtra', '{nowStr}', '{nowStr}')
                ON CONFLICT (""TaxRateId"") DO NOTHING;
            ");

            // Seed Tax Rates for India (Karnataka - 18% GST split as 9% CGST + 9% SGST)
            var karnatakaGstComponents = JsonSerializer.Serialize(new[]
            {
                new { component = "CGST", rate = 9.0m },
                new { component = "SGST", rate = 9.0m }
            });

            var karnatakaGstRateId = Guid.NewGuid();
            migrationBuilder.Sql($@"
                INSERT INTO ""TaxRates"" (""TaxRateId"", ""JurisdictionId"", ""TaxFrameworkId"", ""ProductServiceCategoryId"", ""TaxRate"", ""EffectiveFrom"", ""EffectiveTo"", ""IsExempt"", ""IsZeroRated"", ""TaxComponents"", ""Description"", ""CreatedAt"", ""UpdatedAt"")
                VALUES 
                    ('{karnatakaGstRateId}', '{karnatakaJurisdictionId}', '{indiaFrameworkId}', NULL, 18.00, '{todayStr}', NULL, false, false, '{karnatakaGstComponents.Replace("'", "''")}', 'Standard GST rate for Karnataka', '{nowStr}', '{nowStr}')
                ON CONFLICT (""TaxRateId"") DO NOTHING;
            ");

            // Seed Tax Rates for UAE (Dubai - 5% VAT)
            var dubaiVatComponents = JsonSerializer.Serialize(new[]
            {
                new { component = "VAT", rate = 5.0m }
            });

            var dubaiVatRateId = Guid.NewGuid();
            migrationBuilder.Sql($@"
                INSERT INTO ""TaxRates"" (""TaxRateId"", ""JurisdictionId"", ""TaxFrameworkId"", ""ProductServiceCategoryId"", ""TaxRate"", ""EffectiveFrom"", ""EffectiveTo"", ""IsExempt"", ""IsZeroRated"", ""TaxComponents"", ""Description"", ""CreatedAt"", ""UpdatedAt"")
                VALUES 
                    ('{dubaiVatRateId}', '{dubaiJurisdictionId}', '{uaeFrameworkId}', NULL, 5.00, '{todayStr}', NULL, false, false, '{dubaiVatComponents.Replace("'", "''")}', 'Standard VAT rate for Dubai', '{nowStr}', '{nowStr}')
                ON CONFLICT (""TaxRateId"") DO NOTHING;
            ");

            // Seed Tax Rates for UAE (Abu Dhabi - 5% VAT)
            var abuDhabiVatComponents = JsonSerializer.Serialize(new[]
            {
                new { component = "VAT", rate = 5.0m }
            });

            var abuDhabiVatRateId = Guid.NewGuid();
            migrationBuilder.Sql($@"
                INSERT INTO ""TaxRates"" (""TaxRateId"", ""JurisdictionId"", ""TaxFrameworkId"", ""ProductServiceCategoryId"", ""TaxRate"", ""EffectiveFrom"", ""EffectiveTo"", ""IsExempt"", ""IsZeroRated"", ""TaxComponents"", ""Description"", ""CreatedAt"", ""UpdatedAt"")
                VALUES 
                    ('{abuDhabiVatRateId}', '{abuDhabiJurisdictionId}', '{uaeFrameworkId}', NULL, 5.00, '{todayStr}', NULL, false, false, '{abuDhabiVatComponents.Replace("'", "''")}', 'Standard VAT rate for Abu Dhabi', '{nowStr}', '{nowStr}')
                ON CONFLICT (""TaxRateId"") DO NOTHING;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove seeded data (in reverse order due to foreign key constraints)
            
            // Delete Tax Rates
            migrationBuilder.Sql(@"
                DELETE FROM ""TaxRates"" 
                WHERE ""JurisdictionId"" IN (
                    SELECT ""JurisdictionId"" FROM ""Jurisdictions"" 
                    WHERE ""CountryId"" IN (
                        SELECT ""CountryId"" FROM ""Countries"" 
                        WHERE ""CountryCode"" IN ('IN', 'AE')
                    )
                );
            ");

            // Delete Product/Service Categories
            migrationBuilder.Sql(@"
                DELETE FROM ""ProductServiceCategories"" 
                WHERE ""CategoryCode"" IN ('SRV', 'PROD', 'SW');
            ");

            // Delete Jurisdictions
            migrationBuilder.Sql(@"
                DELETE FROM ""Jurisdictions"" 
                WHERE ""CountryId"" IN (
                    SELECT ""CountryId"" FROM ""Countries"" 
                    WHERE ""CountryCode"" IN ('IN', 'AE')
                );
            ");

            // Delete Tax Frameworks
            migrationBuilder.Sql(@"
                DELETE FROM ""TaxFrameworks"" 
                WHERE ""CountryId"" IN (
                    SELECT ""CountryId"" FROM ""Countries"" 
                    WHERE ""CountryCode"" IN ('IN', 'AE')
                );
            ");

            // Delete Countries
            migrationBuilder.Sql(@"
                DELETE FROM ""Countries"" 
                WHERE ""CountryCode"" IN ('IN', 'AE');
            ");
        }
    }
}

