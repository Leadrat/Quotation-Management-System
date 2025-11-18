using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    public partial class CreateClientHistoryTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClientHistories",
                columns: table => new
                {
                    HistoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ActionType = table.Column<string>(type: "varchar(50)", nullable: false),
                    ChangedFields = table.Column<string[]>(type: "text[]", nullable: false, defaultValue: new string[0]),
                    BeforeSnapshot = table.Column<string>(type: "jsonb", nullable: true),
                    AfterSnapshot = table.Column<string>(type: "jsonb", nullable: true),
                    Reason = table.Column<string>(type: "varchar(500)", nullable: true),
                    Metadata = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "{}"),
                    SuspicionScore = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientHistories", x => x.HistoryId);
                    table.ForeignKey(
                        name: "FK_ClientHistories_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "ClientId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClientHistories_Users_ActorUserId",
                        column: x => x.ActorUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SuspiciousActivityFlags",
                columns: table => new
                {
                    FlagId = table.Column<Guid>(type: "uuid", nullable: false),
                    HistoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: false),
                    Score = table.Column<short>(type: "smallint", nullable: false),
                    Reasons = table.Column<string[]>(type: "text[]", nullable: false, defaultValue: new string[0]),
                    DetectedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    ReviewedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    Status = table.Column<string>(type: "varchar(32)", nullable: false, defaultValue: "OPEN"),
                    Metadata = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "{}")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SuspiciousActivityFlags", x => x.FlagId);
                    table.ForeignKey(
                        name: "FK_SuspiciousActivityFlags_ClientHistories_HistoryId",
                        column: x => x.HistoryId,
                        principalTable: "ClientHistories",
                        principalColumn: "HistoryId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClientHistories_ClientId_CreatedAt",
                table: "ClientHistories",
                columns: new[] { "ClientId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ClientHistories_ActorUserId_CreatedAt",
                table: "ClientHistories",
                columns: new[] { "ActorUserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SuspiciousActivityFlags_ClientId_DetectedAt",
                table: "SuspiciousActivityFlags",
                columns: new[] { "ClientId", "DetectedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SuspiciousActivityFlags_HistoryId",
                table: "SuspiciousActivityFlags",
                column: "HistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_SuspiciousActivityFlags_Status_DetectedAt",
                table: "SuspiciousActivityFlags",
                columns: new[] { "Status", "DetectedAt" });

            migrationBuilder.Sql(@"
                CREATE INDEX ""IX_ClientHistories_Metadata_GIN""
                ON ""ClientHistories""
                USING GIN (""Metadata"");
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_ClientHistories_Metadata_GIN"";");
            migrationBuilder.DropTable(
                name: "SuspiciousActivityFlags");

            migrationBuilder.DropTable(
                name: "ClientHistories");
        }
    }
}

