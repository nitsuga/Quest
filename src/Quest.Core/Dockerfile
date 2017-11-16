FROM microsoft/aspnetcore:2.0
MAINTAINER GluteusMaximus

## visual studio source directory, by default is bin/Debug if not supplied
ARG source

WORKDIR /app

# copy binaries
ADD ${source:-bin\Debug\netcoreapp2.0\publish\} .

ENTRYPOINT ["dotnet", "Quest.Core.dll"]
