# Local Development Setup Guide

## Quick Start

### 1. Setup Environment Variables

Copy the example environment file from root:
```powershell
# Windows PowerShell
Copy-Item .env.example .env.local
```

```bash
# Linux/macOS
cp .env.example .env.local
```

### 2. Add Your GROQ API Key

Edit `.env.local` (in root) and replace the placeholder:
```env
GROQ_API_KEY=your_actual_groq_api_key_here
```

Get your key from: https://console.groq.com/

### 3. Start Development Environment

**Windows (PowerShell):**
```powershell
# Simple start (from root)
.\scripts\start-dev.ps1

# With rebuild
.\scripts\start-dev.ps1 -Build

# Detached mode (background)
.\scripts\start-dev.ps1 -Detached
```

**Linux/macOS (Bash):**
```bash
# Simple start (from root)
./scripts/start-dev.sh

# With rebuild
./scripts/start-dev.sh --build

# Detached mode (background)
./scripts/start-dev.sh -d

# Make executable first if needed
chmod +x ./scripts/start-dev.sh
```

**Manual start (any OS):**
```bash
docker-compose up --build
```

## Access Points

Once running, access your application at:

| Service | URL | Purpose |
|---------|-----|---------|
| Frontend | http://localhost | React SPA |
| BFF API | http://localhost:5000 | API Gateway |
| Aspire Dashboard | http://localhost:18888 | Monitoring |

## Environment Files Explained

- **`.env.example`** - Template file (committed to git)
  - Shows all available configuration options
  - Safe to commit
  - Located in root

- **`.env.local`** - Your local secrets (git-ignored)
  - Contains your actual GROQ_API_KEY
  - Never committed to git
  - Created locally only
  - Located in root

- **`.gitignore`** - Prevents committing secrets
  - Automatically ignores `.env.local`
  - Ignores build artifacts, node_modules, etc.
  - Located in root

## Important Notes

⚠️ **Never commit `.env.local` to git!** It contains your API keys.

If you accidentally commit it:
```bash
git rm --cached .env.local
git commit --amend -m "Remove .env.local"
```

## Useful Docker Compose Commands

```bash
# View all running containers
docker-compose ps

# View logs for a specific service
docker-compose logs -f client          # Frontend
docker-compose logs -f bff             # API Gateway
docker-compose logs -f training-service # Training Service

# View logs for all services
docker-compose logs -f

# Stop all services
docker-compose down

# Stop and remove volumes
docker-compose down -v

# Rebuild a specific service
docker-compose build client

# Restart a service
docker-compose restart bff

# Execute command in container
docker-compose exec client sh
```

## Troubleshooting

### "Permission denied" on start-dev.sh

Make it executable:
```bash
chmod +x scripts/start-dev.sh
./scripts/start-dev.sh
```

### Port already in use

Check which process is using the port:
```bash
# Windows PowerShell
Get-NetTCPConnection -LocalPort 80 | Select-Object OwningProcess

# Linux/macOS
lsof -i :80
```

Then either:
1. Stop the process using that port
2. Modify port mapping in `compose.yaml`

### Services can't communicate

1. Verify all services are running:
   ```bash
   docker-compose ps
   ```

2. Check service logs:
   ```bash
   docker-compose logs bff
   ```

3. Check if Docker network exists:
   ```bash
   docker network ls
   docker network inspect internal
   ```

### GROQ API Key not recognized

1. Verify `.env.local` contains your key:
   ```bash
   cat .env.local
   ```

2. Restart the AI Chat Service:
   ```bash
   docker-compose restart ai-chat-service
   ```

3. Check logs:
   ```bash
   docker-compose logs ai-chat-service
   ```

### Script can't find .env.local

The scripts look for `.env.local` in the root directory:
- Run scripts from the root: `.\scripts\start-dev.ps1`
- Ensure `.env.local` is in the project root (same level as compose.yaml)
- Don't run scripts from the scripts folder directly

## Development Workflow

### Add a New Environment Variable

1. Add to `.env.example` (in root):
   ```env
   NEW_VAR=example_value
   ```

2. Add to `.env.local` (in root):
   ```env
   NEW_VAR=your_actual_value
   ```

3. Update `compose.yaml` (in root) to use it:
   ```yaml
   services:
     service-name:
       environment:
         NEW_VAR: ${NEW_VAR}
   ```

### Rebuild After Changes

```bash
# Rebuild client (React/Vite changes)
docker-compose up --build client

# Rebuild specific service
docker-compose build ai-chat-service
docker-compose up ai-chat-service

# Rebuild everything
docker-compose up --build
```

## Next Steps

- Read [docs/DOCKER_SETUP.md](./DOCKER_SETUP.md) for architecture details
- Check individual service READMEs in `TrainingProgressSystem.Server/src/*/`
- Review `compose.yaml` for service configuration
- Check [TrainingProgressSystem.Client/nginx.conf](../TrainingProgressSystem.Client/nginx.conf) for frontend routing

## File Structure

```
TrainingProgressSystem/
├── .env.example                       # ✓ Commit - Template
├── .env.local                         # ✗ Git-ignored - Your secrets
├── .gitignore                         # ✓ Commit - Ignore rules
├── compose.yaml                       # ✓ Commit - Docker config
├── docs/
│   ├── DOCKER_SETUP.md               # ✓ Commit - Architecture guide
│   └── LOCAL_SETUP.md                # ✓ Commit - This file
├── scripts/
│   ├── start-dev.ps1                 # ✓ Commit - Windows script
│   └── start-dev.sh                  # ✓ Commit - Linux/Mac script
├── TrainingProgressSystem.Client/
│   ├── Dockerfile                    # ✓ Commit
│   ├── nginx.conf                    # ✓ Commit
│   └── ...
└── TrainingProgressSystem.Server/
    └── ...
```

## Summary

| What | Where | Commit? | Notes |
|------|-------|---------|-------|
| Environment template | Root `.env.example` | ✓ Yes | Placeholder values |
| Your secrets | Root `.env.local` | ✗ No | Git-ignored, local only |
| Startup scripts | `scripts/` folder | ✓ Yes | Auto-detects root directory |
| Architecture docs | `docs/` folder | ✓ Yes | Guides for Docker setup |
| Config files | Root | ✓ Yes | compose.yaml, .gitignore |
