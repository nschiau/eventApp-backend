#!/bin/bash

# EventHorizon API Deployment Script
# This script helps deploy the EventHorizon API to various platforms

set -e

echo "🚀 EventHorizon API Deployment Script"
echo "======================================"

# Function to check if command exists
command_exists() {
    command -v "$1" >/dev/null 2>&1
}

# Check prerequisites
echo "📋 Checking prerequisites..."

if ! command_exists dotnet; then
    echo "❌ .NET CLI not found. Please install .NET 9 SDK"
    exit 1
fi

if ! command_exists docker; then
    echo "❌ Docker not found. Please install Docker"
    exit 1
fi

echo "✅ Prerequisites satisfied"

# Build and test
echo ""
echo "🔨 Building project..."
dotnet clean
dotnet restore
dotnet build --configuration Release

echo ""
echo "🧪 Running tests..."
dotnet test --configuration Release --no-build

echo ""
echo "📦 Publishing application..."
dotnet publish src/EventHorizon.Api -c Release -o ./publish

echo ""
echo "🐳 Building Docker image..."
docker build -t eventhorizon-api:latest -f src/EventHorizon.Api/Dockerfile .

echo ""
echo "✅ Build completed successfully!"
echo ""
echo "Next steps:"
echo "1. Tag the Docker image: docker tag eventhorizon-api:latest your-registry/eventhorizon-api:v1.0.0"
echo "2. Push to registry: docker push your-registry/eventhorizon-api:v1.0.0"
echo "3. Deploy to your platform of choice"
echo ""
echo "For local testing:"
echo "  docker-compose up -d"
echo "  API will be available at http://localhost:8080"
