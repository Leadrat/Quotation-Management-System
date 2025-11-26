using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Spec30_AddSuperAdminRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Seed SuperAdmin role if it doesn't exist
            migrationBuilder.Sql(@"
                INSERT INTO ""Roles"" (""RoleId"", ""RoleName"", ""Description"", ""CreatedAt"", ""UpdatedAt"", ""IsActive"")
                SELECT 
                    '00000000-0000-0000-0000-000000000001'::uuid,
                    'SuperAdmin',
                    'Global administrator with access to all tenants and system configuration',
                    NOW(),
                    NOW(),
                    true
                WHERE NOT EXISTS (SELECT 1 FROM ""Roles"" WHERE ""RoleName"" = 'SuperAdmin');
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DELETE FROM ""Roles"" WHERE ""RoleName"" = 'SuperAdmin';
            ");
        }
    }
}
