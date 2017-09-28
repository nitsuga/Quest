docker login -u gluteusmaximus -p siobhan89

docker-compose -H tcp://13.95.220.243:2375 -f docker-compose-live.yml -p dung_live down
docker-compose -H tcp://13.95.220.243:2375 -f docker-compose-live.yml -p dung_live up -d webproc internalapi
