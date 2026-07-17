FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY src/Services/AuthService/AuthService.csproj \
     src/Services/AuthService/
     
COPY src/BuildingBlocks/BuildingBlocks.Logging/BuildingBlocks.Logging.csproj \
     src/BuildingBlocks/BuildingBlocks.Logging/

RUN dotnet restore src/Services/AuthService/AuthService.csproj

COPY . .

RUN dotnet publish \
    src/Services/AuthService/AuthService.csproj \
    -c Release \
    -o /app/publish

FROM base AS final
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "AuthService.dll"]