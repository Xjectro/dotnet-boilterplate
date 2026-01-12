DOCKER := docker
DEV_COMPOSE := docker/docker-compose.dev.yml
PROD_COMPOSE := docker/docker-compose.prod.yml

.PHONY: dev prod up down build logs ps restart

dev:
	$(DOCKER) compose -f $(DEV_COMPOSE) up

prod:
	$(DOCKER) compose -f $(PROD_COMPOSE) up -d --build

down:
	$(DOCKER) compose -f $(DEV_COMPOSE) down

build:
	$(DOCKER) compose -f $(DEV_COMPOSE) build

logs:
	$(DOCKER) compose -f $(DEV_COMPOSE) logs -f

restart:
	$(DOCKER) compose -f $(DEV_COMPOSE) down && \
	$(DOCKER) compose -f $(DEV_COMPOSE) up --build -d
