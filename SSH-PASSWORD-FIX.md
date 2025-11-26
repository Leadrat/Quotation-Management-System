# 🔐 SSH Password Issue - Fix Guide

## **Problem: SSH Key Asking for Password**

**Issue**: SSH key `C:\Users\Pankaj Joshi\ovh\ovh` is prompting for password instead of using key-based authentication.

## **🔍 Common Causes & Solutions:**

### **1. SSH Key is Encrypted (Most Likely)**
If your SSH key was created with a passphrase, you need to:
```powershell
# Option A: Use ssh-agent to cache the password
ssh-add "C:\Users\Pankaj Joshi\ovh\ovh"
# Enter passphrase once, then use without password

# Option B: Create new key without passphrase
ssh-keygen -t rsa -b 4096 -f "C:\Users\Pankaj Joshi\ovh\ovh_new" -N ""
# This creates a key without passphrase
```

### **2. Server Doesn't Have Your Public Key**
The server needs your public key in `~/.ssh/authorized_keys`:
```bash
# Copy public key to server (if you can login with password temporarily)
ssh-copy-id -i "C:\Users\Pankaj Joshi\ovh\ovh.pub" ubuntu@148.113.37.88

# Or manually add it:
ssh ubuntu@148.113.37.88 "mkdir -p ~/.ssh && echo '$(cat "C:\Users\Pankaj Joshi\ovh\ovh.pub")' >> ~/.ssh/authorized_keys && chmod 600 ~/.ssh/authorized_keys"
```

### **3. Key Permissions Issue**
```powershell
# Fix private key permissions (Windows)
icacls "C:\Users\Pankaj Joshi\ovh\ovh" /inheritance:r
icacls "C:\Users\Pankaj Joshi\ovh\ovh" /grant:r "Pankaj Joshi:(R)"

# Make sure only you can read the key
```

### **4. Server SSH Configuration**
Server might be configured to require passwords:
```bash
# Check server SSH config
ssh ubuntu@148.113.37.88 "sudo grep PasswordAuthentication /etc/ssh/sshd_config"

# Should show: PasswordAuthentication no
# If it shows "yes", key auth might be disabled
```

## 🚀 **Quick Solutions:**

### **Solution 1: Use ssh-agent (Recommended)**
```powershell
# Start ssh-agent
ssh-agent

# Add your key (enter passphrase once)
ssh-add "C:\Users\Pankaj Joshi\ovh\ovh"

# Test connection
ssh -i "C:\Users\Pankaj Joshi\ovh\ovh" ubuntu@148.113.37.88

# Now run deployment
.\scripts\deploy.ps1 prod
```

### **Solution 2: Create New Key Without Passphrase**
```powershell
# Generate new key without passphrase
ssh-keygen -t rsa -b 4096 -f "C:\Users\Pankaj Joshi\ovh\ovh_nopass" -N ""

# Copy new public key to server
ssh-copy-id -i "C:\Users\Pankaj Joshi\ovh\ovh_nopass.pub" ubuntu@148.113.37.88

# Update deployment script to use new key
# Change: $SSHKeyPath = "C:\Users\Pankaj Joshi\ovh\ovh_nopass"
```

### **Solution 3: Temporarily Use Password Auth**
If you can login with password:
```powershell
# Enable password auth in deployment temporarily
# Edit scripts/deploy.ps1, remove -i parameter for testing

# Or use this command to copy key:
ssh-copy-id -i "C:\Users\Pankaj Joshi\ovh\ovh.pub" ubuntu@148.113.37.88
```

## 🔧 **Step-by-Step Fix:**

### **Step 1: Test Current Key**
```powershell
# Test with verbose output to see what's happening
ssh -vvv -i "C:\Users\Pankaj Joshi\ovh\ovh" ubuntu@148.113.37.88
```

### **Step 2: Check if Key Has Passphrase**
```powershell
# Try to load key
ssh-keygen -y -f "C:\Users\Pankaj Joshi\ovh\ovh"
# If it asks for passphrase, your key is encrypted
```

### **Step 3: Use ssh-agent**
```powershell
# Start ssh-agent
ssh-agent

# Add key (enter passphrase when prompted)
ssh-add "C:\Users\Pankaj Joshi\ovh\ovh"

# Test connection
ssh -i "C:\Users\Pankaj Joshi\ovh\ovh" ubuntu@148.113.37.88
```

### **Step 4: Run Deployment**
```powershell
# Once SSH works without password
.\scripts\deploy.ps1 prod
```

## 🎯 **Easiest Solution:**

### **Use ssh-agent (Best for encrypted keys):**
```powershell
# One-time setup
ssh-agent
ssh-add "C:\Users\Pankaj Joshi\ovh\ovh"
# Enter your passphrase once

# Now SSH works without password prompts
ssh -i "C:\Users\Pankaj Joshi\ovh\ovh" ubuntu@148.113.37.88

# Run deployment
.\scripts\deploy.ps1 prod
```

### **Create new key without passphrase (Alternative):**
```powershell
# Generate new key
ssh-keygen -t rsa -b 4096 -f "C:\Users\Pankaj Joshi\ovh\ovh_deploy" -N ""

# Copy to server
ssh-copy-id -i "C:\Users\Pankaj Joshi\ovh\ovh_deploy.pub" ubuntu@148.113.37.88

# Update deployment script key path
# Change line 11 in scripts/deploy.ps1 to:
# $SSHKeyPath = "C:\Users\Pankaj Joshi\ovh\ovh_deploy"

# Run deployment
.\scripts\deploy.ps1 prod
```

## 📞 **Choose Your Solution:**

1. **ssh-agent** - Keep your current encrypted key (recommended)
2. **New key** - Create deployment-specific key without passphrase
3. **Password auth** - Temporary solution to copy keys

**Once SSH works without password prompts, the deployment will proceed smoothly!** 🔐
