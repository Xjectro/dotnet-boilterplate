PROJECT_INFRA=src/Infrastructure
PROJECT_API=src/Api
OUTPUT_DIR=Data/Migrations

.PHONY: migrate update

migrate-%:
	dotnet ef migrations add $* --project $(PROJECT_INFRA) --startup-project $(PROJECT_API) --output-dir $(OUTPUT_DIR)

update:
	dotnet ef database update --project $(PROJECT_INFRA) --startup-project $(PROJECT_API)

run:
	dotnet run --project src/Api

build:
	dotnet build