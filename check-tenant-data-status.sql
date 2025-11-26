-- Check tenant data distribution in all tables
DO $$
DECLARE
    users_count INTEGER;
    clients_count INTEGER;
    quotations_count INTEGER;
    payments_count INTEGER;
    users_with_tenant INTEGER;
    clients_with_tenant INTEGER;
    quotations_with_tenant INTEGER;
    payments_with_tenant INTEGER;
BEGIN
    -- Get total counts
    SELECT COUNT(*) INTO users_count FROM "Users";
    SELECT COUNT(*) INTO clients_count FROM "Clients";
    SELECT COUNT(*) INTO quotations_count FROM "Quotations";
    SELECT COUNT(*) INTO payments_count FROM "Payments";
    
    -- Get counts with non-null TenantId
    SELECT COUNT(*) INTO users_with_tenant FROM "Users" WHERE "TenantId" IS NOT NULL;
    SELECT COUNT(*) INTO clients_with_tenant FROM "Clients" WHERE "TenantId" IS NOT NULL;
    SELECT COUNT(*) INTO quotations_with_tenant FROM "Quotations" WHERE "TenantId" IS NOT NULL;
    SELECT COUNT(*) INTO payments_with_tenant FROM "Payments" WHERE "TenantId" IS NOT NULL;
    
    RAISE NOTICE 'Tenant Data Status:';
    RAISE NOTICE 'Users: % total, % with TenantId', users_count, users_with_tenant;
    RAISE NOTICE 'Clients: % total, % with TenantId', clients_count, clients_with_tenant;
    RAISE NOTICE 'Quotations: % total, % with TenantId', quotations_count, quotations_with_tenant;
    RAISE NOTICE 'Payments: % total, % with TenantId', payments_count, payments_with_tenant;
    
    -- Check for empty TenantIds
    IF EXISTS (SELECT 1 FROM "Users" WHERE "TenantId" = '00000000-0000-0000-0000-000000000000') THEN
        RAISE NOTICE 'WARNING: Users table has old tenant ID (00000000-0000-0000-0000-000000000000)';
    END IF;
    
    IF EXISTS (SELECT 1 FROM "Users" WHERE "TenantId" = '11111111-1111-1111-1111-111111111111') THEN
        RAISE NOTICE 'SUCCESS: Users table has correct tenant ID (11111111-1111-1111-1111-111111111111)';
    END IF;
END $$;
