# PowerShell script to verify and apply Company Details migrations
# This script checks database state and applies migrations if needed

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Company Details Migration Verification" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Get connection string from environment or use default
$connectionString = $env:POSTGRES_CONNECTION
if ([string]::IsNullOrWhiteSpace($connectionString)) {
    $connectionString = "Host=localhost;Port=5432;Database=crm;Username=postgres;Password=postgres"
    Write-Host "Using default connection string (set POSTGRES_CONNECTION env var to override)" -ForegroundColor Yellow
}

Write-Host "Connection: $($connectionString -replace 'Password=[^;]+', 'Password=***')" -ForegroundColor Gray
Write-Host ""

# Check if psql is available
$psqlPath = Get-Command psql -ErrorAction SilentlyContinue
if (-not $psqlPath) {
    Write-Host "ERROR: psql command not found. Please install PostgreSQL client tools." -ForegroundColor Red
    Write-Host "Alternatively, run the SQL script manually: verify-migrations.sql" -ForegroundColor Yellow
    exit 1
}

Write-Host "Step 1: Verifying current database state..." -ForegroundColor Cyan
Write-Host ""

# Run verification query
$verifyQuery = @"
SELECT 
    CASE 
        WHEN EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'CompanyDetails') 
        THEN 'EXISTS' 
        ELSE 'MISSING' 
    END AS company_details,
    CASE 
        WHEN EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'BankDetails') 
        THEN 'EXISTS' 
        ELSE 'MISSING' 
    END AS bank_details,
    CASE 
        WHEN EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'Quotations' AND column_name = 'CompanyDetailsSnapshot') 
        THEN 'EXISTS' 
        ELSE 'MISSING' 
    END AS snapshot_column;
"@

try {
    $result = $verifyQuery | psql $connectionString -t -A -F "|"
    
    if ($LASTEXITCODE -eq 0) {
        $parts = $result -split '\|'
        Write-Host "  CompanyDetails table: $($parts[0])" -ForegroundColor $(if ($parts[0] -eq 'EXISTS') { 'Green' } else { 'Yellow' })
        Write-Host "  BankDetails table: $($parts[1])" -ForegroundColor $(if ($parts[1] -eq 'EXISTS') { 'Green' } else { 'Yellow' })
        Write-Host "  CompanyDetailsSnapshot column: $($parts[2])" -ForegroundColor $(if ($parts[2] -eq 'EXISTS') { 'Green' } else { 'Yellow' })
        Write-Host ""
        
        if ($parts[0] -eq 'MISSING' -or $parts[1] -eq 'MISSING' -or $parts[2] -eq 'MISSING') {
            Write-Host "Step 2: Applying migrations..." -ForegroundColor Cyan
            Write-Host ""
            
            # Read and execute SQL script
            $sqlScript = Get-Content "verify-migrations.sql" -Raw
            $sqlScript | psql $connectionString
            
            if ($LASTEXITCODE -eq 0) {
                Write-Host ""
                Write-Host "✓ Migrations applied successfully!" -ForegroundColor Green
            } else {
                Write-Host ""
                Write-Host "✗ Error applying migrations. Check the error messages above." -ForegroundColor Red
                exit 1
            }
        } else {
            Write-Host "✓ All tables and columns already exist. No migration needed." -ForegroundColor Green
        }
    } else {
        Write-Host "ERROR: Could not connect to database. Please check your connection string." -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    Write-Host "Alternative: Run the SQL script manually using:" -ForegroundColor Yellow
    Write-Host "  psql `"$connectionString`" -f verify-migrations.sql" -ForegroundColor Yellow
    exit 1
}

Write-Host ""
Write-Host "Step 3: Final verification..." -ForegroundColor Cyan
Write-Host ""

$finalVerify = $verifyQuery | psql $connectionString -t -A -F "|"
$finalParts = $finalVerify -split '\|'

$allExist = $true
if ($finalParts[0] -ne 'EXISTS') { $allExist = $false }
if ($finalParts[1] -ne 'EXISTS') { $allExist = $false }
if ($finalParts[2] -ne 'EXISTS') { $allExist = $false }

if ($allExist) {
    Write-Host "✓ All migrations verified successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Cyan
    Write-Host "  1. Test the API endpoints (see TESTING_GUIDE.md)" -ForegroundColor White
    Write-Host "  2. Configure company details via admin interface" -ForegroundColor White
    Write-Host "  3. Test quotation generation with company details" -ForegroundColor White
} else {
    Write-Host "✗ Some migrations may not have been applied correctly." -ForegroundColor Red
    Write-Host "  Please check the error messages and run verify-migrations.sql manually." -ForegroundColor Yellow
    exit 1
}

