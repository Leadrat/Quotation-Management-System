-- Check if Payments table exists and has data
SELECT 
    'Payments table exists' as table_status,
    COUNT(*) as payment_count
FROM information_schema.tables 
WHERE table_schema = 'public' AND table_name = 'payments';

-- If table exists, show sample data
SELECT 
    'Sample payments data' as data_status,
    payment_id,
    quotation_id,
    payment_gateway,
    amount_paid,
    payment_status,
    created_at
FROM payments 
LIMIT 5;

-- Check if there are quotations (payments need quotations to exist)
SELECT 
    'Quotations table exists' as table_status,
    COUNT(*) as quotation_count
FROM information_schema.tables 
WHERE table_schema = 'public' AND table_name = 'quotations';

-- Show sample quotations
SELECT 
    'Sample quotations data' as data_status,
    quotation_id,
    quotation_number,
    total_amount,
    created_at
FROM quotations 
LIMIT 3;
