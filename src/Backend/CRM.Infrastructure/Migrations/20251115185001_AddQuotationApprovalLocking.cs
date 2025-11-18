using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddQuotationApprovalLocking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPendingApproval",
                table: "Quotations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<System.Guid>(
                name: "PendingApprovalId",
                table: "Quotations",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Quotations_IsPendingApproval",
                table: "Quotations",
                column: "IsPendingApproval");

            migrationBuilder.CreateIndex(
                name: "IX_Quotations_PendingApprovalId",
                table: "Quotations",
                column: "PendingApprovalId",
                filter: "\"PendingApprovalId\" IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Quotations_DiscountApprovals_PendingApprovalId",
                table: "Quotations",
                column: "PendingApprovalId",
                principalTable: "DiscountApprovals",
                principalColumn: "ApprovalId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Quotations_DiscountApprovals_PendingApprovalId",
                table: "Quotations");

            migrationBuilder.DropIndex(
                name: "IX_Quotations_PendingApprovalId",
                table: "Quotations");

            migrationBuilder.DropIndex(
                name: "IX_Quotations_IsPendingApproval",
                table: "Quotations");

            migrationBuilder.DropColumn(
                name: "PendingApprovalId",
                table: "Quotations");

            migrationBuilder.DropColumn(
                name: "IsPendingApproval",
                table: "Quotations");
        }
    }
}

