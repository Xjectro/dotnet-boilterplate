# Docker Setup
## Overview
This project uses Docker and Docker Compose to orchestrate multiple services. All services run in isolated containers and are easy to manage.

## Container Architecture

```
┌─────────────────────────────────────────────────┐
│                   Docker Host                    │
│                                                  │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐      │
│  │   API    │  │  Redis   │  │ ScyllaDB │      │
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
# Docker Setup

## Overview
This project uses Docker and Docker Compose to orchestrate multiple services. All services run in isolated containers and are easy to manage.

## Container Architecture

```
┌────────────────────────────────────────────────────────────┐
│                   Docker Host                             │
│                                                           │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐                 │
│  │   API    │  │  Redis   │  │ ScyllaDB │                │
│  │ (.NET 10)│  │  Cache   │  │ Database │                │
│  │ Port 5143│  │ Port 6379│  │ Port 9042│                │
│  └─────┬────┘  └────┬─────┘  └────┬─────┘                 │
│        │            │             │                        │
│        └────────────┴─────────────┴─────────────┐          │
│                     │                           │          │
│              ┌──────┴──────┐                    │          │
│              │  RabbitMQ   │                    │          │
│              │   Queue     │                    │          │
│              │ Port 5672   │                    │          │
│              │ Mgmt 15672  │                    │          │
│              └─────────────┘                    │          │
└────────────────────────────────────────────────────────────┘
```

## Makefile Commands

The Makefile in the project root simplifies Docker operations:

```makefile
# Start development environment
make dev
# Equivalent: docker compose -f deploy/docker/dev/docker-compose.yml up --build

# Start production environment
make prod
# Equivalent: docker compose -f deploy/docker/prod/docker-compose.yml up -d --build
```

**Benefits:**
- Short and clear commands
- Consistency across environments
- No need to remember file paths
- Automatic build on start

## Files

### deploy/docker/dev/docker-compose.yml
For development environment:
- Hot reload
- Debug settings
- Development credentials
- Local volume mount

### deploy/docker/prod/docker-compose.yml
For production environment:
- Optimized settings
- Production credentials
- Security improvements
- Different replication strategies

### Dockerfile
Application container definition:
- Multi-stage build
- .NET 10 runtime
- Minimal base image

## Services

- **api** – Built from the repository root using `deploy/docker/api/Dockerfile`; exposes port 5143 and wires in all environment variables for development and production profiles.
- **scylla** – Runs `scylladb/scylla:latest` as the Cassandra-compatible datastore with data persisted via the `cassandra_data` volume and cluster metadata configured through environment variables.
- **redis** – Provides caching through `redis:latest`, enables AOF persistence, and mounts `redis_data` for durability.
- **rabbitmq** – Uses `rabbitmq:latest` with the management plugin enabled, publishes AMQP on 5672 and the UI on 15672, and persists queues in `rabbitmq_data`.
- **minio** – Hosts object storage with `minio/minio:latest`, exposes 9000/9001, and stores data in `minio_data`.
- **seq** – Centralized Serilog sink powered by `datalust/seq:latest`, available at http://localhost:5341 with data kept in `seq_data`.

## Running the Application

### Development Environment

```bash
# Using Makefile (Recommended)
make dev

# Manual command
docker compose -f deploy/docker/dev/docker-compose.yml up --build

# View logs
docker compose -f deploy/docker/dev/docker-compose.yml logs -f

# Stop services
docker compose -f deploy/docker/dev/docker-compose.yml down

# Stop and remove volumes
docker compose -f deploy/docker/dev/docker-compose.yml down -v
```

### Production Environment

```bash
# Using Makefile (Recommended)
make prod

# Manual command
docker compose -f deploy/docker/prod/docker-compose.yml up -d --build

# View logs
docker compose -f deploy/docker/prod/docker-compose.yml logs -f api

# Stop
docker compose -f deploy/docker/prod/docker-compose.yml down
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
cd deploy/docker
docker compose -f dev/docker-compose.yml up --build
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

### ScyllaDB
- **Host**: localhost
- **Port**: 9042
- **Connect**: 
  ```bash
  docker exec -it scylla cqlsh
  ```

### Redis
- **Host**: localhost
- **Port**: 6379
- **Connect**:
  ```bash
  docker exec -it redis redis-cli
  ```

## Environment Variables

### Development (deploy/docker/dev/docker-compose.yml)
```yaml
environment:
  DOTNET_ENVIRONMENT: Development
  JwtSettings__Secret: "a19b225eb281d6c5e09d0d329aa67fd1"
  JwtSettings__ExpiryMinutes: "60"
  Redis__Host: "redis:6379"
  Cassandra__ContactPoints: "scylla"
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
docker compose ps
docker inspect --format='{{.State.Health.Status}}' api-dev
```

## Networking

All services are on the same Docker network and can communicate by service name:

```
api → redis:6379
api → scylla:9042
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
docker compose -f deploy/docker/dev/docker-compose.yml logs

# Specific service
docker compose -f deploy/docker/dev/docker-compose.yml logs api

# Follow logs
docker compose -f deploy/docker/dev/docker-compose.yml logs -f api

# Last 100 lines
docker compose -f deploy/docker/dev/docker-compose.yml logs --tail=100 api
```

### Access Container Shell
```bash
# API container
docker exec -it api-dev /bin/bash

# ScyllaDB
docker exec -it scylla /bin/bash

# Redis
docker exec -it redis /bin/sh

# RabbitMQ
docker exec -it rabbitmq /bin/bash
```

### Restart Services
```bash
# Restart all
docker compose -f deploy/docker/dev/docker-compose.yml restart

# Restart specific service
docker compose -f deploy/docker/dev/docker-compose.yml restart api
```

### Clean Everything
```bash
# Stop and remove containers, networks
docker compose -f deploy/docker/dev/docker-compose.yml down

# Also remove volumes
docker compose -f deploy/docker/dev/docker-compose.yml down -v

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
