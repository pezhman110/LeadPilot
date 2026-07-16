@echo off
echo LeadPilot - Local startup
dotnet restore
dotnet build
dotnet test
dotnet tool restore
dotnet ef database update -p src\LeadPilot.Infrastructure -s src\LeadPilot.Server
dotnet run --project src\LeadPilot.Server
