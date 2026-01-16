# System Architecture Documentation

## Table of Contents
1. [Overview](#overview)
2. [Project Structure](#project-structure)
3. [Core Services](#core-services)
4. [Infrastructure](#infrastructure)
5. [API Endpoints](#api-endpoints)

---

## Overview

This is a .NET 10 based boilerplate project with the following features:
- RESTful API with Swagger documentation
- Cassandra database integration
- Redis caching
- RabbitMQ message queue
- JWT authentication
- Email service with queue support
- Docker containerization

## Project Structure

```
├── Source/
│   ├── Configurations/      # Configuration classes
│   ├── Controllers/         # API Controllers
│   ├── DTOs/               # Data Transfer Objects
│   ├── Extensions/         # Service extensions
│   ├── Models/             # Database models
│   ├── Repositories/       # Data access layer
│   └── Services/           # Business logic services
├── Docker/                 # Docker compose files
└── Documentation/          # Project documentation
```

## Core Services

### Services Overview
- **BCrypt Service**: Password hashing and verification
- **JWT Service**: Token generation and validation
- **Redis Service**: Caching operations
- **Cassandra Service**: NoSQL database operations
- **RabbitMQ Service**: Message queue operations
- **Mail Service**: Email sending with queue support
- **Worker Service**: Background job processing
- **Rate Limiting**: API request throttling and DDoS protection

### Architecture Patterns
- Dependency Injection
- Repository Pattern
- Service Layer Pattern
- Background Services (Worker Pattern)

## Infrastructure

### Database
- **Type**: Apache Cassandra (NoSQL)
- **Port**: 9042
- **Features**: Distributed, highly scalable

### Cache
- **Type**: Redis
- **Port**: 6379
- **Usage**: Session storage, temporary data

### Message Queue
- **Type**: RabbitMQ
- **Ports**: 5672 (AMQP), 15672 (Management UI)
- **Usage**: Async task processing, email queue

## API Endpoints

- `/health` - Health check endpoint
- `/api/client` - Client management
- `/api/mail` - Email operations

For detailed API documentation, run the application and visit `/swagger`

## Getting Started

### Quick Start with Makefile

```bash
# Development environment
make dev

# Production environment
make prod
```

See specific documentation files for detailed information:
- [Cassandra Usage](cassandra.md)
- [RabbitMQ Usage](rabbitmq.md)
- [Mail Service](mail-service.md)
- [Redis Cache](redis.md)
- [JWT Authentication](jwt.md)
- [Rate Limiting](rate-limiting.md)
- [Docker Setup](docker.md)
