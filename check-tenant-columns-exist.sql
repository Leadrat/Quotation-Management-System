-- Check if TenantId columns exist in main tables
DO $$
DECLARE
    users_has_tenantid BOOLEAN;
    clients_has_tenantid BOOLEAN;
    quotations_has_tenantid BOOLEAN;
    payments_has_tenantid BOOLEAN;
BEGIN
    -- Check Users table
    SELECT EXISTS (
        SELECT FROM information_schema.columns 
        WHERE table_schema = 'public' 
        AND table_name = 'Users' 
        AND column_name = 'TenantId'
    ) INTO users_has_tenantid;
    
    -- Check Clients table
    SELECT EXISTS (
        SELECT FROM information_schema.columns 
        WHERE table_schema = 'public' 
        AND table_name = 'Clients' 
        AND column_name = 'TenantId'
    ) INTO clients_has_tenantid;
    
    -- Check Quotations table
    SELECT EXISTS (
        SELECT FROM information_schema.columns 
        WHERE table_schema = 'public' 
        AND table_name = 'Quotations' 
        AND column_name = 'TenantId'
    ) INTO quotations_has_tenantid;
    
    -- Check Payments table
    SELECT EXISTS (
        SELECT FROM information_schema.columns 
        WHERE table_schema = 'public' 
        AND table_name = 'Payments' 
        AND column_name = 'TenantId'
    ) INTO payments_has_tenantid;
    
    RAISE NOTICE 'TenantId Columns Status:';
    RAISE NOTICE 'Users table has TenantId: %', users_has_tenantid;
    RAISE NOTICE 'Clients table has TenantId: %', clients_has_tenantid;
    RAISE NOTICE 'Quotations table has TenantId: %', quotations_has_tenantid;
    RAISE NOTICE 'Payments table has TenantId: %', payments_has_tenantid;
    
    IF users_has_tenantid AND clients_has_tenantid AND quotations_has_tenantid AND payments_has_tenantid THEN
        RAISE NOTICE 'SUCCESS: All main tables have TenantId column - migrations are applied!';
    ELSE
        RAISE NOTICE 'WARNING: Some tables missing TenantId column';
    END IF;
END $$;
