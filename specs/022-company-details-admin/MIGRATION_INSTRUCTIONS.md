# Migration Instructions: Company Details Feature

## Quick Start

### Option 1: Automated PowerShell Script (Recommended)

```powershell
cd specs/022-company-details-admin
.\apply-migrations.ps1
```

This script will:
1. Check current database state
2. Apply migrations if needed
3. Verify final state

**Requirements:**
- PostgreSQL client tools (`psql`) installed
- `POSTGRES_CONNECTION` environment variable set (or uses default: `Host=localhost;Port=5432;Database=crm;Username=postgres;Password=postgres`)

### Option 2: Manual SQL Script

```bash
# Using psql
psql "Host=localhost;Port=5432;Database=crm;Username=postgres;Password=postgres" -f verify-migrations.sql

# Or set environment variable
export POSTGRES_CONNECTION="Host=localhost;Port=5432;Database=crm;Username=postgres;Password=postgres"
psql "$POSTGRES_CONNECTION" -f verify-migrations.sql
```

### Option 3: Using EF Core CLI

```bash
cd src/Backend/CRM.Infrastructure
dotnet ef database update --project CRM.Infrastructure.csproj --startup-project ../CRM.Api/CRM.Api.csproj
```

**Note:** If EF Core reports "database is already up to date" but tables don't exist, use Option 1 or 2.

## What Gets Created

### Tables

1. **CompanyDetails** - Singleton table for company information
   - Primary Key: `CompanyDetailsId` (fixed GUID: `00000000-0000-0000-0000-000000000001`)
   - Fields: PAN, TAN, GST, company name, address, contact info, logo URL, legal disclaimer
   - Foreign Key: `UpdatedBy` → `Users.UserId`

2. **BankDetails** - Country-specific bank information
   - Primary Key: `BankDetailsId` (auto-generated UUID)
   - Unique Constraint: `(CompanyDetailsId, Country)` - one bank detail per country
   - Fields: Account number, IFSC (India), IBAN/SWIFT (Dubai), bank name, branch
   - Foreign Keys: `CompanyDetailsId` → `CompanyDetails.CompanyDetailsId`, `UpdatedBy` → `Users.UserId`

### Column Addition

3. **Quotations.CompanyDetailsSnapshot** - JSONB column
   - Stores snapshot of company details at quotation creation time
   - Ensures historical accuracy

## Verification

After running migrations, verify with:

```sql
-- Check tables exist
SELECT table_name 
FROM information_schema.tables 
WHERE table_schema = 'public' 
AND table_name IN ('CompanyDetails', 'BankDetails');

-- Check column exists
SELECT column_name, data_type 
FROM information_schema.columns 
WHERE table_name = 'Quotations' 
AND column_name = 'CompanyDetailsSnapshot';

-- Check migration history
SELECT "MigrationId", "ProductVersion" 
FROM "__EFMigrationsHistory" 
WHERE "MigrationId" LIKE '%CompanyDetails%';
```

## Troubleshooting

### Error: "relation already exists"
- Tables may already exist from a previous migration
- The script will skip creation if tables exist
- Check if data is already present

### Error: "foreign key constraint"
- Ensure `Users` table exists
- Ensure at least one admin user exists (for `UpdatedBy` foreign key)

### Error: "column already exists"
- The `CompanyDetailsSnapshot` column may already exist
- The script will skip if column exists

### EF Core says "already up to date" but tables don't exist
- This can happen if migrations aren't tracked
- Use the SQL script (Option 1 or 2) to create tables manually
- Then add migrations to history (script does this automatically)

## Next Steps

After migrations are applied:

1. ✅ Verify tables exist (use SQL queries above)
2. ✅ Test API endpoints:
   - `GET /api/v1/company-details`
   - `PUT /api/v1/company-details`
   - `POST /api/v1/company-details/logo`
3. ✅ Configure company details via admin interface
4. ✅ Test quotation generation with company details
5. ✅ Verify company details appear in PDFs and emails

See `TESTING_GUIDE.md` for comprehensive test scenarios.

## Rollback (if needed)

If you need to rollback:

```sql
-- Remove column
ALTER TABLE "Quotations" DROP COLUMN IF EXISTS "CompanyDetailsSnapshot";

-- Drop tables (WARNING: This will delete all data!)
DROP TABLE IF EXISTS "BankDetails" CASCADE;
DROP TABLE IF EXISTS "CompanyDetails" CASCADE;

-- Remove from migration history
DELETE FROM "__EFMigrationsHistory" 
WHERE "MigrationId" IN (
    '20250127_CreateCompanyDetailsTables',
    '20250127_AddCompanyDetailsSnapshotToQuotations'
);
```

**⚠️ WARNING:** Rolling back will delete all company details data. Make sure to backup if needed.

---

**Migration Files:**
- `verify-migrations.sql` - Complete SQL script with verification and creation
- `apply-migrations.ps1` - Automated PowerShell script
- Migration C# files in `src/Backend/CRM.Infrastructure/Migrations/`

