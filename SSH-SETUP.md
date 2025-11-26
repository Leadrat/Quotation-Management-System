# 🔧 SSH Configuration for OVH Cloud

## **Issue: SSH Connection Failed**

**Current Setup:**
- **Public Key**: `C:\Users\Pankaj Joshi\ovh\ovh.pub`
- **Server**: ubuntu@148.113.37.88
- **Error**: SSH connection failed

## **🔑 SSH Configuration Steps:**

### **Step 1: Verify SSH Key Files**
```bash
# Check if key files exist
dir "C:\Users\Pankaj Joshi\ovh\"

# You should see:
# ovh (private key)
# ovh.pub (public key)
```

### **Step 2: Set Correct Permissions**
```bash
# Set private key permissions (Windows)
icacls "C:\Users\Pankaj Joshi\ovh\ovh" /inheritance:r
icacls "C:\Users\Pankaj Joshi\ovh\ovh" /grant:r "Pankaj Joshi:(R)"

# Or using PowerShell
$acl = Get-Acl "C:\Users\Pankaj Joshi\ovh\ovh"
$acl.SetAccessRuleProtection($true, $false)
$rule = New-Object System.Security.AccessControl.FileSystemAccessRule("Pankaj Joshi","Read","Allow")
$acl.SetAccessRule($rule)
Set-Acl "C:\Users\Pankaj Joshi\ovh\ovh" $acl
```

### **Step 3: Test SSH Connection**
```bash
# Test connection with specific key
ssh -i "C:\Users\Pankaj Joshi\ovh\ovh" ubuntu@148.113.37.88

# If this works, add to SSH config for easier access
```

### **Step 4: Configure SSH Config File**
```bash
# Create/edit SSH config file
notepad $env:USERPROFILE\.ssh\config

# Add this content:
Host ovh-crm
    HostName 148.113.37.88
    User ubuntu
    Port 22
    IdentityFile "C:\Users\Pankaj Joshi\ovh\ovh"
    StrictHostKeyChecking no

# Save the file
```

### **Step 5: Test with SSH Config**
```bash
# Test connection using config
ssh ovh-crm

# Should connect without password
```

## **🔧 Alternative: Update Deployment Script**

If SSH config doesn't work, let's update the deployment script to use the specific key:

### **Option 1: Update PowerShell Script**
```powershell
# Edit scripts/deploy.ps1
# Find the SSH commands and add -i parameter

# Change this:
ssh $Server

# To this:
ssh -i "C:\Users\Pankaj Joshi\ovh\ovh" $Server
```

### **Option 2: Create SSH Environment Variable**
```bash
# Set SSH key environment variable
$env:SSH_KEY_PATH = "C:\Users\Pankaj Joshi\ovh\ovh"

# Add to deployment script
ssh -i $env:SSH_KEY_PATH $Server
```

## **🚨 Troubleshooting SSH Issues:**

### **1. Key Not Found**
```bash
# Check if key exists
Test-Path "C:\Users\Pankaj Joshi\ovh\ovh"

# If not found, locate your SSH keys
dir $env:USERPROFILE\.ssh\
dir "C:\Users\Pankaj Joshi\"
```

### **2. Permission Denied**
```bash
# Reset key permissions
# Remove old key and regenerate if needed
ssh-keygen -t rsa -b 4096 -f "C:\Users\Pankaj Joshi\ovh\ovh_new"

# Add new public key to server
ssh-copy-id -i "C:\Users\Pankaj Joshi\ovh\ovh_new.pub" ubuntu@148.113.37.88
```

### **3. Server Not Accepting Key**
```bash
# Try with verbose output
ssh -v -i "C:\Users\Pankaj Joshi\ovh\ovh" ubuntu@148.113.37.88

# Look for these messages:
# "Offering public key"
# "Authentication succeeded"
```

### **4. Host Key Verification**
```bash
# Remove old host key
ssh-keygen -R 148.113.37.88

# Try connection again
ssh -i "C:\Users\Pankaj Joshi\ovh\ovh" ubuntu@148.113.37.88
```

## **🔄 Quick Fix Steps:**

### **Step 1: Test Basic Connection**
```bash
# Test with verbose output
ssh -v -i "C:\Users\Pankaj Joshi\ovh\ovh" ubuntu@148.113.37.88
```

### **Step 2: If Works, Update Deployment Script**
I can update the deployment script to use your specific key path.

### **Step 3: Run Deployment**
```powershell
# After SSH is working
.\scripts\deploy.ps1 prod
```

## **📞 Need Help?**

### **Check These:**
1. **Key file exists** at `C:\Users\Pankaj Joshi\ovh\ovh`
2. **Key permissions** are correct (read-only for user)
3. **Server is accessible** from your network
4. **Firewall allows** SSH (port 22)

### **Debug Commands:**
```bash
# Test connection with debugging
ssh -vvv -i "C:\Users\Pankaj Joshi\ovh\ovh" ubuntu@148.113.37.88

# Check if server is reachable
Test-NetConnection -ComputerName 148.113.37.88 -Port 22
```

---

## **🎯 Next Steps:**

1. **Test SSH connection** with the commands above
2. **Let me know the error message** if it still fails
3. **I'll update the deployment script** with your key path
4. **Run the deployment** once SSH is working

**Once SSH is working, the deployment will proceed smoothly!** 🔧
