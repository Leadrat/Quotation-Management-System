# Apply Migrations: Company Details Feature

This guide explains how to apply the database migrations for the Company Details feature.

## Migrations to Apply

1. **CreateCompanyDetailsTables** - Creates `CompanyDetails` and `BankDetails` tables
2. **AddCompanyDetailsSnapshotToQuotations** - Adds `CompanyDetailsSnapshot` column to `Quotations` table

## Method 1: Using EF Core CLI (Recommended)

```bash
# Navigate to the Infrastructure project
cd src/Backend/CRM.Infrastructure

# Apply all pending migrations
dotnet ef database update --startup-project ../CRM.Api

# Or apply specific migration
dotnet ef database update CreateCompanyDetailsTables --startup-project ../CRM.Api
dotnet ef database update AddCompanyDetailsSnapshotToQuotations --startup-project ../CRM.Api
```

## Method 2: Using the Migrator Project

```bash
# Navigate to the Migrator project
cd src/Backend/CRM.Migrator

# Run the migrator (applies all pending migrations)
dotnet run
```

## Method 3: Manual SQL Execution

If you prefer to run SQL directly:

### Migration 1: CreateCompanyDetailsTables

```sql
-- Create CompanyDetails table
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
    "UpdatedBy" UUID NOT NULL REFERENCES "Users"("UserId") ON DELETE RESTRICT
);

CREATE INDEX "IX_CompanyDetails_UpdatedAt" ON "CompanyDetails"("UpdatedAt");
CREATE INDEX "IX_CompanyDetails_UpdatedBy" ON "CompanyDetails"("UpdatedBy");

-- Create BankDetails table
CREATE TABLE "BankDetails" (
    "BankDetailsId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "CompanyDetailsId" UUID NOT NULL REFERENCES "CompanyDetails"("CompanyDetailsId") ON DELETE CASCADE,
    "Country" VARCHAR(50) NOT NULL,
    "AccountNumber" VARCHAR(50) NOT NULL,
    "IfscCode" VARCHAR(11),
    "Iban" VARCHAR(34),
    "SwiftCode" VARCHAR(11),
    "BankName" VARCHAR(255) NOT NULL,
    "BranchName" VARCHAR(255),
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedBy" UUID NOT NULL REFERENCES "Users"("UserId") ON DELETE RESTRICT,
    CONSTRAINT "UQ_BankDetails_CompanyDetailsId_Country" UNIQUE ("CompanyDetailsId", "Country")
);

CREATE INDEX "IX_BankDetails_CompanyDetailsId" ON "BankDetails"("CompanyDetailsId");
CREATE INDEX "IX_BankDetails_UpdatedBy" ON "BankDetails"("UpdatedBy");
```

### Migration 2: AddCompanyDetailsSnapshotToQuotations

```sql
-- Add CompanyDetailsSnapshot column to Quotations table
ALTER TABLE "Quotations" ADD COLUMN "CompanyDetailsSnapshot" JSONB;
```

## Verification

After applying migrations, verify they were successful:

```sql
-- Check CompanyDetails table exists
SELECT table_name, column_name, data_type 
FROM information_schema.columns 
WHERE table_name = 'CompanyDetails'
ORDER BY ordinal_position;

-- Check BankDetails table exists
SELECT table_name, column_name, data_type 
FROM information_schema.columns 
WHERE table_name = 'BankDetails'
ORDER BY ordinal_position;

-- Check CompanyDetailsSnapshot column exists in Quotations
SELECT column_name, data_type 
FROM information_schema.columns 
WHERE table_name = 'Quotations' 
AND column_name = 'CompanyDetailsSnapshot';

-- Check indexes
SELECT indexname, indexdef 
FROM pg_indexes 
WHERE tablename IN ('CompanyDetails', 'BankDetails')
ORDER BY tablename, indexname;
```

## Rollback (if needed)

If you need to rollback migrations:

```bash
# Rollback to previous migration
dotnet ef database update PreviousMigrationName --startup-project ../CRM.Api

# Or rollback all migrations for this feature
dotnet ef migrations remove --startup-project ../CRM.Api
```

**Note**: Rolling back will remove the tables and data. Make sure to backup data if needed.

## Troubleshooting

### Error: "relation already exists"
- The tables may already exist. Check if migrations were already applied.
- Solution: Skip this migration or drop existing tables (if safe to do so).

### Error: "foreign key constraint"
- Ensure the `Users` table exists and has data.
- Solution: Create at least one admin user before applying migrations.

### Error: "column already exists"
- The `CompanyDetailsSnapshot` column may already exist in `Quotations`.
- Solution: Check if the column exists and skip this migration if it does.

## Next Steps

After applying migrations:
1. Verify tables exist (use SQL queries above)
2. Test the API endpoints (see IMPLEMENTATION_COMPLETE.md)
3. Configure company details via admin interface
4. Test quotation generation with company details

