-- Fix migration history by marking the first migration as applied
-- Since tables already exist from EnsureCreated(), we can safely mark this migration as applied

-- Ensure __EFMigrationsHistory table exists
CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" VARCHAR(150) NOT NULL,
    "ProductVersion" VARCHAR(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

-- Insert the failing migration into history (mark as applied)
-- This will allow remaining migrations to proceed
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20251114062356_UserRoles_AddAndBackfill', '8.0.8')
ON CONFLICT ("MigrationId") DO NOTHING;

