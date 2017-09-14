FROM microsoft/aspnetcore
ARG source
WORKDIR /app
EXPOSE 80
COPY ${source:-bin/Debug/net461/publish} .
ENTRYPOINT ["dotnet", "Quest.WebCore.dll"]
