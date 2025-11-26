-- Check if migrations are applied by checking the __EFMigrationsHistory table
SELECT 'Migration History:' as info;
SELECT * FROM "__EFMigrationsHistory" ORDER BY "AppliedOn" DESC LIMIT 10;

-- Check if the tenant-related tables exist
SELECT 'Tenants table exists:' as info;
SELECT EXISTS (
    SELECT FROM information_schema.tables 
    WHERE table_schema = 'public' 
    AND table_name = 'Tenants'
) as tenants_exists;

-- Check if TenantId columns exist in main tables
SELECT 'Users table TenantId column:' as info;
SELECT EXISTS (
    SELECT FROM information_schema.columns 
    WHERE table_schema = 'public' 
    AND table_name = 'Users' 
    AND column_name = 'TenantId'
) as users_tenantid_exists;

SELECT 'Clients table TenantId column:' as info;
SELECT EXISTS (
    SELECT FROM information_schema.columns 
    WHERE table_schema = 'public' 
    AND table_name = 'Clients' 
    AND column_name = 'TenantId'
) as clients_tenantid_exists;

SELECT 'Quotations table TenantId column:' as info;
SELECT EXISTS (
    SELECT FROM information_schema.columns 
    WHERE table_schema = 'public' 
    AND table_name = 'Quotations' 
    AND column_name = 'TenantId'
) as quotations_tenantid_exists;

SELECT 'Payments table TenantId column:' as info;
SELECT EXISTS (
    SELECT FROM information_schema.columns 
    WHERE table_schema = 'public' 
    AND table_name = 'Payments' 
    AND column_name = 'TenantId'
) as payments_tenantid_exists;
