#!/bin/bash
# Development Startup Script
# This script loads environment variables and starts Docker Compose
# Run from scripts folder: ./start-dev.sh or from root: ./scripts/start-dev.sh

# Get the script directory and root directory
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" &> /dev/null && pwd)"
ROOT_DIR="$(dirname "$SCRIPT_DIR")"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
CYAN='\033[0;36m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

BUILD=false
DETACHED=false
NO_CACHE=false

# Parse arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        --build)
            BUILD=true
            shift
            ;;
        -d|--detached)
            DETACHED=true
            shift
            ;;
        --no-cache)
            NO_CACHE=true
            shift
            ;;
        *)
            echo "Unknown option: $1"
            exit 1
            ;;
    esac
done

echo -e "${CYAN}═══════════════════════════════════════════════════════${NC}"
echo -e "${CYAN}  Training Progress System - Development Startup${NC}"
echo -e "${CYAN}═══════════════════════════════════════════════════════${NC}"
echo ""

# Check if .env.local exists in root directory
ENV_LOCAL_PATH="$ROOT_DIR/.env.local"
if [ ! -f "$ENV_LOCAL_PATH" ]; then
    echo -e "${RED}❌ .env.local file not found in root directory!${NC}"
    echo -e "${CYAN}Please create .env.local with your GROQ_API_KEY:${NC}"
    echo "  1. Copy from .env.example:"
    echo "     cp .env.example .env.local"
    echo "  2. Edit .env.local and add your actual GROQ_API_KEY"
    echo "  3. Run this script again"
    exit 1
fi

# Load environment variables from .env.local
echo -e "${CYAN}📦 Loading environment variables from .env.local...${NC}"
if [ -f "$ENV_LOCAL_PATH" ]; then
    set -a
    source "$ENV_LOCAL_PATH"
    set +a
    echo -e "${GREEN}✓ Environment variables loaded${NC}"
else
    echo -e "${RED}❌ Failed to load environment variables${NC}"
    exit 1
fi
echo ""

# Change to root directory
cd "$ROOT_DIR" || exit 1

# Build docker-compose command
COMPOSE_CMD="docker-compose up"

if [ "$BUILD" = true ]; then
    COMPOSE_CMD="$COMPOSE_CMD --build"
fi

if [ "$DETACHED" = true ]; then
    COMPOSE_CMD="$COMPOSE_CMD -d"
fi

if [ "$NO_CACHE" = true ]; then
    COMPOSE_CMD="$COMPOSE_CMD --no-cache"
fi

echo -e "${CYAN}🐳 Starting Docker Compose...${NC}"
echo -e "${CYAN}   Command: $COMPOSE_CMD${NC}"
echo ""

# Run docker-compose
eval "$COMPOSE_CMD"

if [ $? -eq 0 ]; then
    echo -e "${GREEN}✓ Docker Compose started successfully!${NC}"
    echo ""
    echo -e "${CYAN}📍 Access the application:${NC}"
    echo "   • Frontend:     http://localhost"
    echo "   • BFF API:      http://localhost:5000"
    echo "   • Dashboard:    http://localhost:18888"
    echo ""
    echo -e "${CYAN}📝 Useful commands:${NC}"
    echo "   docker-compose logs -f client      # Frontend logs"
    echo "   docker-compose logs -f bff         # BFF logs"
    echo "   docker-compose ps                  # View all containers"
    echo "   docker-compose down                # Stop all services"
else
    echo -e "${RED}❌ Docker Compose failed to start${NC}"
    exit 1
fi
