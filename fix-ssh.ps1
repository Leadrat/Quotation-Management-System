# Fix SSH Password Issue - Automated Setup
# Usage: .\fix-ssh.ps1

# Configuration
$SSHKeyPath = "C:\Users\Pankaj Joshi\ovh"
$Server = "ubuntu@148.113.37.88"
$NewKeyPath = "C:\Users\Pankaj Joshi\ovh\ovh_deploy"

Write-Host "🔐 Fixing SSH Password Issue..." -ForegroundColor Blue
Write-Host "Current Key: $SSHKeyPath" -ForegroundColor Blue
Write-Host "Server: $Server" -ForegroundColor Blue
Write-Host ""

# Check if current key exists
if (-not (Test-Path $SSHKeyPath)) {
    Write-Host "❌ Current SSH key not found: $SSHKeyPath" -ForegroundColor Red
    exit 1
}

Write-Host "✅ Current SSH key found" -ForegroundColor Green

# Check if key has passphrase
Write-Host "🔍 Checking if key has passphrase..." -ForegroundColor Blue

try {
    $null = ssh-keygen -y -f "$SSHKeyPath" 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Key appears to be unencrypted" -ForegroundColor Green
        $HasPassphrase = $false
    } else {
        Write-Host "🔒 Key is encrypted (has passphrase)" -ForegroundColor Yellow
        $HasPassphrase = $true
    }
} catch {
    Write-Host "🔒 Key is encrypted (has passphrase)" -ForegroundColor Yellow
    $HasPassphrase = $true
}

# Offer solutions
Write-Host ""
Write-Host "🎯 Choose a solution:" -ForegroundColor Cyan
Write-Host ""

if ($HasPassphrase) {
    Write-Host "1. Use ssh-agent (keep current encrypted key)" -ForegroundColor White
    Write-Host "2. Create new deployment key without passphrase" -ForegroundColor White
    Write-Host "3. Test current setup" -ForegroundColor White
} else {
    Write-Host "1. Test current setup" -ForegroundColor White
    Write-Host "2. Create new deployment key (alternative)" -ForegroundColor White
}

Write-Host ""
$choice = Read-Host "Enter your choice (1, 2, or 3)"

switch ($choice) {
    "1" {
        if ($HasPassphrase) {
            Write-Host "🚀 Setting up ssh-agent..." -ForegroundColor Blue
            
            # Start ssh-agent
            Write-Host "Starting ssh-agent..." -ForegroundColor Blue
            ssh-agent
            
            # Add key to agent
            Write-Host "Adding key to ssh-agent..." -ForegroundColor Blue
            Write-Host "Enter your SSH key passphrase when prompted:" -ForegroundColor Yellow
            ssh-add "$SSHKeyPath"
            
            if ($LASTEXITCODE -eq 0) {
                Write-Host "✅ Key added to ssh-agent successfully!" -ForegroundColor Green
                Write-Host ""
                Write-Host "🧪 Testing SSH connection..." -ForegroundColor Blue
                $result = ssh -i "$SSHKeyPath" -o ConnectTimeout=10 -o BatchMode=yes $Server "echo 'SSH connection successful'" 2>&1
                
                if ($LASTEXITCODE -eq 0) {
                    Write-Host "✅ SSH connection successful!" -ForegroundColor Green
                    Write-Host ""
                    Write-Host "🚀 Ready to deploy! Run:" -ForegroundColor Cyan
                    Write-Host ".\scripts\deploy.ps1 prod" -ForegroundColor White
                } else {
                    Write-Host "❌ SSH connection still failed" -ForegroundColor Red
                    Write-Host "Error: $result" -ForegroundColor Red
                }
            } else {
                Write-Host "❌ Failed to add key to ssh-agent" -ForegroundColor Red
            }
        } else {
            # Test current setup
            Write-Host "🧪 Testing SSH connection..." -ForegroundColor Blue
            $result = ssh -i "$SSHKeyPath" -o ConnectTimeout=10 -o BatchMode=yes $Server "echo 'SSH connection successful'" 2>&1
            
            if ($LASTEXITCODE -eq 0) {
                Write-Host "✅ SSH connection successful!" -ForegroundColor Green
                Write-Host ""
                Write-Host "🚀 Ready to deploy! Run:" -ForegroundColor Cyan
                Write-Host ".\scripts\deploy.ps1 prod" -ForegroundColor White
            } else {
                Write-Host "❌ SSH connection failed" -ForegroundColor Red
                Write-Host "Error: $result" -ForegroundColor Red
                Write-Host ""
                Write-Host "🔧 Try option 2 to create a new key" -ForegroundColor Yellow
            }
        }
    }
    
    "2" {
        Write-Host "🔑 Creating new deployment key without passphrase..." -ForegroundColor Blue
        
        # Generate new key
        Write-Host "Generating new SSH key..." -ForegroundColor Blue
        ssh-keygen -t rsa -b 4096 -f "$NewKeyPath" -N ""
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ New key created successfully!" -ForegroundColor Green
            Write-Host "New key: $NewKeyPath" -ForegroundColor Green
            
            # Try to copy public key to server
            Write-Host ""
            Write-Host "📤 Copying public key to server..." -ForegroundColor Blue
            Write-Host "You may be prompted for your server password:" -ForegroundColor Yellow
            
            $copyResult = ssh-copy-id -i "$NewKeyPath.pub" $Server 2>&1
            
            if ($LASTEXITCODE -eq 0) {
                Write-Host "✅ Public key copied to server!" -ForegroundColor Green
                
                # Test new key
                Write-Host "🧪 Testing new SSH key..." -ForegroundColor Blue
                $result = ssh -i "$NewKeyPath" -o ConnectTimeout=10 -o BatchMode=yes $Server "echo 'SSH connection successful'" 2>&1
                
                if ($LASTEXITCODE -eq 0) {
                    Write-Host "✅ New SSH key works!" -ForegroundColor Green
                    
                    # Update deployment script
                    Write-Host ""
                    Write-Host "🔄 Updating deployment script..." -ForegroundColor Blue
                    (Get-Content "scripts\deploy.ps1") -replace '$SSHKeyPath = "C:\\Users\\Pankaj Joshi\\ovh\\ovh"', '$SSHKeyPath = "C:\\Users\\Pankaj Joshi\\ovh\\ovh_deploy"' | Set-Content "scripts\deploy.ps1"
                    
                    Write-Host "✅ Deployment script updated!" -ForegroundColor Green
                    Write-Host ""
                    Write-Host "🚀 Ready to deploy! Run:" -ForegroundColor Cyan
                    Write-Host ".\scripts\deploy.ps1 prod" -ForegroundColor White
                } else {
                    Write-Host "❌ New SSH key test failed" -ForegroundColor Red
                    Write-Host "Error: $result" -ForegroundColor Red
                }
            } else {
                Write-Host "❌ Failed to copy public key to server" -ForegroundColor Red
                Write-Host "Error: $copyResult" -ForegroundColor Red
                Write-Host ""
                Write-Host "🔧 You may need to manually copy the public key:" -ForegroundColor Yellow
                Write-Host "1. SSH to server with password: ssh ubuntu@148.113.37.88" -ForegroundColor Gray
                Write-Host "2. Add key to ~/.ssh/authorized_keys" -ForegroundColor Gray
                Write-Host "3. Run this script again" -ForegroundColor Gray
            }
        } else {
            Write-Host "❌ Failed to generate new key" -ForegroundColor Red
        }
    }
    
    "3" {
        Write-Host "🧪 Testing current SSH setup..." -ForegroundColor Blue
        $result = ssh -i "$SSHKeyPath" -o ConnectTimeout=10 -o BatchMode=yes $Server "echo 'SSH connection successful'" 2>&1
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ SSH connection successful!" -ForegroundColor Green
            Write-Host ""
            Write-Host "🚀 Ready to deploy! Run:" -ForegroundColor Cyan
            Write-Host ".\scripts\deploy.ps1 prod" -ForegroundColor White
        } else {
            Write-Host "❌ SSH connection failed" -ForegroundColor Red
            Write-Host "Error: $result" -ForegroundColor Red
            Write-Host ""
            Write-Host "🔧 Try option 1 or 2 to fix the issue" -ForegroundColor Yellow
        }
    }
    
    default {
        Write-Host "❌ Invalid choice. Please run the script again." -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "📋 Summary:" -ForegroundColor Cyan
Write-Host "- If SSH works: Run .\scripts\deploy.ps1 prod" -ForegroundColor White
Write-Host "- If not: Try this script again with a different option" -ForegroundColor White
Write-Host "- Need help: Check SSH-PASSWORD-FIX.md" -ForegroundColor White
