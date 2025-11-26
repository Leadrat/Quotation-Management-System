using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentTemplateSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    -- Add TemplateFilePath column to QuotationTemplates
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'QuotationTemplates' AND column_name = 'TemplateFilePath') THEN
                        ALTER TABLE ""QuotationTemplates"" ADD COLUMN ""TemplateFilePath"" character varying(500);
                    END IF;

                    -- Add OriginalFileName column to QuotationTemplates
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'QuotationTemplates' AND column_name = 'OriginalFileName') THEN
                        ALTER TABLE ""QuotationTemplates"" ADD COLUMN ""OriginalFileName"" character varying(255);
                    END IF;

                    -- Add ProcessingStatus column to QuotationTemplates
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'QuotationTemplates' AND column_name = 'ProcessingStatus') THEN
                        ALTER TABLE ""QuotationTemplates"" ADD COLUMN ""ProcessingStatus"" character varying(50);
                    END IF;

                    -- Add ProcessingErrorMessage column to QuotationTemplates
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'QuotationTemplates' AND column_name = 'ProcessingErrorMessage') THEN
                        ALTER TABLE ""QuotationTemplates"" ADD COLUMN ""ProcessingErrorMessage"" text;
                    END IF;
                END $$;
            ");

            // Create TemplatePlaceholders table
            migrationBuilder.CreateTable(
                name: "TemplatePlaceholders",
                columns: table => new
                {
                    PlaceholderId = table.Column<Guid>(type: "uuid", nullable: false),
                    TemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlaceholderName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PlaceholderType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    OriginalText = table.Column<string>(type: "text", nullable: true),
                    PositionInDocument = table.Column<int>(type: "integer", nullable: true),
                    IsManuallyAdded = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemplatePlaceholders", x => x.PlaceholderId);
                    table.ForeignKey(
                        name: "FK_TemplatePlaceholders_Template",
                        column: x => x.TemplateId,
                        principalTable: "QuotationTemplates",
                        principalColumn: "TemplateId",
                        onDelete: ReferentialAction.Cascade);
                });

            // Create indexes
            migrationBuilder.CreateIndex(
                name: "IX_TemplatePlaceholders_TemplateId",
                table: "TemplatePlaceholders",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_TemplatePlaceholders_TemplateId_Type",
                table: "TemplatePlaceholders",
                columns: new[] { "TemplateId", "PlaceholderType" });

            migrationBuilder.CreateIndex(
                name: "IX_TemplatePlaceholders_TemplateId_PlaceholderName",
                table: "TemplatePlaceholders",
                columns: new[] { "TemplateId", "PlaceholderName" },
                unique: true);

            // Create indexes on QuotationTemplates for file-based templates
            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS IX_QuotationTemplates_IsFileBased_TemplateType
                ON ""QuotationTemplates"" (""IsFileBased"", ""TemplateType"")
                WHERE ""DeletedAt"" IS NULL AND ""IsFileBased"" = true;

                CREATE INDEX IF NOT EXISTS IX_QuotationTemplates_ProcessingStatus
                ON ""QuotationTemplates"" (""ProcessingStatus"")
                WHERE ""DeletedAt"" IS NULL AND ""IsFileBased"" = true;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "TemplatePlaceholders");

            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'QuotationTemplates' AND column_name = 'TemplateFilePath') THEN
                        ALTER TABLE ""QuotationTemplates"" DROP COLUMN ""TemplateFilePath"";
                    END IF;

                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'QuotationTemplates' AND column_name = 'OriginalFileName') THEN
                        ALTER TABLE ""QuotationTemplates"" DROP COLUMN ""OriginalFileName"";
                    END IF;

                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'QuotationTemplates' AND column_name = 'ProcessingStatus') THEN
                        ALTER TABLE ""QuotationTemplates"" DROP COLUMN ""ProcessingStatus"";
                    END IF;

                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'QuotationTemplates' AND column_name = 'ProcessingErrorMessage') THEN
                        ALTER TABLE ""QuotationTemplates"" DROP COLUMN ""ProcessingErrorMessage"";
                    END IF;
                END $$;

                DROP INDEX IF EXISTS IX_QuotationTemplates_IsFileBased_TemplateType;
                DROP INDEX IF EXISTS IX_QuotationTemplates_ProcessingStatus;
            ");
        }
    }
}

