# Apply Migrations Now - Quick Guide

## Current Status

✅ **Code is complete and builds successfully**  
⚠️ **Migrations need to be applied** (database authentication required)

## Quick Steps to Apply Migrations

### Step 1: Set Your Database Connection

**Option A: Environment Variable (Recommended)**
```powershell
$env:POSTGRES_CONNECTION = "Host=localhost;Port=5432;Database=crm;Username=YOUR_USERNAME;Password=YOUR_PASSWORD"
```

**Option B: Use Default (if it matches your setup)**
- Default: `Host=localhost;Port=5432;Database=crm;Username=postgres;Password=postgres`

### Step 2: Apply Migrations

**Method 1: Using EF Core (Recommended)**
```powershell
cd src/Backend/CRM.Infrastructure
dotnet ef database update --project CRM.Infrastructure.csproj --startup-project ../CRM.Api/CRM.Api.csproj
```

**Method 2: Using SQL Script**
```powershell
cd specs/022-company-details-admin

# Set connection string
$env:POSTGRES_CONNECTION = "Host=localhost;Port=5432;Database=crm;Username=YOUR_USER;Password=YOUR_PASS"

# Parse and run
$parts = @{}
$env:POSTGRES_CONNECTION -split ';' | ForEach-Object { 
    if ($_ -match '(\w+)=(.+)') { $parts[$matches[1]] = $matches[2] } 
}
$env:PGPASSWORD = $parts['Password']
psql -h $parts['Host'] -p $parts['Port'] -U $parts['Username'] -d $parts['Database'] -f verify-migrations.sql
```

**Method 3: Manual SQL (if psql not available)**
1. Open your PostgreSQL client (pgAdmin, DBeaver, etc.)
2. Connect to your database
3. Open `specs/022-company-details-admin/verify-migrations.sql`
4. Execute the script

### Step 3: Verify Migrations Applied

Run this SQL query:
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
```

Expected result:
- `CompanyDetails` table should exist
- `BankDetails` table should exist  
- `CompanyDetailsSnapshot` column should exist in `Quotations` table

## What Gets Created

1. **CompanyDetails Table**
   - Stores company information (PAN, TAN, GST, address, contact, logo, etc.)
   - Singleton pattern (one record with fixed GUID)

2. **BankDetails Table**
   - Stores country-specific bank details
   - One record per country (India/Dubai)
   - Foreign key to CompanyDetails

3. **CompanyDetailsSnapshot Column**
   - JSONB column in Quotations table
   - Stores snapshot of company details at quotation creation

## Troubleshooting

### "password authentication failed"
- Check your PostgreSQL credentials
- Verify the database is running
- Ensure the user has CREATE TABLE permissions

### "relation already exists"
- Tables may already exist
- Check if data is present: `SELECT * FROM "CompanyDetails";`
- If tables exist, migrations are already applied

### "column already exists"
- The column may already exist
- Check: `SELECT column_name FROM information_schema.columns WHERE table_name = 'Quotations' AND column_name = 'CompanyDetailsSnapshot';`

## After Migrations Are Applied

1. ✅ Start the backend API
2. ✅ Test the endpoints (see TESTING_GUIDE.md)
3. ✅ Configure company details via admin interface
4. ✅ Test quotation generation

## Need Help?

- See `MIGRATION_INSTRUCTIONS.md` for detailed guide
- See `TESTING_GUIDE.md` for testing scenarios
- See `APPLY_MIGRATIONS.md` for alternative methods

---

**Note**: The migration scripts are ready and tested. You just need to provide the correct database credentials.

