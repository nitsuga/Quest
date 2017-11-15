FROM microsoft/dotnet:2.0-runtime
MAINTAINER GluteusMaximus

ARG source

WORKDIR /app

ADD ${source:-bin/Release/netcoreapp2.0} .

ENTRYPOINT ["dotnet", "Quest.Core.dll"]
