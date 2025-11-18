using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    public partial class AddClientFtsAndIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Ensure required extensions (safe if already present)
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS pg_trgm;");
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS unaccent;");
            
            // Make unaccent function immutable for use in indexes (requires superuser, but safe to try)
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    -- Try to alter unaccent function to be immutable (requires superuser)
                    -- If it fails, the index creation will just use the function as-is
                    BEGIN
                        ALTER FUNCTION unaccent(text) IMMUTABLE;
                    EXCEPTION WHEN OTHERS THEN
                        -- Function might already be immutable or we don't have permission
                        -- This is okay, we'll create the index without it
                        NULL;
                    END;
                END $$;
            ");

            // Create GIN FTS index over CompanyName + ContactName + Email
            // Use a simpler version without unaccent if unaccent is not immutable
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    -- Try to create index with unaccent
                    BEGIN
                        CREATE INDEX IF NOT EXISTS ""IX_Clients_FTS""
                        ON ""Clients"" USING GIN (
                          to_tsvector('simple',
                            coalesce(unaccent(""CompanyName""),'') || ' ' ||
                            coalesce(unaccent(""ContactName""),'') || ' ' ||
                            coalesce(unaccent(""Email""),'')
                          )
                        );
                    EXCEPTION WHEN OTHERS THEN
                        -- If unaccent is not immutable, create index without it
                        DROP INDEX IF EXISTS ""IX_Clients_FTS"";
                        CREATE INDEX IF NOT EXISTS ""IX_Clients_FTS""
                        ON ""Clients"" USING GIN (
                          to_tsvector('simple',
                            coalesce(""CompanyName"",'') || ' ' ||
                            coalesce(""ContactName"",'') || ' ' ||
                            coalesce(""Email"",'')
                          )
                        );
                    END;
                END $$;
            ");

            // Supportive indexes for filters
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS \"IX_Clients_State\" ON \"Clients\" (\"State\");");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS \"IX_Clients_City\" ON \"Clients\" (\"City\");");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS \"IX_Clients_StateCode\" ON \"Clients\" (\"StateCode\");");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS \"IX_Clients_Email\" ON \"Clients\" (\"Email\");");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS \"IX_Clients_CompanyName\" ON \"Clients\" (\"CompanyName\");");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_Clients_FTS\";");
            migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_Clients_State\";");
            migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_Clients_City\";");
            migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_Clients_StateCode\";");
            migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_Clients_Email\";");
            migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_Clients_CompanyName\";");
        }
    }
}
