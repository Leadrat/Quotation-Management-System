-- Script to remove Client role users from Users table
-- This script soft deletes all users with Client role
-- Run this script directly on the PostgreSQL database

UPDATE "Users"
SET 
    "DeletedAt" = CURRENT_TIMESTAMP,
    "IsActive" = false,
    "UpdatedAt" = CURRENT_TIMESTAMP
WHERE 
    "RoleId" = '00F3CF90-C1A2-4B46-96A2-6A58EF54E8DD' -- Client Role ID
    AND "DeletedAt" IS NULL;

-- Verify deletion (optional - check how many Client users were soft-deleted)
-- SELECT COUNT(*) FROM "Users" WHERE "RoleId" = '00F3CF90-C1A2-4B46-96A2-6A58EF54E8DD' AND "DeletedAt" IS NOT NULL;

