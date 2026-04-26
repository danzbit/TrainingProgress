# Docker Setup Guide

This guide explains how the application is structured in Docker and how all services communicate with each other.

## Architecture Overview

The application is composed of multiple microservices that run in Docker containers:

```
┌─────────────────────────────────────────────────────────┐
│                   Browser (Host)                         │
│                  http://localhost                        │
└────────────────────┬────────────────────────────────────┘
                     │
                     │ HTTP Request
                     ▼
┌─────────────────────────────────────────────────────────┐
│              Client Container (Nginx)                    │
│  ┌───────────────────────────────────────────────────┐  │
│  │  Port 80:80 (exposed as localhost:80)            │  │
│  │  - Serves React SPA (static files)               │  │
│  │  - Routes: /auth-service, /training-service, etc │  │
│  └───────────────────────────────────────────────────┘  │
└────────────────────┬────────────────────────────────────┘
                     │
        ┌────────────┼────────────┐
        │            │            │
        ▼            ▼            ▼
   ┌────────┐  ┌─────────┐  ┌──────────┐
   │  BFF   │  │ Redis   │  │ Dashboard│
   │ :5000  │  │ :6379   │  │ :18888   │
   └────────┘  └─────────┘  └──────────┘
        │
        │ Internal Docker Network (http://bff:80)
        │
   ┌────┴───┬───────────┬───────────────┬──────────────┐
   │        │           │               │              │
   ▼        ▼           ▼               ▼              ▼
┌──────┐ ┌──────┐ ┌──────────┐ ┌────────────┐ ┌──────────┐
│Train │ │Auth  │ │Notif.    │ │Analytics   │ │ AI Chat  │
│:5001 │ │:5003 │ │:5002     │ │:5004       │ │:5005     │
└──────┘ └──────┘ └──────────┘ └────────────┘ └──────────┘
```

## Environment Configuration

### Build-Time Configuration (Dockerfile)
- **VITE_API_BASE_URL**: Empty string (uses relative URLs)
  - Frontend makes requests to `/auth-service`, `/training-service`, etc.
  - Nginx proxies these requests to the BFF service
  - This approach works for both localhost and production domains

### Runtime Configuration (compose.yaml)
- **CLIENT SERVICE**: Nginx serves the React app on port 80
- **BFF SERVICE**: API gateway on port 5000 (80 internally)
- **Backend Services**: Each service runs on its own port
  - Training Service: 5001
  - Notification Service: 5002
  - Auth Service: 5003
  - Analytics Service: 5004
  - AI Chat Service: 5005

### Service-to-Service Communication
All services communicate via the internal Docker network:
- Format: `http://<service-name>:<internal-port>`
- Examples:
  - `http://bff:80` (from client)
  - `http://training-service:80` (from BFF)
  - `http://redis:6379` (Redis connection string)

## Client Service Details

### Dockerfile
The client Dockerfile uses a multi-stage build:
1. **Build Stage**: Node Alpine image builds the React app with Vite
2. **Runtime Stage**: Nginx Alpine image serves the static files

### nginx.conf
Nginx configuration handles:
- **Static Files**: Serves React SPA from `/usr/share/nginx/html`
- **SPA Routing**: Routes non-file paths to `index.html` for React Router
- **API Proxying**: Proxies API requests to the BFF service:
  - `/auth-service/*` → `http://bff:80/auth-service/*`
  - `/training-service/*` → `http://bff:80/training-service/*`
  - `/analytics-service/*` → `http://bff:80/analytics-service/*`
  - `/notification-service/*` → `http://bff:80/notification-service/*`
  - `/ai-chat-service/*` → `http://bff:80/ai-chat-service/*`
  - `/hubs/*` → `http://bff:80/hubs/*` (WebSocket support)

### Features
- ✅ Automatic SPA routing (client-side routing works)
- ✅ API proxying to BFF service
- ✅ WebSocket support for SignalR hubs
- ✅ Proper HTTP headers forwarding
- ✅ Request/response compression ready

## Running the Application

### Prerequisites
- Docker
- Docker Compose
- GROQ_API_KEY environment variable set (for AI Chat Service)

### Start All Services
```bash
cd TrainingProgressSystem
./scripts/start-dev.ps1 -Build    # Windows
./scripts/start-dev.sh --build    # Linux/macOS
```

Or manually:
```bash
docker-compose up --build
```

### Access the Application
- **Frontend**: http://localhost
- **BFF API**: http://localhost:5000
- **Aspire Dashboard**: http://localhost:18888

## Development vs. Production

### Local Development (without Docker)
- Run React: `npm run dev` (port 3000)
- Set `VITE_API_BASE_URL=http://localhost:5187` in `.env.local`
- Run backend services separately

### Docker Environment
- Client Dockerfile sets `VITE_API_BASE_URL=''` (relative URLs)
- Nginx proxies all API calls to BFF (http://bff:80)
- All services communicate via internal Docker network

### Production Deployment
Update environment variables in compose.yaml or `.env` file:
- `OTEL_EXPORTER_OTLP_ENDPOINT`: Aspire Dashboard endpoint
- `GROQ_API_KEY`: AI service key
- Any service-specific configurations

## Troubleshooting

### Frontend can't connect to API
1. Check if BFF service is running: `docker-compose ps`
2. Check nginx configuration: `docker exec client cat /etc/nginx/nginx.conf`
3. Check BFF logs: `docker-compose logs bff`

### Services can't communicate with each other
- Use internal network names: `http://<service-name>:80`
- Use external ports only from host machine
- Verify all services are on the same network: `docker network inspect <network-name>`

### Port conflicts
If ports are already in use, modify compose.yaml:
```yaml
ports:
  - "8080:80"  # Maps host port 8080 to container port 80
```

### Rebuild client without rebuilding other services
```bash
docker-compose build client
docker-compose up client
```

## Key Points

1. **Client uses relative URLs**: The frontend makes requests to `/api/...` and Nginx proxies them
2. **Internal communication**: Services use Docker network names (e.g., `http://bff:80`)
3. **External access**: Access services via `localhost:<exposed-port>` from your machine
4. **All services on same network**: Enables service discovery and communication
5. **SPA Routing**: Nginx `try_files` ensures React Router works correctly

## References

- [Vite Documentation](https://vitejs.dev/)
- [Docker Compose Documentation](https://docs.docker.com/compose/)
- [Nginx Documentation](https://nginx.org/en/docs/)
