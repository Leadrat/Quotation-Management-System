using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    public partial class _20251113_UpdateRolesCaseInsensitive : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS citext;");
            migrationBuilder.Sql("ALTER TABLE \"Roles\" ALTER COLUMN \"RoleName\" TYPE citext;");
            migrationBuilder.Sql("DO $$ BEGIN IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'UX_Roles_RoleName') THEN CREATE UNIQUE INDEX \"UX_Roles_RoleName\" ON \"Roles\"(\"RoleName\"); END IF; END $$;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE \"Roles\" ALTER COLUMN \"RoleName\" TYPE character varying(100);");
        }
    }
}
