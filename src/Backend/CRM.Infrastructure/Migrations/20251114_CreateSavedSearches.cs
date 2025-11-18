using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    public partial class CreateSavedSearches : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SavedSearches",
                columns: table => new
                {
                    SavedSearchId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SearchName = table.Column<string>(type: "varchar(255)", nullable: false),
                    FilterCriteria = table.Column<string>(type: "jsonb", nullable: false),
                    SortBy = table.Column<string>(type: "varchar(50)", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavedSearches", x => x.SavedSearchId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SavedSearches_UserId",
                table: "SavedSearches",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SavedSearches_IsActive",
                table: "SavedSearches",
                column: "IsActive");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "SavedSearches");
        }
    }
}
