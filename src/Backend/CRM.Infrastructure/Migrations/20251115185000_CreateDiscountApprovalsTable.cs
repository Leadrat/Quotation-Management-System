using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateDiscountApprovalsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DiscountApprovals",
                columns: table => new
                {
                    ApprovalId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuotationId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApproverUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RequestDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ApprovalDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RejectionDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CurrentDiscountPercentage = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    Threshold = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    ApprovalLevel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Reason = table.Column<string>(type: "TEXT", nullable: false),
                    Comments = table.Column<string>(type: "TEXT", nullable: true),
                    EscalatedToAdmin = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscountApprovals", x => x.ApprovalId);
                    table.ForeignKey(
                        name: "FK_DiscountApprovals_Quotations_QuotationId",
                        column: x => x.QuotationId,
                        principalTable: "Quotations",
                        principalColumn: "QuotationId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DiscountApprovals_Users_ApproverUserId",
                        column: x => x.ApproverUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DiscountApprovals_Users_RequestedByUserId",
                        column: x => x.RequestedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.CheckConstraint("CK_DiscountApprovals_CurrentDiscountPercentage", "\"CurrentDiscountPercentage\" >= 0");
                    table.CheckConstraint("CK_DiscountApprovals_Threshold", "\"Threshold\" >= 0");
                });

            migrationBuilder.CreateIndex(
                name: "IX_DiscountApprovals_ApproverUserId_Status",
                table: "DiscountApprovals",
                columns: new[] { "ApproverUserId", "Status" },
                filter: "\"ApproverUserId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_DiscountApprovals_QuotationId",
                table: "DiscountApprovals",
                column: "QuotationId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscountApprovals_RequestedByUserId",
                table: "DiscountApprovals",
                column: "RequestedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscountApprovals_CurrentDiscountPercentage",
                table: "DiscountApprovals",
                column: "CurrentDiscountPercentage");

            migrationBuilder.CreateIndex(
                name: "IX_DiscountApprovals_Status",
                table: "DiscountApprovals",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_DiscountApprovals_CreatedAt_Status",
                table: "DiscountApprovals",
                columns: new[] { "CreatedAt", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DiscountApprovals");
        }
    }
}

