using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFileBasedTemplateFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    -- Add TemplateType column
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'QuotationTemplates' AND column_name = 'TemplateType') THEN
                        ALTER TABLE ""QuotationTemplates"" ADD COLUMN ""TemplateType"" character varying(50);
                    END IF;

                    -- Add IsFileBased column
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'QuotationTemplates' AND column_name = 'IsFileBased') THEN
                        ALTER TABLE ""QuotationTemplates"" ADD COLUMN ""IsFileBased"" boolean NOT NULL DEFAULT false;
                    END IF;

                    -- Add FileName column
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'QuotationTemplates' AND column_name = 'FileName') THEN
                        ALTER TABLE ""QuotationTemplates"" ADD COLUMN ""FileName"" character varying(255);
                    END IF;

                    -- Add FileUrl column (stores path or URL to the file)
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'QuotationTemplates' AND column_name = 'FileUrl') THEN
                        ALTER TABLE ""QuotationTemplates"" ADD COLUMN ""FileUrl"" text;
                    END IF;

                    -- Add FileSize column (in bytes)
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'QuotationTemplates' AND column_name = 'FileSize') THEN
                        ALTER TABLE ""QuotationTemplates"" ADD COLUMN ""FileSize"" bigint;
                    END IF;

                    -- Add MimeType column
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'QuotationTemplates' AND column_name = 'MimeType') THEN
                        ALTER TABLE ""QuotationTemplates"" ADD COLUMN ""MimeType"" character varying(100);
                    END IF;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'QuotationTemplates' AND column_name = 'TemplateType') THEN
                        ALTER TABLE ""QuotationTemplates"" DROP COLUMN ""TemplateType"";
                    END IF;

                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'QuotationTemplates' AND column_name = 'IsFileBased') THEN
                        ALTER TABLE ""QuotationTemplates"" DROP COLUMN ""IsFileBased"";
                    END IF;

                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'QuotationTemplates' AND column_name = 'FileName') THEN
                        ALTER TABLE ""QuotationTemplates"" DROP COLUMN ""FileName"";
                    END IF;

                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'QuotationTemplates' AND column_name = 'FileUrl') THEN
                        ALTER TABLE ""QuotationTemplates"" DROP COLUMN ""FileUrl"";
                    END IF;

                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'QuotationTemplates' AND column_name = 'FileSize') THEN
                        ALTER TABLE ""QuotationTemplates"" DROP COLUMN ""FileSize"";
                    END IF;

                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'QuotationTemplates' AND column_name = 'MimeType') THEN
                        ALTER TABLE ""QuotationTemplates"" DROP COLUMN ""MimeType"";
                    END IF;
                END $$;
            ");
        }
    }
}

