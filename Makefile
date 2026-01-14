DEV_COMPOSE := Docker/docker-compose.dev.yml
PROD_COMPOSE := Docker/docker-compose.prod.yml

dev:
	docker compose -f $(DEV_COMPOSE) up --build

prod:
	docker compose -f $(PROD_COMPOSE) up -d --build
