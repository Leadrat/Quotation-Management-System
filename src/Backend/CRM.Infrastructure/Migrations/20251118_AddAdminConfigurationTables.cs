using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    public partial class AddAdminConfigurationTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create AuditLog table
            migrationBuilder.CreateTable(
                name: "AuditLog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ActionType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Entity = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    PerformedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    Timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Changes = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLog_Users_PerformedBy",
                        column: x => x.PerformedBy,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_PerformedBy",
                table: "AuditLog",
                column: "PerformedBy");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_Timestamp",
                table: "AuditLog",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_Entity",
                table: "AuditLog",
                column: "Entity");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_ActionType",
                table: "AuditLog",
                column: "ActionType");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_EntityId",
                table: "AuditLog",
                column: "EntityId",
                filter: "\"EntityId\" IS NOT NULL");

            // Create SystemSettings table
            migrationBuilder.CreateTable(
                name: "SystemSettings",
                columns: table => new
                {
                    Key = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Value = table.Column<string>(type: "jsonb", nullable: false),
                    LastModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    LastModifiedBy = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemSettings", x => x.Key);
                    table.ForeignKey(
                        name: "FK_SystemSettings_Users_LastModifiedBy",
                        column: x => x.LastModifiedBy,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SystemSettings_LastModifiedAt",
                table: "SystemSettings",
                column: "LastModifiedAt");

            // Create IntegrationKeys table
            migrationBuilder.CreateTable(
                name: "IntegrationKeys",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    KeyName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    KeyValueEncrypted = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Provider = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    LastUsedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntegrationKeys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IntegrationKeys_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IntegrationKeys_Users_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IntegrationKeys_Provider",
                table: "IntegrationKeys",
                column: "Provider");

            migrationBuilder.CreateIndex(
                name: "IX_IntegrationKeys_KeyName",
                table: "IntegrationKeys",
                column: "KeyName");

            migrationBuilder.CreateIndex(
                name: "IX_IntegrationKeys_CreatedAt",
                table: "IntegrationKeys",
                column: "CreatedAt");

            // Create CustomBranding table
            migrationBuilder.CreateTable(
                name: "CustomBranding",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LogoUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PrimaryColor = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
                    SecondaryColor = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
                    AccentColor = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
                    FooterHtml = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomBranding", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomBranding_Users_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomBranding_Id",
                table: "CustomBranding",
                column: "Id",
                unique: true);

            // Create DataRetentionPolicy table
            migrationBuilder.CreateTable(
                name: "DataRetentionPolicy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RetentionPeriodMonths = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    AutoPurgeEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataRetentionPolicy", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataRetentionPolicy_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DataRetentionPolicy_Users_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DataRetentionPolicy_EntityType",
                table: "DataRetentionPolicy",
                column: "EntityType",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DataRetentionPolicy_IsActive",
                table: "DataRetentionPolicy",
                column: "IsActive");

            // Create NotificationSettings table
            migrationBuilder.CreateTable(
                name: "NotificationSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BannerMessage = table.Column<string>(type: "text", nullable: true),
                    BannerType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    IsVisible = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationSettings_Users_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationSettings_Id",
                table: "NotificationSettings",
                column: "Id",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "NotificationSettings");
            migrationBuilder.DropTable(name: "DataRetentionPolicy");
            migrationBuilder.DropTable(name: "CustomBranding");
            migrationBuilder.DropTable(name: "IntegrationKeys");
            migrationBuilder.DropTable(name: "SystemSettings");
            migrationBuilder.DropTable(name: "AuditLog");
        }
    }
}

