using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationDispatchInfrastructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create NotificationTemplates table
            migrationBuilder.CreateTable(
                name: "NotificationTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TemplateKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Channel = table.Column<int>(type: "integer", nullable: false),
                    Subject = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    BodyTemplate = table.Column<string>(type: "text", nullable: false),
                    Variables = table.Column<string>(type: "jsonb", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationTemplates", x => x.Id);
                });

            // Create NotificationChannelConfigurations table
            migrationBuilder.CreateTable(
                name: "NotificationChannelConfigurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Channel = table.Column<int>(type: "integer", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Configuration = table.Column<string>(type: "jsonb", nullable: false),
                    MaxRetryAttempts = table.Column<int>(type: "integer", nullable: false, defaultValue: 3),
                    RetryDelay = table.Column<TimeSpan>(type: "interval", nullable: false, defaultValue: TimeSpan.FromMinutes(5)),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationChannelConfigurations", x => x.Id);
                });

            // Create NotificationDispatchAttempts table
            migrationBuilder.CreateTable(
                name: "NotificationDispatchAttempts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NotificationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Channel = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    AttemptedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ExternalReference = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RetryCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    NextRetryAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationDispatchAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationDispatchAttempts_Notifications_NotificationId",
                        column: x => x.NotificationId,
                        principalTable: "Notifications",
                        principalColumn: "NotificationId",
                        onDelete: ReferentialAction.Cascade);
                });

            // Create indexes for NotificationTemplates
            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplates_TemplateKey",
                table: "NotificationTemplates",
                column: "TemplateKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplates_Channel",
                table: "NotificationTemplates",
                column: "Channel");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplates_IsActive",
                table: "NotificationTemplates",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplates_Channel_IsActive",
                table: "NotificationTemplates",
                columns: new[] { "Channel", "IsActive" });

            // Create indexes for NotificationChannelConfigurations
            migrationBuilder.CreateIndex(
                name: "IX_NotificationChannelConfigurations_Channel",
                table: "NotificationChannelConfigurations",
                column: "Channel",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NotificationChannelConfigurations_IsEnabled",
                table: "NotificationChannelConfigurations",
                column: "IsEnabled");

            // Create indexes for NotificationDispatchAttempts
            migrationBuilder.CreateIndex(
                name: "IX_NotificationDispatchAttempts_NotificationId",
                table: "NotificationDispatchAttempts",
                column: "NotificationId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationDispatchAttempts_Status",
                table: "NotificationDispatchAttempts",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationDispatchAttempts_Channel",
                table: "NotificationDispatchAttempts",
                column: "Channel");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationDispatchAttempts_AttemptedAt",
                table: "NotificationDispatchAttempts",
                column: "AttemptedAt");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationDispatchAttempts_Status_NextRetryAt",
                table: "NotificationDispatchAttempts",
                columns: new[] { "Status", "NextRetryAt" });

            // Insert default channel configurations
            migrationBuilder.InsertData(
                table: "NotificationChannelConfigurations",
                columns: new[] { "Channel", "IsEnabled", "Configuration", "MaxRetryAttempts", "RetryDelay", "CreatedAt", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, true, "{\"realTimeEnabled\": true}", 3, TimeSpan.FromMinutes(5), DateTimeOffset.UtcNow, DateTimeOffset.UtcNow }, // InApp
                    { 2, true, "{\"smtpHost\": \"\", \"smtpPort\": 587, \"enableSsl\": true, \"username\": \"\", \"password\": \"\"}", 5, TimeSpan.FromMinutes(10), DateTimeOffset.UtcNow, DateTimeOffset.UtcNow }, // Email
                    { 3, false, "{\"apiKey\": \"\", \"fromNumber\": \"\", \"provider\": \"\"}", 3, TimeSpan.FromMinutes(15), DateTimeOffset.UtcNow, DateTimeOffset.UtcNow } // SMS
                });

            // Insert default notification templates
            migrationBuilder.InsertData(
                table: "NotificationTemplates",
                columns: new[] { "TemplateKey", "Name", "Description", "Channel", "Subject", "BodyTemplate", "Variables", "IsActive", "CreatedAt", "UpdatedAt" },
                values: new object[,]
                {
                    // InApp templates
                    { "quotation_approved_inapp", "Quotation Approved (In-App)", "In-app notification for quotation approval", 1, "Quotation Approved", "Your quotation {{QuotationNumber}} has been approved.", "[\"QuotationNumber\", \"ClientName\"]", true, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow },
                    { "payment_request_inapp", "Payment Request (In-App)", "In-app notification for payment request", 1, "Payment Request", "Payment is requested for quotation {{QuotationNumber}}. Amount: {{Amount}}", "[\"QuotationNumber\", \"Amount\", \"ClientName\"]", true, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow },
                    
                    // Email templates
                    { "quotation_approved_email", "Quotation Approved (Email)", "Email notification for quotation approval", 2, "Quotation {{QuotationNumber}} Approved", "<h2>Quotation Approved</h2><p>Dear {{ClientName}},</p><p>Your quotation {{QuotationNumber}} has been approved.</p><p>Best regards,<br/>The Team</p>", "[\"QuotationNumber\", \"ClientName\"]", true, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow },
                    { "payment_request_email", "Payment Request (Email)", "Email notification for payment request", 2, "Payment Request for Quotation {{QuotationNumber}}", "<h2>Payment Request</h2><p>Dear {{ClientName}},</p><p>Payment is requested for quotation {{QuotationNumber}}.</p><p>Amount: {{Amount}}</p><p>Best regards,<br/>The Team</p>", "[\"QuotationNumber\", \"Amount\", \"ClientName\"]", true, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow },
                    
                    // SMS templates
                    { "quotation_approved_sms", "Quotation Approved (SMS)", "SMS notification for quotation approval", 3, "", "Quotation {{QuotationNumber}} approved. Contact us for details.", "[\"QuotationNumber\"]", true, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow },
                    { "payment_request_sms", "Payment Request (SMS)", "SMS notification for payment request", 3, "", "Payment requested for quotation {{QuotationNumber}}. Amount: {{Amount}}", "[\"QuotationNumber\", \"Amount\"]", true, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NotificationDispatchAttempts");

            migrationBuilder.DropTable(
                name: "NotificationTemplates");

            migrationBuilder.DropTable(
                name: "NotificationChannelConfigurations");
        }
    }
}