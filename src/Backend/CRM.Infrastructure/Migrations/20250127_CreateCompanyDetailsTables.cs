using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    public partial class CreateCompanyDetailsTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create CompanyDetails table
            migrationBuilder.CreateTable(
                name: "CompanyDetails",
                columns: table => new
                {
                    CompanyDetailsId = table.Column<Guid>(type: "uuid", nullable: false, defaultValue: new Guid("00000000-0000-0000-0000-000000000001")),
                    PanNumber = table.Column<string>(type: "varchar(10)", nullable: true),
                    TanNumber = table.Column<string>(type: "varchar(10)", nullable: true),
                    GstNumber = table.Column<string>(type: "varchar(15)", nullable: true),
                    CompanyName = table.Column<string>(type: "varchar(255)", nullable: true),
                    CompanyAddress = table.Column<string>(type: "text", nullable: true),
                    City = table.Column<string>(type: "varchar(100)", nullable: true),
                    State = table.Column<string>(type: "varchar(100)", nullable: true),
                    PostalCode = table.Column<string>(type: "varchar(20)", nullable: true),
                    Country = table.Column<string>(type: "varchar(100)", nullable: true),
                    ContactEmail = table.Column<string>(type: "varchar(255)", nullable: true),
                    ContactPhone = table.Column<string>(type: "varchar(20)", nullable: true),
                    Website = table.Column<string>(type: "varchar(255)", nullable: true),
                    LegalDisclaimer = table.Column<string>(type: "text", nullable: true),
                    LogoUrl = table.Column<string>(type: "varchar(500)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyDetails", x => x.CompanyDetailsId);
                    table.ForeignKey(
                        name: "FK_CompanyDetails_Users_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompanyDetails_UpdatedAt",
                table: "CompanyDetails",
                column: "UpdatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyDetails_UpdatedBy",
                table: "CompanyDetails",
                column: "UpdatedBy");

            // Create BankDetails table
            migrationBuilder.CreateTable(
                name: "BankDetails",
                columns: table => new
                {
                    BankDetailsId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyDetailsId = table.Column<Guid>(type: "uuid", nullable: false),
                    Country = table.Column<string>(type: "varchar(50)", nullable: false),
                    AccountNumber = table.Column<string>(type: "varchar(50)", nullable: false),
                    IfscCode = table.Column<string>(type: "varchar(11)", nullable: true),
                    Iban = table.Column<string>(type: "varchar(34)", nullable: true),
                    SwiftCode = table.Column<string>(type: "varchar(11)", nullable: true),
                    BankName = table.Column<string>(type: "varchar(255)", nullable: false),
                    BranchName = table.Column<string>(type: "varchar(255)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankDetails", x => x.BankDetailsId);
                    table.ForeignKey(
                        name: "FK_BankDetails_CompanyDetails_CompanyDetailsId",
                        column: x => x.CompanyDetailsId,
                        principalTable: "CompanyDetails",
                        principalColumn: "CompanyDetailsId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BankDetails_Users_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.UniqueConstraint(
                        name: "UQ_BankDetails_CompanyDetailsId_Country",
                        columns: x => new { x.CompanyDetailsId, x.Country });
                });

            migrationBuilder.CreateIndex(
                name: "IX_BankDetails_CompanyDetailsId",
                table: "BankDetails",
                column: "CompanyDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_BankDetails_UpdatedBy",
                table: "BankDetails",
                column: "UpdatedBy");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "BankDetails");
            migrationBuilder.DropTable(name: "CompanyDetails");
        }
    }
}

