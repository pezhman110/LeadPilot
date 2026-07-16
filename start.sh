#!/usr/bin/env bash
set -e
echo "LeadPilot — راه‌اندازی محلی"
echo "پیش‌نیاز: PostgreSQL در حال اجرا و رشتهٔ اتصال در appsettings.json"
dotnet restore
dotnet build
dotnet test
dotnet tool restore 2>/dev/null || dotnet tool install --local dotnet-ef
dotnet ef database update -p src/LeadPilot.Infrastructure -s src/LeadPilot.Server
dotnet run --project src/LeadPilot.Server
