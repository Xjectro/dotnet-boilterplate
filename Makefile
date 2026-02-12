dev:
	docker compose -f deploy/docker/dev/docker-compose.yml up --build

prod:
	docker compose -f deploy/docker/prod/docker-compose.yml up -d --build

test:
	dotnet test

format:
	dotnet format