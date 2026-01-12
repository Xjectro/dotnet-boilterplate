DEV_COMPOSE := docker/docker-compose.dev.yml
PROD_COMPOSE := docker/docker-compose.prod.yml

.PHONY: dev prod up down build logs ps restart

dev:
	docker compose -f $(DEV_COMPOSE) up

prod:
	docker compose -f $(PROD_COMPOSE) up -d --build

down:
	docker compose -f $(DEV_COMPOSE) down

build:
	docker compose -f $(DEV_COMPOSE) build

logs:
	docker compose -f $(DEV_COMPOSE) logs -f

restart:
	docker compose -f $(DEV_COMPOSE) down && \
	docker compose -f $(DEV_COMPOSE) up --build -d
