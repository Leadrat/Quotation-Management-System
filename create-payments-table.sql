-- Create Payments table if it doesn't exist
CREATE TABLE IF NOT EXISTS "Payments" (
    "PaymentId" UUID NOT NULL PRIMARY KEY DEFAULT gen_random_uuid(),
    "TenantId" UUID NOT NULL DEFAULT '00000000-0000-0000-0000-000000000001',
    "QuotationId" UUID NOT NULL,
    "PaymentGateway" VARCHAR(50) NOT NULL,
    "PaymentReference" VARCHAR(255) NOT NULL,
    "AmountPaid" DECIMAL(18,2) NOT NULL,
    "Currency" VARCHAR(3) NOT NULL DEFAULT 'INR',
    "PaymentStatus" INTEGER NOT NULL DEFAULT 0,
    "PaymentDate" TIMESTAMP,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "FailureReason" TEXT,
    "IsRefundable" BOOLEAN NOT NULL DEFAULT true,
    "RefundAmount" DECIMAL(18,2),
    "RefundReason" TEXT,
    "RefundDate" TIMESTAMP,
    "Metadata" JSONB
);

-- Create indexes
CREATE INDEX IF NOT EXISTS "IX_Payments_QuotationId" ON "Payments" ("QuotationId");
CREATE INDEX IF NOT EXISTS "IX_Payments_TenantId" ON "Payments" ("TenantId");
CREATE INDEX IF NOT EXISTS "IX_Payments_PaymentStatus" ON "Payments" ("PaymentStatus");
CREATE INDEX IF NOT EXISTS "IX_Payments_PaymentGateway" ON "Payments" ("PaymentGateway");

-- Add foreign key constraint if Quotations table exists
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'quotations') THEN
        ALTER TABLE "Payments" 
        ADD CONSTRAINT "FK_Payments_Quotations_QuotationId" 
        FOREIGN KEY ("QuotationId") REFERENCES "Quotations" ("QuotationId") 
        ON DELETE CASCADE;
    END IF;
END $$;

-- Create sample payment if table is empty
DO $$
DECLARE
    payment_count INTEGER;
    sample_quotation_id UUID;
BEGIN
    -- Check if payments exist
    SELECT COUNT(*) INTO payment_count FROM "Payments";
    
    IF payment_count = 0 THEN
        -- Get first quotation for sample payment
        SELECT "QuotationId" INTO sample_quotation_id 
        FROM "Quotations" 
        LIMIT 1;
        
        IF sample_quotation_id IS NOT NULL THEN
            -- Insert sample payment
            INSERT INTO "Payments" (
                "QuotationId",
                "PaymentGateway",
                "PaymentReference",
                "AmountPaid",
                "Currency",
                "PaymentStatus",
                "PaymentDate",
                "CreatedAt",
                "UpdatedAt"
            ) VALUES (
                sample_quotation_id,
                'Manual',
                'SAMPLE-' || gen_random_uuid(),
                5000.00,
                'INR',
                1, -- Success
                CURRENT_TIMESTAMP,
                CURRENT_TIMESTAMP,
                CURRENT_TIMESTAMP
            );
            
            RAISE NOTICE 'Sample payment created for dashboard testing';
        END IF;
    END IF;
END $$;

-- Show result
SELECT 
    'Payments table setup complete' as status,
    COUNT(*) as total_payments
FROM "Payments";
