rem this should be run from the e:\maps\extent drive 

docker build -t quest-mapserver .

rem run docker using this
rem docker run -d -P -p 8090:80 --name mapserver gluteusmaximus/quest-mapserver

rem you can inspect the maps directory here
rem docker exec -i -t mapserver bash

rem test you get a tile
rem http://localhost:8090/cgi-bin/mapserv?MAP=/maps/extent.map&crs=EPSG:27700&service=WMS&request=GetMap&layers=Stations&styles=&format=image%2Fpng&transparent=false&version=1.1.1&continuousWorld=true&height=256&width=256&srs=EPSG%3A3857&bbox=10854.058016495028,6684112.375425522,11006.932073065382,6684265.249482094

rem  locate the image using...
docker images

docker login -u gluteusmaximus -p siobhan89

rem tag the image with my account
docker tag quest-mapserver gluteusmaximus/quest-mapserver

rem push the image to docker
docker push gluteusmaximus/quest-mapserver