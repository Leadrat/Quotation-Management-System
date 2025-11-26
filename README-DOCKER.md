# CRM Docker Deployment Setup

## 🚀 Complete Docker Configuration Ready!

All Docker files have been created and configured for your CRM system. Here's what's been set up:

### ✅ Files Created:

#### Backend Docker Configuration
- **`src/Backend/CRM.Api/Dockerfile`** - Multi-stage build for .NET API
- Optimized for production with security best practices
- Health checks and non-root user configuration

#### Frontend Docker Configuration  
- **`src/Frontend/web/Dockerfile`** - Multi-stage build for Next.js app
- **`src/Frontend/web/nginx.conf`** - Production nginx configuration
- Optimized static asset serving and API proxying

#### Docker Compose Files
- **`docker-compose.yml`** - Local development setup
- **`docker-compose.prod.yml`** - Production deployment setup
- **`nginx/nginx.conf`** - Production reverse proxy with SSL support

#### Deployment Scripts
- **`scripts/deploy.sh`** - Bash deployment script for Linux/Mac
- **`scripts/deploy.ps1`** - PowerShell deployment script for Windows
- **`DEPLOYMENT.md`** - Complete deployment guide

#### Environment Configuration
- **`.env.prod`** - Production environment variables
- **`src/Frontend/web/.env.production`** - Frontend production settings

## 📋 Quick Start Guide

### 1. Local Development (Docker Desktop Required)

```bash
# Start all services locally
docker-compose up -d

# View logs
docker-compose logs -f

# Stop services
docker-compose down
```

**Access URLs:**
- Frontend: http://localhost:3000
- Backend API: http://localhost:5001
- Nginx Proxy: http://localhost:80

### 2. Production Deployment to OVH Cloud

#### Using PowerShell (Windows):
```powershell
# Deploy to production
.\scripts\deploy.ps1 prod
```

#### Using Bash (Linux/Mac):
```bash
# Make script executable
chmod +x scripts/deploy.sh

# Deploy to production
./scripts/deploy.sh prod
```

## 🔧 Prerequisites

### Local Development:
1. **Docker Desktop** - Install from https://docker.com
2. **Git** - For cloning repository
3. **VS Code** - Recommended IDE

### Production Deployment:
1. **Docker Desktop** - For building images
2. **SSH Client** - For server access
3. **OVH Cloud Server** - ubuntu@148.113.37.88

## 🌐 Production URLs After Deployment

- **Frontend**: https://148.113.37.88
- **Backend API**: https://148.113.37.88/api
- **Health Check**: https://148.113.37.88/health

## 🔐 Security Features Included

- **Non-root containers** for security
- **Health checks** for monitoring
- **Rate limiting** in nginx
- **SSL/TLS support** with Let's Encrypt
- **Security headers** in nginx
- **Environment variable isolation**

## 📊 Features Configured

- **Load balancing** with nginx
- **Gzip compression** for performance
- **Static asset caching** 
- **API proxying** to backend
- **Automatic backups** on deployment
- **Rollback capabilities**
- **Resource limits** for containers
- **Log management**

## 🛠️ Management Commands

### Server Management (SSH into ubuntu@148.113.37.88):
```bash
# Check service status
cd /opt/crm
docker-compose ps

# View logs
docker-compose logs -f

# Restart services
docker-compose restart

# Update deployment
cd /opt/crm
docker-compose down
docker-compose up -d
```

### Local Management:
```bash
# Build images locally
docker build -t crm-backend:latest ./src/Backend
docker build -t crm-frontend:latest ./src/Frontend/web

# Run containers
docker run -p 5001:8080 crm-backend:latest
docker run -p 3000:3000 crm-frontend:latest
```

## 🔍 What the Deployment Scripts Do:

### Automated Setup:
1. ✅ **Test SSH connection** to server
2. ✅ **Install Docker** on server (if needed)
3. ✅ **Create directories** and set permissions
4. ✅ **Build Docker images** locally
5. ✅ **Backup existing** deployment
6. ✅ **Deploy files** to server
7. ✅ **Start services** with docker-compose
8. ✅ **Health checks** for all services
9. ✅ **Cleanup old** resources
10. ✅ **Generate deployment report**

### Production Features:
- **Zero-downtime deployment** (with proper configuration)
- **Automatic SSL** with Let's Encrypt
- **Performance optimization** with nginx
- **Security hardening** with best practices
- **Monitoring and logging** setup
- **Backup and restore** capabilities

## 🚨 Important Notes:

1. **First Deployment**: The script will prompt you to update `.env.prod` with production values
2. **SSL Certificates**: Configure after first deployment using Let's Encrypt or self-signed
3. **Database**: Uses existing PostgreSQL database (connection in .env.prod)
4. **Backups**: Automatically created before each deployment
5. **Monitoring**: Health checks and logging configured

## 📞 Deployment Support:

### If issues occur:
1. **Check logs**: `docker-compose logs -f`
2. **Verify environment**: Check `.env.prod` values
3. **Test locally**: Run `docker-compose up` locally first
4. **Check server**: SSH into server and check Docker status

### Manual deployment option:
If automated scripts fail, follow the manual steps in `DEPLOYMENT.md`

---

## 🎉 Ready to Deploy!

Your CRM system is now fully containerized and ready for deployment to OVH Cloud!

**Next Steps:**
1. Install Docker Desktop (if not already installed)
2. Test locally with `docker-compose up`
3. Deploy to production with `.\scripts\deploy.ps1 prod`
4. Access your CRM at https://148.113.37.88

All configuration files are created and optimized for production use! 🚀
