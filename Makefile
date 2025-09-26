PROJECT_ROOT=.
PROJECT_INFRA=src/Infrastructure
PROJECT_API=src/Api
OUTPUT_DIR=Data/Migrations
PORT=5000

.PHONY: migrate update run build docker-build docker-run docker-stop

# --------------------------------------------------
# EF Migrations
# --------------------------------------------------
migrate-%:
	dotnet ef migrations add $* --project $(PROJECT_INFRA) --startup-project $(PROJECT_API) --output-dir $(OUTPUT_DIR)

update:
	dotnet ef database update --project $(PROJECT_INFRA) --startup-project $(PROJECT_API)

# --------------------------------------------------
# Local run & build
# --------------------------------------------------
run:
	dotnet run --project $(PROJECT_API)

build:
	dotnet build $(PROJECT_API)

# --------------------------------------------------
# Docker
# --------------------------------------------------
docker:
	docker compose up