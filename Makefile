init:
	cp .env.example .env
	docker-compose up -d
	
up:
	docker-compose up -d

build:
	docker-compose up -d --build

down:
	docker-compose down

restart:
	docker-compose restart

dul:
	make down; make build; make logs

logs:
	docker logs -f rs_server_service

logs-db:
	docker logs -f rs_server_db