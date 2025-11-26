-- Create Tax Management Tables
-- This script creates all tables needed for tax calculation

-- Create Countries table
CREATE TABLE IF NOT EXISTS "Countries" (
    "CountryId" uuid NOT NULL,
    "CountryName" character varying(100) NOT NULL,
    "CountryCode" character varying(2) NOT NULL,
    "TaxFrameworkType" integer NOT NULL,
    "DefaultCurrency" character varying(3) NOT NULL,
    "IsActive" boolean NOT NULL DEFAULT true,
    "IsDefault" boolean NOT NULL DEFAULT false,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    "DeletedAt" timestamp with time zone NULL,
    CONSTRAINT "PK_Countries" PRIMARY KEY ("CountryId")
);

-- Create TaxFrameworks table
CREATE TABLE IF NOT EXISTS "TaxFrameworks" (
    "TaxFrameworkId" uuid NOT NULL,
    "CountryId" uuid NOT NULL,
    "FrameworkName" character varying(100) NOT NULL,
    "FrameworkType" integer NOT NULL,
    "Description" text NULL,
    "TaxComponents" jsonb NOT NULL,
    "IsActive" boolean NOT NULL DEFAULT true,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    "DeletedAt" timestamp with time zone NULL,
    CONSTRAINT "PK_TaxFrameworks" PRIMARY KEY ("TaxFrameworkId"),
    CONSTRAINT "FK_TaxFrameworks_Countries_CountryId" FOREIGN KEY ("CountryId") 
        REFERENCES "Countries" ("CountryId") ON DELETE CASCADE
);

-- Create Jurisdictions table
CREATE TABLE IF NOT EXISTS "Jurisdictions" (
    "JurisdictionId" uuid NOT NULL,
    "CountryId" uuid NOT NULL,
    "ParentJurisdictionId" uuid NULL,
    "JurisdictionName" character varying(100) NOT NULL,
    "JurisdictionCode" character varying(20) NULL,
    "JurisdictionType" character varying(20) NULL,
    "IsActive" boolean NOT NULL DEFAULT true,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    "DeletedAt" timestamp with time zone NULL,
    CONSTRAINT "PK_Jurisdictions" PRIMARY KEY ("JurisdictionId"),
    CONSTRAINT "FK_Jurisdictions_Countries_CountryId" FOREIGN KEY ("CountryId") 
        REFERENCES "Countries" ("CountryId") ON DELETE CASCADE,
    CONSTRAINT "FK_Jurisdictions_Jurisdictions_ParentJurisdictionId" FOREIGN KEY ("ParentJurisdictionId") 
        REFERENCES "Jurisdictions" ("JurisdictionId") ON DELETE SET NULL
);

-- Create ProductServiceCategories table
CREATE TABLE IF NOT EXISTS "ProductServiceCategories" (
    "CategoryId" uuid NOT NULL,
    "CategoryName" character varying(100) NOT NULL,
    "CategoryCode" character varying(20) NULL,
    "Description" character varying(500) NULL,
    "IsActive" boolean NOT NULL DEFAULT true,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    "DeletedAt" timestamp with time zone NULL,
    CONSTRAINT "PK_ProductServiceCategories" PRIMARY KEY ("CategoryId")
);

-- Create TaxRates table
CREATE TABLE IF NOT EXISTS "TaxRates" (
    "TaxRateId" uuid NOT NULL,
    "JurisdictionId" uuid NULL,
    "TaxFrameworkId" uuid NOT NULL,
    "ProductServiceCategoryId" uuid NULL,
    "TaxRate" numeric(5,2) NOT NULL,
    "EffectiveFrom" date NOT NULL,
    "EffectiveTo" date NULL,
    "IsExempt" boolean NOT NULL DEFAULT false,
    "IsZeroRated" boolean NOT NULL DEFAULT false,
    "TaxComponents" jsonb NOT NULL DEFAULT '[]'::jsonb,
    "Description" character varying(500) NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_TaxRates" PRIMARY KEY ("TaxRateId"),
    CONSTRAINT "FK_TaxRates_Jurisdictions_JurisdictionId" FOREIGN KEY ("JurisdictionId") 
        REFERENCES "Jurisdictions" ("JurisdictionId") ON DELETE SET NULL,
    CONSTRAINT "FK_TaxRates_TaxFrameworks_TaxFrameworkId" FOREIGN KEY ("TaxFrameworkId") 
        REFERENCES "TaxFrameworks" ("TaxFrameworkId") ON DELETE CASCADE,
    CONSTRAINT "FK_TaxRates_ProductServiceCategories_ProductServiceCategoryId" FOREIGN KEY ("ProductServiceCategoryId") 
        REFERENCES "ProductServiceCategories" ("CategoryId") ON DELETE SET NULL
);

-- Create TaxCalculationLogs table (only if Quotations table exists)
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'Quotations') THEN
        CREATE TABLE IF NOT EXISTS "TaxCalculationLogs" (
            "LogId" uuid NOT NULL,
            "QuotationId" uuid NULL,
            "ActionType" integer NOT NULL,
            "CountryId" uuid NULL,
            "JurisdictionId" uuid NULL,
            "CalculationDetails" jsonb NOT NULL DEFAULT '{}'::jsonb,
            "ChangedByUserId" uuid NOT NULL,
            "ChangedAt" timestamp with time zone NOT NULL,
            CONSTRAINT "PK_TaxCalculationLogs" PRIMARY KEY ("LogId"),
            CONSTRAINT "FK_TaxCalculationLogs_Quotations_QuotationId" FOREIGN KEY ("QuotationId") 
                REFERENCES "Quotations" ("QuotationId") ON DELETE SET NULL,
            CONSTRAINT "FK_TaxCalculationLogs_Countries_CountryId" FOREIGN KEY ("CountryId") 
                REFERENCES "Countries" ("CountryId") ON DELETE SET NULL,
            CONSTRAINT "FK_TaxCalculationLogs_Jurisdictions_JurisdictionId" FOREIGN KEY ("JurisdictionId") 
                REFERENCES "Jurisdictions" ("JurisdictionId") ON DELETE SET NULL,
            CONSTRAINT "FK_TaxCalculationLogs_Users_ChangedByUserId" FOREIGN KEY ("ChangedByUserId") 
                REFERENCES "Users" ("UserId") ON DELETE SET NULL
        );
        RAISE NOTICE 'Created TaxCalculationLogs table';
    ELSE
        RAISE NOTICE 'Quotations table does not exist, skipping TaxCalculationLogs';
    END IF;
END $$;

-- Create indexes for Countries
CREATE UNIQUE INDEX IF NOT EXISTS "IX_Countries_CountryCode" 
    ON "Countries" ("CountryCode") 
    WHERE "DeletedAt" IS NULL;

CREATE UNIQUE INDEX IF NOT EXISTS "IX_Countries_CountryName" 
    ON "Countries" ("CountryName") 
    WHERE "DeletedAt" IS NULL;

CREATE INDEX IF NOT EXISTS "IX_Countries_IsActive" ON "Countries" ("IsActive");
CREATE INDEX IF NOT EXISTS "IX_Countries_IsDefault" ON "Countries" ("IsDefault");

-- Create indexes for TaxFrameworks
CREATE UNIQUE INDEX IF NOT EXISTS "IX_TaxFrameworks_CountryId" 
    ON "TaxFrameworks" ("CountryId") 
    WHERE "DeletedAt" IS NULL;

CREATE INDEX IF NOT EXISTS "IX_TaxFrameworks_FrameworkType" ON "TaxFrameworks" ("FrameworkType");
CREATE INDEX IF NOT EXISTS "IX_TaxFrameworks_IsActive" ON "TaxFrameworks" ("IsActive");

CREATE INDEX IF NOT EXISTS "IX_TaxFrameworks_TaxComponents" 
    ON "TaxFrameworks" USING gin ("TaxComponents");

-- Create indexes for Jurisdictions
CREATE INDEX IF NOT EXISTS "IX_Jurisdictions_CountryId" ON "Jurisdictions" ("CountryId");
CREATE INDEX IF NOT EXISTS "IX_Jurisdictions_ParentJurisdictionId" ON "Jurisdictions" ("ParentJurisdictionId");
CREATE INDEX IF NOT EXISTS "IX_Jurisdictions_IsActive" ON "Jurisdictions" ("IsActive");

CREATE UNIQUE INDEX IF NOT EXISTS "IX_Jurisdictions_CountryId_ParentJurisdictionId_JurisdictionCode"
    ON "Jurisdictions" ("CountryId", "ParentJurisdictionId", "JurisdictionCode")
    WHERE "JurisdictionCode" IS NOT NULL AND "DeletedAt" IS NULL;

-- Create indexes for ProductServiceCategories
CREATE UNIQUE INDEX IF NOT EXISTS "IX_ProductServiceCategories_CategoryName" 
    ON "ProductServiceCategories" ("CategoryName") 
    WHERE "DeletedAt" IS NULL;

CREATE UNIQUE INDEX IF NOT EXISTS "IX_ProductServiceCategories_CategoryCode" 
    ON "ProductServiceCategories" ("CategoryCode") 
    WHERE "CategoryCode" IS NOT NULL AND "DeletedAt" IS NULL;

CREATE INDEX IF NOT EXISTS "IX_ProductServiceCategories_IsActive" ON "ProductServiceCategories" ("IsActive");

-- Create indexes for TaxRates
CREATE INDEX IF NOT EXISTS "IX_TaxRates_JurisdictionId" ON "TaxRates" ("JurisdictionId");
CREATE INDEX IF NOT EXISTS "IX_TaxRates_TaxFrameworkId" ON "TaxRates" ("TaxFrameworkId");
CREATE INDEX IF NOT EXISTS "IX_TaxRates_ProductServiceCategoryId" ON "TaxRates" ("ProductServiceCategoryId");
CREATE INDEX IF NOT EXISTS "IX_TaxRates_EffectiveFrom_EffectiveTo" ON "TaxRates" ("EffectiveFrom", "EffectiveTo");
CREATE INDEX IF NOT EXISTS "IX_TaxRates_Lookup" ON "TaxRates" ("JurisdictionId", "ProductServiceCategoryId", "EffectiveFrom", "EffectiveTo");

-- Create indexes for TaxCalculationLogs (if table exists)
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'TaxCalculationLogs') THEN
        CREATE INDEX IF NOT EXISTS "IX_TaxCalculationLogs_QuotationId" ON "TaxCalculationLogs" ("QuotationId");
        CREATE INDEX IF NOT EXISTS "IX_TaxCalculationLogs_ChangedAt" ON "TaxCalculationLogs" ("ChangedAt");
        CREATE INDEX IF NOT EXISTS "IX_TaxCalculationLogs_ActionType" ON "TaxCalculationLogs" ("ActionType");
        CREATE INDEX IF NOT EXISTS "IX_TaxCalculationLogs_CountryId_JurisdictionId" ON "TaxCalculationLogs" ("CountryId", "JurisdictionId");
        CREATE INDEX IF NOT EXISTS "IX_TaxCalculationLogs_ChangedAt_ActionType" ON "TaxCalculationLogs" ("ChangedAt", "ActionType");
    END IF;
END $$;

-- Update Clients table foreign keys if they don't exist
DO $$
BEGIN
    -- Add FK to Countries if it doesn't exist
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
    END IF;

    -- Add FK to Jurisdictions if it doesn't exist
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
    END IF;
END $$;

-- Verify tables were created
SELECT 
    table_name,
    (SELECT COUNT(*) FROM information_schema.columns WHERE table_name = t.table_name) as column_count
FROM information_schema.tables t
WHERE table_name IN ('Countries', 'TaxFrameworks', 'Jurisdictions', 'ProductServiceCategories', 'TaxRates', 'TaxCalculationLogs')
ORDER BY table_name;

