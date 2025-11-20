using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductCatalogTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Helper function to conditionally create tables and columns
            // All table/column creations are wrapped in IF NOT EXISTS checks

            // Conditionally add columns to Quotations table
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Quotations' AND column_name = 'TaxCountryId') THEN
                        ALTER TABLE ""Quotations"" ADD COLUMN ""TaxCountryId"" uuid;
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Quotations' AND column_name = 'TaxFrameworkId') THEN
                        ALTER TABLE ""Quotations"" ADD COLUMN ""TaxFrameworkId"" uuid;
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Quotations' AND column_name = 'TaxJurisdictionId') THEN
                        ALTER TABLE ""Quotations"" ADD COLUMN ""TaxJurisdictionId"" uuid;
                    END IF;
                END $$;
            ");

            // Conditionally add columns to QuotationLineItems
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'QuotationLineItems' AND column_name = 'BillingCycle') THEN
                        ALTER TABLE ""QuotationLineItems"" ADD COLUMN ""BillingCycle"" integer;
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'QuotationLineItems' AND column_name = 'DiscountAmount') THEN
                        ALTER TABLE ""QuotationLineItems"" ADD COLUMN ""DiscountAmount"" numeric(18,2);
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'QuotationLineItems' AND column_name = 'Hours') THEN
                        ALTER TABLE ""QuotationLineItems"" ADD COLUMN ""Hours"" numeric(10,2);
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'QuotationLineItems' AND column_name = 'OriginalProductPrice') THEN
                        ALTER TABLE ""QuotationLineItems"" ADD COLUMN ""OriginalProductPrice"" numeric(18,2);
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'QuotationLineItems' AND column_name = 'ProductId') THEN
                        ALTER TABLE ""QuotationLineItems"" ADD COLUMN ""ProductId"" uuid;
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'QuotationLineItems' AND column_name = 'ProductServiceCategoryId') THEN
                        ALTER TABLE ""QuotationLineItems"" ADD COLUMN ""ProductServiceCategoryId"" uuid;
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'QuotationLineItems' AND column_name = 'TaxCategoryId') THEN
                        ALTER TABLE ""QuotationLineItems"" ADD COLUMN ""TaxCategoryId"" uuid;
                    END IF;
                END $$;
            ");

            // Conditionally add columns to Clients
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Clients' AND column_name = 'CountryId') THEN
                        ALTER TABLE ""Clients"" ADD COLUMN ""CountryId"" uuid;
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Clients' AND column_name = 'JurisdictionId') THEN
                        ALTER TABLE ""Clients"" ADD COLUMN ""JurisdictionId"" uuid;
                    END IF;
                END $$;
            ");

            // Conditionally create Countries table
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'Countries') THEN
                        CREATE TABLE ""Countries"" (
                            ""CountryId"" uuid NOT NULL,
                            ""CountryName"" character varying(100) NOT NULL,
                            ""CountryCode"" character varying(2) NOT NULL,
                            ""TaxFrameworkType"" integer NOT NULL,
                            ""DefaultCurrency"" character varying(3) NOT NULL,
                            ""IsActive"" boolean NOT NULL DEFAULT TRUE,
                            ""IsDefault"" boolean NOT NULL DEFAULT FALSE,
                            ""CreatedAt"" timestamp with time zone NOT NULL,
                            ""UpdatedAt"" timestamp with time zone NOT NULL,
                            ""DeletedAt"" timestamp with time zone,
                            CONSTRAINT ""PK_Countries"" PRIMARY KEY (""CountryId"")
                        );
                    END IF;
                END $$;
            ");

            // Conditionally create ProductCategories table
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'ProductCategories') THEN
                        CREATE TABLE ""ProductCategories"" (
                            ""CategoryId"" uuid NOT NULL,
                            ""CategoryName"" character varying(100) NOT NULL,
                            ""CategoryCode"" character varying(50) NOT NULL,
                            ""Description"" character varying(1000),
                            ""ParentCategoryId"" uuid,
                            ""IsActive"" boolean NOT NULL DEFAULT TRUE,
                            ""CreatedAt"" timestamp with time zone NOT NULL,
                            ""UpdatedAt"" timestamp with time zone NOT NULL,
                            ""CreatedByUserId"" uuid,
                            ""UpdatedByUserId"" uuid,
                            CONSTRAINT ""PK_ProductCategories"" PRIMARY KEY (""CategoryId""),
                            CONSTRAINT ""FK_ProductCategories_ProductCategories_ParentCategoryId"" FOREIGN KEY (""ParentCategoryId"") REFERENCES ""ProductCategories"" (""CategoryId"") ON DELETE SET NULL,
                            CONSTRAINT ""FK_ProductCategories_Users_CreatedByUserId"" FOREIGN KEY (""CreatedByUserId"") REFERENCES ""Users"" (""UserId"") ON DELETE SET NULL,
                            CONSTRAINT ""FK_ProductCategories_Users_UpdatedByUserId"" FOREIGN KEY (""UpdatedByUserId"") REFERENCES ""Users"" (""UserId"") ON DELETE SET NULL
                        );
                        CREATE UNIQUE INDEX ""IX_ProductCategories_CategoryCode"" ON ""ProductCategories"" (""CategoryCode"");
                        CREATE INDEX ""IX_ProductCategories_ParentCategoryId"" ON ""ProductCategories"" (""ParentCategoryId"");
                        CREATE INDEX ""IX_ProductCategories_IsActive"" ON ""ProductCategories"" (""IsActive"");
                        CREATE INDEX ""IX_ProductCategories_CategoryName"" ON ""ProductCategories"" (""CategoryName"");
                    END IF;
                END $$;
            ");

            // Conditionally create ProductServiceCategories table
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'ProductServiceCategories') THEN
                        CREATE TABLE ""ProductServiceCategories"" (
                            ""CategoryId"" uuid NOT NULL,
                            ""CategoryName"" character varying(100) NOT NULL,
                            ""CategoryCode"" character varying(20),
                            ""Description"" character varying(500),
                            ""IsActive"" boolean NOT NULL DEFAULT TRUE,
                            ""CreatedAt"" timestamp with time zone NOT NULL,
                            ""UpdatedAt"" timestamp with time zone NOT NULL,
                            ""DeletedAt"" timestamp with time zone,
                            CONSTRAINT ""PK_ProductServiceCategories"" PRIMARY KEY (""CategoryId"")
                        );
                    END IF;
                END $$;
            ");

            // Conditionally create Jurisdictions table
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'Jurisdictions') THEN
                        CREATE TABLE ""Jurisdictions"" (
                            ""JurisdictionId"" uuid NOT NULL,
                            ""CountryId"" uuid NOT NULL,
                            ""ParentJurisdictionId"" uuid,
                            ""JurisdictionName"" character varying(100) NOT NULL,
                            ""JurisdictionCode"" character varying(20),
                            ""JurisdictionType"" character varying(20),
                            ""IsActive"" boolean NOT NULL DEFAULT TRUE,
                            ""CreatedAt"" timestamp with time zone NOT NULL,
                            ""UpdatedAt"" timestamp with time zone NOT NULL,
                            ""DeletedAt"" timestamp with time zone,
                            CONSTRAINT ""PK_Jurisdictions"" PRIMARY KEY (""JurisdictionId""),
                            CONSTRAINT ""FK_Jurisdictions_Countries_CountryId"" FOREIGN KEY (""CountryId"") REFERENCES ""Countries"" (""CountryId"") ON DELETE CASCADE,
                            CONSTRAINT ""FK_Jurisdictions_Jurisdictions_ParentJurisdictionId"" FOREIGN KEY (""ParentJurisdictionId"") REFERENCES ""Jurisdictions"" (""JurisdictionId"") ON DELETE SET NULL
                        );
                        CREATE INDEX ""IX_Jurisdictions_CountryId"" ON ""Jurisdictions"" (""CountryId"");
                        CREATE INDEX ""IX_Jurisdictions_ParentJurisdictionId"" ON ""Jurisdictions"" (""ParentJurisdictionId"");
                    END IF;
                END $$;
            ");

            // Conditionally create TaxFrameworks table
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'TaxFrameworks') THEN
                        CREATE TABLE ""TaxFrameworks"" (
                            ""TaxFrameworkId"" uuid NOT NULL,
                            ""CountryId"" uuid NOT NULL,
                            ""FrameworkName"" character varying(100) NOT NULL,
                            ""FrameworkType"" integer NOT NULL,
                            ""Description"" text,
                            ""TaxComponents"" jsonb NOT NULL,
                            ""IsActive"" boolean NOT NULL DEFAULT TRUE,
                            ""CreatedAt"" timestamp with time zone NOT NULL,
                            ""UpdatedAt"" timestamp with time zone NOT NULL,
                            ""DeletedAt"" timestamp with time zone,
                            CONSTRAINT ""PK_TaxFrameworks"" PRIMARY KEY (""TaxFrameworkId""),
                            CONSTRAINT ""FK_TaxFrameworks_Countries_CountryId"" FOREIGN KEY (""CountryId"") REFERENCES ""Countries"" (""CountryId"") ON DELETE CASCADE
                        );
                        CREATE INDEX ""IX_TaxFrameworks_CountryId"" ON ""TaxFrameworks"" (""CountryId"");
                    END IF;
                END $$;
            ");

            // Conditionally create Products table
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'Products') THEN
                        CREATE TABLE ""Products"" (
                            ""ProductId"" uuid NOT NULL,
                            ""ProductName"" character varying(200) NOT NULL,
                            ""ProductType"" integer NOT NULL,
                            ""Description"" character varying(2000),
                            ""CategoryId"" uuid,
                            ""BasePricePerUserPerMonth"" numeric(18,2),
                            ""BillingCycleMultipliers"" jsonb,
                            ""AddOnPricing"" jsonb,
                            ""CustomDevelopmentPricing"" jsonb,
                            ""Currency"" character varying(3) NOT NULL DEFAULT 'USD',
                            ""IsActive"" boolean NOT NULL DEFAULT TRUE,
                            ""CreatedAt"" timestamp with time zone NOT NULL,
                            ""UpdatedAt"" timestamp with time zone NOT NULL,
                            ""CreatedByUserId"" uuid,
                            ""UpdatedByUserId"" uuid,
                            CONSTRAINT ""PK_Products"" PRIMARY KEY (""ProductId""),
                            CONSTRAINT ""FK_Products_ProductCategories_CategoryId"" FOREIGN KEY (""CategoryId"") REFERENCES ""ProductCategories"" (""CategoryId"") ON DELETE SET NULL,
                            CONSTRAINT ""FK_Products_Users_CreatedByUserId"" FOREIGN KEY (""CreatedByUserId"") REFERENCES ""Users"" (""UserId"") ON DELETE SET NULL,
                            CONSTRAINT ""FK_Products_Users_UpdatedByUserId"" FOREIGN KEY (""UpdatedByUserId"") REFERENCES ""Users"" (""UserId"") ON DELETE SET NULL
                        );
                        CREATE UNIQUE INDEX ""IX_Products_ProductName"" ON ""Products"" (""ProductName"");
                        CREATE INDEX ""IX_Products_ProductType"" ON ""Products"" (""ProductType"");
                        CREATE INDEX ""IX_Products_CategoryId"" ON ""Products"" (""CategoryId"");
                        CREATE INDEX ""IX_Products_IsActive"" ON ""Products"" (""IsActive"");
                    END IF;
                END $$;
            ");

            // Conditionally create ProductPriceHistory table
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'ProductPriceHistory') THEN
                        CREATE TABLE ""ProductPriceHistory"" (
                            ""PriceHistoryId"" uuid NOT NULL,
                            ""ProductId"" uuid NOT NULL,
                            ""PriceType"" integer NOT NULL,
                            ""PriceValue"" numeric(18,4) NOT NULL,
                            ""OldPriceValue"" numeric(18,4),
                            ""NewPriceValue"" numeric(18,4),
                            ""EffectiveFrom"" timestamp with time zone NOT NULL,
                            ""EffectiveTo"" timestamp with time zone,
                            ""ChangedByUserId"" uuid NOT NULL,
                            ""ChangedAt"" timestamp with time zone NOT NULL,
                            CONSTRAINT ""PK_ProductPriceHistory"" PRIMARY KEY (""PriceHistoryId""),
                            CONSTRAINT ""FK_ProductPriceHistory_Products_ProductId"" FOREIGN KEY (""ProductId"") REFERENCES ""Products"" (""ProductId"") ON DELETE CASCADE,
                            CONSTRAINT ""FK_ProductPriceHistory_Users_ChangedByUserId"" FOREIGN KEY (""ChangedByUserId"") REFERENCES ""Users"" (""UserId"") ON DELETE RESTRICT
                        );
                        CREATE INDEX ""IX_ProductPriceHistory_ProductId"" ON ""ProductPriceHistory"" (""ProductId"");
                        CREATE INDEX ""IX_ProductPriceHistory_EffectiveFrom"" ON ""ProductPriceHistory"" (""EffectiveFrom"");
                        CREATE INDEX ""IX_ProductPriceHistory_ChangedByUserId"" ON ""ProductPriceHistory"" (""ChangedByUserId"");
                    END IF;
                END $$;
            ");

            // Conditionally create TaxCalculationLogs table
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'TaxCalculationLogs') THEN
                        CREATE TABLE ""TaxCalculationLogs"" (
                            ""LogId"" uuid NOT NULL,
                            ""QuotationId"" uuid,
                            ""ActionType"" integer NOT NULL,
                            ""CountryId"" uuid,
                            ""JurisdictionId"" uuid,
                            ""CalculationDetails"" jsonb NOT NULL DEFAULT '{}',
                            ""ChangedByUserId"" uuid NOT NULL,
                            ""ChangedAt"" timestamp with time zone NOT NULL,
                            CONSTRAINT ""PK_TaxCalculationLogs"" PRIMARY KEY (""LogId""),
                            CONSTRAINT ""FK_TaxCalculationLogs_Countries_CountryId"" FOREIGN KEY (""CountryId"") REFERENCES ""Countries"" (""CountryId"") ON DELETE SET NULL,
                            CONSTRAINT ""FK_TaxCalculationLogs_Jurisdictions_JurisdictionId"" FOREIGN KEY (""JurisdictionId"") REFERENCES ""Jurisdictions"" (""JurisdictionId"") ON DELETE SET NULL,
                            CONSTRAINT ""FK_TaxCalculationLogs_Quotations_QuotationId"" FOREIGN KEY (""QuotationId"") REFERENCES ""Quotations"" (""QuotationId"") ON DELETE SET NULL,
                            CONSTRAINT ""FK_TaxCalculationLogs_Users_ChangedByUserId"" FOREIGN KEY (""ChangedByUserId"") REFERENCES ""Users"" (""UserId"") ON DELETE SET NULL
                        );
                    END IF;
                END $$;
            ");

            // Conditionally create TaxRates table
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'TaxRates') THEN
                        CREATE TABLE ""TaxRates"" (
                            ""TaxRateId"" uuid NOT NULL,
                            ""TaxFrameworkId"" uuid NOT NULL,
                            ""JurisdictionId"" uuid,
                            ""CategoryId"" uuid,
                            ""RateName"" character varying(100) NOT NULL,
                            ""RateValue"" numeric(5,4) NOT NULL,
                            ""RateType"" integer NOT NULL,
                            ""EffectiveFrom"" timestamp with time zone NOT NULL,
                            ""EffectiveTo"" timestamp with time zone,
                            ""IsActive"" boolean NOT NULL DEFAULT TRUE,
                            ""CreatedAt"" timestamp with time zone NOT NULL,
                            ""UpdatedAt"" timestamp with time zone NOT NULL,
                            ""CreatedByUserId"" uuid NOT NULL,
                            ""UpdatedByUserId"" uuid,
                            CONSTRAINT ""PK_TaxRates"" PRIMARY KEY (""TaxRateId""),
                            CONSTRAINT ""FK_TaxRates_TaxFrameworks_TaxFrameworkId"" FOREIGN KEY (""TaxFrameworkId"") REFERENCES ""TaxFrameworks"" (""TaxFrameworkId"") ON DELETE CASCADE,
                            CONSTRAINT ""FK_TaxRates_Jurisdictions_JurisdictionId"" FOREIGN KEY (""JurisdictionId"") REFERENCES ""Jurisdictions"" (""JurisdictionId"") ON DELETE SET NULL,
                            CONSTRAINT ""FK_TaxRates_ProductServiceCategories_CategoryId"" FOREIGN KEY (""CategoryId"") REFERENCES ""ProductServiceCategories"" (""CategoryId"") ON DELETE SET NULL,
                            CONSTRAINT ""FK_TaxRates_Users_CreatedByUserId"" FOREIGN KEY (""CreatedByUserId"") REFERENCES ""Users"" (""UserId"") ON DELETE RESTRICT,
                            CONSTRAINT ""FK_TaxRates_Users_UpdatedByUserId"" FOREIGN KEY (""UpdatedByUserId"") REFERENCES ""Users"" (""UserId"") ON DELETE SET NULL
                        );
                        CREATE INDEX ""IX_TaxRates_TaxFrameworkId"" ON ""TaxRates"" (""TaxFrameworkId"");
                        CREATE INDEX ""IX_TaxRates_JurisdictionId"" ON ""TaxRates"" (""JurisdictionId"");
                        CREATE INDEX ""IX_TaxRates_CategoryId"" ON ""TaxRates"" (""CategoryId"");
                    END IF;
                END $$;
            ");

            // Conditionally create indexes and foreign keys for Quotations
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_Quotations_TaxCountryId') THEN
                        CREATE INDEX ""IX_Quotations_TaxCountryId"" ON ""Quotations"" (""TaxCountryId"");
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_Quotations_TaxFrameworkId') THEN
                        CREATE INDEX ""IX_Quotations_TaxFrameworkId"" ON ""Quotations"" (""TaxFrameworkId"");
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_Quotations_TaxJurisdictionId') THEN
                        CREATE INDEX ""IX_Quotations_TaxJurisdictionId"" ON ""Quotations"" (""TaxJurisdictionId"");
                    END IF;
                END $$;
            ");

            // Conditionally create foreign keys for Quotations
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM information_schema.table_constraints WHERE constraint_name = 'FK_Quotations_Countries_TaxCountryId') THEN
                        ALTER TABLE ""Quotations"" ADD CONSTRAINT ""FK_Quotations_Countries_TaxCountryId"" FOREIGN KEY (""TaxCountryId"") REFERENCES ""Countries"" (""CountryId"") ON DELETE SET NULL;
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.table_constraints WHERE constraint_name = 'FK_Quotations_TaxFrameworks_TaxFrameworkId') THEN
                        ALTER TABLE ""Quotations"" ADD CONSTRAINT ""FK_Quotations_TaxFrameworks_TaxFrameworkId"" FOREIGN KEY (""TaxFrameworkId"") REFERENCES ""TaxFrameworks"" (""TaxFrameworkId"") ON DELETE SET NULL;
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.table_constraints WHERE constraint_name = 'FK_Quotations_Jurisdictions_TaxJurisdictionId') THEN
                        ALTER TABLE ""Quotations"" ADD CONSTRAINT ""FK_Quotations_Jurisdictions_TaxJurisdictionId"" FOREIGN KEY (""TaxJurisdictionId"") REFERENCES ""Jurisdictions"" (""JurisdictionId"") ON DELETE SET NULL;
                    END IF;
                END $$;
            ");

            // Conditionally create indexes and foreign keys for QuotationLineItems
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_QuotationLineItems_ProductId') THEN
                        CREATE INDEX ""IX_QuotationLineItems_ProductId"" ON ""QuotationLineItems"" (""ProductId"");
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_QuotationLineItems_ProductServiceCategoryId') THEN
                        CREATE INDEX ""IX_QuotationLineItems_ProductServiceCategoryId"" ON ""QuotationLineItems"" (""ProductServiceCategoryId"");
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_QuotationLineItems_TaxCategoryId') THEN
                        CREATE INDEX ""IX_QuotationLineItems_TaxCategoryId"" ON ""QuotationLineItems"" (""TaxCategoryId"");
                    END IF;
                END $$;
            ");

            // Conditionally create foreign keys for QuotationLineItems
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM information_schema.table_constraints WHERE constraint_name = 'FK_QuotationLineItems_Products_ProductId') THEN
                        ALTER TABLE ""QuotationLineItems"" ADD CONSTRAINT ""FK_QuotationLineItems_Products_ProductId"" FOREIGN KEY (""ProductId"") REFERENCES ""Products"" (""ProductId"") ON DELETE SET NULL;
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.table_constraints WHERE constraint_name = 'FK_QuotationLineItems_ProductServiceCategories_ProductServiceCategoryId') THEN
                        ALTER TABLE ""QuotationLineItems"" ADD CONSTRAINT ""FK_QuotationLineItems_ProductServiceCategories_ProductServiceCategoryId"" FOREIGN KEY (""ProductServiceCategoryId"") REFERENCES ""ProductServiceCategories"" (""CategoryId"") ON DELETE SET NULL;
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.table_constraints WHERE constraint_name = 'FK_QuotationLineItems_ProductCategories_TaxCategoryId') THEN
                        ALTER TABLE ""QuotationLineItems"" ADD CONSTRAINT ""FK_QuotationLineItems_ProductCategories_TaxCategoryId"" FOREIGN KEY (""TaxCategoryId"") REFERENCES ""ProductCategories"" (""CategoryId"") ON DELETE SET NULL;
                    END IF;
                END $$;
            ");

            // Conditionally create indexes and foreign keys for Clients
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_Clients_CountryId') THEN
                        CREATE INDEX ""IX_Clients_CountryId"" ON ""Clients"" (""CountryId"");
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_Clients_JurisdictionId') THEN
                        CREATE INDEX ""IX_Clients_JurisdictionId"" ON ""Clients"" (""JurisdictionId"");
                    END IF;
                END $$;
            ");

            // Conditionally create foreign keys for Clients
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM information_schema.table_constraints WHERE constraint_name = 'FK_Clients_Countries_CountryId') THEN
                        ALTER TABLE ""Clients"" ADD CONSTRAINT ""FK_Clients_Countries_CountryId"" FOREIGN KEY (""CountryId"") REFERENCES ""Countries"" (""CountryId"") ON DELETE SET NULL;
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.table_constraints WHERE constraint_name = 'FK_Clients_Jurisdictions_JurisdictionId') THEN
                        ALTER TABLE ""Clients"" ADD CONSTRAINT ""FK_Clients_Jurisdictions_JurisdictionId"" FOREIGN KEY (""JurisdictionId"") REFERENCES ""Jurisdictions"" (""JurisdictionId"") ON DELETE SET NULL;
                    END IF;
                END $$;
            ");

            // Add check constraints for Products table
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM information_schema.table_constraints WHERE constraint_name = 'CK_Products_BasePricePerUserPerMonth') THEN
                        ALTER TABLE ""Products"" ADD CONSTRAINT ""CK_Products_BasePricePerUserPerMonth"" CHECK ((""ProductType"" = 0 AND ""BasePricePerUserPerMonth"" IS NOT NULL) OR ""ProductType"" != 0);
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.table_constraints WHERE constraint_name = 'CK_Products_AddOnPricing') THEN
                        ALTER TABLE ""Products"" ADD CONSTRAINT ""CK_Products_AddOnPricing"" CHECK ((""ProductType"" IN (1, 2) AND ""AddOnPricing"" IS NOT NULL) OR ""ProductType"" NOT IN (1, 2));
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.table_constraints WHERE constraint_name = 'CK_Products_CustomDevelopmentPricing') THEN
                        ALTER TABLE ""Products"" ADD CONSTRAINT ""CK_Products_CustomDevelopmentPricing"" CHECK ((""ProductType"" = 3 AND ""CustomDevelopmentPricing"" IS NOT NULL) OR ""ProductType"" != 3);
                    END IF;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop in reverse order
            migrationBuilder.DropTable(name: "ProductPriceHistory");
            migrationBuilder.DropTable(name: "TaxRates");
            migrationBuilder.DropTable(name: "TaxCalculationLogs");
            migrationBuilder.DropTable(name: "Products");
            migrationBuilder.DropTable(name: "TaxFrameworks");
            migrationBuilder.DropTable(name: "Jurisdictions");
            migrationBuilder.DropTable(name: "ProductServiceCategories");
            migrationBuilder.DropTable(name: "ProductCategories");
            migrationBuilder.DropTable(name: "Countries");

            // Drop columns
            migrationBuilder.DropColumn(name: "TaxJurisdictionId", table: "Quotations");
            migrationBuilder.DropColumn(name: "TaxFrameworkId", table: "Quotations");
            migrationBuilder.DropColumn(name: "TaxCountryId", table: "Quotations");

            migrationBuilder.DropColumn(name: "TaxCategoryId", table: "QuotationLineItems");
            migrationBuilder.DropColumn(name: "ProductServiceCategoryId", table: "QuotationLineItems");
            migrationBuilder.DropColumn(name: "ProductId", table: "QuotationLineItems");
            migrationBuilder.DropColumn(name: "OriginalProductPrice", table: "QuotationLineItems");
            migrationBuilder.DropColumn(name: "Hours", table: "QuotationLineItems");
            migrationBuilder.DropColumn(name: "DiscountAmount", table: "QuotationLineItems");
            migrationBuilder.DropColumn(name: "BillingCycle", table: "QuotationLineItems");

            migrationBuilder.DropColumn(name: "JurisdictionId", table: "Clients");
            migrationBuilder.DropColumn(name: "CountryId", table: "Clients");
        }
    }
}
