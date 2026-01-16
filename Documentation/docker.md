# Docker Setup

## Overview
This project uses Docker for containerization and Docker Compose for orchestrating multiple services.

## Container Architecture

```
┌─────────────────────────────────────────────────┐
│                   Docker Host                    │
│                                                  │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐      │
│  │   API    │  │  Redis   │  │Cassandra │      │
│  │ (NET 10) │  │  Cache   │  │ Database │      │
│  │ Port 5143│  │ Port 6379│  │ Port 9042│      │
│  └─────┬────┘  └────┬─────┘  └────┬─────┘      │
│        │            │             │             │
│        └────────────┴─────────────┘             │
│                     │                           │
│              ┌──────┴──────┐                    │
│              │  RabbitMQ   │                    │
│              │   Queue     │                    │
│              │ Port 5672   │                    │
│              │ Mgmt 15672  │                    │
│              └─────────────┘                    │
└─────────────────────────────────────────────────┘
```

## Makefile Commands

The project includes a Makefile for simplified Docker operations:

```makefile
# Start development environment
make dev
# Equivalent to: docker compose -f Docker/docker-compose.dev.yml up --build

# Start production environment
make prod
# Equivalent to: docker compose -f Docker/docker-compose.prod.yml up -d --build
```

**Benefits:**
- Shorter commands
- Consistent across environments
- No need to remember file paths
- Automatic build on start

## Files

### docker-compose.dev.yml
Development environment configuration with:
- Hot reload support
- Debug settings
- Development credentials
- Local volume mounts

### docker-compose.prod.yml
Production environment configuration with:
- Optimized settings
- Production credentials
- Security hardening
- Different replication strategies

### Dockerfile
Application container definition:
- Multi-stage build
- .NET 10 runtime
- Minimal base image

## Services

### 1. API Service
```yaml
api:
  build: ..
  container_name: api-dev
  ports:
    - "5143:5143"
  environment:
    - DOTNET_ENVIRONMENT=Development
  depends_on:
    - redis
    - cassandra
    - rabbitmq
```

**Features:**
- Automatic restart
- Health checks
- Depends on infrastructure services
- Environment-specific configuration

### 2. Cassandra
```yaml
cassandra:
  image: cassandra:4.1
  ports:
    - "9042:9042"
  volumes:
    - cassandra_data:/var/lib/cassandra
```

**Features:**
- Data persistence
- Health checks
- Cluster configuration

### 3. Redis
```yaml
redis:
  image: redis:7.2
  ports:
    - "6379:6379"
  command: ["redis-server", "--appendonly", "yes"]
  volumes:
    - redis_data:/data
```

**Features:**
- AOF persistence
- Data volume
- Health checks

### 4. RabbitMQ
```yaml
rabbitmq:
  image: rabbitmq:3.13-management
  ports:
    - "5672:5672"   # AMQP
    - "15672:15672" # Management UI
  environment:
    - RABBITMQ_DEFAULT_USER=admin
    - RABBITMQ_DEFAULT_PASS=admin123
```

**Features:**
- Management UI enabled
- Default credentials
- Data persistence
- Health checks

## Running the Application

### Development Environment

```bash
# Using Makefile (Recommended)
make dev

# Manual command
docker compose -f Docker/docker-compose.dev.yml up --build

# View logs
docker compose -f Docker/docker-compose.dev.yml logs -f

# Stop services
docker compose -f Docker/docker-compose.dev.yml down

# Stop and remove volumes
docker compose -f Docker/docker-compose.dev.yml down -v
```

### Production Environment

```bash
# Using Makefile (Recommended)
make prod

# Manual command
docker compose -f Docker/docker-compose.prod.yml up -d --build

# View logs
docker compose -f Docker/docker-compose.prod.yml logs -f api

# Stop
docker compose -f Docker/docker-compose.prod.yml down
```

## Building the Application

### Build API Container
```bash
# From project root
docker build -t dotnet-boilerplate-api -f Dockerfile .

# With specific tag
docker build -t dotnet-boilerplate-api:v1.0.0 .
```

### Rebuild After Code Changes
```bash
cd Docker
docker-compose -f docker-compose.dev.yml up --build
```

## Accessing Services

### API
- **URL**: http://localhost:5143
- **Swagger**: http://localhost:5143/swagger
- **Health**: http://localhost:5143/health

### RabbitMQ Management
- **URL**: http://localhost:15672
- **Username**: admin
- **Password**: admin123

### Cassandra
- **Host**: localhost
- **Port**: 9042
- **Connect**: 
  ```bash
  docker exec -it cassandra cqlsh
  ```

### Redis
- **Host**: localhost
- **Port**: 6379
- **Connect**:
  ```bash
  docker exec -it redis redis-cli
  ```

## Environment Variables

### Development (docker-compose.dev.yml)
```yaml
environment:
  DOTNET_ENVIRONMENT: Development
  JwtSettings__Secret: "a19b225eb281d6c5e09d0d329aa67fd1"
  JwtSettings__ExpiryMinutes: "60"
  Redis__Host: "redis:6379"
  Cassandra__ContactPoints: "cassandra"
  Cassandra__Port: "9042"
  Cassandra__Datacenter: "istanbul"
  Cassandra__Keyspace: "default_keyspace"
  RabbitMq__Host: "rabbitmq"
  RabbitMq__Port: "5672"
  Mail__SmtpHost: "smtp.gmail.com"
  Mail__SmtpPort: "587"
```

### Override Values
Create `.env` file:
```env
CASSANDRA_KEYSPACE=my_keyspace
JWT_SECRET=my-super-secret-key
MAIL_SMTP_PASSWORD=my-app-password
```

## Volumes

### Data Persistence
```yaml
volumes:
  cassandra_data:
  redis_data:
  rabbitmq_data:
```

**View volumes:**
```bash
docker volume ls
docker volume inspect docker_cassandra_data
```

**Remove volumes:**
```bash
docker volume rm docker_cassandra_data
docker volume rm docker_redis_data
docker volume rm docker_rabbitmq_data
```

## Health Checks

All services have health checks configured:

### API Health Check
```yaml
healthcheck:
  test: ["CMD-SHELL", "curl -f http://localhost:5143/health || exit 1"]
  interval: 30s
  timeout: 10s
  retries: 5
```

### Check Health Status
```bash
docker-compose ps
docker inspect --format='{{.State.Health.Status}}' api-dev
```

## Networking

All services are on the same Docker network and can communicate by service name:

```
api → redis:6379
api → cassandra:9042
api → rabbitmq:5672
```

**View networks:**
```bash
docker network ls
docker network inspect docker_default
```

## Troubleshooting

### View Container Logs
```bash
# All services
docker-compose -f docker-compose.dev.yml logs

# Specific service
docker-compose -f docker-compose.dev.yml logs api

# Follow logs
docker-compose -f docker-compose.dev.yml logs -f api

# Last 100 lines
docker-compose -f docker-compose.dev.yml logs --tail=100 api
```

### Access Container Shell
```bash
# API container
docker exec -it api-dev /bin/bash

# Cassandra
docker exec -it cassandra /bin/bash

# Redis
docker exec -it redis /bin/sh

# RabbitMQ
docker exec -it rabbitmq /bin/bash
```

### Restart Services
```bash
# Restart all
docker-compose -f docker-compose.dev.yml restart

# Restart specific service
docker-compose -f docker-compose.dev.yml restart api
```

### Clean Everything
```bash
# Stop and remove containers, networks
docker-compose -f docker-compose.dev.yml down

# Also remove volumes
docker-compose -f docker-compose.dev.yml down -v

# Remove all unused Docker resources
docker system prune -a
```

## Performance Optimization

### 1. Multi-stage Dockerfile
```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet build -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Api.dll"]
```

### 2. Resource Limits
```yaml
api:
  deploy:
    resources:
      limits:
        cpus: '1.0'
        memory: 512M
      reservations:
        cpus: '0.5'
        memory: 256M
```

### 3. Build Cache
```bash
# Use build cache
docker-compose build

# No cache
docker-compose build --no-cache
```

## Production Deployment

### Best Practices
1. Use specific image tags (not `latest`)
2. Set resource limits
3. Enable health checks
4. Use secrets management
5. Configure restart policies
6. Set up monitoring
7. Use read-only file systems where possible
8. Run as non-root user

### Security
```yaml
api:
  security_opt:
    - no-new-privileges:true
  read_only: true
  user: "1000:1000"
```

## Monitoring

### Container Stats
```bash
# Real-time stats
docker stats

# Specific container
docker stats api-dev
```

### Inspect Configuration
```bash
docker inspect api-dev
docker-compose -f docker-compose.dev.yml config
```
