FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY src/Services/NotificationService/NotificationService.csproj \
     src/Services/NotificationService/
     
COPY src/BuildingBlocks/BuildingBlocks.Logging/BuildingBlocks.Logging.csproj \
     src/BuildingBlocks/BuildingBlocks.Logging/

RUN dotnet restore src/Services/NotificationService/NotificationService.csproj

COPY . .

RUN dotnet publish \
    src/Services/NotificationService/NotificationService.csproj \
    -c Release \
    -o /app/publish

FROM base AS final
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "NotificationService.dll"]