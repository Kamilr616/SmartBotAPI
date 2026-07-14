
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0.300 AS build
WORKDIR /src
COPY ["global.json", "/src/global.json"]
COPY ["Directory.Build.props", "/src/Directory.Build.props"]
COPY ["LICENSE", "/LICENSE"]
COPY ["THIRD_PARTY_NOTICES.md", "/THIRD_PARTY_NOTICES.md"]
COPY ["src/server/SmartBotBlazorApp/SmartBotBlazorApp.csproj", "SmartBotBlazorApp/"]
COPY ["src/server/SmartBotBlazorApp/packages.lock.json", "SmartBotBlazorApp/"]
COPY ["src/server/SmartBotBlazorApp.Client/SmartBotBlazorApp.Client.csproj", "SmartBotBlazorApp.Client/"]
COPY ["src/server/SmartBotBlazorApp.Client/packages.lock.json", "SmartBotBlazorApp.Client/"]

RUN dotnet restore "SmartBotBlazorApp/SmartBotBlazorApp.csproj" --locked-mode
COPY ["src/server/", "."]

FROM build AS publish
RUN dotnet publish "SmartBotBlazorApp/SmartBotBlazorApp.csproj" -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
USER 1654
ENTRYPOINT ["dotnet", "SmartBotBlazorApp.dll"]
