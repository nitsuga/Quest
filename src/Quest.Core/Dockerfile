FROM microsoft/dotnet:2.0-runtime
MAINTAINER GluteusMaximus

ARG source

WORKDIR /app

COPY ${source:-bin/Release/netcoreapp2.0} .

ENTRYPOINT ["dotnet", "Quest.Core.dll"]
