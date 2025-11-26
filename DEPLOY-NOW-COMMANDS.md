# 🚀 Deploy Now - Execute These Commands

## **Step 1: Build Docker Images (Run Locally)**
```powershell
cd c:\Users\Public\CRM

# Build backend
docker build -t crm-backend:latest ./src/Backend

# Build frontend
docker build -t crm-frontend:latest ./src/Frontend/web

# Verify images
docker images | findstr crm
```

## **Step 2: Setup Server (SSH Commands)**
```powershell
# SSH into server
ssh -i "C:\Users\Pankaj Joshi\ovh" ubuntu@148.113.37.88

# Run these commands ON THE SERVER:
sudo mkdir -p /opt/crm
sudo mkdir -p /opt/backups/crm
sudo mkdir -p /opt/crm/nginx/ssl
sudo mkdir -p /opt/crm/logs/nginx

sudo chown -R $USER:$USER /opt/crm
sudo chown -R $USER:$USER /opt/backups/crm

# Install Docker if needed
if ! command -v docker &> /dev/null; then
    curl -fsSL https://get.docker.com -o get-docker.sh
    sudo sh get-docker.sh
    sudo usermod -aG docker $USER
    rm get-docker.sh
fi

# Install Docker Compose if needed
if ! command -v docker-compose &> /dev/null; then
    sudo curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-linux-x86_64" -o /usr/local/bin/docker-compose
    sudo chmod +x /usr/local/bin/docker-compose
fi

# Verify installation
docker --version
docker-compose --version

# Exit server when done
exit
```

## **Step 3: Copy Files to Server**
```powershell
# From your LOCAL machine:
scp -i "C:\Users\Pankaj Joshi\ovh" docker-compose.prod.yml ubuntu@148.113.37.88:/opt/crm/docker-compose.yml
scp -i "C:\Users\Pankaj Joshi\ovh" .env.prod ubuntu@148.113.37.88:/opt/crm/.env
scp -i "C:\Users\Pankaj Joshi\ovh" -r nginx ubuntu@148.113.37.88:/opt/crm/
scp -i "C:\Users\Pankaj Joshi\ovh" -r src ubuntu@148.113.37.88:/tmp/crm-build/
```

## **Step 4: Build Images on Server**
```powershell
# SSH back into server
ssh -i "C:\Users\Pankaj Joshi\ovh" ubuntu@148.113.37.88

# Build images ON THE SERVER:
cd /tmp/crm-build
docker build -t crm-backend:latest ./src/Backend
docker build -t crm-frontend:latest ./src/Frontend/web

# Verify images
docker images | grep crm
```

## **Step 5: Start Services**
```powershell
# Still ON THE SERVER:
cd /opt/crm

# Stop existing services (if any)
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

## **Step 6: Test Deployment**
```powershell
# From your LOCAL machine, test access:
curl http://148.113.37.88/health
curl http://148.113.37.88/api/health

# Or open in browser:
# http://148.113.37.88
# http://148.113.37.88/api
```

## **🎯 Quick Copy-Paste Commands:**

### **LOCAL MACHINE (Step 1):**
```powershell
docker build -t crm-backend:latest ./src/Backend
docker build -t crm-frontend:latest ./src/Frontend/web
```

### **SERVER (Step 2):**
```bash
ssh -i "C:\Users\Pankaj Joshi\ovh" ubuntu@148.113.37.88
sudo mkdir -p /opt/crm /opt/backups/crm /opt/crm/nginx/ssl /opt/crm/logs/nginx
sudo chown -R $USER:$USER /opt/crm /opt/backups/crm
# Install Docker commands if needed...
exit
```

### **LOCAL MACHINE (Step 3):**
```powershell
scp -i "C:\Users\Pankaj Joshi\ovh" docker-compose.prod.yml ubuntu@148.113.37.88:/opt/crm/docker-compose.yml
scp -i "C:\Users\Pankaj Joshi\ovh" .env.prod ubuntu@148.113.37.88:/opt/crm/.env
scp -i "C:\Users\Pankaj Joshi\ovh" -r nginx ubuntu@148.113.37.88:/opt/crm/
scp -i "C:\Users\Pankaj Joshi\ovh" -r src ubuntu@148.113.37.88:/tmp/crm-build/
```

### **SERVER (Steps 4-5):**
```bash
ssh -i "C:\Users\Pankaj Joshi\ovh" ubuntu@148.113.37.88
cd /tmp/crm-build
docker build -t crm-backend:latest ./src/Backend
docker build -t crm-frontend:latest ./src/Frontend/web
cd /opt/crm
docker-compose down
docker-compose up -d
docker-compose ps
docker-compose logs -f
```

## **🌐 After Deployment:**
- **CRM Frontend**: http://148.113.37.88
- **Backend API**: http://148.113.37.88/api
- **Health Check**: http://148.113.37.88/health

## **🔧 Management:**
```bash
# SSH to server
ssh -i "C:\Users\Pankaj Joshi\ovh" ubuntu@148.113.37.88

# Check services
cd /opt/crm
docker-compose ps
docker-compose logs -f

# Restart services
docker-compose restart
```

---

**🚀 START WITH STEP 1 NOW - Build the Docker images!**
