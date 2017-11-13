cd quest-data
docker build . -t gluteusmaximus/quest.database:latest
cd ../quest-mapserver
docker build . -t gluteusmaximus/quest.mapserver:latest
cd ../quest-api
docker build . -t gluteusmaximus/quest.api:latest
cd ../quest-core
docker build . -t gluteusmaximus/quest.core:latest
cd ../quest-web
docker build . -t gluteusmaximus/quest.web:latest
cd ..

