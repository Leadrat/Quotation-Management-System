-- Create leadrat tenant and update all existing data
-- This script creates a default tenant for all existing data

-- 1. Create Tenants table if it doesn't exist
CREATE TABLE IF NOT EXISTS "Tenants" (
    "TenantId" UUID PRIMARY KEY,
    "Identifier" VARCHAR(100) NOT NULL UNIQUE,
    "Name" VARCHAR(200) NOT NULL,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
);

-- 2. Insert the leadrat tenant
INSERT INTO "Tenants" ("TenantId", "Identifier", "Name", "IsActive", "CreatedAt", "UpdatedAt")
VALUES (
    '00000000-0000-0000-0000-000000000001',
    'leadrat',
    'Leadrat',
    TRUE,
    NOW(),
    NOW()
) ON CONFLICT ("TenantId") DO NOTHING;

-- 3. Update all existing users with leadrat tenant
UPDATE "Users"
SET "TenantId" = '00000000-0000-0000-0000-000000000001'
WHERE "TenantId" IS NULL OR "TenantId" = '00000000-0000-0000-0000-000000000000';

-- 4. Update all existing clients with leadrat tenant
UPDATE "Clients"
SET "TenantId" = '00000000-0000-0000-0000-000000000001'
WHERE "TenantId" IS NULL OR "TenantId" = '00000000-0000-0000-0000-000000000000';

-- 5. Update all existing quotations with leadrat tenant
UPDATE "Quotations"
SET "TenantId" = '00000000-0000-0000-0000-000000000001'
WHERE "TenantId" IS NULL OR "TenantId" = '00000000-0000-0000-0000-000000000000';

-- 6. Update all existing payments with leadrat tenant
UPDATE "Payments"
SET "TenantId" = '00000000-0000-0000-0000-000000000001'
WHERE "TenantId" IS NULL OR "TenantId" = '00000000-0000-0000-0000-000000000000';

-- 7. Update any other tables that might have TenantId
-- You can add more tables here as needed

-- 8. Verify the updates
SELECT 
    'Users' as table_name, 
    COUNT(*) as total_records,
    COUNT(CASE WHEN "TenantId" = '00000000-0000-0000-0000-000000000001' THEN 1 END) as leadrat_tenant_records
FROM "Users"
UNION ALL
SELECT 
    'Clients' as table_name, 
    COUNT(*) as total_records,
    COUNT(CASE WHEN "TenantId" = '00000000-0000-0000-0000-000000000001' THEN 1 END) as leadrat_tenant_records
FROM "Clients"
UNION ALL
SELECT 
    'Quotations' as table_name, 
    COUNT(*) as total_records,
    COUNT(CASE WHEN "TenantId" = '00000000-0000-0000-0000-000000000001' THEN 1 END) as leadrat_tenant_records
FROM "Quotations"
UNION ALL
SELECT 
    'Payments' as table_name, 
    COUNT(*) as total_records,
    COUNT(CASE WHEN "TenantId" = '00000000-0000-0000-0000-000000000001' THEN 1 END) as leadrat_tenant_records
FROM "Payments";
