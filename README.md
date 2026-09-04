# Authentication Service

A robust, enterprise-grade microservice for user authentication and authorization built with **.NET 10**, featuring multi-layered architecture, comprehensive logging, and full containerization support.

[![License](https://img.shields.io/badge/License-LGPL%202.1-blue.svg)](LICENSE)
![.NET Version](https://img.shields.io/badge/.NET-10.0-512BD4.svg)
![Status](https://img.shields.io/badge/Status-Active-brightgreen.svg)

---

## Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [Features](#features)
- [Prerequisites](#prerequisites)
- [Quick Start](#quick-start)
- [Configuration](#configuration)
- [Project Structure](#project-structure)
- [API Documentation](#api-documentation)
- [Database Migrations](#database-migrations)
- [Docker & Containers](#docker--containers)
- [Kubernetes Deployment](#kubernetes-deployment)
- [Observability](#observability)
- [Development](#development)
- [Contributing](#contributing)
- [License](#license)

---

## Overview

The Authentication Service is a standalone microservice responsible for:

- User registration and account management
- Credential validation and authentication
- JWT token generation and refresh mechanisms
- Password management with secure hashing
- Session management and refresh token lifecycle
- Secure password recovery workflows

This service is designed to be independently deployable, scalable, and easily integrated into microservice architectures while maintaining enterprise-level security standards.

---

## Architecture

The service follows **Clean Architecture** principles with clear separation of concerns:

```
┌─────────────────────────────────────────┐
│      AuthenticationService.API          │
│         (Controllers, Middleware)        │
├─────────────────────────────────────────┤
│    AuthenticationService.Application    │
│    (Use Cases, Business Logic)          │
├─────────────────────────────────────────┤
│     AuthenticationService.Domain        │
│      (Entities, Core Concepts)          │
├─────────────────────────────────────────┤
│   AuthenticationService.Infrastructure  │
│  (Database, External Services)          │
└─────────────────────────────────────────┘
```

### Layered Components

| Layer              | Purpose        | Responsibilities                                              |
| ------------------ | -------------- | ------------------------------------------------------------- |
| **API Layer**      | Presentation   | HTTP endpoints, request validation, response formatting       |
| **Application**    | Business Logic | Use cases, DTOs, service orchestration                        |
| **Domain**         | Core Entities  | Business rules, domain models, interfaces                     |
| **Infrastructure** | Data Access    | Database context, repositories, external service integrations |

---

## Features

### Authentication & Authorization

- ✅ User registration with email validation
- ✅ Secure login with password verification
- ✅ JWT token generation and validation
- ✅ Refresh token mechanism with rotation
- ✅ Password hashing using industry-standard algorithms
- ✅ Multi-factor security ready

### User Management

- ✅ Forgot password workflow
- ✅ Password reset functionality
- ✅ User profile management
- ✅ Account status tracking

### Infrastructure & Deployment

- ✅ PostgreSQL persistence layer
- ✅ Dragonfly (Redis) caching
- ✅ Docker containerization
- ✅ Kubernetes orchestration (HPA, ConfigMap, Secrets)
- ✅ Microservice-ready design

### Observability & Monitoring

- ✅ Structured logging with Serilog
- ✅ JSON-formatted log output
- ✅ ELK Stack integration (Elasticsearch, Logstash, Kibana)
- ✅ Elastic Agent monitoring
- ✅ Request/Response HTTP logging

### Development Experience

- ✅ REST API with .http file testing
- ✅ Entity Framework Core migrations
- ✅ Dependency injection configuration
- ✅ Environment-based configuration
- ✅ Development and production settings

---

## Prerequisites

### Local Development

- **.NET 10.0 SDK** or later ([Download](https://dotnet.microsoft.com/download))
- **PostgreSQL 17+** ([Download](https://www.postgresql.org/download/))
- **Dragonfly** (optional, for caching)
- **Git** for version control

### Docker & Kubernetes

- **Docker Desktop** or Docker Engine
- **Docker Compose** (included with Docker Desktop)
- **Kubernetes** (Minikube, Docker Desktop K8s, or cloud cluster)
- **kubectl** CLI tool

### Tools

- Visual Studio Code or Visual Studio 2022+
- REST client (Postman, Thunder Client, or VS Code REST Client)

---

## Quick Start

### 1. Clone the Repository

```bash
git clone <repository-url>
cd ms-auth
```

### 2. Configure Environment Variables

Create `.env` file in `src/AuthenticationService.API/`:

```env
POSTGRES_USER=authuser
POSTGRES_PASSWORD=securepassword
POSTGRES_DB=authdb
POSTGRES_HOST=localhost
POSTGRES_PORT=5432
LOG_FILE_PATH=./logs/auth-service
JWT_SECRET=your-secret-key-min-32-characters-long
JWT_EXPIRY=3600
REFRESH_TOKEN_EXPIRY=604800
```

### 3. Restore Dependencies

```bash
dotnet restore
```

### 4. Run Database Migrations

```bash
cd src/AuthenticationService.Infrastructure
dotnet ef database update --startup-project ../AuthenticationService.API
```

Or using the bundled EF Core tool:

```bash
./efbundle
```

### 5. Build the Solution

```bash
dotnet build
```

### 6. Run the Service

```bash
cd src/AuthenticationService.API
dotnet run
```

The service will start at `https://localhost:5001` and `http://localhost:5000`.

### 7. Verify the Service

```bash
curl https://localhost:5001/health
```

---

## Configuration

### Environment Variables

The service uses environment variables for configuration:

| Variable                 | Default             | Description                               |
| ------------------------ | ------------------- | ----------------------------------------- |
| `POSTGRES_HOST`          | localhost           | PostgreSQL server hostname                |
| `POSTGRES_DB`            | authdb              | Database name                             |
| `POSTGRES_USER`          | authuser            | Database username                         |
| `POSTGRES_PASSWORD`      | -                   | Database password (required)              |
| `POSTGRES_PORT`          | 5432                | PostgreSQL port                           |
| `LOG_FILE_PATH`          | ./logs/auth-service | Log file output path                      |
| `JWT_SECRET`             | -                   | Secret key for JWT signing (min 32 chars) |
| `JWT_EXPIRY`             | 3600                | JWT token expiry (seconds)                |
| `REFRESH_TOKEN_EXPIRY`   | 604800              | Refresh token expiry (seconds)            |
| `ASPNETCORE_ENVIRONMENT` | Production          | Execution environment                     |

### appsettings.json

Configure logging levels, CORS, and API-specific settings in `src/AuthenticationService.API/appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### appsettings.Development.json

Development-specific overrides:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information"
    }
  }
}
```

---

## Project Structure

```
ms-auth/
├── src/
│   ├── AuthenticationService.API/          # REST API layer
│   │   ├── Controllers/                     # HTTP endpoints
│   │   ├── Middleware/                      # Request/response processing
│   │   ├── Program.cs                       # Service startup configuration
│   │   ├── appsettings.json                 # Configuration
│   │   ├── AuthenticationService.API.http   # REST client tests
│   │   └── logs/                            # Application logs
│   │
│   ├── AuthenticationService.Application/   # Business logic layer
│   │   ├── Interfaces/                      # Service contracts
│   │   ├── Users/                           # User-related use cases
│   │   │   ├── Login/
│   │   │   ├── Signup/
│   │   │   ├── RefreshToken/
│   │   │   └── ForgotPassword/
│   │   └── ServiceCollectionExtensions.cs  # DI configuration
│   │
│   ├── AuthenticationService.Domain/        # Core entities & rules
│   │   ├── Entities/                        # Domain models
│   │   └── Repositories/                    # Repository interfaces
│   │
│   └── AuthenticationService.Infrastructure/ # Data access layer
│       ├── Persistence/                     # Database context
│       ├── Repositories/                    # Repository implementations
│       ├── Services/                        # External service integrations
│       └── ServiceCollectionExtensions.cs  # DI configuration
│
├── k8s/                                     # Kubernetes manifests
│   ├── namespace.yaml
│   ├── configmap.yaml
│   ├── secret.yaml
│   ├── service-deployment.yaml
│   ├── service-service.yaml
│   ├── service-hpa.yaml
│   ├── postgres.yaml
│   ├── dragonfly.yaml
│   ├── migration-job.yaml
│   └── observability/                       # ELK Stack manifests
│
├── observability/                           # Observability configuration
│   ├── docker-compose.yml
│   └── elastic-agent/
│
├── helps/                                   # Documentation helpers
│   ├── docker.txt
│   ├── migration.txt
│   └── minikube.txt
│
├── AuthenticationService.slnx               # Solution file
├── Dockerfile                               # Multi-stage build
├── docker-compose.yml                       # Local development environment
├── LICENSE                                  # LGPL 2.1
└── README.md                                # This file
```

---

## API Documentation

### Authentication Endpoints

#### Register New User

```http
POST /api/auth/signup
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "SecurePassword123!",
  "firstName": "John",
  "lastName": "Doe"
}
```

#### Login

```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "SecurePassword123!"
}
```

**Response:**

```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "eyJhbGciOiJIUzI1NiIs...",
  "expiresIn": 3600,
  "user": {
    "id": "uuid",
    "email": "user@example.com",
    "firstName": "John",
    "lastName": "Doe"
  }
}
```

#### Refresh Token

```http
POST /api/auth/refresh-token
Content-Type: application/json

{
  "refreshToken": "eyJhbGciOiJIUzI1NiIs..."
}
```

#### Forgot Password

```http
POST /api/auth/forgot-password
Content-Type: application/json

{
  "email": "user@example.com"
}
```

#### Reset Password

```http
POST /api/auth/reset-password
Content-Type: application/json

{
  "token": "reset-token",
  "newPassword": "NewSecurePassword123!"
}
```

### Error Response Format

```json
{
  "error": "Unauthorized",
  "message": "Invalid credentials",
  "statusCode": 401,
  "timestamp": "2024-01-15T10:30:45Z"
}
```

For detailed API documentation, see [AuthenticationService.API.http](src/AuthenticationService.API/AuthenticationService.API.http)

---

## Database Migrations

### Entity Framework Core Setup

Migrations are managed via Entity Framework Core and are automatically applied on service startup.

### Create a New Migration

```bash
cd src/AuthenticationService.Infrastructure
dotnet ef migrations add MigrationName --startup-project ../AuthenticationService.API
```

### Apply Migrations

```bash
cd src/AuthenticationService.API
dotnet ef database update
```

### Revert Last Migration

```bash
cd src/AuthenticationService.API
dotnet ef migrations remove
```

### Generate EF Bundle (Self-contained executable)

```bash
dotnet ef migrations bundle \
  --project src/AuthenticationService.Infrastructure \
  --startup-project src/AuthenticationService.API \
  --self-contained \
  --target-runtime linux-x64 \
  --output ./efbundle
```

---

## Docker & Containers

### Local Development with Docker Compose

#### Start Services

```bash
docker-compose up -d
```

This starts:

- **ms-auth** service (Port 8080)
- **PostgreSQL 17** (Port 5432)
- **Dragonfly** (Redis) (Port 6379)

#### View Logs

```bash
docker-compose logs -f ms-auth
```

#### Stop Services

```bash
docker-compose down
```

#### Reset Database

```bash
docker-compose down -v
docker-compose up -d
```

### Building Docker Image

```bash
docker build -t ms-auth:latest .
```

### Docker Image Details

The Dockerfile uses a **multi-stage build** pattern:

1. **Build Stage**: Uses `mcr.microsoft.com/dotnet/sdk:10.0-preview`
   - Restores dependencies
   - Builds the solution
   - Publishes release artifacts
   - Generates EF Core migration bundle

2. **Runtime Stage**: Uses `mcr.microsoft.com/dotnet/aspnet:10.0-preview`
   - Minimal image size
   - Only runtime dependencies
   - Ready for production deployment

### Environment File for Docker

Create `.env` or reference in docker-compose.yml:

```env
POSTGRES_USER=authuser
POSTGRES_PASSWORD=authpassword
POSTGRES_DB=authdb
POSTGRES_HOST=postgres
POSTGRES_PORT=5432
```

---

## Kubernetes Deployment

### Prerequisites

- Kubernetes cluster (1.24+)
- `kubectl` configured
- Docker image pushed to registry

### Quick Deploy

```bash
# Create namespace
kubectl apply -f k8s/namespace.yaml

# Apply required secrets (must be created before deploying resources)
kubectl apply -f k8s/secret.yaml -n auth-service

# Deploy all resources
kubectl apply -f k8s/

# Watch deployment
kubectl get pods -n auth-service -w
```

### Secrets

The `k8s/secret.yaml` file should contain the sensitive configuration values required by the service (database password, JWT signing secret, etc.). Keep secrets out of source control — prefer sealed secrets or a secret manager in production.

Minimal keys the service expects (adjust names to match your deployment):

- `POSTGRES_PASSWORD` — database password used by the PostgreSQL deployment
- `JWT_SECRET` — secret used to sign JWT tokens (minimum 32 characters)
- `JWT_ISSUER` (optional) — issuer claim for tokens
- `JWT_AUDIENCE` (optional) — audience claim for tokens

Example `k8s/secret.yaml`:

```yaml
apiVersion: v1
kind: Secret
metadata:
  name: auth-service-secret
  namespace: auth-service
type: Opaque
data:
  POSTGRES_PASSWORD: <base64-encoded-password>
  JWT_SECRET: <base64-encoded-jwt-secret>
  JWT_ISSUER: <base64-encoded-issuer>
  JWT_AUDIENCE: <base64-encoded-audience>
```

Create a secret locally with `kubectl` (avoids committing secrets):

```bash
kubectl create secret generic auth-service-secret -n auth-service \
  --from-literal=POSTGRES_PASSWORD='supersecret' \
  --from-literal=JWT_SECRET='your-secret-key-min-32-chars'
```

Recommendations:

- Do not commit real secrets to the repository. Replace the `data:` values with base64 placeholders if you must keep a template in `k8s/`.
- For production, use sealed-secrets, External Secrets Operator, or your cloud provider's Secret Manager.

### Kubernetes Resources

| Resource    | File                    | Purpose                            |
| ----------- | ----------------------- | ---------------------------------- |
| Namespace   | namespace.yaml          | Isolated environment               |
| ConfigMap   | configmap.yaml          | Non-sensitive configuration        |
| Secret      | secret.yaml             | Sensitive data (tokens, passwords) |
| Deployment  | service-deployment.yaml | Service replicas                   |
| Service     | service-service.yaml    | Internal/External access           |
| HPA         | service-hpa.yaml        | Horizontal Pod Autoscaling         |
| StatefulSet | postgres.yaml           | PostgreSQL persistence             |
| Deployment  | dragonfly.yaml          | Dragonfly (Redis) cache            |
| Job         | migration-job.yaml      | Database migrations pre-deployment |

### Minikube Setup

```bash
# Start Minikube
minikube start

# Build image in Minikube
eval $(minikube docker-env)
docker build -t ms-auth:latest .

# Deploy
kubectl apply -f k8s/

# Access service
minikube service ms-auth -n auth-service
```

See [helps/minikube.txt](helps/minikube.txt) for detailed instructions.

### Scaling

```bash
# Manual scaling
kubectl scale deployment ms-auth --replicas=3 -n auth-service

# Check HPA status
kubectl get hpa -n auth-service
```

---

## Observability

### Structured Logging with Serilog

Logs are emitted in **compact JSON format** for easy parsing:

```json
{
  "MessageTemplate": "User login attempt",
  "@t": "2024-01-15T10:30:45.1234567Z",
  "@l": "Information",
  "@x": null,
  "UserId": "12345",
  "Email": "user@example.com"
}
```

### Log Files

Logs are written to:

- **Console**: Real-time structured output
- **File** (Dev): `src/AuthenticationService.API/logs/auth-service-{date}.json`

### ELK Stack Integration

The service integrates with **Elasticsearch, Logstash, and Kibana**:

#### Start ELK Stack

```bash
cd observability
docker-compose up -d
```

Access:

- **Kibana**: http://localhost:5601
- **Elasticsearch**: http://localhost:9200

#### View Logs in Kibana

1. Open http://localhost:5601
2. Create index pattern: `auth-service-*`
3. Navigate to Discover tab
4. Filter and analyze logs

### Elastic Agent

The service includes Elastic Agent configuration for:

- Application performance monitoring
- Log aggregation
- Metrics collection
- Security monitoring

Configure in `observability/elastic-agent/elastic-agent.yml`

---

## Development

### Prerequisites

```bash
# Install .NET 10 SDK
dotnet --version

# Verify PostgreSQL
psql --version

# Clone and restore
git clone <repository-url>
cd ms-auth
dotnet restore
```

### Build & Run

```bash
# Build solution
dotnet build

# Run service
dotnet run --project src/AuthenticationService.API

# Run with watch (auto-reload)
dotnet watch --project src/AuthenticationService.API run
```

### Testing Endpoints

Use the included REST client file:

```bash
# Open in VS Code
code src/AuthenticationService.API/AuthenticationService.API.http
```

Or use curl:

```bash
# Test health endpoint
curl https://localhost:5001/health

# Test login
curl -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com","password":"password123"}'
```

### Debugging

#### Visual Studio Code

1. Install C# extension
2. Press `F5` to start debugging
3. Set breakpoints in code

#### Visual Studio 2022

1. Open solution: `AuthenticationService.slnx`
2. Press `F5` to start debugging
3. Use Debug → Windows → Exception Settings

### Code Quality

```bash
# Run static analysis
dotnet build /p:TreatWarningsAsErrors=true

# Code style
dotnet format --verify-no-changes

# Security audit
dotnet list package --vulnerable
```

### Hot Reload

```bash
dotnet watch --project src/AuthenticationService.API run
```

---

## Contributing

We welcome contributions! Please follow these guidelines:

1. **Fork the repository**
2. **Create a feature branch**: `git checkout -b feature/your-feature`
3. **Commit changes**: `git commit -m "Add feature description"`
4. **Push to branch**: `git push origin feature/your-feature`
5. **Open a Pull Request**

### Code Standards

- Follow C# naming conventions (PascalCase for classes/methods)
- Write meaningful commit messages
- Add comments for complex logic
- Ensure all tests pass
- Update README for significant changes

### Bug Reports

Please include:

- Reproduction steps
- Expected vs actual behavior
- Environment details (OS, .NET version, etc.)
- Relevant logs

---

## Troubleshooting

### Database Connection Issues

```bash
# Test PostgreSQL connection
psql -h localhost -U authuser -d authdb

# Check environment variables
echo $POSTGRES_HOST
echo $POSTGRES_DB
```

### Migration Errors

```bash
# View pending migrations
dotnet ef migrations list --project src/AuthenticationService.Infrastructure

# See detailed error
dotnet ef database update --verbose --project src/AuthenticationService.API
```

### Docker Issues

```bash
# Rebuild without cache
docker-compose up --build

# Check logs
docker-compose logs postgres
docker-compose logs ms-auth

# Verify port availability
netstat -tulpn | grep :8080
```

### Common Errors

| Error                           | Solution                                                 |
| ------------------------------- | -------------------------------------------------------- |
| "Unable to connect to database" | Verify PostgreSQL is running and credentials are correct |
| "JWT_SECRET not configured"     | Set environment variable with min 32 characters          |
| "Port 8080 already in use"      | Stop other services or use `docker-compose up -p`        |
| "EF Migration not found"        | Run `dotnet ef migrations add` with correct project path |

---

## Performance & Security

### Security Best Practices

- ✅ Passwords hashed using industry-standard algorithms
- ✅ JWT tokens with expiration
- ✅ Refresh token rotation
- ✅ SQL injection prevention via EF Core parameterized queries
- ✅ HTTPS enforced in production
- ✅ Secure environment variable handling
- ✅ CORS properly configured

### Performance Considerations

- **Caching**: Dragonfly for session caching
- **Async Operations**: Non-blocking API calls
- **Connection Pooling**: PostgreSQL connection management
- **Logging**: Minimal overhead with structured logging
- **Horizontal Scaling**: Stateless design for Kubernetes HPA

---

## License

This project is licensed under the **GNU Lesser General Public License v2.1** - see the [LICENSE](LICENSE) file for details.

---

## Support & Resources

### Documentation

- [.NET 10 Documentation](https://learn.microsoft.com/en-us/dotnet/)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)
- [Kubernetes Documentation](https://kubernetes.io/docs/)
- [Serilog Wiki](https://github.com/serilog/serilog/wiki)

### Additional Help

- See [helps/docker.txt](helps/docker.txt) for Docker troubleshooting
- See [helps/migration.txt](helps/migration.txt) for database migration details
- See [helps/minikube.txt](helps/minikube.txt) for Kubernetes setup

---

## Contact & Contribution

For questions, issues, or suggestions, please open a GitHub issue or contact the development team.

**Last Updated**: January 2026  
**Version**: 1.0.0
