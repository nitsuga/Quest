copy E:\data\questX\QuestServer\Quest.Cmd\bin\Docker\*.dll bin
copy E:\data\questX\QuestServer\Quest.Cmd\bin\Docker\*.exe bin
docker login -u gluteusmaximus -p siobhan89
docker build . -t gluteusmaximus/quest.cmd:latest
pause
docker push gluteusmaximus/quest.cmd:latest
