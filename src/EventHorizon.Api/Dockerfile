# .NET API Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY src/EventHorizon.Api/EventHorizon.Api.csproj EventHorizon.Api/
RUN dotnet restore EventHorizon.Api/EventHorizon.Api.csproj
COPY . .
WORKDIR /src/EventHorizon.Api
RUN dotnet build EventHorizon.Api.csproj -c Release -o /app/build

FROM build AS publish
RUN dotnet publish EventHorizon.Api/EventHorizon.Api.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EventHorizon.Api.dll"]
