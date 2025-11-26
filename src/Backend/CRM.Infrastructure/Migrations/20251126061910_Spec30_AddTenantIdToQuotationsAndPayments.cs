using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Spec30_AddTenantIdToQuotationsAndPayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Quotations",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Payments",
                type: "uuid",
                nullable: true);

            // Backfill existing rows with the default tenant ID
            var defaultTenantId = new Guid("11111111-1111-1111-1111-111111111111");
            migrationBuilder.Sql($"UPDATE \"Quotations\" SET \"TenantId\" = '{defaultTenantId}' WHERE \"TenantId\" IS NULL");
            migrationBuilder.Sql($"UPDATE \"Payments\" SET \"TenantId\" = '{defaultTenantId}' WHERE \"TenantId\" IS NULL");

            // Make the column non-nullable after backfilling
            migrationBuilder.AlterColumn<Guid>(
                name: "TenantId",
                table: "Quotations",
                type: "uuid",
                nullable: false);

            migrationBuilder.AlterColumn<Guid>(
                name: "TenantId",
                table: "Payments",
                type: "uuid",
                nullable: false);

            migrationBuilder.CreateIndex(
                name: "IX_Quotations_TenantId",
                table: "Quotations",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_TenantId",
                table: "Payments",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Tenants_TenantId",
                table: "Payments",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Quotations_Tenants_TenantId",
                table: "Quotations",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Tenants_TenantId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Quotations_Tenants_TenantId",
                table: "Quotations");

            migrationBuilder.DropIndex(
                name: "IX_Quotations_TenantId",
                table: "Quotations");

            migrationBuilder.DropIndex(
                name: "IX_Payments_TenantId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Quotations");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Payments");
        }
    }
}
