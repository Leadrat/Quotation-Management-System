-- Fix the tenant foreign key constraint issue
-- Either create the missing tenant or disable the constraint

DO $$
DECLARE
    tenants_table_exists BOOLEAN;
    payments_table_exists BOOLEAN;
    tenant_count INTEGER;
BEGIN
    -- Check if Tenants table exists
    SELECT EXISTS (
        SELECT FROM information_schema.tables 
        WHERE table_schema = 'public' 
        AND table_name = 'Tenants'
    ) INTO tenants_table_exists;
    
    -- Check if Payments table exists
    SELECT EXISTS (
        SELECT FROM information_schema.tables 
        WHERE table_schema = 'public' 
        AND table_name = 'Payments'
    ) INTO payments_table_exists;
    
    RAISE NOTICE 'Tenants table exists: %', tenants_table_exists;
    RAISE NOTICE 'Payments table exists: %', payments_table_exists;
    
    IF tenants_table_exists AND payments_table_exists THEN
        -- Check if our tenant ID exists
        SELECT COUNT(*) INTO tenant_count FROM "Tenants" WHERE "TenantId" = '11111111-1111-1111-1111-111111111111';
        
        IF tenant_count = 0 THEN
            -- Insert the missing tenant
            INSERT INTO "Tenants" (
                "TenantId",
                "Identifier", 
                "Name",
                "IsActive",
                "CreatedAt",
                "UpdatedAt"
            ) VALUES (
                '11111111-1111-1111-1111-111111111111',
                'leadrat',
                'Leadrat CRM',
                true,
                CURRENT_TIMESTAMP,
                CURRENT_TIMESTAMP
            );
            
            RAISE NOTICE 'Created missing leadrat tenant';
        ELSE
            RAISE NOTICE 'Leadrat tenant already exists';
        END IF;
    END IF;
    
    -- If Tenants table doesn't exist, drop the foreign key constraint
    IF NOT tenants_table_exists AND payments_table_exists THEN
        -- Drop the foreign key constraint if it exists
        BEGIN
            ALTER TABLE "Payments" DROP CONSTRAINT IF EXISTS "FK_Payments_Tenants_TenantId";
            RAISE NOTICE 'Dropped foreign key constraint from Payments table';
        EXCEPTION WHEN OTHERS THEN
            RAISE NOTICE 'No foreign key constraint to drop or error dropping it';
        END;
    END IF;
END $$;

-- Show result
SELECT 
    'Tenant constraint fix completed' as status,
    (SELECT COUNT(*) FROM information_schema.table_constraints WHERE constraint_name = 'FK_Payments_Tenants_TenantId') as fk_constraint_exists;
