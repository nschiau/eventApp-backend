# Stage 1: Base runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

# Stage 2: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy only the .csproj first (for layer caching)
COPY src/EventHorizon.Api/EventHorizon.Api.csproj EventHorizon.Api/
RUN dotnet restore EventHorizon.Api/EventHorizon.Api.csproj

# Copy full source folder (not entire repo)
COPY src/EventHorizon.Api/ ./EventHorizon.Api/

# Copy init-db.sql from repo root into /app inside container
# The context of build must be the project root (EVENTAPP-BACKEND)
COPY init-db.sql ./EventHorizon.Api/init-db.sql

# Switch into the project directory
WORKDIR /src/EventHorizon.Api
RUN dotnet build EventHorizon.Api.csproj -c Release -o /app/build

# Stage 3: Publish
FROM build AS publish
WORKDIR /src/EventHorizon.Api
RUN dotnet publish EventHorizon.Api.csproj -c Release -o /app/publish

# Stage 4: Runtime
FROM base AS final
WORKDIR /app

# Copy published output
COPY --from=publish /app/publish .

# Copy init-db.sql into final image for deployment use
COPY --from=build /src/EventHorizon.Api/init-db.sql /app/init-db.sql

ENTRYPOINT ["dotnet", "EventHorizon.Api.dll"]
