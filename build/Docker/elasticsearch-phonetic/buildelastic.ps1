docker build --tag=elasticsearch-phonetic .
docker login -u gluteusmaximus -p siobhan89
# tag the image with my account
docker tag elasticsearch-phonetic gluteusmaximus/quest-elastic
# push the image to docker
docker push gluteusmaximus/quest-elastic