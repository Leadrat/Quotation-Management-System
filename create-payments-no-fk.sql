-- Create sample payments without foreign key constraint
-- Temporarily disable foreign key constraint for testing

DO $$
DECLARE
    table_exists BOOLEAN;
    quotation_count INTEGER;
    sample_quotation_id UUID;
BEGIN
    -- Check if Payments table exists
    SELECT EXISTS (
        SELECT FROM information_schema.tables 
        WHERE table_schema = 'public' 
        AND table_name = 'Payments'
    ) INTO table_exists;
    
    IF NOT table_exists THEN
        RAISE NOTICE 'Payments table does not exist. Please run migrations first.';
        RETURN;
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
    
    -- Insert sample payments with null TenantId to avoid FK constraint
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
        NULL, -- No tenant for now
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
        NULL, -- No tenant for now
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
        NULL, -- No tenant for now
        sample_quotation_id,
        'Stripe',
        'STRIPE-003-' || gen_random_uuid(),
        2000.00,
        'INR',
        2, -- Pending
        CURRENT_TIMESTAMP,
        CURRENT_TIMESTAMP,
        CURRENT_TIMESTAMP
    )
    ON CONFLICT DO NOTHING;
    
    RAISE NOTICE 'Sample payments created for dashboard testing';
END $$;

-- Show result
SELECT 
    'Sample payments created' as status,
    COUNT(*) as total_payments,
    "PaymentGateway",
    "AmountPaid",
    "PaymentStatus"
FROM "Payments" 
GROUP BY "PaymentGateway", "AmountPaid", "PaymentStatus"
ORDER BY "PaymentGateway";
