-- Check current tenant data status
-- See what tenant IDs exist in each table

-- Check Users table
SELECT 
    'Users' as table_name,
    COUNT(*) as total_records,
    COUNT(CASE WHEN "TenantId" IS NULL THEN 1 END) as null_tenant,
    COUNT(CASE WHEN "TenantId" = '11111111-1111-1111-1111-111111111111' THEN 1 END) as leadrat_tenant,
    COUNT(CASE WHEN "TenantId" = '00000000-0000-0000-0000-000000000001' THEN 1 END) as default_tenant,
    "TenantId"
FROM "Users" 
GROUP BY "TenantId"
ORDER BY "TenantId";

-- Check Clients table
SELECT 
    'Clients' as table_name,
    COUNT(*) as total_records,
    COUNT(CASE WHEN "TenantId" IS NULL THEN 1 END) as null_tenant,
    COUNT(CASE WHEN "TenantId" = '11111111-1111-1111-1111-111111111111' THEN 1 END) as leadrat_tenant,
    COUNT(CASE WHEN "TenantId" = '00000000-0000-0000-0000-000000000001' THEN 1 END) as default_tenant,
    "TenantId"
FROM "Clients" 
GROUP BY "TenantId"
ORDER BY "TenantId";

-- Check Quotations table
SELECT 
    'Quotations' as table_name,
    COUNT(*) as total_records,
    COUNT(CASE WHEN "TenantId" IS NULL THEN 1 END) as null_tenant,
    COUNT(CASE WHEN "TenantId" = '11111111-1111-1111-1111-111111111111' THEN 1 END) as leadrat_tenant,
    COUNT(CASE WHEN "TenantId" = '00000000-0000-0000-0000-000000000001' THEN 1 END) as default_tenant,
    "TenantId"
FROM "Quotations" 
GROUP BY "TenantId"
ORDER BY "TenantId";

-- Check Payments table
SELECT 
    'Payments' as table_name,
    COUNT(*) as total_records,
    COUNT(CASE WHEN "TenantId" IS NULL THEN 1 END) as null_tenant,
    COUNT(CASE WHEN "TenantId" = '11111111-1111-1111-1111-111111111111' THEN 1 END) as leadrat_tenant,
    COUNT(CASE WHEN "TenantId" = '00000000-0000-0000-0000-000000000001' THEN 1 END) as default_tenant,
    "TenantId"
FROM "Payments" 
GROUP BY "TenantId"
ORDER BY "TenantId";

-- Show sample data from each table
SELECT 'Sample Users:' as info;
SELECT "UserId", "Email", "TenantId" FROM "Users" LIMIT 3;

SELECT 'Sample Clients:' as info;
SELECT "ClientId", "ClientName", "TenantId" FROM "Clients" LIMIT 3;

SELECT 'Sample Quotations:' as info;
SELECT "QuotationId", "QuotationNumber", "TenantId" FROM "Quotations" LIMIT 3;

SELECT 'Sample Payments:' as info;
SELECT "PaymentId", "PaymentReference", "TenantId" FROM "Payments" LIMIT 3;
