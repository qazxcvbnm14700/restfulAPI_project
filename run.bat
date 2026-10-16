@echo off
cd /d "%~dp0"
echo Publishing the latest API to web\bin...
dotnet publish "src\TofuShopApi.csproj" -c Release -o "web\bin"
if errorlevel 1 (
    echo API publishing failed. Stop any existing API server and run this script again.
    pause
    exit /b 1
)
cd /d "%~dp0web\bin"
start "" http://localhost:5050/index.html
dotnet TofuShopApi.dll --urls "http://localhost:5050"
pause