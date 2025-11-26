#!/bin/bash

# CRM Deployment Script for OVH Cloud
# Usage: ./scripts/deploy.sh [environment]
# Environment: dev, staging, prod (default: prod)

set -e  # Exit on any error

# Configuration
ENVIRONMENT=${1:-prod}
SERVER="ubuntu@148.113.37.88"
PROJECT_NAME="crm"
REMOTE_DIR="/opt/$PROJECT_NAME"
BACKUP_DIR="/opt/backups/$PROJECT_NAME"
TIMESTAMP=$(date +%Y%m%d_%H%M%S)

echo "🚀 Starting CRM deployment to OVH Cloud..."
echo "📋 Environment: $ENVIRONMENT"
echo "🖥️  Server: $SERVER"
echo "📅 Timestamp: $TIMESTAMP"
echo ""

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to print colored output
print_status() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Check if required tools are installed
check_requirements() {
    print_status "Checking requirements..."
    
    if ! command -v ssh &> /dev/null; then
        print_error "SSH is not installed"
        exit 1
    fi
    
    if ! command -v docker &> /dev/null; then
        print_error "Docker is not installed"
        exit 1
    fi
    
    if ! command -v docker-compose &> /dev/null; then
        print_error "Docker Compose is not installed"
        exit 1
    fi
    
    print_success "All requirements are met"
}

# Test SSH connection
test_ssh_connection() {
    print_status "Testing SSH connection to $SERVER..."
    
    if ssh -o ConnectTimeout=10 -o BatchMode=yes $SERVER "echo 'SSH connection successful'" > /dev/null 2>&1; then
        print_success "SSH connection test passed"
    else
        print_error "SSH connection failed. Please check your SSH configuration."
        exit 1
    fi
}

# Prepare environment files
prepare_env_files() {
    print_status "Preparing environment files..."
    
    # Create production environment file if it doesn't exist
    if [ ! -f ".env.prod" ]; then
        print_warning ".env.prod file not found. Creating from .env.example..."
        cp .env.example .env.prod
        print_warning "Please update .env.prod with production values before proceeding."
        read -p "Press Enter to continue after updating .env.prod..."
    fi
    
    # Create frontend production environment file
    if [ ! -f "src/Frontend/web/.env.production" ]; then
        print_warning "Frontend .env.production not found. Creating template..."
        cat > src/Frontend/web/.env.production << EOF
NODE_ENV=production
NEXT_PUBLIC_API_URL=https://your-domain.com/api
NEXT_PUBLIC_APP_NAME=CRM System
EOF
        print_warning "Please update src/Frontend/web/.env.production with production values."
    fi
}

# Build Docker images locally
build_images() {
    print_status "Building Docker images..."
    
    # Build backend
    print_status "Building backend image..."
    docker build -t $PROJECT_NAME-backend:$TIMESTAMP ./src/Backend
    
    # Build frontend
    print_status "Building frontend image..."
    docker build -t $PROJECT_NAME-frontend:$TIMESTAMP ./src/Frontend/web
    
    print_success "Docker images built successfully"
}

# Setup remote server
setup_remote_server() {
    print_status "Setting up remote server..."
    
    ssh $SERVER << EOF
        # Create directories
        sudo mkdir -p $REMOTE_DIR
        sudo mkdir -p $BACKUP_DIR
        sudo mkdir -p $REMOTE_DIR/nginx/ssl
        sudo mkdir -p $REMOTE_DIR/logs/nginx
        
        # Set permissions
        sudo chown -R \$USER:\$USER $REMOTE_DIR
        sudo chown -R \$USER:\$USER $BACKUP_DIR
        
        # Install Docker and Docker Compose if not installed
        if ! command -v docker &> /dev/null; then
            echo "Installing Docker..."
            curl -fsSL https://get.docker.com -o get-docker.sh
            sudo sh get-docker.sh
            sudo usermod -aG docker \$USER
            rm get-docker.sh
        fi
        
        if ! command -v docker-compose &> /dev/null; then
            echo "Installing Docker Compose..."
            sudo curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
            sudo chmod +x /usr/local/bin/docker-compose
        fi
        
        echo "Remote server setup completed"
EOF
    
    print_success "Remote server setup completed"
}

# Backup existing deployment
backup_existing() {
    print_status "Backing up existing deployment..."
    
    ssh $SERVER << EOF
        if [ -d "$REMOTE_DIR/docker-compose.yml" ]; then
            echo "Creating backup..."
            tar -czf $BACKUP_DIR/backup_$TIMESTAMP.tar.gz -C $REMOTE_DIR .
            echo "Backup created: $BACKUP_DIR/backup_$TIMESTAMP.tar.gz"
        else
            echo "No existing deployment to backup"
        fi
EOF
    
    print_success "Backup completed"
}

# Deploy files to server
deploy_files() {
    print_status "Deploying files to server..."
    
    # Create temporary directory for deployment
    TEMP_DIR="/tmp/$PROJECT_NAME-deploy-$TIMESTAMP"
    mkdir -p $TEMP_DIR
    
    # Copy files to temporary directory
    cp docker-compose.prod.yml $TEMP_DIR/docker-compose.yml
    cp -r nginx $TEMP_DIR/
    cp .env.prod $TEMP_DIR/.env
    
    # Copy to remote server
    scp -r $TEMP_DIR/* $SERVER:$REMOTE_DIR/
    
    # Clean up temporary directory
    rm -rf $TEMP_DIR
    
    print_success "Files deployed successfully"
}

# Deploy application
deploy_application() {
    print_status "Deploying application..."
    
    ssh $SERVER << EOF
        cd $REMOTE_DIR
        
        # Stop existing services
        echo "Stopping existing services..."
        docker-compose down || true
        
        # Pull latest images (if using registry)
        # docker-compose pull
        
        # Start services
        echo "Starting services..."
        docker-compose up -d
        
        # Wait for services to be healthy
        echo "Waiting for services to be healthy..."
        sleep 30
        
        # Check status
        echo "Checking service status..."
        docker-compose ps
        
        # Show logs
        echo "Recent logs:"
        docker-compose logs --tail=50
EOF
    
    print_success "Application deployed successfully"
}

# Health check
health_check() {
    print_status "Performing health check..."
    
    # Wait for services to start
    sleep 10
    
    # Check frontend
    if curl -f -s http://148.113.37.88/health > /dev/null; then
        print_success "Frontend health check passed"
    else
        print_error "Frontend health check failed"
    fi
    
    # Check backend
    if curl -f -s http://148.113.37.88/api/health > /dev/null; then
        print_success "Backend health check passed"
    else
        print_warning "Backend health check failed (endpoint may not exist)"
    fi
}

# Cleanup old images and backups
cleanup() {
    print_status "Cleaning up old resources..."
    
    ssh $SERVER << EOF
        cd $REMOTE_DIR
        
        # Remove unused Docker images
        docker image prune -f
        
        # Remove old backups (keep last 5)
        cd $BACKUP_DIR
        ls -t backup_*.tar.gz | tail -n +6 | xargs -r rm
        
        echo "Cleanup completed"
EOF
    
    print_success "Cleanup completed"
}

# Main deployment flow
main() {
    print_status "Starting deployment process..."
    
    check_requirements
    test_ssh_connection
    prepare_env_files
    build_images
    setup_remote_server
    backup_existing
    deploy_files
    deploy_application
    health_check
    cleanup
    
    echo ""
    print_success "🎉 Deployment completed successfully!"
    echo ""
    echo "📊 Deployment Summary:"
    echo "   - Environment: $ENVIRONMENT"
    echo "   - Server: $SERVER"
    echo "   - Timestamp: $TIMESTAMP"
    echo "   - Frontend URL: http://148.113.37.88"
    echo "   - Backend URL: http://148.113.37.88/api"
    echo "   - Backup: $BACKUP_DIR/backup_$TIMESTAMP.tar.gz"
    echo ""
    echo "🔧 To manage the deployment:"
    echo "   ssh $SERVER"
    echo "   cd $REMOTE_DIR"
    echo "   docker-compose logs -f"
    echo "   docker-compose ps"
    echo ""
}

# Run main function
main
