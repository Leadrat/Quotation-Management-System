# 🔧 Manual Deployment Guide - OVH Cloud

## **SSH is Working! Let's Deploy Manually**

Since the PowerShell scripts have syntax issues, let's deploy step by step manually.

## **📋 Step-by-Step Deployment:**

### **Step 1: Build Docker Images Locally**
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

### **Step 2: Setup Remote Server**
```bash
# SSH into server
ssh -i "C:\Users\Pankaj Joshi\ovh" ubuntu@148.113.37.88

# Run these commands on the server:
# Create directories
sudo mkdir -p /opt/crm
sudo mkdir -p /opt/backups/crm
sudo mkdir -p /opt/crm/nginx/ssl
sudo mkdir -p /opt/crm/logs/nginx

# Set permissions
sudo chown -R $USER:$USER /opt/crm
sudo chown -R $USER:$USER /opt/backups/crm

# Install Docker (if not installed)
if ! command -v docker &> /dev/null; then
    curl -fsSL https://get.docker.com -o get-docker.sh
    sudo sh get-docker.sh
    sudo usermod -aG docker $USER
    rm get-docker.sh
fi

# Install Docker Compose (if not installed)
if ! command -v docker-compose &> /dev/null; then
    sudo curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-linux-x86_64" -o /usr/local/bin/docker-compose
    sudo chmod +x /usr/local/bin/docker-compose
fi

# Verify installation
docker --version
docker-compose --version

# Exit server
exit
```

### **Step 3: Deploy Files to Server**
```bash
# From your local machine, copy deployment files
scp -i "C:\Users\Pankaj Joshi\ovh" docker-compose.prod.yml ubuntu@148.113.37.88:/opt/crm/docker-compose.yml
scp -i "C:\Users\Pankaj Joshi\ovh" .env.prod ubuntu@148.113.37.88:/opt/crm/.env

# Copy nginx configuration (directory)
scp -i "C:\Users\Pankaj Joshi\ovh" -r nginx ubuntu@148.113.37.88:/opt/crm/
```

### **Step 4: Push Docker Images to Server**
Since we can't easily push to a registry, let's build directly on server:

```bash
# SSH into server
ssh -i "C:\Users\Pankaj Joshi\ovh" ubuntu@148.113.37.88

# Create temporary directory for source code
mkdir -p /tmp/crm-build
cd /tmp/crm-build

# Copy source code from your local machine to server
# (From your local machine, in a NEW terminal window)
scp -i "C:\Users\Pankaj Joshi\ovh" -r src ubuntu@148.113.37.88:/tmp/crm-build/

# Back on the server, build images
cd /tmp/crm-build

# Build backend
docker build -t crm-backend:latest ./src/Backend

# Build frontend
docker build -t crm-frontend:latest ./src/Frontend/web

# Verify images
docker images | grep crm
```

### **Step 5: Start Services**
```bash
# On the server, go to deployment directory
cd /opt/crm

# Stop any existing services
docker-compose down

# Start services
docker-compose up -d

# Check status
docker-compose ps

# Check logs
docker-compose logs -f

# Wait 30 seconds for services to start
sleep 30
```

### **Step 6: Health Check**
```bash
# On the server, check if services are running
docker-compose ps

# Check if ports are accessible
netstat -tlnp | grep :80
netstat -tlnp | grep :8080

# From your local machine, test access
curl http://148.113.37.88/health
curl http://148.113.37.88/api/health
```

## **🚀 Quick Commands Summary:**

### **Local Machine:**
```bash
# 1. Build images
docker build -t crm-backend:latest ./src/Backend
docker build -t crm-frontend:latest ./src/Frontend/web

# 2. Copy files
scp -i "C:\Users\Pankaj Joshi\ovh" docker-compose.prod.yml ubuntu@148.113.37.88:/opt/crm/docker-compose.yml
scp -i "C:\Users\Pankaj Joshi\ovh" .env.prod ubuntu@148.113.37.88:/opt/crm/.env
scp -i "C:\Users\Pankaj Joshi\ovh" -r nginx ubuntu@148.113.37.88:/opt/crm/
scp -i "C:\Users\Pankaj Joshi\ovh" -r src ubuntu@148.113.37.88:/tmp/crm-build/
```

### **On Server:**
```bash
# 3. Setup server
ssh -i "C:\Users\Pankaj Joshi\ovh" ubuntu@148.113.37.88
# (run server setup commands from Step 2)

# 4. Build images on server
cd /tmp/crm-build
docker build -t crm-backend:latest ./src/Backend
docker build -t crm-frontend:latest ./src/Frontend/web

# 5. Start services
cd /opt/crm
docker-compose down
docker-compose up -d

# 6. Check status
docker-compose ps
docker-compose logs -f
```

## **🌐 After Deployment:**

### **Access URLs:**
- **CRM Frontend**: http://148.113.37.88
- **Backend API**: http://148.113.37.88/api
- **Health Check**: http://148.113.37.88/health

### **Management Commands:**
```bash
# SSH into server
ssh -i "C:\Users\Pankaj Joshi\ovh" ubuntu@148.113.37.88

# Check services
cd /opt/crm
docker-compose ps
docker-compose logs -f

# Restart services
docker-compose restart

# Stop services
docker-compose down
```

## **🔍 Troubleshooting:**

### **If Services Won't Start:**
```bash
# Check logs
docker-compose logs backend
docker-compose logs frontend

# Check configuration
docker-compose config

# Check Docker status
docker info
```

### **If Can't Access URLs:**
```bash
# Check if ports are listening
netstat -tlnp | grep :80
netstat -tlnp | grep :8080

# Check firewall
sudo ufw status

# Check nginx logs
docker-compose logs frontend
```

---

## **🎯 Start Deployment Now!**

**Run these commands in order:**

1. **Build images locally**
2. **Setup server** (SSH and run setup commands)
3. **Copy files to server**
4. **Build images on server**
5. **Start services**
6. **Test access**

**Your CRM will be live at http://148.113.37.88!** 🚀
