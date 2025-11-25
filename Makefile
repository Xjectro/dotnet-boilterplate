# DotnetBackend Makefile
# Project paths
PROJECT_FILE=Api.csproj
MIGRATIONS_DIR=Source/Data/Migrations
PORT=5000

.PHONY: migrate update run build clean restore docker-build docker-run docker-stop docker-up help

# --------------------------------------------------
# Help
# --------------------------------------------------
help:
	@echo "Available commands:"
	@echo "  build         - Build the project"
	@echo "  clean         - Clean build artifacts"
	@echo "  restore       - Restore NuGet packages"
	@echo "  run           - Run the application locally"
	@echo "  migrate-NAME  - Add new migration (replace NAME with migration name)"
	@echo "  update-db     - Apply pending migrations to database"
	@echo "  docker-up     - Start with docker compose"
	@echo "  docker-build  - Build and start with docker compose"
	@echo "  docker-stop   - Stop docker containers"
	@echo "  help          - Show this help message"

# --------------------------------------------------
# .NET Core Commands
# --------------------------------------------------
restore:
	dotnet restore $(PROJECT_FILE)

clean:
	dotnet clean $(PROJECT_FILE)

build:
	dotnet build $(PROJECT_FILE)

run:
	dotnet run

# --------------------------------------------------
# EF Core Migrations
# --------------------------------------------------
migrate-%:
	dotnet ef migrations add $* --project $(PROJECT_FILE) --output-dir $(MIGRATIONS_DIR)

update-db:
	dotnet ef database update --project $(PROJECT_FILE)

# --------------------------------------------------
# Docker Commands
# --------------------------------------------------
docker-up:
	docker compose up --build

docker-build:
	docker compose up -d --build

docker-stop:
	docker compose down

docker-restart: docker-stop docker-up

# --------------------------------------------------
# Development Commands
# --------------------------------------------------
dev: clean restore build
	@echo "Development build completed successfully!"

deploy-build: clean restore
	dotnet build $(PROJECT_FILE) --configuration Release
	@echo "Release build completed successfully!"