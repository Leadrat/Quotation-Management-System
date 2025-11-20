using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCountryCompanyIdentifiers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add IdentifierValues and CountryId to existing CompanyDetails table (if not exists)
            migrationBuilder.Sql(@"DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'CompanyDetails' AND column_name = 'IdentifierValues') THEN
        ALTER TABLE ""CompanyDetails"" ADD ""IdentifierValues"" jsonb;
    END IF;
    
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'CompanyDetails' AND column_name = 'CountryId') THEN
        ALTER TABLE ""CompanyDetails"" ADD ""CountryId"" uuid;
    END IF;
END $$;");

            // Add FieldValues and CountryId to existing BankDetails table (if not exists)
            migrationBuilder.Sql(@"DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'BankDetails' AND column_name = 'FieldValues') THEN
        ALTER TABLE ""BankDetails"" ADD ""FieldValues"" jsonb;
    END IF;
    
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'BankDetails' AND column_name = 'CountryId') THEN
        ALTER TABLE ""BankDetails"" ADD ""CountryId"" uuid;
    END IF;
END $$;");

            // Create new tables
            migrationBuilder.CreateTable(
                name: "BankFieldTypes",
                columns: table => new
                {
                    BankFieldTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankFieldTypes", x => x.BankFieldTypeId);
                });

            migrationBuilder.CreateTable(
                name: "IdentifierTypes",
                columns: table => new
                {
                    IdentifierTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdentifierTypes", x => x.IdentifierTypeId);
                });

            migrationBuilder.CreateTable(
                name: "CountryBankFieldConfigurations",
                columns: table => new
                {
                    ConfigurationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CountryId = table.Column<Guid>(type: "uuid", nullable: false),
                    BankFieldTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ValidationRegex = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    MinLength = table.Column<int>(type: "integer", nullable: true),
                    MaxLength = table.Column<int>(type: "integer", nullable: true),
                    DisplayName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    HelpText = table.Column<string>(type: "text", nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CountryBankFieldConfigurations", x => x.ConfigurationId);
                    table.ForeignKey(
                        name: "FK_CountryBankFieldConfigurations_BankFieldTypes_BankFieldTypeId",
                        column: x => x.BankFieldTypeId,
                        principalTable: "BankFieldTypes",
                        principalColumn: "BankFieldTypeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CountryBankFieldConfigurations_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "CountryId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CountryIdentifierConfigurations",
                columns: table => new
                {
                    ConfigurationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CountryId = table.Column<Guid>(type: "uuid", nullable: false),
                    IdentifierTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ValidationRegex = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    MinLength = table.Column<int>(type: "integer", nullable: true),
                    MaxLength = table.Column<int>(type: "integer", nullable: true),
                    DisplayName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    HelpText = table.Column<string>(type: "text", nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CountryIdentifierConfigurations", x => x.ConfigurationId);
                    table.ForeignKey(
                        name: "FK_CountryIdentifierConfigurations_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "CountryId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CountryIdentifierConfigurations_IdentifierTypes_IdentifierTypeId",
                        column: x => x.IdentifierTypeId,
                        principalTable: "IdentifierTypes",
                        principalColumn: "IdentifierTypeId",
                        onDelete: ReferentialAction.Cascade);
                });

            // Add foreign key constraints for newly added columns
            migrationBuilder.AddForeignKey(
                name: "FK_CompanyDetails_Countries_CountryId",
                table: "CompanyDetails",
                column: "CountryId",
                principalTable: "Countries",
                principalColumn: "CountryId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BankDetails_Countries_CountryId",
                table: "BankDetails",
                column: "CountryId",
                principalTable: "Countries",
                principalColumn: "CountryId",
                onDelete: ReferentialAction.Restrict);

            // Create indexes
            migrationBuilder.CreateIndex(
                name: "IX_BankDetails_CountryId",
                table: "BankDetails",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_BankDetails_FieldValues",
                table: "BankDetails",
                column: "FieldValues")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "IX_BankFieldTypes_IsActive",
                table: "BankFieldTypes",
                column: "IsActive",
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BankFieldTypes_Name",
                table: "BankFieldTypes",
                column: "Name",
                unique: true,
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyDetails_CountryId",
                table: "CompanyDetails",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyDetails_IdentifierValues",
                table: "CompanyDetails",
                column: "IdentifierValues")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "IX_CountryBankFieldConfigurations_BankFieldTypeId",
                table: "CountryBankFieldConfigurations",
                column: "BankFieldTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_CountryBankFieldConfigurations_CountryId",
                table: "CountryBankFieldConfigurations",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_CountryBankFieldConfigurations_CountryId_BankFieldTypeId",
                table: "CountryBankFieldConfigurations",
                columns: new[] { "CountryId", "BankFieldTypeId" },
                unique: true,
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CountryBankFieldConfigurations_CountryId_IsActive",
                table: "CountryBankFieldConfigurations",
                columns: new[] { "CountryId", "IsActive" },
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CountryBankFieldConfigurations_IsActive",
                table: "CountryBankFieldConfigurations",
                column: "IsActive",
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CountryIdentifierConfigurations_CountryId",
                table: "CountryIdentifierConfigurations",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_CountryIdentifierConfigurations_CountryId_IdentifierTypeId",
                table: "CountryIdentifierConfigurations",
                columns: new[] { "CountryId", "IdentifierTypeId" },
                unique: true,
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CountryIdentifierConfigurations_CountryId_IsActive",
                table: "CountryIdentifierConfigurations",
                columns: new[] { "CountryId", "IsActive" },
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CountryIdentifierConfigurations_IdentifierTypeId",
                table: "CountryIdentifierConfigurations",
                column: "IdentifierTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_CountryIdentifierConfigurations_IsActive",
                table: "CountryIdentifierConfigurations",
                column: "IsActive",
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_IdentifierTypes_IsActive",
                table: "IdentifierTypes",
                column: "IsActive",
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_IdentifierTypes_Name",
                table: "IdentifierTypes",
                column: "Name",
                unique: true,
                filter: "\"DeletedAt\" IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop indexes
            migrationBuilder.DropIndex(
                name: "IX_IdentifierTypes_Name",
                table: "IdentifierTypes");

            migrationBuilder.DropIndex(
                name: "IX_IdentifierTypes_IsActive",
                table: "IdentifierTypes");

            migrationBuilder.DropIndex(
                name: "IX_CountryIdentifierConfigurations_IsActive",
                table: "CountryIdentifierConfigurations");

            migrationBuilder.DropIndex(
                name: "IX_CountryIdentifierConfigurations_IdentifierTypeId",
                table: "CountryIdentifierConfigurations");

            migrationBuilder.DropIndex(
                name: "IX_CountryIdentifierConfigurations_CountryId_IsActive",
                table: "CountryIdentifierConfigurations");

            migrationBuilder.DropIndex(
                name: "IX_CountryIdentifierConfigurations_CountryId_IdentifierTypeId",
                table: "CountryIdentifierConfigurations");

            migrationBuilder.DropIndex(
                name: "IX_CountryIdentifierConfigurations_CountryId",
                table: "CountryIdentifierConfigurations");

            migrationBuilder.DropIndex(
                name: "IX_CountryBankFieldConfigurations_IsActive",
                table: "CountryBankFieldConfigurations");

            migrationBuilder.DropIndex(
                name: "IX_CountryBankFieldConfigurations_CountryId_IsActive",
                table: "CountryBankFieldConfigurations");

            migrationBuilder.DropIndex(
                name: "IX_CountryBankFieldConfigurations_CountryId_BankFieldTypeId",
                table: "CountryBankFieldConfigurations");

            migrationBuilder.DropIndex(
                name: "IX_CountryBankFieldConfigurations_CountryId",
                table: "CountryBankFieldConfigurations");

            migrationBuilder.DropIndex(
                name: "IX_CountryBankFieldConfigurations_BankFieldTypeId",
                table: "CountryBankFieldConfigurations");

            migrationBuilder.DropIndex(
                name: "IX_CompanyDetails_IdentifierValues",
                table: "CompanyDetails");

            migrationBuilder.DropIndex(
                name: "IX_CompanyDetails_CountryId",
                table: "CompanyDetails");

            migrationBuilder.DropIndex(
                name: "IX_BankFieldTypes_Name",
                table: "BankFieldTypes");

            migrationBuilder.DropIndex(
                name: "IX_BankFieldTypes_IsActive",
                table: "BankFieldTypes");

            migrationBuilder.DropIndex(
                name: "IX_BankDetails_FieldValues",
                table: "BankDetails");

            migrationBuilder.DropIndex(
                name: "IX_BankDetails_CountryId",
                table: "BankDetails");

            // Drop foreign keys
            migrationBuilder.DropForeignKey(
                name: "FK_BankDetails_Countries_CountryId",
                table: "BankDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_CompanyDetails_Countries_CountryId",
                table: "CompanyDetails");

            // Drop tables
            migrationBuilder.DropTable(
                name: "CountryIdentifierConfigurations");

            migrationBuilder.DropTable(
                name: "CountryBankFieldConfigurations");

            migrationBuilder.DropTable(
                name: "BankFieldTypes");

            migrationBuilder.DropTable(
                name: "IdentifierTypes");

            // Drop columns (with existence check)
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'BankDetails' AND column_name = 'FieldValues') THEN
                        ALTER TABLE ""BankDetails"" DROP COLUMN ""FieldValues"";
                    END IF;
                    
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'BankDetails' AND column_name = 'CountryId') THEN
                        ALTER TABLE ""BankDetails"" DROP COLUMN ""CountryId"";
                    END IF;
                    
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'CompanyDetails' AND column_name = 'IdentifierValues') THEN
                        ALTER TABLE ""CompanyDetails"" DROP COLUMN ""IdentifierValues"";
                    END IF;
                    
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'CompanyDetails' AND column_name = 'CountryId') THEN
                        ALTER TABLE ""CompanyDetails"" DROP COLUMN ""CountryId"";
                    END IF;
                END $$;
            ");
        }
    }
}
