-- Create Tenants table for multi-tenancy
-- This table is required for Payments foreign key constraint

CREATE TABLE IF NOT EXISTS "Tenants" (
    "TenantId" UUID NOT NULL PRIMARY KEY DEFAULT gen_random_uuid(),
    "Identifier" VARCHAR(100) NOT NULL UNIQUE,
    "Name" VARCHAR(200) NOT NULL,
    "ConnectionString" TEXT NULL,
    "IsActive" BOOLEAN NOT NULL DEFAULT true,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Insert default leadrat tenant
INSERT INTO "Tenants" (
    "TenantId",
    "Identifier", 
    "Name",
    "IsActive",
    "CreatedAt",
    "UpdatedAt"
) VALUES (
    '00000000-0000-0000-0000-000000000001',
    'leadrat',
    'Leadrat CRM',
    true,
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
) ON CONFLICT ("TenantId") DO NOTHING;

-- Show result
SELECT 
    'Tenants table setup complete' as status,
    COUNT(*) as total_tenants
FROM "Tenants";
