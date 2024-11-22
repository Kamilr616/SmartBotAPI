
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["SmartBotBlazorApp/SmartBotBlazorApp.csproj", "SmartBotBlazorApp/"]
COPY ["SmartBotBlazorApp.Client/SmartBotBlazorApp.Client.csproj", "SmartBotBlazorApp.Client/"]

RUN dotnet restore "SmartBotBlazorApp/SmartBotBlazorApp.csproj"
COPY . .
RUN dotnet build "SmartBotBlazorApp/SmartBotBlazorApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SmartBotBlazorApp/SmartBotBlazorApp.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SmartBotBlazorApp.dll"]
