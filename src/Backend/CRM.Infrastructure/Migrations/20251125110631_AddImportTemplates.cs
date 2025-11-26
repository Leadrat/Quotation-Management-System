using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddImportTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Guard: index may not exist on some environments
            migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_QuotationTemplates_Name_OwnerUserId\";");

            migrationBuilder.CreateTable(
                name: "ImportedTemplates",
                columns: table => new
                {
                    ImportedTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    ImportSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ContentRef = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportedTemplates", x => x.ImportedTemplateId);
                });

            migrationBuilder.CreateTable(
                name: "ImportSessions",
                columns: table => new
                {
                    ImportSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceType = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    SourceFileRef = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SuggestedMappingsJson = table.Column<string>(type: "jsonb", nullable: true),
                    ConfirmedMappingsJson = table.Column<string>(type: "jsonb", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportSessions", x => x.ImportSessionId);
                });

            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM pg_indexes 
                        WHERE schemaname = 'public' AND indexname = 'IX_QuotationTemplates_Name_OwnerUserId_Version'
                    ) THEN
                        CREATE UNIQUE INDEX ""IX_QuotationTemplates_Name_OwnerUserId_Version"" 
                        ON ""QuotationTemplates""(""Name"", ""OwnerUserId"", ""Version"") 
                        WHERE ""DeletedAt"" IS NULL;
                    END IF;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImportedTemplates");

            migrationBuilder.DropTable(
                name: "ImportSessions");

            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1 FROM pg_indexes 
                        WHERE schemaname = 'public' AND indexname = 'IX_QuotationTemplates_Name_OwnerUserId_Version'
                    ) THEN
                        DROP INDEX ""IX_QuotationTemplates_Name_OwnerUserId_Version"";
                    END IF;
                END $$;
            ");

            migrationBuilder.CreateIndex(
                name: "IX_QuotationTemplates_Name_OwnerUserId",
                table: "QuotationTemplates",
                columns: new[] { "Name", "OwnerUserId" },
                unique: true,
                filter: "[DeletedAt] IS NULL");
        }
    }
}
