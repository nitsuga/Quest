docker login -u gluteusmaximus -p siobhan89
del publish\bin\*owin*
docker build . -t gluteusmaximus/quest.web:latest
pause
docker push gluteusmaximus/quest.web:latest
