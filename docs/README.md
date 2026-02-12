# System Architecture Documentation

## Table of Contents
1. [Overview](#overview)
2. [Project Structure](#project-structure)
3. [Core Services](#core-services)
4. [Infrastructure](#infrastructure)
5. [API Endpoints](#api-endpoints)

---

## Overview

This .NET 10 based boilerplate project includes the following features:
- RESTful API (with Swagger/ReDoc documentation)
- ScyllaDB (Cassandra-compatible) integration
- Redis cache
- RabbitMQ message queue
- JWT authentication
- Queue-based mail service
- Docker container management

## Project Structure

```
├── src/
│   ├── core/                # Domain entities and shared contracts
│   ├── application/         # Use cases, validation, and interfaces
│   ├── infrastructure/      # Implementations for databases, messaging, etc.
│   └── presentation/api/    # ASP.NET Core API host and middleware
├── deploy/docker/           # Dockerfiles and compose definitions
├── tests/                   # Automated test projects
├── ops/                     # Pipelines and ops scripts
└── docs/                    # Project documentation
```

## Core Services

### Services
- BCrypt Service: Password hashing and verification
- JWT Service: Token generation and validation
- Redis Service: Cache operations
- Scylla Service: NoSQL database operations
- RabbitMQ Service: Message queue operations
- Mail Service: Queue-based email sending
- Worker Service: Background processing
- Rate Limiting: API request throttling and DDoS protection

### Architecture Patterns
- Dependency Injection
- Repository Pattern
- Service Layer Pattern
- Background Services (Worker Pattern)

## Infrastructure

### Database
- Type: ScyllaDB (Cassandra-compatible NoSQL)
- Port: 9042
- Features: Distributed, highly scalable

### Cache
- Type: Redis
- Port: 6379
- Usage: Session storage, temporary data

### Message Queue
- Type: RabbitMQ
- Ports: 5672 (AMQP), 15672 (Management UI)
- Usage: Asynchronous processing, email queue

## API Endpoints

- `/health` - Health check
- `/api/client` - Client management
- `/api/mail` - Mail operations

For detailed API documentation, run the application and visit `/swagger`.

## Getting Started

### Quick Start with Makefile

```bash
# Development environment
make dev

# Production environment
make prod
```

For more details, see the related documentation files:
- [ScyllaDB Usage](scylla.md)
- [RabbitMQ Usage](rabbitmq.md)
- [Mail Service](mail-service.md)
- [Redis Cache](redis.md)
- [JWT Authentication](jwt.md)
- [Rate Limiting](rate-limiting.md)
- [Docker Setup](docker.md)
