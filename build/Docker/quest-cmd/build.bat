del bin\*.*
copy ..\..\..\src\Quest.Cmd\bin\Docker\*.* bin
pause
docker build . -t gluteusmaximus/quest.cmd:latest
pause
docker push gluteusmaximus/quest.cmd:latest
