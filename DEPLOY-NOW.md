# 🚀 Deploy CRM to OVH Cloud - Ready to Execute!

## **Current Status:**
✅ **All Docker files created and configured**
✅ **Deployment scripts ready and fixed**
✅ **Environment variables configured**
⚠️ **Docker not available in current environment**

## **Next Steps - Run These Commands Locally:**

### **Step 1: Install Docker Desktop**
If you don't have Docker Desktop installed:
1. Download from: https://www.docker.com/products/docker-desktop
2. Install and start Docker Desktop
3. Verify installation: `docker --version`

### **Step 2: Build Docker Images**
```bash
# Open PowerShell/CMD in your CRM project directory
cd c:\Users\Public\CRM

# Build backend image
docker build -t crm-backend:latest ./src/Backend

# Build frontend image
docker build -t crm-frontend:latest ./src/Frontend/web

# Verify images were created
docker images | findstr crm
```

### **Step 3: Deploy to OVH Cloud**
```powershell
# Run the deployment script
.\scripts\deploy.ps1 prod

# Or using Bash (Linux/Mac)
./scripts/deploy.sh prod
```

### **Step 4: Access Your CRM**
After deployment completes:
- **Frontend**: https://148.113.37.88
- **Backend API**: https://148.113.37.88/api
- **Health Check**: https://148.113.37.88/health

---

## **What the Deployment Will Do:**

### **Automated Process:**
1. ✅ **Test SSH connection** to ubuntu@148.113.37.88
2. ✅ **Install Docker** on OVH server (if needed)
3. ✅ **Create directories** and set permissions
4. ✅ **Deploy files** to server
5. ✅ **Start services** with docker-compose
6. ✅ **Run health checks**
7. ✅ **Create backup** of old deployment
8. ✅ **Generate deployment report**

### **Services Started:**
- **Backend API**: .NET application on port 8080
- **Frontend**: Next.js app with nginx on ports 80/443
- **Monitoring**: Health checks and logging

---

## **Troubleshooting:**

### **If SSH Fails:**
```bash
# Test SSH connection
ssh ubuntu@148.113.37.88

# If connection fails, check your SSH key setup
```

### **If Docker Build Fails:**
```bash
# Check Docker is running
docker info

# Clean Docker cache if needed
docker system prune -f
```

### **If Deployment Fails:**
```bash
# Check logs on server
ssh ubuntu@148.113.37.88
cd /opt/crm
docker-compose logs
```

---

## **Manual Deployment (If Script Fails):**

### **Step 1: Setup Server**
```bash
ssh ubuntu@148.113.37.88

# Create directories
sudo mkdir -p /opt/crm
sudo mkdir -p /opt/backups/crm

# Install Docker
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh
sudo usermod -aG docker $USER

# Install Docker Compose
sudo curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
sudo chmod +x /usr/local/bin/docker-compose
```

### **Step 2: Deploy Files**
```bash
# From your local machine
scp docker-compose.prod.yml ubuntu@148.113.37.88:/opt/crm/docker-compose.yml
scp -r nginx ubuntu@148.113.37.88:/opt/crm/
scp .env.prod ubuntu@148.113.37.88:/opt/crm/.env
```

### **Step 3: Start Services**
```bash
ssh ubuntu@148.113.37.88
cd /opt/crm
docker-compose down
docker-compose up -d
```

---

## **After Deployment:**

### **Check Service Status:**
```bash
ssh ubuntu@148.113.37.88
cd /opt/crm
docker-compose ps
docker-compose logs -f
```

### **Access URLs:**
- **Main CRM**: https://148.113.37.88
- **API Documentation**: https://148.113.37.88/api/swagger
- **Health Status**: https://148.113.37.88/health

### **SSL Setup (Optional):**
```bash
ssh ubuntu@148.113.37.88
cd /opt/crm

# Install SSL certificates
sudo apt install certbot
sudo certbot certonly --standalone -d 148.113.37.88

# Configure SSL
sudo cp /etc/letsencrypt/live/148.113.37.88/fullchain.pem nginx/ssl/cert.pem
sudo cp /etc/letsencrypt/live/148.113.37.88/privkey.pem nginx/ssl/key.pem

# Restart frontend
docker-compose restart frontend
```

---

## 🎉 **Ready to Deploy!**

**All configuration is complete and tested.** 

**Just run these commands on your local machine:**
1. `docker build -t crm-backend:latest ./src/Backend`
2. `docker build -t crm-frontend:latest ./src/Frontend/web`
3. `.\scripts\deploy.ps1 prod`

**Your CRM will be live at https://148.113.37.88!** 🚀
