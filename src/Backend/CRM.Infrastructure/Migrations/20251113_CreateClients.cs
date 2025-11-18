using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    public partial class CreateClients : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Clients",
                columns: table => new
                {
                    ClientId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyName = table.Column<string>(type: "varchar(255)", nullable: false),
                    ContactName = table.Column<string>(type: "varchar(255)", nullable: true),
                    Email = table.Column<string>(type: "varchar(255)", nullable: false),
                    Mobile = table.Column<string>(type: "varchar(20)", nullable: false),
                    PhoneCode = table.Column<string>(type: "varchar(5)", nullable: true),
                    Gstin = table.Column<string>(type: "varchar(15)", nullable: true),
                    StateCode = table.Column<string>(type: "varchar(2)", nullable: true),
                    Address = table.Column<string>(type: "text", nullable: true),
                    City = table.Column<string>(type: "varchar(100)", nullable: true),
                    State = table.Column<string>(type: "varchar(100)", nullable: true),
                    PinCode = table.Column<string>(type: "varchar(10)", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clients", x => x.ClientId);
                    table.ForeignKey(
                        name: "FK_Clients_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Clients_Gstin",
                table: "Clients",
                column: "Gstin");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_CreatedByUserId",
                table: "Clients",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_CreatedAt",
                table: "Clients",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_UpdatedAt",
                table: "Clients",
                column: "UpdatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_DeletedAt",
                table: "Clients",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_CreatedByUserId_DeletedAt",
                table: "Clients",
                columns: new[] { "CreatedByUserId", "DeletedAt" });

            // Partial unique index on lower(email) for active records
            migrationBuilder.Sql(
                "CREATE UNIQUE INDEX \"IX_Clients_Email_Active\" ON \"Clients\" (lower(\"Email\")) WHERE \"DeletedAt\" IS NULL;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Clients");
        }
    }
}
