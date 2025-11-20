-- Add CountryId and JurisdictionId columns to Clients table
-- These columns are nullable and used for tax management

-- Add CountryId column if it doesn't exist
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 
        FROM information_schema.columns 
        WHERE table_name = 'Clients' 
        AND column_name = 'CountryId'
    ) THEN
        ALTER TABLE "Clients" ADD COLUMN "CountryId" uuid NULL;
        RAISE NOTICE 'Added CountryId column to Clients table';
    ELSE
        RAISE NOTICE 'CountryId column already exists';
    END IF;
END $$;

-- Add JurisdictionId column if it doesn't exist
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 
        FROM information_schema.columns 
        WHERE table_name = 'Clients' 
        AND column_name = 'JurisdictionId'
    ) THEN
        ALTER TABLE "Clients" ADD COLUMN "JurisdictionId" uuid NULL;
        RAISE NOTICE 'Added JurisdictionId column to Clients table';
    ELSE
        RAISE NOTICE 'JurisdictionId column already exists';
    END IF;
END $$;

-- Create indexes if they don't exist
CREATE INDEX IF NOT EXISTS "IX_Clients_CountryId" ON "Clients" ("CountryId");
CREATE INDEX IF NOT EXISTS "IX_Clients_JurisdictionId" ON "Clients" ("JurisdictionId");

-- Add foreign key constraints only if Countries and Jurisdictions tables exist
DO $$
BEGIN
    -- Add FK to Countries if Countries table exists
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'Countries') THEN
        IF NOT EXISTS (
            SELECT 1 
            FROM information_schema.table_constraints 
            WHERE constraint_name = 'FK_Clients_Countries_CountryId'
        ) THEN
            ALTER TABLE "Clients"
            ADD CONSTRAINT "FK_Clients_Countries_CountryId" 
            FOREIGN KEY ("CountryId") 
            REFERENCES "Countries" ("CountryId") 
            ON DELETE SET NULL;
            RAISE NOTICE 'Added FK_Clients_Countries_CountryId constraint';
        ELSE
            RAISE NOTICE 'FK_Clients_Countries_CountryId constraint already exists';
        END IF;
    ELSE
        RAISE NOTICE 'Countries table does not exist, skipping FK constraint';
    END IF;

    -- Add FK to Jurisdictions if Jurisdictions table exists
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'Jurisdictions') THEN
        IF NOT EXISTS (
            SELECT 1 
            FROM information_schema.table_constraints 
            WHERE constraint_name = 'FK_Clients_Jurisdictions_JurisdictionId'
        ) THEN
            ALTER TABLE "Clients"
            ADD CONSTRAINT "FK_Clients_Jurisdictions_JurisdictionId" 
            FOREIGN KEY ("JurisdictionId") 
            REFERENCES "Jurisdictions" ("JurisdictionId") 
            ON DELETE SET NULL;
            RAISE NOTICE 'Added FK_Clients_Jurisdictions_JurisdictionId constraint';
        ELSE
            RAISE NOTICE 'FK_Clients_Jurisdictions_JurisdictionId constraint already exists';
        END IF;
    ELSE
        RAISE NOTICE 'Jurisdictions table does not exist, skipping FK constraint';
    END IF;
END $$;

-- Verify the columns were added
SELECT 
    column_name, 
    data_type, 
    is_nullable
FROM information_schema.columns
WHERE table_name = 'Clients' 
AND column_name IN ('CountryId', 'JurisdictionId')
ORDER BY column_name;

