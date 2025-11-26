# Simple SSH Test for No-Password Key
# Usage: .\test-ssh-simple.ps1

# Configuration
$SSHKeyPath = "C:\Users\Pankaj Joshi\ovh"
$Server = "ubuntu@148.113.37.88"

Write-Host "🔑 Testing SSH Connection..." -ForegroundColor Blue
Write-Host "Key: $SSHKeyPath" -ForegroundColor Blue
Write-Host "Server: $Server" -ForegroundColor Blue
Write-Host ""

# Check if key exists
if (-not (Test-Path $SSHKeyPath)) {
    Write-Host "❌ SSH key not found: $SSHKeyPath" -ForegroundColor Red
    exit 1
}

Write-Host "✅ SSH key found" -ForegroundColor Green

# Test SSH connection
Write-Host "🔍 Testing SSH connection..." -ForegroundColor Blue

try {
    $result = ssh -i "$SSHKeyPath" -o ConnectTimeout=10 -o BatchMode=yes $Server "echo 'SSH connection successful'" 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ SSH connection successful!" -ForegroundColor Green
        Write-Host "Response: $result" -ForegroundColor Green
        Write-Host ""
        Write-Host "🚀 Ready to deploy! Run:" -ForegroundColor Cyan
        Write-Host ".\scripts\deploy.ps1 prod" -ForegroundColor White
    } else {
        Write-Host "❌ SSH connection failed" -ForegroundColor Red
        Write-Host "Error: $result" -ForegroundColor Red
        Write-Host ""
        Write-Host "🔧 Try these fixes:" -ForegroundColor Yellow
        Write-Host "1. Check server connectivity:" -ForegroundColor White
        Write-Host "   Test-NetConnection -ComputerName 148.113.37.88 -Port 22" -ForegroundColor Gray
        Write-Host ""
        Write-Host "2. Try verbose SSH:" -ForegroundColor White
        Write-Host "   ssh -vvv -i '$SSHKeyPath' $Server" -ForegroundColor Gray
        Write-Host ""
        Write-Host "3. Check key permissions:" -ForegroundColor White
        Write-Host "   icacls '$SSHKeyPath' /inheritance:r" -ForegroundColor Gray
        Write-Host "   icacls '$SSHKeyPath' /grant:r 'Pankaj Joshi:(R)'" -ForegroundColor Gray
        Write-Host ""
        Write-Host "4. Remove old host keys:" -ForegroundColor White
        Write-Host "   ssh-keygen -R 148.113.37.88" -ForegroundColor Gray
    }
} catch {
    Write-Host "❌ SSH connection failed with exception" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "📋 If SSH works, your deployment is ready!" -ForegroundColor Blue
