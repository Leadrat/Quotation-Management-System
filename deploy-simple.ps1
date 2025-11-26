# Simple CRM Deployment to OVH Cloud
# Usage: .\deploy-simple.ps1

# Configuration
$Server = "ubuntu@148.113.37.88"
$SSHKeyPath = "C:\Users\Pankaj Joshi\ovh"
$ProjectName = "crm"
$RemoteDir = "/opt/$ProjectName"
$Timestamp = Get-Date -Format "yyyyMMdd_HHmmss"

Write-Host "🚀 Starting CRM Deployment to OVH Cloud..." -ForegroundColor Blue
Write-Host "Server: $Server" -ForegroundColor Blue
Write-Host "Key: $SSHKeyPath" -ForegroundColor Blue
Write-Host ""

# Step 1: Build Docker images
Write-Host "📦 Building Docker images..." -ForegroundColor Blue

try {
    Write-Host "Building backend..." -ForegroundColor Blue
    docker build -t crm-backend:latest ./src/Backend
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Backend build failed" -ForegroundColor Red
        exit 1
    }
    
    Write-Host "Building frontend..." -ForegroundColor Blue
    docker build -t crm-frontend:latest ./src/Frontend/web
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Frontend build failed" -ForegroundColor Red
        exit 1
    }
    
    Write-Host "✅ Docker images built successfully" -ForegroundColor Green
} catch {
    Write-Host "❌ Docker build failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Step 2: Setup server
Write-Host "🔧 Setting up remote server..." -ForegroundColor Blue

$serverSetup = @"
# Create directories
sudo mkdir -p $RemoteDir
sudo mkdir -p /opt/backups/$ProjectName
sudo mkdir -p $RemoteDir/nginx/ssl
sudo mkdir -p $RemoteDir/logs/nginx

# Set permissions
sudo chown -R `$USER:`$USER $RemoteDir
sudo chown -R `$USER:`$USER /opt/backups/$ProjectName

# Install Docker if not present
if ! command -v docker &> /dev/null; then
    echo "Installing Docker..."
    curl -fsSL https://get.docker.com -o get-docker.sh
    sudo sh get-docker.sh
    sudo usermod -aG docker `$USER
    rm get-docker.sh
fi

# Install Docker Compose if not present
if ! command -v docker-compose &> /dev/null; then
    echo "Installing Docker Compose..."
    sudo curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-linux-x86_64" -o /usr/local/bin/docker-compose
    sudo chmod +x /usr/local/bin/docker-compose
fi

echo "Server setup completed"
"@

ssh -i "$SSHKeyPath" $Server $serverSetup
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Server setup failed" -ForegroundColor Red
    exit 1
}

Write-Host "✅ Server setup completed" -ForegroundColor Green

# Step 3: Backup existing deployment
Write-Host "💾 Creating backup..." -ForegroundColor Blue

$backupCmd = @"
if [ -d "$RemoteDir/docker-compose.yml" ]; then
    tar -czf /opt/backups/$ProjectName/backup_$Timestamp.tar.gz -C $RemoteDir .
    echo "Backup created"
else
    echo "No existing deployment to backup"
fi
"@

ssh -i "$SSHKeyPath" $Server $backupCmd
Write-Host "✅ Backup completed" -ForegroundColor Green

# Step 4: Deploy files
Write-Host "📤 Deploying files to server..." -ForegroundColor Blue

# Create temporary directory
$tempDir = "C:\temp\$ProjectName-deploy-$Timestamp"
New-Item -ItemType Directory -Path $tempDir -Force | Out-Null

try {
    # Copy files to temp directory
    Copy-Item "docker-compose.prod.yml" "$tempDir\docker-compose.yml"
    Copy-Item ".env.prod" "$tempDir\.env"
    
    # Copy to server
    scp -i "$SSHKeyPath" -r "$tempDir\*" "$Server`:$RemoteDir/"
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ File deployment failed" -ForegroundColor Red
        exit 1
    }
    
    Write-Host "✅ Files deployed successfully" -ForegroundColor Green
} finally {
    # Clean up temp directory
    Remove-Item -Path $tempDir -Recurse -Force
}

# Step 5: Start services
Write-Host "🚀 Starting services..." -ForegroundColor Blue

$startServices = @"
cd $RemoteDir

# Stop existing services
docker-compose down || true

# Start new services
docker-compose up -d

# Wait for services to start
sleep 30

# Check status
docker-compose ps

echo "Services started"
"@

ssh -i "$SSHKeyPath" $Server $startServices
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Service start failed" -ForegroundColor Red
    exit 1
}

Write-Host "✅ Services started successfully" -ForegroundColor Green

# Step 6: Health check
Write-Host "🔍 Performing health check..." -ForegroundColor Blue

# Wait a bit more for services to be ready
Start-Sleep -Seconds 10

try {
    $frontendCheck = Invoke-WebRequest -Uri "http://148.113.37.88/health" -UseBasicParsing -TimeoutSec 10
    if ($frontendCheck.StatusCode -eq 200) {
        Write-Host "✅ Frontend health check passed" -ForegroundColor Green
    } else {
        Write-Host "⚠️ Frontend health check returned $($frontendCheck.StatusCode)" -ForegroundColor Yellow
    }
} catch {
    Write-Host "⚠️ Frontend health check failed: $($_.Exception.Message)" -ForegroundColor Yellow
}

try {
    $backendCheck = Invoke-WebRequest -Uri "http://148.113.37.88/api/health" -UseBasicParsing -TimeoutSec 10
    if ($backendCheck.StatusCode -eq 200) {
        Write-Host "✅ Backend health check passed" -ForegroundColor Green
    } else {
        Write-Host "⚠️ Backend health check returned $($backendCheck.StatusCode)" -ForegroundColor Yellow
    }
} catch {
    Write-Host "⚠️ Backend health check failed: $($_.Exception.Message)" -ForegroundColor Yellow
}

# Step 7: Show deployment summary
Write-Host ""
Write-Host "🎉 Deployment completed!" -ForegroundColor Green
Write-Host ""
Write-Host "📊 Deployment Summary:" -ForegroundColor Cyan
Write-Host "   - Server: $Server" -ForegroundColor White
Write-Host "   - Timestamp: $Timestamp" -ForegroundColor White
Write-Host "   - Frontend URL: http://148.113.37.88" -ForegroundColor White
Write-Host "   - Backend URL: http://148.113.37.88/api" -ForegroundColor White
Write-Host "   - Backup: /opt/backups/$ProjectName/backup_$Timestamp.tar.gz" -ForegroundColor White
Write-Host ""
Write-Host "🔧 Management Commands:" -ForegroundColor Cyan
Write-Host "   ssh $Server" -ForegroundColor White
Write-Host "   cd $RemoteDir" -ForegroundColor White
Write-Host "   docker-compose logs -f" -ForegroundColor White
Write-Host "   docker-compose ps" -ForegroundColor White
Write-Host ""
Write-Host "🌐 Access your CRM at: http://148.113.37.88" -ForegroundColor Green
