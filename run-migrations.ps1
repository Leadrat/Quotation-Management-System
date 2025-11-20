# PowerShell script to mark existing migrations as applied and run remaining migrations

$connectionString = "Host=postgresql-caa1dffb-o9d7d637a.database.cloud.ovh.net;Port=20184;Database=pankaj;Username=avnadmin;Password=EmZwIgKyz7b1uTO3a6F5;SslMode=Require;TrustServerCertificate=true"

# SQL script content
$sqlScript = @"
-- Ensure __EFMigrationsHistory table exists
CREATE TABLE IF NOT EXISTS ""__EFMigrationsHistory"" (
    ""MigrationId"" VARCHAR(150) NOT NULL,
    ""ProductVersion"" VARCHAR(32) NOT NULL,
    CONSTRAINT ""PK___EFMigrationsHistory"" PRIMARY KEY (""MigrationId"")
);

-- Insert all migration history records (mark as applied)
INSERT INTO ""__EFMigrationsHistory"" (""MigrationId"", ""ProductVersion"")
SELECT * FROM (VALUES
    ('20251112_CreateRefreshTokensTable', '8.0.8'),
    ('20251112_CreateUsersTable', '8.0.8'),
    ('20251113_CreateClients', '8.0.8'),
    ('20251113_CreatePasswordResetTokens', '8.0.8'),
    ('20251113_UpdateRolesCaseInsensitive', '8.0.8'),
    ('20251114_AddClientFtsAndIndexes', '8.0.8'),
    ('20251114_CreateSavedSearches', '8.0.8'),
    ('20251114062356_UserRoles_AddAndBackfill', '8.0.8'),
    ('20251115_CreateClientHistoryTables', '8.0.8'),
    ('20251115185000_CreateDiscountApprovalsTable', '8.0.8'),
    ('20251115185001_AddQuotationApprovalLocking', '8.0.8'),
    ('20251115232410_CreateNotificationsTable', '8.0.8'),
    ('20251115232411_CreateNotificationPreferencesTable', '8.0.8'),
    ('20251115232412_CreateEmailNotificationLogTable', '8.0.8'),
    ('20251118100000_AddAdminConfigurationTables', '8.0.8')
) AS v(""MigrationId"", ""ProductVersion"")
WHERE NOT EXISTS (
    SELECT 1 FROM ""__EFMigrationsHistory"" 
    WHERE ""__EFMigrationsHistory"".""MigrationId"" = v.""MigrationId""
);
"@

Write-Host "Step 1: Marking existing migrations as applied..."

# Try to use psql if available, otherwise use .NET approach
$psqlPath = Get-Command psql -ErrorAction SilentlyContinue
if ($psqlPath) {
    # Extract connection details from connection string
    $connParts = $connectionString -split ';'
    $hostName = ($connParts | Where-Object { $_ -like 'Host=*' }) -replace 'Host=', ''
    $port = ($connParts | Where-Object { $_ -like 'Port=*' }) -replace 'Port=', ''
    $database = ($connParts | Where-Object { $_ -like 'Database=*' }) -replace 'Database=', ''
    $username = ($connParts | Where-Object { $_ -like 'Username=*' }) -replace 'Username=', ''
    $password = ($connParts | Where-Object { $_ -like 'Password=*' }) -replace 'Password=', ''
    
    $env:PGPASSWORD = $password
    echo $sqlScript | psql -h $hostName -p $port -U $username -d $database
    Remove-Item Env:\PGPASSWORD
} else {
    Write-Host "psql not found. Creating .NET script to execute SQL..."
    # Create a .NET script instead
}

Write-Host "Step 2: Running remaining migrations..."
Set-Location "src\Backend\CRM.Infrastructure"
dotnet ef database update --startup-project ..\CRM.Api --context AppDbContext

