@echo off
rem Doble click para deployar SmartBuy al stack Docker local.
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0deploy.ps1"
pause
