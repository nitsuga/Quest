docker login -u gluteusmaximus -p siobhan89

docker -H tcp://13.95.220.243:2375 pull extent/dungbeetle.processor:latest
docker-compose -H tcp://13.95.220.243:2375 -f docker-compose-alpha.yml -p dungalpha down
docker-compose -H tcp://13.95.220.243:2375 -f docker-compose-alpha.yml -p dungalpha up -d webproc internalapi
