# Test SSH Connection to OVH Cloud Server
# Usage: .\test-ssh.ps1

# Configuration
$Server = "ubuntu@148.113.37.88"
$SSHKeyPath = "C:\Users\Pankaj Joshi\ovh\ovh"

Write-Host "🔑 Testing SSH connection to OVH Cloud..." -ForegroundColor Blue
Write-Host "Server: $Server" -ForegroundColor Blue
Write-Host "Key: $SSHKeyPath" -ForegroundColor Blue
Write-Host ""

# Check if key file exists
if (-not (Test-Path $SSHKeyPath)) {
    Write-Host "❌ SSH key not found at: $SSHKeyPath" -ForegroundColor Red
    Write-Host "Please check the key path and try again." -ForegroundColor Red
    exit 1
}

Write-Host "✅ SSH key file found" -ForegroundColor Green

# Test SSH connection
Write-Host "🔍 Testing SSH connection..." -ForegroundColor Blue

try {
    $result = ssh -i "$SSHKeyPath" -o ConnectTimeout=10 -o BatchMode=yes $Server "echo 'SSH connection successful'" 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ SSH connection successful!" -ForegroundColor Green
        Write-Host "Response: $result" -ForegroundColor Green
    } else {
        Write-Host "❌ SSH connection failed" -ForegroundColor Red
        Write-Host "Error: $result" -ForegroundColor Red
        Write-Host ""
        Write-Host "🔧 Troubleshooting steps:" -ForegroundColor Yellow
        Write-Host "1. Check if the server is reachable:" -ForegroundColor White
        Write-Host "   Test-NetConnection -ComputerName 148.113.37.88 -Port 22" -ForegroundColor Gray
        Write-Host ""
        Write-Host "2. Try verbose SSH output:" -ForegroundColor White
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
Write-Host "🎯 If SSH works, run the deployment:" -ForegroundColor Blue
Write-Host ".\scripts\deploy.ps1 prod" -ForegroundColor Cyan
