-- Check existing tenant data and create sample payments
-- This script finds a valid tenant ID or creates one, then creates sample payments

DO $$
DECLARE
    payments_table_exists BOOLEAN;
    tenants_table_exists BOOLEAN;
    valid_tenant_id UUID;
    quotation_count INTEGER;
    sample_quotation_id UUID;
BEGIN
    -- Check if Payments table exists
    SELECT EXISTS (
        SELECT FROM information_schema.tables 
        WHERE table_schema = 'public' 
        AND table_name = 'Payments'
    ) INTO payments_table_exists;
    
    IF NOT payments_table_exists THEN
        RAISE NOTICE 'Payments table does not exist';
        RETURN;
    END IF;
    
    -- Check if Tenants table exists
    SELECT EXISTS (
        SELECT FROM information_schema.tables 
        WHERE table_schema = 'public' 
        AND table_name ILIKE 'tenant%'
    ) INTO tenants_table_exists;
    
    -- Try to find a valid tenant ID from existing payments
    BEGIN
        SELECT "TenantId" INTO valid_tenant_id 
        FROM "Payments" 
        LIMIT 1;
    EXCEPTION WHEN OTHERS THEN
        valid_tenant_id := NULL;
    END;
    
    -- If no valid tenant found and tenants table exists, get one from there
    IF valid_tenant_id IS NULL AND tenants_table_exists THEN
        BEGIN
            EXECUTE 'SELECT "TenantId" FROM "Tenants" LIMIT 1' INTO valid_tenant_id;
        EXCEPTION WHEN OTHERS THEN
            valid_tenant_id := NULL;
        END;
    END IF;
    
    -- If still no tenant, use a default one
    IF valid_tenant_id IS NULL THEN
        valid_tenant_id := '00000000-0000-0000-0000-000000000001';
        RAISE NOTICE 'Using default tenant ID: %', valid_tenant_id;
    ELSE
        RAISE NOTICE 'Found existing tenant ID: %', valid_tenant_id;
    END IF;
    
    -- Check if we have quotations
    SELECT COUNT(*) INTO quotation_count FROM "Quotations";
    
    IF quotation_count = 0 THEN
        RAISE NOTICE 'No quotations found. Cannot create payments without quotations.';
        RETURN;
    END IF;
    
    -- Get first quotation for sample payment
    SELECT "QuotationId" INTO sample_quotation_id 
    FROM "Quotations" 
    LIMIT 1;
    
    -- Clear any existing sample payments
    DELETE FROM "Payments" WHERE "PaymentReference" LIKE 'SAMPLE-%' OR "PaymentReference" LIKE 'RAZ-%' OR "PaymentReference" LIKE 'STRIPE-%';
    
    -- Insert sample payments with the valid tenant ID
    INSERT INTO "Payments" (
        "PaymentId",
        "TenantId",
        "QuotationId",
        "PaymentGateway",
        "PaymentReference",
        "AmountPaid",
        "Currency",
        "PaymentStatus",
        "PaymentDate",
        "CreatedAt",
        "UpdatedAt"
    ) VALUES 
    (
        gen_random_uuid(),
        valid_tenant_id,
        sample_quotation_id,
        'Manual',
        'SAMPLE-001-' || gen_random_uuid(),
        5000.00,
        'INR',
        1, -- Success
        CURRENT_TIMESTAMP - INTERVAL '2 days',
        CURRENT_TIMESTAMP - INTERVAL '2 days',
        CURRENT_TIMESTAMP - INTERVAL '2 days'
    ),
    (
        gen_random_uuid(),
        valid_tenant_id,
        sample_quotation_id,
        'Razorpay',
        'RAZ-002-' || gen_random_uuid(),
        3000.00,
        'INR',
        1, -- Success
        CURRENT_TIMESTAMP - INTERVAL '1 day',
        CURRENT_TIMESTAMP - INTERVAL '1 day',
        CURRENT_TIMESTAMP - INTERVAL '1 day'
    ),
    (
        gen_random_uuid(),
        valid_tenant_id,
        sample_quotation_id,
        'Stripe',
        'STRIPE-003-' || gen_random_uuid(),
        2000.00,
        'INR',
        2, -- Pending
        CURRENT_TIMESTAMP,
        CURRENT_TIMESTAMP,
        CURRENT_TIMESTAMP
    );
    
    RAISE NOTICE 'Sample payments created with tenant ID: %', valid_tenant_id;
END $$;

-- Show result
SELECT 
    'Sample payments created successfully' as status,
    COUNT(*) as total_payments,
    "TenantId" as tenant_id_used
FROM "Payments" 
GROUP BY "TenantId";
