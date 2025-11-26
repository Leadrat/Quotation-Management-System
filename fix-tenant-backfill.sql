-- Fix tenant backfill for all existing data
-- Assign all existing users, clients, quotations, and payments to leadrat tenant

DO $$
DECLARE
    leadrat_tenant_id UUID := '11111111-1111-1111-1111-111111111111'; -- Correct tenant ID from database
    users_updated INTEGER;
    clients_updated INTEGER;
    quotations_updated INTEGER;
    payments_updated INTEGER;
BEGIN
    RAISE NOTICE 'Starting tenant backfill for leadrat tenant: %', leadrat_tenant_id;
    
    -- Update Users table
    UPDATE "Users" 
    SET "TenantId" = leadrat_tenant_id 
    WHERE "TenantId" IS NULL OR "TenantId" != leadrat_tenant_id;
    GET DIAGNOSTICS users_updated = ROW_COUNT;
    RAISE NOTICE 'Updated % users with leadrat tenant', users_updated;
    
    -- Update Clients table
    UPDATE "Clients" 
    SET "TenantId" = leadrat_tenant_id 
    WHERE "TenantId" IS NULL OR "TenantId" != leadrat_tenant_id;
    GET DIAGNOSTICS clients_updated = ROW_COUNT;
    RAISE NOTICE 'Updated % clients with leadrat tenant', clients_updated;
    
    -- Update Quotations table
    UPDATE "Quotations" 
    SET "TenantId" = leadrat_tenant_id 
    WHERE "TenantId" IS NULL OR "TenantId" != leadrat_tenant_id;
    GET DIAGNOSTICS quotations_updated = ROW_COUNT;
    RAISE NOTICE 'Updated % quotations with leadrat tenant', quotations_updated;
    
    -- Update Payments table
    UPDATE "Payments" 
    SET "TenantId" = leadrat_tenant_id 
    WHERE "TenantId" IS NULL OR "TenantId" != leadrat_tenant_id;
    GET DIAGNOSTICS payments_updated = ROW_COUNT;
    RAISE NOTICE 'Updated % payments with leadrat tenant', payments_updated;
    
    -- Update other related tables that might have TenantId
    -- ClientHistories
    BEGIN
        UPDATE "ClientHistories" 
        SET "TenantId" = leadrat_tenant_id 
        WHERE "TenantId" IS NULL OR "TenantId" != leadrat_tenant_id;
        GET DIAGNOSTICS users_updated = ROW_COUNT;
        RAISE NOTICE 'Updated % client histories with leadrat tenant', users_updated;
    EXCEPTION WHEN OTHERS THEN
        RAISE NOTICE 'ClientHistories table not found or no TenantId column';
    END;
    
    -- QuotationLineItems
    BEGIN
        UPDATE "QuotationLineItems" 
        SET "TenantId" = leadrat_tenant_id 
        WHERE "TenantId" IS NULL OR "TenantId" != leadrat_tenant_id;
        GET DIAGNOSTICS users_updated = ROW_COUNT;
        RAISE NOTICE 'Updated % quotation line items with leadrat tenant', users_updated;
    EXCEPTION WHEN OTHERS THEN
        RAISE NOTICE 'QuotationLineItems table not found or no TenantId column';
    END;
    
    RAISE NOTICE 'Tenant backfill completed successfully!';
    RAISE NOTICE 'Summary: Users=%, Clients=%, Quotations=%, Payments=%', 
                  users_updated, clients_updated, quotations_updated, payments_updated;
END $$;

-- Show results
SELECT 
    'Tenant Backfill Results' as status,
    (SELECT COUNT(*) FROM "Users" WHERE "TenantId" = '11111111-1111-1111-1111-111111111111') as users_with_leadrat,
    (SELECT COUNT(*) FROM "Clients" WHERE "TenantId" = '11111111-1111-1111-1111-111111111111') as clients_with_leadrat,
    (SELECT COUNT(*) FROM "Quotations" WHERE "TenantId" = '11111111-1111-1111-1111-111111111111') as quotations_with_leadrat,
    (SELECT COUNT(*) FROM "Payments" WHERE "TenantId" = '11111111-1111-1111-1111-111111111111') as payments_with_leadrat;
