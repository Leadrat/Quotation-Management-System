using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateEmailNotificationLogTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmailNotificationLog",
                columns: table => new
                {
                    LogId = table.Column<Guid>(type: "uuid", nullable: false),
                    NotificationId = table.Column<Guid>(type: "uuid", nullable: true),
                    RecipientEmail = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    EventType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Subject = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    SentAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DeliveredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "SENT"),
                    ErrorMsg = table.Column<string>(type: "TEXT", nullable: true),
                    RetryCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    LastRetryAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailNotificationLog", x => x.LogId);
                    table.ForeignKey(
                        name: "FK_EmailNotificationLog_Notifications_NotificationId",
                        column: x => x.NotificationId,
                        principalTable: "Notifications",
                        principalColumn: "NotificationId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmailNotificationLog_NotificationId",
                table: "EmailNotificationLog",
                column: "NotificationId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailNotificationLog_RecipientEmail",
                table: "EmailNotificationLog",
                column: "RecipientEmail");

            migrationBuilder.CreateIndex(
                name: "IX_EmailNotificationLog_EventType",
                table: "EmailNotificationLog",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_EmailNotificationLog_Status",
                table: "EmailNotificationLog",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_EmailNotificationLog_SentAt",
                table: "EmailNotificationLog",
                column: "SentAt",
                descending: new[] { true });

            migrationBuilder.CreateIndex(
                name: "IX_EmailNotificationLog_Failed",
                table: "EmailNotificationLog",
                column: "Status",
                filter: "\"Status\" IN ('FAILED', 'BOUNCED')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailNotificationLog");
        }
    }
}

