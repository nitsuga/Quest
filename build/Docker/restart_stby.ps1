docker login -u gluteusmaximus -p siobhan89


docker-compose -H tcp://13.95.220.243:2375 -f docker-compose-stby.yml -p dung_stby down
docker-compose -H tcp://13.95.220.243:2375 -f docker-compose-stby.yml -p dung_stby up -d webproc internalapi jobproc quotes
 