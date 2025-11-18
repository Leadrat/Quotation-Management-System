using System;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    public partial class _20251112_CreateUsersTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Enable citext extension for case-insensitive email
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS citext;");

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.RoleId);
                });

            migrationBuilder.CreateIndex(
                name: "UX_Roles_RoleName",
                table: "Roles",
                column: "RoleName",
                unique: true);

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "citext", maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Mobile = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    PhoneCode = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportingManagerId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastLoginAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LoginAttempts = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IsLockedOut = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_Users_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Users_Users_ReportingManagerId",
                        column: x => x.ReportingManagerId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "UX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleId",
                table: "Users",
                column: "RoleId");
            migrationBuilder.CreateIndex(
                name: "IX_Users_ReportingManagerId",
                table: "Users",
                column: "ReportingManagerId");
            migrationBuilder.CreateIndex(
                name: "IX_Users_IsActive",
                table: "Users",
                column: "IsActive");
            migrationBuilder.CreateIndex(
                name: "IX_Users_CreatedAt",
                table: "Users",
                column: "CreatedAt");
            migrationBuilder.CreateIndex(
                name: "IX_Users_UpdatedAt",
                table: "Users",
                column: "UpdatedAt");
            migrationBuilder.CreateIndex(
                name: "IX_Users_DeletedAt",
                table: "Users",
                column: "DeletedAt");

            // Seed Roles
            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "RoleId", "RoleName", "Description", "CreatedAt", "UpdatedAt" },
                values: new object[,]
                {
                    { Guid.Parse("AA668EE7-79E9-4AF3-B3ED-1A47F104B8EA"), "Admin", "Full system access, user management, audit trail", DateTime.UtcNow, DateTime.UtcNow },
                    { Guid.Parse("8D38F43B-EB54-4E4A-9582-1C611F7B5DF6"), "Manager", "Approve/reject discount requests, view team performance", DateTime.UtcNow, DateTime.UtcNow },
                    { Guid.Parse("FAE6CEDB-42FD-497B-85F6-F2B14ECA0079"), "SalesRep", "Create quotations, manage clients, submit for approval", DateTime.UtcNow, DateTime.UtcNow },
                    { Guid.Parse("00F3CF90-C1A2-4B46-96A2-6A58EF54E8DD"), "Client", "View quotations, accept/reject, request payment", DateTime.UtcNow, DateTime.UtcNow }
                });

            // Precompute bcrypt hashes (work factor 12)
            string Hash(string plain) => BCrypt.Net.BCrypt.HashPassword(plain, workFactor: 12);

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "Email", "PasswordHash", "FirstName", "LastName", "Mobile", "PhoneCode", "IsActive", "RoleId", "ReportingManagerId", "LastLoginAt", "LoginAttempts", "IsLockedOut", "CreatedAt", "UpdatedAt", "DeletedAt" },
                values: new object[,]
                {
                    { Guid.Parse("05948A48-3272-4FB4-8849-796A61D7A6F2"), "admin@crm.com", Hash("Admin@123"), "Admin", "User", "+919876543200", "+91", true, Guid.Parse("AA668EE7-79E9-4AF3-B3ED-1A47F104B8EA"), null, null, 0, false, DateTime.Parse("2025-01-01T00:00:00Z"), DateTime.Parse("2025-01-01T00:00:00Z"), null },
                    { Guid.Parse("EB4F2FCA-B9F6-46CE-BB6F-2EA0689ABE9F"), "manager@crm.com", Hash("Manager@123"), "Rajesh", "Kumar", "+919876543201", "+91", true, Guid.Parse("8D38F43B-EB54-4E4A-9582-1C611F7B5DF6"), null, null, 0, false, DateTime.Parse("2025-01-01T00:00:00Z"), DateTime.Parse("2025-01-01T00:00:00Z"), null },
                    { Guid.Parse("67B8A7EA-F0D7-46CB-8B9E-F3B2E5EDF336"), "sales@crm.com", Hash("Sales@123"), "Priya", "Singh", "+919876543202", "+91", true, Guid.Parse("FAE6CEDB-42FD-497B-85F6-F2B14ECA0079"), Guid.Parse("EB4F2FCA-B9F6-46CE-BB6F-2EA0689ABE9F"), null, 0, false, DateTime.Parse("2025-01-01T00:00:00Z"), DateTime.Parse("2025-01-01T00:00:00Z"), null },
                    { Guid.Parse("84762D26-BC7F-4133-AA40-0D15D8F21B84"), "client1@crm.com", Hash("Client@123"), "John", "Doe", "+919876543203", "+91", true, Guid.Parse("00F3CF90-C1A2-4B46-96A2-6A58EF54E8DD"), null, null, 0, false, DateTime.Parse("2025-01-01T00:00:00Z"), DateTime.Parse("2025-01-01T00:00:00Z"), null }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Users");
            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
