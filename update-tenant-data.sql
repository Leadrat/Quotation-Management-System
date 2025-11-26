-- Simple script to update all existing data with leadrat tenant
-- No tenant table needed for now

-- 1. Update all existing users with leadrat tenant
UPDATE "Users"
SET "TenantId" = '00000000-0000-0000-0000-000000000001'
WHERE "TenantId" IS NULL OR "TenantId" = '00000000-0000-0000-0000-000000000000';

-- 2. Update all existing clients with leadrat tenant
UPDATE "Clients"
SET "TenantId" = '00000000-0000-0000-0000-000000000001'
WHERE "TenantId" IS NULL OR "TenantId" = '00000000-0000-0000-0000-000000000000';

-- 3. Update all existing quotations with leadrat tenant
UPDATE "Quotations"
SET "TenantId" = '00000000-0000-0000-0000-000000000001'
WHERE "TenantId" IS NULL OR "TenantId" = '00000000-0000-0000-0000-000000000000';

-- 4. Update all existing payments with leadrat tenant
UPDATE "Payments"
SET "TenantId" = '00000000-0000-0000-0000-000000000001'
WHERE "TenantId" IS NULL OR "TenantId" = '00000000-0000-0000-0000-000000000000';

-- 5. Verify the updates
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
