cd quest-data
docker build . -t gluteusmaximus/quest.database:latest
cd ../quest-mapserver
docker build . -t gluteusmaximus/quest.mapserver:latest
cd ..
docker pull gluteusmaximus/quest.api:latest
docker pull gluteusmaximus/quest.core:latest
docker pull gluteusmaximus/quest.web:latest
docker-compose -f docker-compose-alpha.yml -p quest_alpha down
docker-compose -f docker-compose-alpha.yml -p quest_alpha up -d web
