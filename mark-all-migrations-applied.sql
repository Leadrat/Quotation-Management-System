-- Mark all existing migrations as applied
-- This is needed because database was created using EnsureCreated() instead of migrations

-- Ensure __EFMigrationsHistory table exists
CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" VARCHAR(150) NOT NULL,
    "ProductVersion" VARCHAR(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

-- Insert all migration history records (mark as applied)
-- Only insert if not already present
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
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
    ('20251115232412_CreateEmailNotificationLogTable', '8.0.8')
) AS v("MigrationId", "ProductVersion")
WHERE NOT EXISTS (
    SELECT 1 FROM "__EFMigrationsHistory" 
    WHERE "__EFMigrationsHistory"."MigrationId" = v."MigrationId"
);

