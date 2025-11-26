# OVH Cloud Deployment Guide (Direct Cloud Only)

## 🚀 **Direct OVH Cloud Deployment**

This guide covers deploying the CRM system directly to OVH Cloud server `ubuntu@148.113.37.88` without local Docker development.

## 📋 **Prerequisites**

### **Required Tools:**
1. **Docker Desktop** - For building images locally before deployment
2. **SSH Client** - For server access (built into Windows/Mac/Linux)
3. **Git** - For version control

### **Server Requirements:**
- **OVH Cloud Server**: ubuntu@148.113.37.88
- **Ubuntu Server** (any recent version)
- **SSH key-based authentication** configured
- **Minimum 2GB RAM, 10GB storage**

## 🔧 **Quick Deployment Steps**

### **Step 1: Build Docker Images Locally**
```bash
# Build backend API image
docker build -t crm-backend:latest ./src/Backend

# Build frontend web image  
docker build -t crm-frontend:latest ./src/Frontend/web
```

### **Step 2: Deploy to OVH Cloud**
```powershell
# Windows PowerShell
.\scripts\deploy.ps1 prod

# Linux/Mac Bash
./scripts/deploy.sh prod
```

### **Step 3: Access Your Application**
- **Frontend**: https://148.113.37.88
- **Backend API**: https://148.113.37.88/api
- **Health Check**: https://148.113.37.88/health

## 📁 **Files Created for OVH Deployment**

### **Core Docker Files:**
- ✅ `src/Backend/CRM.Api/Dockerfile` - Backend API container
- ✅ `src/Frontend/web/Dockerfile` - Frontend web container
- ✅ `docker-compose.prod.yml` - Production setup only

### **Production Configuration:**
- ✅ `nginx/nginx.conf` - Production reverse proxy
- ✅ `.env.prod` - Production environment variables
- ✅ `src/Frontend/web/.env.production` - Frontend production settings

### **Deployment Automation:**
- ✅ `scripts/deploy.ps1` - Windows PowerShell deployment
- ✅ `scripts/deploy.sh` - Linux/Mac Bash deployment

## 🎯 **What the Deployment Script Does**

### **Automated Process:**
1. **Tests SSH connection** to OVH server
2. **Installs Docker** on server (if needed)
3. **Creates directories** and sets permissions
4. **Builds Docker images** locally on your machine
5. **Backups existing** deployment on server
6. **Deploys files** to OVH server
7. **Starts services** with docker-compose
8. **Runs health checks** for all services
9. **Cleans up old** resources
10. **Generates deployment report**

### **Server Setup:**
- **Docker & Docker Compose** auto-installed
- **Directory structure**: `/opt/crm/`
- **Backup location**: `/opt/backups/crm/`
- **SSL certificates**: `/opt/crm/nginx/ssl/`
- **Log files**: `/opt/crm/logs/nginx/`

## 🌐 **Production URLs**

After deployment, your CRM will be available at:

| Service | URL | Description |
|---------|-----|-------------|
| **Frontend** | https://148.113.37.88 | Main CRM application |
| **Backend API** | https://148.113.37.88/api | REST API endpoints |
| **Health Check** | https://148.113.37.88/health | Service health status |

## 🔐 **Security Features**

- ✅ **Non-root containers** for security
- ✅ **SSL/TLS support** with Let's Encrypt
- ✅ **Rate limiting** (10 req/s, 5 req/min for login)
- ✅ **Security headers** (X-Frame-Options, CSP, etc.)
- ✅ **Health checks** for monitoring
- ✅ **Automatic backups** before updates

## 📊 **Production Features**

- ✅ **Load balancing** with nginx reverse proxy
- ✅ **Gzip compression** for performance
- ✅ **Static asset caching** (1 year for static files)
- ✅ **API proxying** to backend service
- ✅ **Resource limits** (512MB backend, 256MB frontend)
- ✅ **Log management** and rotation
- ✅ **Zero-downtime deployment** capability

## 🛠️ **Management Commands**

### **Check Service Status:**
```bash
# SSH into server
ssh ubuntu@148.113.37.88

# Check running containers
cd /opt/crm
docker-compose ps

# View logs
docker-compose logs -f

# Check resource usage
docker stats
```

### **Update Deployment:**
```bash
# Run deployment script again
.\scripts\deploy.ps1 prod
```

### **Manual Service Management:**
```bash
# Restart services
cd /opt/crm
docker-compose restart

# Stop services
docker-compose down

# Start services
docker-compose up -d
```

## 🔍 **Health Monitoring**

### **Health Check Endpoints:**
```bash
# Frontend health
curl https://148.113.37.88/health

# Backend health
curl https://148.113.37.88/api/health
```

### **Monitoring Commands:**
```bash
# Check container health
docker-compose ps

# View recent logs
docker-compose logs --tail=50

# Monitor resource usage
watch docker stats
```

## 🚨 **Troubleshooting**

### **Common Issues:**

#### **1. SSH Connection Failed:**
```bash
# Test SSH connection
ssh ubuntu@148.113.37.88

# Check SSH key permissions
chmod 600 ~/.ssh/id_rsa
```

#### **2. Docker Build Failed:**
```bash
# Check Docker is running
docker --version
docker info

# Clean build cache
docker system prune -f
```

#### **3. Services Won't Start:**
```bash
# Check logs on server
ssh ubuntu@148.113.37.88
cd /opt/crm
docker-compose logs

# Check configuration
docker-compose config
```

#### **4. Frontend Not Loading:**
```bash
# Check nginx status
docker-compose logs nginx-proxy

# Check nginx configuration
docker-compose exec nginx-proxy nginx -t
```

## 🔄 **Update Process**

### **Deploy Updates:**
1. **Make changes** to your code
2. **Run deployment script**: `.\scripts\deploy.ps1 prod`
3. **Script automatically**:
   - Creates backup of current deployment
   - Builds new Docker images
   - Updates services
   - Runs health checks

### **Rollback:**
```bash
# SSH into server
ssh ubuntu@148.113.37.88

# List available backups
ls -la /opt/backups/crm/

# Restore from backup
cd /opt/crm
docker-compose down
tar -xzf /opt/backups/crm/backup_YYYYMMDD_HHMMSS.tar.gz .
docker-compose up -d
```

## 📞 **Support**

### **Get Help:**
1. **Check logs**: `docker-compose logs -f`
2. **Verify environment**: Check `.env.prod` values
3. **Test connectivity**: Ensure SSH works to server
4. **Check resources**: Verify server has enough memory/disk

### **Manual Deployment:**
If automated script fails, follow manual steps in `DEPLOYMENT.md`

---

## 🎉 **Ready for OVH Cloud Deployment!**

Your CRM system is configured for direct deployment to OVH Cloud:

1. **Build images locally**: `docker build` commands
2. **Deploy to cloud**: `.\scripts\deploy.ps1 prod`
3. **Access application**: https://148.113.37.88

All production optimizations, security features, and monitoring are pre-configured! 🚀
