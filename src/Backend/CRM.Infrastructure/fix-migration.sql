-- Run this SQL script BEFORE running migrations
-- This will mark the first migration as applied since tables already exist from EnsureCreated()

-- Ensure __EFMigrationsHistory table exists
CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" VARCHAR(150) NOT NULL,
    "ProductVersion" VARCHAR(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

-- Mark the first migration as applied (tables already exist)
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20251114062356_UserRoles_AddAndBackfill', '8.0.8')
ON CONFLICT ("MigrationId") DO NOTHING;

