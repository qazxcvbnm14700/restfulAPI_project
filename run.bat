@echo off
cd /d "%~dp0web\bin"
start "" http://localhost:5050/index.html
dotnet TofuShopApi.dll --urls "http://localhost:5050"
pause