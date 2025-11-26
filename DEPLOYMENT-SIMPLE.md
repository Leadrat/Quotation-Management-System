# 🚀 OVH Cloud Deployment - Simple Guide

## **Quick Start - 3 Commands Only!**

### **1. Build Docker Images**
```bash
docker build -t crm-backend:latest ./src/Backend
docker build -t crm-frontend:latest ./src/Frontend/web
```

### **2. Deploy to OVH Cloud**
```powershell
# Windows
.\scripts\deploy.ps1 prod

# Linux/Mac  
./scripts/deploy.sh prod
```

### **3. Access Your CRM**
- **Frontend**: https://148.113.37.88
- **Backend API**: https://148.113.37.88/api

---

## 📋 **What You Need**

### **Required:**
- ✅ Docker Desktop (for building images)
- ✅ SSH access to ubuntu@148.113.37.88
- ✅ Your CRM code

### **Server Setup (Automatic):**
- Docker & Docker Compose auto-installed
- Directories created automatically
- SSL certificates configured later

---

## 🎯 **What Happens During Deployment**

### **Automated Process:**
1. **Tests SSH** connection to server
2. **Installs Docker** on server (if needed)
3. **Builds images** locally on your machine
4. **Deploys files** to OVH server
5. **Starts services** with docker-compose
6. **Runs health checks**
7. **Creates backup** of old deployment

### **Server Structure:**
```
/opt/crm/                    # Main application directory
├── docker-compose.yml      # Production configuration
├── nginx/                   # SSL certificates
├── logs/                    # Application logs
└── .env                     # Environment variables

/opt/backups/crm/           # Backup directory
```

---

## 🔧 **Management Commands**

### **Check Status:**
```bash
ssh ubuntu@148.113.37.88
cd /opt/crm
docker-compose ps
docker-compose logs -f
```

### **Update Deployment:**
```bash
# Just run the deploy script again
.\scripts\deploy.ps1 prod
```

### **Restart Services:**
```bash
ssh ubuntu@148.113.37.88
cd /opt/crm
docker-compose restart
```

---

## 🔐 **Security Features**

- ✅ **Non-root containers**
- ✅ **SSL/TLS support** (configure after deployment)
- ✅ **Rate limiting** (10 req/s API, 5 req/min login)
- ✅ **Security headers**
- ✅ **Health checks**
- ✅ **Automatic backups**

---

## 🚨 **Troubleshooting**

### **SSH Issues:**
```bash
# Test connection
ssh ubuntu@148.113.37.88

# Check SSH key
chmod 600 ~/.ssh/id_rsa
```

### **Docker Build Issues:**
```bash
# Check Docker is running
docker --version
docker info

# Clean if needed
docker system prune -f
```

### **Service Issues:**
```bash
# Check logs on server
ssh ubuntu@148.113.37.88
cd /opt/crm
docker-compose logs
```

---

## 🌐 **After Deployment**

### **Access URLs:**
| Service | URL |
|---------|-----|
| **CRM Frontend** | https://148.113.37.88 |
| **API Endpoints** | https://148.113.37.88/api |
| **Health Check** | https://148.113.37.88/health |

### **SSL Setup (Optional):**
```bash
# SSH into server
ssh ubuntu@148.113.37.88
cd /opt/crm

# Install SSL certificates
sudo apt install certbot
sudo certbot certonly --standalone -d 148.113.37.88

# Copy to nginx directory
sudo cp /etc/letsencrypt/live/148.113.37.88/fullchain.pem nginx/ssl/cert.pem
sudo cp /etc/letsencrypt/live/148.113.37.88/privkey.pem nginx/ssl/key.pem

# Restart frontend
docker-compose restart frontend
```

---

## 📞 **Need Help?**

1. **Check logs**: `docker-compose logs -f`
2. **Verify SSH**: Test connection to server
3. **Check resources**: Ensure server has enough memory
4. **Review environment**: Check `.env.prod` settings

---

## 🎉 **Ready to Deploy!**

Your CRM system is configured for direct OVH Cloud deployment:

1. **Build images** locally
2. **Run deployment script**
3. **Access your CRM** at https://148.113.37.88

All production optimizations and security features are pre-configured! 🚀
