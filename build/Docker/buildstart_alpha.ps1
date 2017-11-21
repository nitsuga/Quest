docker pull gluteusmaximus/quest.database:latest
docker pull gluteusmaximus/quest.mapserver:latest
docker pull gluteusmaximus/quest.api:latest
docker pull gluteusmaximus/quest.core:latest
docker pull gluteusmaximus/quest.web:latest
docker-compose -f docker-compose-alpha.yml -p quest_alpha down
docker-compose -f docker-compose-alpha.yml -p quest_alpha up -d web
