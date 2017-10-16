docker login -u gluteusmaximus -p siobhan89
docker pull microsoft/mssql-server-linux:2017-GA
docker build . -t gluteusmaximus/quest-database:latest
pause
docker push gluteusmaximus/quest-database:latest
