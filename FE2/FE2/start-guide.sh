#!/bin/bash

# Comic Reader - Start All Services Script
# This script helps you start all backend services and the frontend

echo "🚀 Starting Comic Reader Application..."
echo ""

# Function to check if a port is in use
check_port() {
    netstat -ano | findstr :$1 > /dev/null 2>&1
    return $?
}

echo "📋 Checking prerequisites..."

# Check if .NET is installed
if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET SDK not found. Please install .NET 6+ first."
    exit 1
fi

# Check if Node.js is installed  
if ! command -v node &> /dev/null; then
    echo "❌ Node.js not found. Please install Node.js 20+ first."
    exit 1
fi

echo "✅ Prerequisites checked"
echo ""

echo "📦 Installing frontend dependencies..."
cd FE2/FE2
npm install
cd ../..

echo ""
echo "🎯 Starting services..."
echo ""
echo "Please open 4 separate terminal windows and run these commands:"
echo ""
echo "Terminal 1 - UserService (Port 5001):"
echo "  cd UserService.Api"
echo "  dotnet run"
echo ""
echo "Terminal 2 - ReadingService (Port 5002):"
echo "  cd ReadingService.API"
echo "  dotnet run"
echo ""
echo "Terminal 3 - ComicService (Port 5003):"
echo "  cd ComicService.Api"
echo "  dotnet run"
echo ""
echo "Terminal 4 - Frontend (Port 5173):"
echo "  cd FE2/FE2"
echo "  npm run dev"
echo ""
echo "🌐 Once all services are running, open your browser to:"
echo "  http://localhost:5173"
echo ""
echo "📚 Happy reading!"
