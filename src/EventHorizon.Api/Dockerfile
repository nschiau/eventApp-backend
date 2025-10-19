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

# Now copy everything
COPY . .

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
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EventHorizon.Api.dll"]
