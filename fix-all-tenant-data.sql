-- Fix all tenant data to use the correct leadrat tenant ID
-- This ensures all data uses the correct tenant ID that matches TenantContext

DO $$
DECLARE
    correct_tenant_id UUID := '11111111-1111-1111-1111-111111111111';
    old_tenant_id UUID := '00000000-0000-0000-0000-000000000001';
    users_updated INTEGER := 0;
    clients_updated INTEGER := 0;
    quotations_updated INTEGER := 0;
    payments_updated INTEGER := 0;
BEGIN
    RAISE NOTICE 'Fixing tenant data to use correct tenant ID: %', correct_tenant_id;
    
    -- Update Users with old or wrong tenant ID
    UPDATE "Users" 
    SET "TenantId" = correct_tenant_id 
    WHERE "TenantId" IS NULL OR "TenantId" = old_tenant_id OR "TenantId" != correct_tenant_id;
    GET DIAGNOSTICS users_updated = ROW_COUNT;
    
    -- Update Clients with old or wrong tenant ID
    UPDATE "Clients" 
    SET "TenantId" = correct_tenant_id 
    WHERE "TenantId" IS NULL OR "TenantId" = old_tenant_id OR "TenantId" != correct_tenant_id;
    GET DIAGNOSTICS clients_updated = ROW_COUNT;
    
    -- Update Quotations with old or wrong tenant ID
    UPDATE "Quotations" 
    SET "TenantId" = correct_tenant_id 
    WHERE "TenantId" IS NULL OR "TenantId" = old_tenant_id OR "TenantId" != correct_tenant_id;
    GET DIAGNOSTICS quotations_updated = ROW_COUNT;
    
    -- Update Payments with old or wrong tenant ID
    UPDATE "Payments" 
    SET "TenantId" = correct_tenant_id 
    WHERE "TenantId" IS NULL OR "TenantId" = old_tenant_id OR "TenantId" != correct_tenant_id;
    GET DIAGNOSTICS payments_updated = ROW_COUNT;
    
    RAISE NOTICE 'Tenant Data Fix Results:';
    RAISE NOTICE 'Users updated: %', users_updated;
    RAISE NOTICE 'Clients updated: %', clients_updated;
    RAISE NOTICE 'Quotations updated: %', quotations_updated;
    RAISE NOTICE 'Payments updated: %', payments_updated;
    
    IF users_updated > 0 OR clients_updated > 0 OR quotations_updated > 0 OR payments_updated > 0 THEN
        RAISE NOTICE 'SUCCESS: All data has been updated to use the correct tenant ID!';
    ELSE
        RAISE NOTICE 'INFO: All data already uses the correct tenant ID';
    END IF;
END $$;

-- Verify the fix
SELECT 'Verification - Tenant Data Status:' as info;
SELECT 
    'Users' as table_name,
    COUNT(*) as total_count,
    COUNT(CASE WHEN "TenantId" = '11111111-1111-1111-1111-111111111111' THEN 1 END) as correct_tenant_count
FROM "Users"
UNION ALL
SELECT 
    'Clients' as table_name,
    COUNT(*) as total_count,
    COUNT(CASE WHEN "TenantId" = '11111111-1111-1111-1111-111111111111' THEN 1 END) as correct_tenant_count
FROM "Clients"
UNION ALL
SELECT 
    'Quotations' as table_name,
    COUNT(*) as total_count,
    COUNT(CASE WHEN "TenantId" = '11111111-1111-1111-1111-111111111111' THEN 1 END) as correct_tenant_count
FROM "Quotations"
UNION ALL
SELECT 
    'Payments' as table_name,
    COUNT(*) as total_count,
    COUNT(CASE WHEN "TenantId" = '11111111-1111-1111-1111-111111111111' THEN 1 END) as correct_tenant_count
FROM "Payments";
