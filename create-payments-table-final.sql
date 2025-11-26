-- Create Payments table from scratch
-- This table is required for the dashboard to show payment data

CREATE TABLE IF NOT EXISTS "Payments" (
    "PaymentId" UUID NOT NULL PRIMARY KEY DEFAULT gen_random_uuid(),
    "TenantId" UUID NULL, -- Temporarily nullable to avoid FK constraint
    "QuotationId" UUID NOT NULL,
    "PaymentGateway" VARCHAR(50) NOT NULL,
    "PaymentReference" VARCHAR(255) NOT NULL,
    "AmountPaid" DECIMAL(18,2) NOT NULL,
    "Currency" VARCHAR(3) NOT NULL DEFAULT 'INR',
    "PaymentStatus" INTEGER NOT NULL DEFAULT 0,
    "PaymentDate" TIMESTAMP NULL,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "FailureReason" TEXT NULL,
    "IsRefundable" BOOLEAN NOT NULL DEFAULT true,
    "RefundAmount" DECIMAL(18,2) NULL,
    "RefundReason" TEXT NULL,
    "RefundDate" TIMESTAMP NULL,
    "Metadata" JSONB NULL
);

-- Create indexes
CREATE INDEX IF NOT EXISTS "IX_Payments_QuotationId" ON "Payments" ("QuotationId");
CREATE INDEX IF NOT EXISTS "IX_Payments_TenantId" ON "Payments" ("TenantId");
CREATE INDEX IF NOT EXISTS "IX_Payments_PaymentStatus" ON "Payments" ("PaymentStatus");
CREATE INDEX IF NOT EXISTS "IX_Payments_PaymentGateway" ON "Payments" ("PaymentGateway");

-- Insert sample payments for dashboard testing
DO $$
DECLARE
    quotation_count INTEGER;
    sample_quotation_id UUID;
BEGIN
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
    
    -- Insert sample payments
    INSERT INTO "Payments" (
        "PaymentId",
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
    
    RAISE NOTICE 'Sample payments created for dashboard testing';
END $$;

-- Show result
SELECT 
    'Payments table setup complete' as status,
    COUNT(*) as total_payments
FROM "Payments";
