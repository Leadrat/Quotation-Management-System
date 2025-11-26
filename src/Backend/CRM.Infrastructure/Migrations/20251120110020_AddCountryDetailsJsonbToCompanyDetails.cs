using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCountryDetailsJsonbToCompanyDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CountryDetails",
                table: "CompanyDetails",
                type: "jsonb",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompanyDetails_CountryDetails",
                table: "CompanyDetails",
                column: "CountryDetails")
                .Annotation("Npgsql:IndexMethod", "gin");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CompanyDetails_CountryDetails",
                table: "CompanyDetails");

            migrationBuilder.DropColumn(
                name: "CountryDetails",
                table: "CompanyDetails");
        }
    }
}
