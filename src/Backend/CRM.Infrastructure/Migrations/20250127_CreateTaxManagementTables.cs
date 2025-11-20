using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    public partial class CreateTaxManagementTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create Countries table (if not exists)
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS ""Countries"" (
                    ""CountryId"" uuid NOT NULL,
                    ""CountryName"" character varying(100) NOT NULL,
                    ""CountryCode"" character varying(2) NOT NULL,
                    ""TaxFrameworkType"" integer NOT NULL,
                    ""DefaultCurrency"" character varying(3) NOT NULL,
                    ""IsActive"" boolean NOT NULL DEFAULT true,
                    ""IsDefault"" boolean NOT NULL DEFAULT false,
                    ""CreatedAt"" timestamp with time zone NOT NULL,
                    ""UpdatedAt"" timestamp with time zone NOT NULL,
                    ""DeletedAt"" timestamp with time zone,
                    CONSTRAINT ""PK_Countries"" PRIMARY KEY (""CountryId"")
                );
            ");

            // Create TaxFrameworks table (if not exists)
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS ""TaxFrameworks"" (
                    ""TaxFrameworkId"" uuid NOT NULL,
                    ""CountryId"" uuid NOT NULL,
                    ""FrameworkName"" character varying(100) NOT NULL,
                    ""FrameworkType"" integer NOT NULL,
                    ""Description"" text,
                    ""TaxComponents"" jsonb NOT NULL,
                    ""IsActive"" boolean NOT NULL DEFAULT true,
                    ""CreatedAt"" timestamp with time zone NOT NULL,
                    ""UpdatedAt"" timestamp with time zone NOT NULL,
                    ""DeletedAt"" timestamp with time zone,
                    CONSTRAINT ""PK_TaxFrameworks"" PRIMARY KEY (""TaxFrameworkId""),
                    CONSTRAINT ""FK_TaxFrameworks_Countries_CountryId"" FOREIGN KEY (""CountryId"") 
                        REFERENCES ""Countries"" (""CountryId"") ON DELETE CASCADE
                );
            ");

            // Create Jurisdictions table
            migrationBuilder.CreateTable(
                name: "Jurisdictions",
                columns: table => new
                {
                    JurisdictionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CountryId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentJurisdictionId = table.Column<Guid>(type: "uuid", nullable: true),
                    JurisdictionName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    JurisdictionCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    JurisdictionType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jurisdictions", x => x.JurisdictionId);
                    table.ForeignKey(
                        name: "FK_Jurisdictions_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "CountryId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Jurisdictions_Jurisdictions_ParentJurisdictionId",
                        column: x => x.ParentJurisdictionId,
                        principalTable: "Jurisdictions",
                        principalColumn: "JurisdictionId",
                        onDelete: ReferentialAction.SetNull);
                });

            // Create ProductServiceCategories table
            migrationBuilder.CreateTable(
                name: "ProductServiceCategories",
                columns: table => new
                {
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CategoryCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductServiceCategories", x => x.CategoryId);
                });

            // Create TaxRates table
            migrationBuilder.CreateTable(
                name: "TaxRates",
                columns: table => new
                {
                    TaxRateId = table.Column<Guid>(type: "uuid", nullable: false),
                    JurisdictionId = table.Column<Guid>(type: "uuid", nullable: true),
                    TaxFrameworkId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductServiceCategoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    TaxRate = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveTo = table.Column<DateOnly>(type: "date", nullable: true),
                    IsExempt = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsZeroRated = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    TaxComponents = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "[]"),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxRates", x => x.TaxRateId);
                    table.ForeignKey(
                        name: "FK_TaxRates_Jurisdictions_JurisdictionId",
                        column: x => x.JurisdictionId,
                        principalTable: "Jurisdictions",
                        principalColumn: "JurisdictionId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TaxRates_TaxFrameworks_TaxFrameworkId",
                        column: x => x.TaxFrameworkId,
                        principalTable: "TaxFrameworks",
                        principalColumn: "TaxFrameworkId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaxRates_ProductServiceCategories_ProductServiceCategoryId",
                        column: x => x.ProductServiceCategoryId,
                        principalTable: "ProductServiceCategories",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.SetNull);
                });

            // Create TaxCalculationLogs table
            migrationBuilder.CreateTable(
                name: "TaxCalculationLogs",
                columns: table => new
                {
                    LogId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuotationId = table.Column<Guid>(type: "uuid", nullable: true),
                    ActionType = table.Column<int>(type: "integer", nullable: false),
                    CountryId = table.Column<Guid>(type: "uuid", nullable: true),
                    JurisdictionId = table.Column<Guid>(type: "uuid", nullable: true),
                    CalculationDetails = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "{}"),
                    ChangedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChangedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxCalculationLogs", x => x.LogId);
                    table.ForeignKey(
                        name: "FK_TaxCalculationLogs_Quotations_QuotationId",
                        column: x => x.QuotationId,
                        principalTable: "Quotations",
                        principalColumn: "QuotationId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TaxCalculationLogs_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "CountryId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TaxCalculationLogs_Jurisdictions_JurisdictionId",
                        column: x => x.JurisdictionId,
                        principalTable: "Jurisdictions",
                        principalColumn: "JurisdictionId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TaxCalculationLogs_Users_ChangedByUserId",
                        column: x => x.ChangedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.SetNull);
                });

            // Add indexes for Countries
            migrationBuilder.CreateIndex(
                name: "IX_Countries_CountryCode",
                table: "Countries",
                column: "CountryCode",
                unique: true,
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Countries_CountryName",
                table: "Countries",
                column: "CountryName",
                unique: true,
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Countries_IsActive",
                table: "Countries",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Countries_IsDefault",
                table: "Countries",
                column: "IsDefault");

            // Add indexes for TaxFrameworks
            migrationBuilder.CreateIndex(
                name: "IX_TaxFrameworks_CountryId",
                table: "TaxFrameworks",
                column: "CountryId",
                unique: true,
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TaxFrameworks_FrameworkType",
                table: "TaxFrameworks",
                column: "FrameworkType");

            migrationBuilder.CreateIndex(
                name: "IX_TaxFrameworks_IsActive",
                table: "TaxFrameworks",
                column: "IsActive");

            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS ""IX_TaxFrameworks_TaxComponents"" 
                ON ""TaxFrameworks"" USING gin (""TaxComponents"");
            ");

            // Add indexes for Jurisdictions
            migrationBuilder.CreateIndex(
                name: "IX_Jurisdictions_CountryId",
                table: "Jurisdictions",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_Jurisdictions_ParentJurisdictionId",
                table: "Jurisdictions",
                column: "ParentJurisdictionId");

            migrationBuilder.CreateIndex(
                name: "IX_Jurisdictions_IsActive",
                table: "Jurisdictions",
                column: "IsActive");

            migrationBuilder.Sql(@"
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_Jurisdictions_CountryId_ParentJurisdictionId_JurisdictionCode""
                ON ""Jurisdictions"" (""CountryId"", ""ParentJurisdictionId"", ""JurisdictionCode"")
                WHERE ""JurisdictionCode"" IS NOT NULL AND ""DeletedAt"" IS NULL;
            ");

            // Add indexes for ProductServiceCategories
            migrationBuilder.CreateIndex(
                name: "IX_ProductServiceCategories_CategoryName",
                table: "ProductServiceCategories",
                column: "CategoryName",
                unique: true,
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ProductServiceCategories_CategoryCode",
                table: "ProductServiceCategories",
                column: "CategoryCode",
                unique: true,
                filter: "\"CategoryCode\" IS NOT NULL AND \"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ProductServiceCategories_IsActive",
                table: "ProductServiceCategories",
                column: "IsActive");

            // Add indexes for TaxRates
            migrationBuilder.CreateIndex(
                name: "IX_TaxRates_JurisdictionId",
                table: "TaxRates",
                column: "JurisdictionId");

            migrationBuilder.CreateIndex(
                name: "IX_TaxRates_TaxFrameworkId",
                table: "TaxRates",
                column: "TaxFrameworkId");

            migrationBuilder.CreateIndex(
                name: "IX_TaxRates_ProductServiceCategoryId",
                table: "TaxRates",
                column: "ProductServiceCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_TaxRates_EffectiveFrom_EffectiveTo",
                table: "TaxRates",
                columns: new[] { "EffectiveFrom", "EffectiveTo" });

            migrationBuilder.CreateIndex(
                name: "IX_TaxRates_Lookup",
                table: "TaxRates",
                columns: new[] { "JurisdictionId", "ProductServiceCategoryId", "EffectiveFrom", "EffectiveTo" });

            // Add indexes for TaxCalculationLogs
            migrationBuilder.CreateIndex(
                name: "IX_TaxCalculationLogs_QuotationId",
                table: "TaxCalculationLogs",
                column: "QuotationId");

            migrationBuilder.CreateIndex(
                name: "IX_TaxCalculationLogs_ChangedAt",
                table: "TaxCalculationLogs",
                column: "ChangedAt");

            migrationBuilder.CreateIndex(
                name: "IX_TaxCalculationLogs_ActionType",
                table: "TaxCalculationLogs",
                column: "ActionType");

            migrationBuilder.CreateIndex(
                name: "IX_TaxCalculationLogs_CountryId_JurisdictionId",
                table: "TaxCalculationLogs",
                columns: new[] { "CountryId", "JurisdictionId" });

            migrationBuilder.CreateIndex(
                name: "IX_TaxCalculationLogs_ChangedAt_ActionType",
                table: "TaxCalculationLogs",
                columns: new[] { "ChangedAt", "ActionType" });

            // Modify existing tables - Add tax fields to Clients
            migrationBuilder.AddColumn<Guid>(
                name: "CountryId",
                table: "Clients",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "JurisdictionId",
                table: "Clients",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clients_CountryId",
                table: "Clients",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_JurisdictionId",
                table: "Clients",
                column: "JurisdictionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Clients_Countries_CountryId",
                table: "Clients",
                column: "CountryId",
                principalTable: "Countries",
                principalColumn: "CountryId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Clients_Jurisdictions_JurisdictionId",
                table: "Clients",
                column: "JurisdictionId",
                principalTable: "Jurisdictions",
                principalColumn: "JurisdictionId",
                onDelete: ReferentialAction.SetNull);

            // Modify QuotationLineItems - Add ProductServiceCategoryId
            migrationBuilder.AddColumn<Guid>(
                name: "ProductServiceCategoryId",
                table: "QuotationLineItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuotationLineItems_ProductServiceCategoryId",
                table: "QuotationLineItems",
                column: "ProductServiceCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_QuotationLineItems_ProductServiceCategories_ProductServiceCategoryId",
                table: "QuotationLineItems",
                column: "ProductServiceCategoryId",
                principalTable: "ProductServiceCategories",
                principalColumn: "CategoryId",
                onDelete: ReferentialAction.SetNull);

            // Modify Quotations - Add tax fields
            migrationBuilder.AddColumn<Guid>(
                name: "TaxCountryId",
                table: "Quotations",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TaxJurisdictionId",
                table: "Quotations",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TaxFrameworkId",
                table: "Quotations",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaxBreakdown",
                table: "Quotations",
                type: "jsonb",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Quotations_TaxCountryId",
                table: "Quotations",
                column: "TaxCountryId");

            migrationBuilder.CreateIndex(
                name: "IX_Quotations_TaxJurisdictionId",
                table: "Quotations",
                column: "TaxJurisdictionId");

            migrationBuilder.CreateIndex(
                name: "IX_Quotations_TaxFrameworkId",
                table: "Quotations",
                column: "TaxFrameworkId");

            migrationBuilder.AddForeignKey(
                name: "FK_Quotations_Countries_TaxCountryId",
                table: "Quotations",
                column: "TaxCountryId",
                principalTable: "Countries",
                principalColumn: "CountryId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Quotations_Jurisdictions_TaxJurisdictionId",
                table: "Quotations",
                column: "TaxJurisdictionId",
                principalTable: "Jurisdictions",
                principalColumn: "JurisdictionId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Quotations_TaxFrameworks_TaxFrameworkId",
                table: "Quotations",
                column: "TaxFrameworkId",
                principalTable: "TaxFrameworks",
                principalColumn: "TaxFrameworkId",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove foreign keys and columns from Quotations
            migrationBuilder.DropForeignKey(
                name: "FK_Quotations_TaxFrameworks_TaxFrameworkId",
                table: "Quotations");

            migrationBuilder.DropForeignKey(
                name: "FK_Quotations_Jurisdictions_TaxJurisdictionId",
                table: "Quotations");

            migrationBuilder.DropForeignKey(
                name: "FK_Quotations_Countries_TaxCountryId",
                table: "Quotations");

            migrationBuilder.DropIndex(
                name: "IX_Quotations_TaxFrameworkId",
                table: "Quotations");

            migrationBuilder.DropIndex(
                name: "IX_Quotations_TaxJurisdictionId",
                table: "Quotations");

            migrationBuilder.DropIndex(
                name: "IX_Quotations_TaxCountryId",
                table: "Quotations");

            migrationBuilder.DropColumn(
                name: "TaxBreakdown",
                table: "Quotations");

            migrationBuilder.DropColumn(
                name: "TaxFrameworkId",
                table: "Quotations");

            migrationBuilder.DropColumn(
                name: "TaxJurisdictionId",
                table: "Quotations");

            migrationBuilder.DropColumn(
                name: "TaxCountryId",
                table: "Quotations");

            // Remove foreign keys and columns from QuotationLineItems
            migrationBuilder.DropForeignKey(
                name: "FK_QuotationLineItems_ProductServiceCategories_ProductServiceCategoryId",
                table: "QuotationLineItems");

            migrationBuilder.DropIndex(
                name: "IX_QuotationLineItems_ProductServiceCategoryId",
                table: "QuotationLineItems");

            migrationBuilder.DropColumn(
                name: "ProductServiceCategoryId",
                table: "QuotationLineItems");

            // Remove foreign keys and columns from Clients
            migrationBuilder.DropForeignKey(
                name: "FK_Clients_Jurisdictions_JurisdictionId",
                table: "Clients");

            migrationBuilder.DropForeignKey(
                name: "FK_Clients_Countries_CountryId",
                table: "Clients");

            migrationBuilder.DropIndex(
                name: "IX_Clients_JurisdictionId",
                table: "Clients");

            migrationBuilder.DropIndex(
                name: "IX_Clients_CountryId",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "JurisdictionId",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "CountryId",
                table: "Clients");

            // Drop tables
            migrationBuilder.DropTable(name: "TaxCalculationLogs");
            migrationBuilder.DropTable(name: "TaxRates");
            migrationBuilder.DropTable(name: "ProductServiceCategories");
            migrationBuilder.DropTable(name: "Jurisdictions");
            migrationBuilder.DropTable(name: "TaxFrameworks");
            migrationBuilder.DropTable(name: "Countries");
        }
    }
}

