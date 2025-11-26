-- Create sample payments for testing dashboard
-- This will only work if Payments table exists and there are quotations

-- First, check if we have quotations
DO $$
DECLARE
    quotation_count INTEGER;
    sample_quotation_id UUID;
BEGIN
    -- Count quotations
    SELECT COUNT(*) INTO quotation_count FROM quotations;
    
    IF quotation_count = 0 THEN
        RAISE NOTICE 'No quotations found. Cannot create payments without quotations.';
    ELSE
        -- Get first quotation for sample payment
        SELECT quotation_id INTO sample_quotation_id 
        FROM quotations 
        LIMIT 1;
        
        -- Insert sample payment if it doesn't exist
        INSERT INTO payments (
            payment_id,
            tenant_id,
            quotation_id,
            payment_gateway,
            payment_reference,
            amount_paid,
            currency,
            payment_status,
            payment_date,
            created_at,
            updated_at
        ) VALUES (
            gen_random_uuid(),
            '00000000-0000-0000-0000-000000000001', -- leadrat tenant
            sample_quotation_id,
            'Manual',
            'TEST-' || gen_random_uuid(),
            5000.00,
            'INR',
            'Success',
            CURRENT_TIMESTAMP,
            CURRENT_TIMESTAMP,
            CURRENT_TIMESTAMP
        )
        ON CONFLICT DO NOTHING;
        
        RAISE NOTICE 'Sample payment created for quotation: %', sample_quotation_id;
    END IF;
END $$;

-- Show result
SELECT 
    'Payments after sample creation' as status,
    COUNT(*) as total_payments
FROM payments;
