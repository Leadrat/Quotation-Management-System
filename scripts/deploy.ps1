# CRM Deployment Script for OVH Cloud (PowerShell)
# Usage: .\scripts\deploy.ps1 [environment]
# Environment: dev, staging, prod (default: prod)

param(
    [string]$Environment = "prod"
)

# Configuration
$Server = "ubuntu@148.113.37.88"
$SSHKeyPath = "C:\Users\Pankaj Joshi\ovh"
$ProjectName = "crm"
$RemoteDir = "/opt/$ProjectName"
$BackupDir = "/opt/backups/$ProjectName"
$Timestamp = Get-Date -Format "yyyyMMdd_HHmmss"

Write-Host "🚀 Starting CRM deployment to OVH Cloud..." -ForegroundColor Blue
Write-Host "📋 Environment: $Environment" -ForegroundColor Blue
Write-Host "🖥️  Server: $Server" -ForegroundColor Blue
Write-Host "📅 Timestamp: $Timestamp" -ForegroundColor Blue
Write-Host ""

# Function to write colored output
function Write-Status {
    param([string]$Message)
    Write-Host "[INFO] $Message" -ForegroundColor Blue
}

function Write-Success {
    param([string]$Message)
    Write-Host "[SUCCESS] $Message" -ForegroundColor Green
}

function Write-Warning {
    param([string]$Message)
    Write-Host "[WARNING] $Message" -ForegroundColor Yellow
}

function Write-Error {
    param([string]$Message)
    Write-Host "[ERROR] $Message" -ForegroundColor Red
    exit 1
}

# Check if required tools are installed
function Test-Requirements {
    Write-Status "Checking requirements..."
    
    try {
        $null = Get-Command ssh -ErrorAction Stop
        Write-Success "SSH is installed"
    } catch {
        Write-Error "SSH is not installed. Please install OpenSSH."
    }
    
    try {
        $null = Get-Command docker -ErrorAction Stop
        Write-Success "Docker is installed"
    } catch {
        Write-Error "Docker is not installed. Please install Docker Desktop."
    }
    
    try {
        $null = Get-Command docker-compose -ErrorAction Stop
        Write-Success "Docker Compose is installed"
    } catch {
        Write-Error "Docker Compose is not installed. Please install Docker Compose."
    }
    
    Write-Success "All requirements are met"
}

# Test SSH connection
function Test-SSHConnection {
    Write-Status "Testing SSH connection to $Server..."
    
    try {
        $null = ssh -i "$SSHKeyPath" -o ConnectTimeout=10 -o BatchMode=yes $Server "echo 'SSH connection successful'" 2>$null
        if ($LASTEXITCODE -eq 0) {
            Write-Success "SSH connection test passed"
        } else {
            Write-Error "SSH connection failed. Please check your SSH configuration."
        }
    } catch {
        Write-Error "SSH connection failed. Please check your SSH configuration."
    }
}

# Prepare environment files
function Initialize-EnvironmentFiles {
    Write-Status "Preparing environment files..."
    
    # Create production environment file if it doesn't exist
    if (-not (Test-Path ".env.prod")) {
        Write-Warning ".env.prod file not found. Creating from .env.example..."
        Copy-Item ".env.example" ".env.prod"
        Write-Warning "Please update .env.prod with production values before proceeding."
        Read-Host "Press Enter to continue after updating .env.prod..."
    }
    
    # Create frontend production environment file
    if (-not (Test-Path "src\Frontend\web\.env.production")) {
        Write-Warning "Frontend .env.production not found. Creating template..."
        $envContent = @"
NODE_ENV=production
NEXT_PUBLIC_API_URL=https://your-domain.com/api
NEXT_PUBLIC_APP_NAME=CRM System
"@
        Set-Content -Path "src\Frontend\web\.env.production" -Value $envContent
        Write-Warning "Please update src\Frontend\web\.env.production with production values."
    }
}

# Build Docker images locally
function New-DockerImages {
    Write-Status "Building Docker images..."
    
    try {
        # Build backend
        Write-Status "Building backend image..."
        docker build -t "$ProjectName-backend:$Timestamp" "./src/Backend"
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Backend image build failed"
        }
        
        # Build frontend
        Write-Status "Building frontend image..."
        docker build -t "$ProjectName-frontend:$Timestamp" "./src/Frontend/web"
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Frontend image build failed"
        }
        
        Write-Success "Docker images built successfully"
    } catch {
        Write-Error "Docker image build failed: $($_.Exception.Message)"
    }
}

# Setup remote server
function Initialize-RemoteServer {
    Write-Status "Setting up remote server..."
    
    $remoteScript = @"
# Create directories
sudo mkdir -p $RemoteDir
sudo mkdir -p $BackupDir
sudo mkdir -p $RemoteDir/nginx/ssl
sudo mkdir -p $RemoteDir/logs/nginx

# Set permissions
sudo chown -R `$USER:`$USER $RemoteDir
sudo chown -R `$USER:`$USER $BackupDir

# Install Docker and Docker Compose if not installed
if ! command -v docker &> /dev/null; then
    echo "Installing Docker..."
    curl -fsSL https://get.docker.com -o get-docker.sh
    sudo sh get-docker.sh
    sudo usermod -aG docker `$USER
    rm get-docker.sh
fi

if ! command -v docker-compose &> /dev/null; then
    echo "Installing Docker Compose..."
    sudo curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
    sudo chmod +x /usr/local/bin/docker-compose
fi

echo "Remote server setup completed"
"@
    
    ssh -i "$SSHKeyPath" $Server $remoteScript
    if ($LASTEXITCODE -eq 0) {
        Write-Success "Remote server setup completed"
    } else {
        Write-Error "Remote server setup failed"
    }
}

# Backup existing deployment
function Backup-ExistingDeployment {
    Write-Status "Backing up existing deployment..."
    
    $backupScript = @"
if [ -d "$RemoteDir/docker-compose.yml" ]; then
    echo "Creating backup..."
    tar -czf $BackupDir/backup_$Timestamp.tar.gz -C $RemoteDir .
    echo "Backup created: $BackupDir/backup_$Timestamp.tar.gz"
else
    echo "No existing deployment to backup"
fi
"@
    
    ssh -i "$SSHKeyPath" $Server $backupScript
    if ($LASTEXITCODE -eq 0) {
        Write-Success "Backup completed"
    } else {
        Write-Warning "Backup failed, but continuing with deployment"
    }
}

# Deploy files to server
function Copy-DeploymentFiles {
    Write-Status "Deploying files to server..."
    
    # Create temporary directory for deployment
    $TempDir = "C:\temp\$ProjectName-deploy-$Timestamp"
    New-Item -ItemType Directory -Path $TempDir -Force | Out-Null
    
    try {
        # Copy files to temporary directory
        Copy-Item "docker-compose.prod.yml" "$TempDir\docker-compose.yml"
        Copy-Item "nginx" "$TempDir\" -Recurse
        Copy-Item ".env.prod" "$TempDir\.env"
        
        # Copy to remote server using scp
        scp -i "$SSHKeyPath" -r "$TempDir\*" "$Server`:$RemoteDir/"
        if ($LASTEXITCODE -eq 0) {
            Write-Success "Files deployed successfully"
        } else {
            Write-Error "File deployment failed"
        }
    } finally {
        # Clean up temporary directory
        Remove-Item -Path $TempDir -Recurse -Force
    }
}

# Deploy application
function Start-ApplicationDeployment {
    Write-Status "Deploying application..."
    
    $deployScript = @"
cd $RemoteDir

# Stop existing services
echo "Stopping existing services..."
docker-compose down || true

# Start services
echo "Starting services..."
docker-compose up -d

# Wait for services to be healthy
echo "Waiting for services to be healthy..."
sleep 30

# Check status
echo "Checking service status..."
docker-compose ps

# Show recent logs
echo "Recent logs:"
docker-compose logs --tail=50
"@
    
    ssh -i "$SSHKeyPath" $Server $deployScript
    if ($LASTEXITCODE -eq 0) {
        Write-Success "Application deployed successfully"
    } else {
        Write-Error "Application deployment failed"
    }
}

# Health check
function Test-HealthCheck {
    Write-Status "Performing health check..."
    
    # Wait for services to start
    Start-Sleep -Seconds 10
    
    try {
        # Check frontend
        $frontendResponse = Invoke-WebRequest -Uri "http://148.113.37.88/health" -UseBasicParsing -TimeoutSec 10
        if ($frontendResponse.StatusCode -eq 200) {
            Write-Success "Frontend health check passed"
        } else {
            Write-Error "Frontend health check failed"
        }
    } catch {
        Write-Warning "Frontend health check failed: $($_.Exception.Message)"
    }
    
    try {
        # Check backend
        $backendResponse = Invoke-WebRequest -Uri "http://148.113.37.88/api/health" -UseBasicParsing -TimeoutSec 10
        if ($backendResponse.StatusCode -eq 200) {
            Write-Success "Backend health check passed"
        } else {
            Write-Warning "Backend health check failed (endpoint may not exist)"
        }
    } catch {
        Write-Warning "Backend health check failed: $($_.Exception.Message)"
    }
}

# Cleanup old images and backups
function Clear-OldResources {
    Write-Status "Cleaning up old resources..."
    
    $cleanupScript = @"
cd $RemoteDir

# Remove unused Docker images
docker image prune -f

# Remove old backups (keep last 5)
cd $BackupDir
ls -t backup_*.tar.gz | tail -n +6 | xargs -r rm

echo "Cleanup completed"
"@
    
    ssh -i "$SSHKeyPath" $Server $cleanupScript
    if ($LASTEXITCODE -eq 0) {
        Write-Success "Cleanup completed"
    } else {
        Write-Warning "Cleanup failed, but deployment succeeded"
    }
}

# Main deployment flow
function Start-Deployment {
    Write-Status "Starting deployment process..."
    
    Test-Requirements
    Test-SSHConnection
    Initialize-EnvironmentFiles
    Build-DockerImages
    Initialize-RemoteServer
    Backup-ExistingDeployment
    Copy-DeploymentFiles
    Start-ApplicationDeployment
    Test-HealthCheck
    Clear-OldResources
    
    Write-Host ""
    Write-Success "🎉 Deployment completed successfully!"
    Write-Host ""
    Write-Host "📊 Deployment Summary:" -ForegroundColor Cyan
    Write-Host "   - Environment: $Environment" -ForegroundColor White
    Write-Host "   - Server: $Server" -ForegroundColor White
    Write-Host "   - Timestamp: $Timestamp" -ForegroundColor White
    Write-Host "   - Frontend URL: http://148.113.37.88" -ForegroundColor White
    Write-Host "   - Backend URL: http://148.113.37.88/api" -ForegroundColor White
    Write-Host "   - Backup: $BackupDir/backup_$Timestamp.tar.gz" -ForegroundColor White
    Write-Host ""
    Write-Host "🔧 To manage the deployment:" -ForegroundColor Cyan
    Write-Host "   ssh $Server" -ForegroundColor White
    Write-Host "   cd $RemoteDir" -ForegroundColor White
    Write-Host "   docker-compose logs -f" -ForegroundColor White
    Write-Host "   docker-compose ps" -ForegroundColor White
    Write-Host ""
}

# Run main function
Start-Deployment
