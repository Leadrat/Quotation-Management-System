# CRM System Deployment Guide

## Overview
This guide covers deploying the CRM system to OVH Cloud using Docker and Docker Compose.

## Prerequisites

### Local Machine Requirements
- Docker Desktop (Windows/Mac) or Docker Engine (Linux)
- Docker Compose
- SSH client
- Git

### OVH Cloud Server Requirements
- Ubuntu Server (recommended)
- SSH access with key-based authentication
- At least 2GB RAM
- At least 10GB storage
- Docker and Docker Compose (auto-installed by deployment script)

## Environment Files

### 1. Backend Environment (.env.prod)
Copy and configure the production environment file:
```bash
cp .env.example .env.prod
```

Update the following values:
- `POSTGRES_CONNECTION`: Database connection string
- `JWT__SECRET`: Secure JWT secret (generate a new one)
- `Cors__FrontendOrigin`: Production frontend URL
- `FRONTEND_URL` and `BACKEND_URL`: Production URLs

### 2. Frontend Environment (.env.production)
Configure frontend production variables:
```bash
# src/Frontend/web/.env.production
NODE_ENV=production
NEXT_PUBLIC_API_URL=https://your-domain.com/api
NEXT_PUBLIC_APP_NAME=CRM System
```

## Deployment Methods

### Method 1: Automated Deployment (Recommended)

#### Using PowerShell (Windows)
```powershell
# Deploy to production
.\scripts\deploy.ps1 prod

# Deploy to staging
.\scripts\deploy.ps1 staging
```

#### Using Bash (Linux/Mac)
```bash
# Make script executable
chmod +x scripts/deploy.sh

# Deploy to production
./scripts/deploy.sh prod

# Deploy to staging
./scripts/deploy.sh staging
```

### Method 2: Manual Deployment

#### Step 1: Build Docker Images
```bash
# Build backend
docker build -t crm-backend:latest ./src/Backend

# Build frontend
docker build -t crm-frontend:latest ./src/Frontend/web
```

#### Step 2: Setup Server
```bash
# Connect to server
ssh ubuntu@148.113.37.88

# Create directories
sudo mkdir -p /opt/crm
sudo mkdir -p /opt/backups/crm

# Install Docker (if not installed)
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh
sudo usermod -aG docker $USER

# Install Docker Compose
sudo curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
sudo chmod +x /usr/local/bin/docker-compose
```

#### Step 3: Deploy Files
```bash
# Copy files to server
scp docker-compose.prod.yml ubuntu@148.113.37.88:/opt/crm/docker-compose.yml
scp -r nginx ubuntu@148.113.37.88:/opt/crm/
scp .env.prod ubuntu@148.113.37.88:/opt/crm/.env
```

#### Step 4: Start Services
```bash
# Connect to server and start services
ssh ubuntu@148.113.37.88
cd /opt/crm
docker-compose down
docker-compose up -d
```

## Local Development

### Using Docker Compose
```bash
# Start all services
docker-compose up -d

# View logs
docker-compose logs -f

# Stop services
docker-compose down
```

### Individual Services
```bash
# Start backend only
docker-compose up -d backend

# Start frontend only
docker-compose up -d frontend
```

## Monitoring and Maintenance

### Check Service Status
```bash
# Connect to server
ssh ubuntu@148.113.37.88
cd /opt/crm

# Check running containers
docker-compose ps

# View logs
docker-compose logs -f

# Check resource usage
docker stats
```

### Health Checks
```bash
# Frontend health
curl http://148.113.37.88/health

# Backend health
curl http://148.113.37.88/api/health
```

### Backup and Restore
```bash
# Backup current deployment
ssh ubuntu@148.113.37.88
cd /opt/crm
docker-compose down
tar -czf /opt/backups/crm/backup_$(date +%Y%m%d_%H%M%S).tar.gz .
docker-compose up -d

# Restore from backup
ssh ubuntu@148.113.37.88
cd /opt/crm
docker-compose down
tar -xzf /opt/backups/crm/backup_YYYYMMDD_HHMMSS.tar.gz .
docker-compose up -d
```

### Updates
```bash
# Update application
.\scripts\deploy.ps1 prod

# Update Docker images only
ssh ubuntu@148.113.37.88
cd /opt/crm
docker-compose pull
docker-compose up -d
```

## SSL Configuration

### Option 1: Let's Encrypt (Recommended)
```bash
# Install Certbot on server
ssh ubuntu@148.113.37.88
sudo apt update
sudo apt install certbot

# Generate SSL certificate
sudo certbot certonly --standalone -d your-domain.com

# Copy certificates to nginx directory
sudo cp /etc/letsencrypt/live/your-domain.com/fullchain.pem /opt/crm/nginx/ssl/cert.pem
sudo cp /etc/letsencrypt/live/your-domain.com/privkey.pem /opt/crm/nginx/ssl/key.pem

# Restart nginx
cd /opt/crm
docker-compose restart nginx-proxy
```

### Option 2: Self-Signed Certificate
```bash
# Generate self-signed certificate
ssh ubuntu@148.113.37.88
cd /opt/crm/nginx/ssl
openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
  -keyout key.pem -out cert.pem

# Restart nginx
cd /opt/crm
docker-compose restart nginx-proxy
```

## Troubleshooting

### Common Issues

#### 1. Container Won't Start
```bash
# Check logs
docker-compose logs backend
docker-compose logs frontend

# Check configuration
docker-compose config
```

#### 2. Database Connection Issues
- Verify database connection string in .env.prod
- Check if database server is accessible from OVH server
- Verify firewall rules

#### 3. Frontend Not Loading
- Check nginx configuration
- Verify API URL in frontend environment
- Check CORS configuration

#### 4. SSH Connection Issues
- Verify SSH key is properly configured
- Check server firewall settings
- Verify user permissions

### Performance Optimization

#### Backend Optimization
```bash
# Monitor resource usage
docker stats crm-backend-prod

# Adjust container limits in docker-compose.prod.yml
deploy:
  resources:
    limits:
      memory: 512M
    reservations:
      memory: 256M
```

#### Frontend Optimization
- Enable gzip compression (configured in nginx)
- Use CDN for static assets
- Implement browser caching

## Security Considerations

1. **Environment Variables**: Never commit sensitive data to version control
2. **SSL**: Always use HTTPS in production
3. **Firewall**: Configure firewall to allow only necessary ports
4. **Updates**: Keep Docker images and dependencies updated
5. **Backups**: Regularly backup application and database
6. **Monitoring**: Monitor logs and resource usage

## Support

For deployment issues:
1. Check the troubleshooting section
2. Review logs using `docker-compose logs`
3. Verify environment configurations
4. Check server resources and connectivity

## Directory Structure

```
CRM/
├── src/
│   ├── Backend/
│   │   └── CRM.Api/
│   │       ├── Dockerfile
│   │       └── ...
│   └── Frontend/
│       └── web/
│           ├── Dockerfile
│           ├── nginx.conf
│           └── ...
├── docker-compose.yml
├── docker-compose.prod.yml
├── nginx/
│   └── nginx.conf
├── scripts/
│   ├── deploy.sh
│   └── deploy.ps1
├── .env.example
├── .env.prod
└── DEPLOYMENT.md
```
