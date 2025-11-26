using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTemplateUniqueIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop old unique index (Name + OwnerUserId) filtered on DeletedAt IS NULL
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1 FROM pg_indexes 
                        WHERE schemaname = 'public' AND indexname = 'IX_QuotationTemplates_Name_OwnerUserId'
                    ) THEN
                        DROP INDEX ""IX_QuotationTemplates_Name_OwnerUserId"";
                    END IF;
                END $$;
            ");

            // Create new unique index including Version so multiple versions can share the same name per owner
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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop the new index
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

            // Recreate the old unique index (if needed)
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM pg_indexes 
                        WHERE schemaname = 'public' AND indexname = 'IX_QuotationTemplates_Name_OwnerUserId'
                    ) THEN
                        CREATE UNIQUE INDEX ""IX_QuotationTemplates_Name_OwnerUserId"" 
                        ON ""QuotationTemplates""(""Name"", ""OwnerUserId"") 
                        WHERE ""DeletedAt"" IS NULL;
                    END IF;
                END $$;
            ");
        }
    }
}
