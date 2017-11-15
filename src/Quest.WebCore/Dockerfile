FROM microsoft/aspnetcore
ARG source
WORKDIR /app
EXPOSE 80
COPY ${source:-bin/Debug/net461/publish} .
ENTRYPOINT ["dotnet", "Quest.WebCore.dll"]

FROM microsoft/aspnetcore:2.0
MAINTAINER GluteusMaximus

ARG source

WORKDIR /app

ADD ${source:-bin/Release/netcoreapp2.0} .

EXPOSE 80
ENTRYPOINT ["dotnet", "Quest.WebCore.dll"]