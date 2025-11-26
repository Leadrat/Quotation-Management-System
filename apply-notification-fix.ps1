# PowerShell script to apply the notification dispatch table fix

Write-Host "Applying NotificationDispatchAttempts table fix..." -ForegroundColor Green

# Check if .env file exists and load connection string
$envFile = ".env"
if (Test-Path $envFile) {
    Write-Host "Loading environment variables from .env file..." -ForegroundColor Yellow
    Get-Content $envFile | ForEach-Object {
        if ($_ -match "^([^#][^=]+)=(.*)$") {
            $name = $matches[1].Trim()
            $value = $matches[2].Trim()
            [Environment]::SetEnvironmentVariable($name, $value, "Process")
        }
    }
}

# Get connection string from environment
$connectionString = $env:ConnectionStrings__DefaultConnection
if (-not $connectionString) {
    Write-Host "Error: ConnectionStrings__DefaultConnection not found in environment variables" -ForegroundColor Red
    Write-Host "Please ensure your .env file contains the database connection string" -ForegroundColor Red
    exit 1
}

Write-Host "Connection string found" -ForegroundColor Green

# Apply the SQL fix
try {
    Write-Host "Applying SQL fix to database..." -ForegroundColor Yellow
    
    # Use psql if available, otherwise suggest manual application
    $sqlContent = Get-Content "fix-notification-dispatch-table.sql" -Raw
    
    # Try to find psql
    $psqlPath = Get-Command psql -ErrorAction SilentlyContinue
    if ($psqlPath) {
        Write-Host "Found psql, applying fix..." -ForegroundColor Green
        $env:PGPASSWORD = ($connectionString -split "Password=")[1] -split ";")[0]
        
        # Parse connection string to get components
        $server = ($connectionString -split "Host=")[1] -split ";")[0]
        $database = ($connectionString -split "Database=")[1] -split ";")[0]
        $username = ($connectionString -split "Username=")[1] -split ";")[0]
        $port = if ($connectionString -match "Port=(\d+)") { $matches[1] } else { "5432" }
        
        psql -h $server -p $port -U $username -d $database -f "fix-notification-dispatch-table.sql"
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "SQL fix applied successfully!" -ForegroundColor Green
        } else {
            Write-Host "Error applying SQL fix" -ForegroundColor Red
            exit 1
        }
    } else {
        Write-Host "psql not found. Please apply the SQL fix manually:" -ForegroundColor Yellow
        Write-Host "File: fix-notification-dispatch-table.sql" -ForegroundColor Cyan
        Write-Host $sqlContent -ForegroundColor Gray
    }
    
    # Also run EF Core migration
    Write-Host "Running Entity Framework migration..." -ForegroundColor Yellow
    Set-Location "src/Backend/CRM.Api"
    dotnet ef database update --verbose
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Entity Framework migration completed successfully!" -ForegroundColor Green
    } else {
        Write-Host "Warning: Entity Framework migration may have failed" -ForegroundColor Yellow
    }
    
} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host "NotificationDispatchAttempts table fix completed!" -ForegroundColor Green
Write-Host "You can now restart your application." -ForegroundColor Cyan