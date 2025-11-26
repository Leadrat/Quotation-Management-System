# PowerShell script to fix the NotificationDispatchAttempts database schema issue

Write-Host "=== CRM Database Schema Fix ===" -ForegroundColor Cyan
Write-Host "Fixing NotificationDispatchAttempts table schema..." -ForegroundColor Green

# Function to load environment variables from .env file
function Load-EnvFile {
    param([string]$EnvFilePath = ".env")
    
    if (Test-Path $EnvFilePath) {
        Write-Host "Loading environment variables from $EnvFilePath..." -ForegroundColor Yellow
        Get-Content $EnvFilePath | ForEach-Object {
            if ($_ -match "^([^#][^=]+)=(.*)$") {
                $name = $matches[1].Trim()
                $value = $matches[2].Trim()
                # Remove quotes if present
                $value = $value -replace '^"(.*)"$', '$1'
                $value = $value -replace "^'(.*)'$", '$1'
                [Environment]::SetEnvironmentVariable($name, $value, "Process")
                Write-Host "  Set $name" -ForegroundColor Gray
            }
        }
        return $true
    } else {
        Write-Host "Warning: .env file not found at $EnvFilePath" -ForegroundColor Yellow
        return $false
    }
}

# Function to stop running processes
function Stop-CrmProcesses {
    Write-Host "Checking for running CRM processes..." -ForegroundColor Yellow
    
    $processes = Get-Process | Where-Object { $_.ProcessName -like "*CRM*" -or $_.ProcessName -like "*dotnet*" }
    
    if ($processes) {
        Write-Host "Found running processes. Attempting to stop them..." -ForegroundColor Yellow
        foreach ($proc in $processes) {
            try {
                if ($proc.ProcessName -eq "dotnet" -and $proc.MainWindowTitle -like "*CRM*") {
                    Write-Host "Stopping dotnet process: $($proc.Id)" -ForegroundColor Yellow
                    Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue
                }
                elseif ($proc.ProcessName -like "*CRM*") {
                    Write-Host "Stopping CRM process: $($proc.ProcessName) ($($proc.Id))" -ForegroundColor Yellow
                    Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue
                }
            }
            catch {
                Write-Host "Could not stop process $($proc.Id): $($_.Exception.Message)" -ForegroundColor Red
            }
        }
        
        # Wait a moment for processes to stop
        Start-Sleep -Seconds 3
    } else {
        Write-Host "No CRM processes found running." -ForegroundColor Green
    }
}

# Function to apply SQL fix directly
function Apply-SqlFix {
    param([string]$ConnectionString)
    
    Write-Host "Applying SQL schema fix..." -ForegroundColor Yellow
    
    $sqlScript = @"
-- Fix database schema issues for notifications
-- First, create the main Notifications table if it doesn't exist
DO `$`$
BEGIN
    -- Check if the Notifications table exists
    IF NOT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'Notifications') THEN
        -- Create the Notifications table
        CREATE TABLE "Notifications" (
            "Id" uuid NOT NULL DEFAULT gen_random_uuid(),
            "NotificationId" uuid NOT NULL DEFAULT gen_random_uuid(),
            "UserId" uuid NOT NULL,
            "RecipientUserId" uuid NOT NULL,
            "NotificationTypeId" uuid NOT NULL,
            "EventType" character varying(100) NOT NULL,
            "Title" character varying(255) NOT NULL,
            "Message" character varying(10000) NOT NULL,
            "RelatedEntityId" uuid,
            "RelatedEntityType" character varying(100),
            "IsRead" boolean NOT NULL DEFAULT false,
            "ReadAt" timestamp with time zone,
            "IsArchived" boolean NOT NULL DEFAULT false,
            "ArchivedAt" timestamp with time zone,
            "SentVia" character varying(100) NOT NULL,
            "DeliveredChannels" character varying(500),
            "DeliveryStatus" character varying(50) NOT NULL DEFAULT 'PENDING',
            "Meta" character varying(4000),
            "Metadata" character varying(4000),
            "Priority" integer NOT NULL DEFAULT 1,
            "CreatedAt" timestamp with time zone NOT NULL DEFAULT NOW(),
            "UpdatedAt" timestamp with time zone NOT NULL DEFAULT NOW(),
            CONSTRAINT "PK_Notifications" PRIMARY KEY ("NotificationId")
        );
        
        -- Create indexes for Notifications table
        CREATE INDEX "IX_Notifications_UserId" ON "Notifications" ("UserId");
        CREATE INDEX "IX_Notifications_RecipientUserId" ON "Notifications" ("RecipientUserId");
        CREATE INDEX "IX_Notifications_NotificationTypeId" ON "Notifications" ("NotificationTypeId");
        CREATE INDEX "IX_Notifications_CreatedAt" ON "Notifications" ("CreatedAt");
        CREATE INDEX "IX_Notifications_IsRead" ON "Notifications" ("IsRead");
        CREATE INDEX "IX_Notifications_IsArchived" ON "Notifications" ("IsArchived");
        CREATE INDEX "IX_Notifications_UserId_IsRead" ON "Notifications" ("UserId", "IsRead");
        CREATE INDEX "IX_Notifications_RecipientUserId_IsRead" ON "Notifications" ("RecipientUserId", "IsRead");
        CREATE INDEX "IX_Notifications_RecipientUserId_IsArchived" ON "Notifications" ("RecipientUserId", "IsArchived");
        CREATE INDEX "IX_Notifications_UserId_CreatedAt" ON "Notifications" ("UserId", "CreatedAt");
        CREATE INDEX "IX_Notifications_RelatedEntityId" ON "Notifications" ("RelatedEntityId");
        
        RAISE NOTICE 'Notifications table created successfully';
    END IF;
    
    -- Check if NotificationTypes table exists, if not create it
    IF NOT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'NotificationTypes') THEN
        CREATE TABLE "NotificationTypes" (
            "NotificationTypeId" uuid NOT NULL DEFAULT gen_random_uuid(),
            "TypeName" character varying(100) NOT NULL,
            "Description" character varying(500),
            "IsActive" boolean NOT NULL DEFAULT true,
            "CreatedAt" timestamp with time zone NOT NULL DEFAULT NOW(),
            "UpdatedAt" timestamp with time zone NOT NULL DEFAULT NOW(),
            CONSTRAINT "PK_NotificationTypes" PRIMARY KEY ("NotificationTypeId")
        );
        
        CREATE UNIQUE INDEX "IX_NotificationTypes_TypeName" ON "NotificationTypes" ("TypeName");
        
        -- Insert some default notification types
        INSERT INTO "NotificationTypes" ("TypeName", "Description") VALUES
        ('QuotationApproved', 'Notification sent when a quotation is approved'),
        ('PaymentRequest', 'Notification sent when a payment is requested'),
        ('QuotationSent', 'Notification sent when a quotation is sent to client'),
        ('QuotationViewed', 'Notification sent when a quotation is viewed by client'),
        ('QuotationResponseReceived', 'Notification sent when client responds to quotation'),
        ('QuotationExpiring', 'Notification sent when quotation is about to expire'),
        ('SystemAlert', 'General system alert notification');
        
        RAISE NOTICE 'NotificationTypes table created with default types';
    END IF;
END `$`$;

-- Fix NotificationDispatchAttempts table by adding missing columns
-- Add missing columns to NotificationDispatchAttempts table
ALTER TABLE "NotificationDispatchAttempts" 
ADD COLUMN IF NOT EXISTS "DeliveredAt" timestamp with time zone,
ADD COLUMN IF NOT EXISTS "ErrorDetails" character varying(4000),
ADD COLUMN IF NOT EXISTS "ExternalId" character varying(500),
ADD COLUMN IF NOT EXISTS "AttemptNumber" integer NOT NULL DEFAULT 1,
ADD COLUMN IF NOT EXISTS "NotificationTemplateId" integer;

-- Update existing records to have AttemptNumber = 1 if it's NULL or 0
UPDATE "NotificationDispatchAttempts" 
SET "AttemptNumber" = 1 
WHERE "AttemptNumber" IS NULL OR "AttemptNumber" = 0;

-- Add foreign key constraint for NotificationTemplateId if the NotificationTemplates table exists
DO `$`$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'NotificationTemplates') THEN
        -- Check if constraint already exists
        IF NOT EXISTS (SELECT 1 FROM information_schema.table_constraints 
                      WHERE constraint_name = 'FK_NotificationDispatchAttempts_NotificationTemplates_NotificationTemplateId') THEN
            -- Add foreign key constraint
            ALTER TABLE "NotificationDispatchAttempts" 
            ADD CONSTRAINT "FK_NotificationDispatchAttempts_NotificationTemplates_NotificationTemplateId"
            FOREIGN KEY ("NotificationTemplateId") 
            REFERENCES "NotificationTemplates"("Id")
            ON DELETE SET NULL;
        END IF;
    END IF;
END `$`$;

-- Add indexes for the new columns
CREATE INDEX IF NOT EXISTS "IX_NotificationDispatchAttempts_DeliveredAt" 
ON "NotificationDispatchAttempts" ("DeliveredAt");

CREATE INDEX IF NOT EXISTS "IX_NotificationDispatchAttempts_AttemptNumber" 
ON "NotificationDispatchAttempts" ("AttemptNumber");

CREATE INDEX IF NOT EXISTS "IX_NotificationDispatchAttempts_NotificationTemplateId" 
ON "NotificationDispatchAttempts" ("NotificationTemplateId");

-- Verify the table structure
SELECT 'Schema fix completed successfully' as status;
"@

    # Save SQL to temp file
    $tempSqlFile = [System.IO.Path]::GetTempFileName() + ".sql"
    $sqlScript | Out-File -FilePath $tempSqlFile -Encoding UTF8
    
    try {
        # Parse connection string to get components
        $connParts = @{}
        $ConnectionString -split ";" | ForEach-Object {
            if ($_ -match "^([^=]+)=(.+)$") {
                $connParts[$matches[1].Trim()] = $matches[2].Trim()
            }
        }
        
        $server = $connParts["Host"] -or $connParts["Server"]
        $database = $connParts["Database"]
        $username = $connParts["Username"] -or $connParts["User ID"]
        $password = $connParts["Password"]
        $port = $connParts["Port"] -or "5432"
        
        Write-Host "Connecting to database: ${server}:${port}/${database}" -ForegroundColor Gray
        
        # Try to find psql
        $psqlPath = Get-Command psql -ErrorAction SilentlyContinue
        if ($psqlPath) {
            Write-Host "Using psql to apply schema fix..." -ForegroundColor Green
            
            # Set password environment variable
            $env:PGPASSWORD = $password
            
            # Execute SQL
            $result = & psql -h $server -p $port -U $username -d $database -f $tempSqlFile -q
            
            if ($LASTEXITCODE -eq 0) {
                Write-Host "SQL schema fix applied successfully!" -ForegroundColor Green
                return $true
            } else {
                Write-Host "Error applying SQL fix with psql. Exit code: $LASTEXITCODE" -ForegroundColor Red
                Write-Host "Output: $result" -ForegroundColor Red
                return $false
            }
        } else {
            Write-Host "psql not found. Please install PostgreSQL client tools or apply the SQL manually." -ForegroundColor Yellow
            Write-Host "SQL script saved to: $tempSqlFile" -ForegroundColor Cyan
            Write-Host "You can apply it manually using your preferred PostgreSQL client." -ForegroundColor Cyan
            return $false
        }
    }
    finally {
        # Clean up temp file
        if (Test-Path $tempSqlFile) {
            Remove-Item $tempSqlFile -Force -ErrorAction SilentlyContinue
        }
        # Clear password environment variable
        $env:PGPASSWORD = $null
    }
}

# Function to apply EF Core migration
function Apply-EfMigration {
    Write-Host "Applying Entity Framework migration..." -ForegroundColor Yellow
    
    try {
        Push-Location "src/Backend/CRM.Api"
        
        # Check if migration exists
        $migrationExists = Test-Path "../CRM.Infrastructure/Migrations/20251124130000_AddMissingNotificationDispatchColumns.cs"
        
        if ($migrationExists) {
            Write-Host "Running EF Core database update..." -ForegroundColor Green
            $result = dotnet ef database update --verbose 2>&1
            
            if ($LASTEXITCODE -eq 0) {
                Write-Host "EF Core migration completed successfully!" -ForegroundColor Green
                return $true
            } else {
                Write-Host "EF Core migration failed. Output:" -ForegroundColor Red
                Write-Host $result -ForegroundColor Red
                return $false
            }
        } else {
            Write-Host "Migration file not found. Creating new migration..." -ForegroundColor Yellow
            
            # Add migration
            $addResult = dotnet ef migrations add AddMissingNotificationDispatchColumns --verbose 2>&1
            
            if ($LASTEXITCODE -eq 0) {
                Write-Host "Migration created successfully. Applying..." -ForegroundColor Green
                
                # Apply migration
                $updateResult = dotnet ef database update --verbose 2>&1
                
                if ($LASTEXITCODE -eq 0) {
                    Write-Host "EF Core migration completed successfully!" -ForegroundColor Green
                    return $true
                } else {
                    Write-Host "EF Core database update failed. Output:" -ForegroundColor Red
                    Write-Host $updateResult -ForegroundColor Red
                    return $false
                }
            } else {
                Write-Host "Failed to create migration. Output:" -ForegroundColor Red
                Write-Host $addResult -ForegroundColor Red
                return $false
            }
        }
    }
    finally {
        Pop-Location
    }
}

# Main execution
try {
    # Step 1: Load environment variables
    $envLoaded = Load-EnvFile
    
    # Step 2: Stop running processes
    Stop-CrmProcesses
    
    # Step 3: Get connection string
    $connectionString = $env:POSTGRES_CONNECTION
    if (-not $connectionString) {
        Write-Host "Error: POSTGRES_CONNECTION not found in environment variables" -ForegroundColor Red
        Write-Host "Please ensure your .env file contains the database connection string" -ForegroundColor Red
        exit 1
    }
    
    Write-Host "Connection string found" -ForegroundColor Green
    
    # Step 4: Apply SQL fix first (faster and more reliable)
    $sqlSuccess = Apply-SqlFix -ConnectionString $connectionString
    
    # Step 5: Apply EF migration (to keep EF in sync)
    $efSuccess = Apply-EfMigration
    
    # Step 6: Summary
    Write-Host "`n=== Fix Summary ===" -ForegroundColor Cyan
    if ($sqlSuccess) {
        Write-Host "✓ SQL schema fix applied successfully" -ForegroundColor Green
    } else {
        Write-Host "✗ SQL schema fix failed" -ForegroundColor Red
    }
    
    if ($efSuccess) {
        Write-Host "✓ EF Core migration completed successfully" -ForegroundColor Green
    } else {
        Write-Host "✗ EF Core migration failed" -ForegroundColor Red
    }
    
    if ($sqlSuccess -or $efSuccess) {
        Write-Host "`nDatabase schema has been updated!" -ForegroundColor Green
        Write-Host "You can now restart your CRM application." -ForegroundColor Cyan
        Write-Host "`nTo start the application:" -ForegroundColor Yellow
        Write-Host "  cd src/Backend/CRM.Api" -ForegroundColor Gray
        Write-Host "  dotnet run" -ForegroundColor Gray
    } else {
        Write-Host "`nSchema fix failed. Please check the errors above and try manual application." -ForegroundColor Red
    }
    
} catch {
    Write-Host "Error during execution: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Stack trace: $($_.ScriptStackTrace)" -ForegroundColor Red
    exit 1
}