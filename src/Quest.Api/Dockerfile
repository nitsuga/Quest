FROM microsoft/aspnetcore:2.0
MAINTAINER GluteusMaximus

## visual studio source directory, by default is bin/Debug if not supplied
ARG source

WORKDIR /app

# copy binaries
ADD ${source:-bin/Release/netcoreapp2.0} .

EXPOSE 80
ENTRYPOINT ["dotnet", "Quest.Api.dll"]
