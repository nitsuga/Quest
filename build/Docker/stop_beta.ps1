docker login -u gluteusmaximus -p siobhan89

docker-compose -H tcp://13.95.220.243:2375 -f docker-compose-beta.yml -p dung_beta down --remove-orphans
