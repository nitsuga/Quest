docker login -u gluteusmaximus -p siobhan89
docker build . -t gluteusmaximus/quest-database:latest
pause
docker push gluteusmaximus/quest-database:latest
