-- Verification and Migration Script for Company Details Feature
-- Run this script to verify and apply migrations if needed

-- ============================================
-- PART 1: VERIFICATION
-- ============================================

-- Check if CompanyDetails table exists
SELECT 
    CASE 
        WHEN EXISTS (
            SELECT 1 FROM information_schema.tables 
            WHERE table_schema = 'public' 
            AND table_name = 'CompanyDetails'
        ) 
        THEN '✓ CompanyDetails table EXISTS'
        ELSE '✗ CompanyDetails table MISSING'
    END AS company_details_status;

-- Check if BankDetails table exists
SELECT 
    CASE 
        WHEN EXISTS (
            SELECT 1 FROM information_schema.tables 
            WHERE table_schema = 'public' 
            AND table_name = 'BankDetails'
        ) 
        THEN '✓ BankDetails table EXISTS'
        ELSE '✗ BankDetails table MISSING'
    END AS bank_details_status;

-- Check if CompanyDetailsSnapshot column exists in Quotations
SELECT 
    CASE 
        WHEN EXISTS (
            SELECT 1 FROM information_schema.columns 
            WHERE table_schema = 'public' 
            AND table_name = 'Quotations' 
            AND column_name = 'CompanyDetailsSnapshot'
        ) 
        THEN '✓ CompanyDetailsSnapshot column EXISTS'
        ELSE '✗ CompanyDetailsSnapshot column MISSING'
    END AS snapshot_column_status;

-- Check migration history
SELECT 
    CASE 
        WHEN EXISTS (
            SELECT 1 FROM "__EFMigrationsHistory" 
            WHERE "MigrationId" = '20250127_CreateCompanyDetailsTables'
        ) 
        THEN '✓ CreateCompanyDetailsTables migration tracked'
        ELSE '✗ CreateCompanyDetailsTables migration NOT tracked'
    END AS migration1_status;

SELECT 
    CASE 
        WHEN EXISTS (
            SELECT 1 FROM "__EFMigrationsHistory" 
            WHERE "MigrationId" = '20250127_AddCompanyDetailsSnapshotToQuotations'
        ) 
        THEN '✓ AddCompanyDetailsSnapshotToQuotations migration tracked'
        ELSE '✗ AddCompanyDetailsSnapshotToQuotations migration NOT tracked'
    END AS migration2_status;

-- ============================================
-- PART 2: CREATE TABLES (if missing)
-- ============================================

-- Create CompanyDetails table if it doesn't exist
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.tables 
        WHERE table_schema = 'public' 
        AND table_name = 'CompanyDetails'
    ) THEN
        CREATE TABLE "CompanyDetails" (
            "CompanyDetailsId" UUID PRIMARY KEY DEFAULT '00000000-0000-0000-0000-000000000001',
            "PanNumber" VARCHAR(10),
            "TanNumber" VARCHAR(10),
            "GstNumber" VARCHAR(15),
            "CompanyName" VARCHAR(255),
            "CompanyAddress" TEXT,
            "City" VARCHAR(100),
            "State" VARCHAR(100),
            "PostalCode" VARCHAR(20),
            "Country" VARCHAR(100),
            "ContactEmail" VARCHAR(255),
            "ContactPhone" VARCHAR(20),
            "Website" VARCHAR(255),
            "LegalDisclaimer" TEXT,
            "LogoUrl" VARCHAR(500),
            "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
            "UpdatedAt" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
            "UpdatedBy" UUID NOT NULL
        );

        -- Add foreign key constraint if Users table exists
        IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'Users') THEN
            ALTER TABLE "CompanyDetails" 
            ADD CONSTRAINT "FK_CompanyDetails_Users_UpdatedBy" 
            FOREIGN KEY ("UpdatedBy") REFERENCES "Users"("UserId") ON DELETE RESTRICT;
        END IF;

        -- Create indexes
        CREATE INDEX "IX_CompanyDetails_UpdatedAt" ON "CompanyDetails"("UpdatedAt");
        CREATE INDEX "IX_CompanyDetails_UpdatedBy" ON "CompanyDetails"("UpdatedBy");

        RAISE NOTICE 'CompanyDetails table created successfully';
    ELSE
        RAISE NOTICE 'CompanyDetails table already exists';
    END IF;
END $$;

-- Create BankDetails table if it doesn't exist
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.tables 
        WHERE table_schema = 'public' 
        AND table_name = 'BankDetails'
    ) THEN
        CREATE TABLE "BankDetails" (
            "BankDetailsId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
            "CompanyDetailsId" UUID NOT NULL,
            "Country" VARCHAR(50) NOT NULL,
            "AccountNumber" VARCHAR(50) NOT NULL,
            "IfscCode" VARCHAR(11),
            "Iban" VARCHAR(34),
            "SwiftCode" VARCHAR(11),
            "BankName" VARCHAR(255) NOT NULL,
            "BranchName" VARCHAR(255),
            "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
            "UpdatedAt" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
            "UpdatedBy" UUID NOT NULL,
            CONSTRAINT "UQ_BankDetails_CompanyDetailsId_Country" UNIQUE ("CompanyDetailsId", "Country")
        );

        -- Add foreign key constraints
        IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'CompanyDetails') THEN
            ALTER TABLE "BankDetails" 
            ADD CONSTRAINT "FK_BankDetails_CompanyDetails_CompanyDetailsId" 
            FOREIGN KEY ("CompanyDetailsId") REFERENCES "CompanyDetails"("CompanyDetailsId") ON DELETE CASCADE;
        END IF;

        IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'Users') THEN
            ALTER TABLE "BankDetails" 
            ADD CONSTRAINT "FK_BankDetails_Users_UpdatedBy" 
            FOREIGN KEY ("UpdatedBy") REFERENCES "Users"("UserId") ON DELETE RESTRICT;
        END IF;

        -- Create indexes
        CREATE INDEX "IX_BankDetails_CompanyDetailsId" ON "BankDetails"("CompanyDetailsId");
        CREATE INDEX "IX_BankDetails_UpdatedBy" ON "BankDetails"("UpdatedBy");

        RAISE NOTICE 'BankDetails table created successfully';
    ELSE
        RAISE NOTICE 'BankDetails table already exists';
    END IF;
END $$;

-- Add CompanyDetailsSnapshot column to Quotations if it doesn't exist
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.tables 
        WHERE table_schema = 'public' 
        AND table_name = 'Quotations'
    ) THEN
        IF NOT EXISTS (
            SELECT 1 FROM information_schema.columns 
            WHERE table_schema = 'public' 
            AND table_name = 'Quotations' 
            AND column_name = 'CompanyDetailsSnapshot'
        ) THEN
            ALTER TABLE "Quotations" 
            ADD COLUMN "CompanyDetailsSnapshot" JSONB;
            
            RAISE NOTICE 'CompanyDetailsSnapshot column added to Quotations table';
        ELSE
            RAISE NOTICE 'CompanyDetailsSnapshot column already exists in Quotations table';
        END IF;
    ELSE
        RAISE NOTICE 'Quotations table does not exist - skipping column addition';
    END IF;
END $$;

-- ============================================
-- PART 3: UPDATE MIGRATION HISTORY (if needed)
-- ============================================

-- Add migrations to history if tables exist but migrations aren't tracked
DO $$
BEGIN
    -- Check if CompanyDetails table exists and migration isn't tracked
    IF EXISTS (
        SELECT 1 FROM information_schema.tables 
        WHERE table_schema = 'public' 
        AND table_name = 'CompanyDetails'
    ) AND NOT EXISTS (
        SELECT 1 FROM "__EFMigrationsHistory" 
        WHERE "MigrationId" = '20250127_CreateCompanyDetailsTables'
    ) THEN
        INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
        VALUES ('20250127_CreateCompanyDetailsTables', '8.0.8');
        RAISE NOTICE 'Added CreateCompanyDetailsTables to migration history';
    END IF;

    -- Check if CompanyDetailsSnapshot column exists and migration isn't tracked
    IF EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'public' 
        AND table_name = 'Quotations' 
        AND column_name = 'CompanyDetailsSnapshot'
    ) AND NOT EXISTS (
        SELECT 1 FROM "__EFMigrationsHistory" 
        WHERE "MigrationId" = '20250127_AddCompanyDetailsSnapshotToQuotations'
    ) THEN
        INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
        VALUES ('20250127_AddCompanyDetailsSnapshotToQuotations', '8.0.8');
        RAISE NOTICE 'Added AddCompanyDetailsSnapshotToQuotations to migration history';
    END IF;
END $$;

-- ============================================
-- PART 4: FINAL VERIFICATION
-- ============================================

-- Final status check
SELECT 
    'CompanyDetails' AS table_name,
    CASE WHEN EXISTS (
        SELECT 1 FROM information_schema.tables 
        WHERE table_schema = 'public' 
        AND table_name = 'CompanyDetails'
    ) THEN 'EXISTS' ELSE 'MISSING' END AS status
UNION ALL
SELECT 
    'BankDetails' AS table_name,
    CASE WHEN EXISTS (
        SELECT 1 FROM information_schema.tables 
        WHERE table_schema = 'public' 
        AND table_name = 'BankDetails'
    ) THEN 'EXISTS' ELSE 'MISSING' END AS status
UNION ALL
SELECT 
    'Quotations.CompanyDetailsSnapshot' AS table_name,
    CASE WHEN EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'public' 
        AND table_name = 'Quotations' 
        AND column_name = 'CompanyDetailsSnapshot'
    ) THEN 'EXISTS' ELSE 'MISSING' END AS status;

-- Show table structures
SELECT 
    column_name, 
    data_type, 
    character_maximum_length,
    is_nullable
FROM information_schema.columns 
WHERE table_schema = 'public' 
AND table_name = 'CompanyDetails'
ORDER BY ordinal_position;

SELECT 
    column_name, 
    data_type, 
    character_maximum_length,
    is_nullable
FROM information_schema.columns 
WHERE table_schema = 'public' 
AND table_name = 'BankDetails'
ORDER BY ordinal_position;

