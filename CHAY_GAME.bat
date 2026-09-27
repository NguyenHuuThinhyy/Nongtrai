@echo off
if not exist "%~dp0Builds\Windows-ChestLoot\NongTrai.exe" (
    echo Missing Builds\Windows-ChestLoot\NongTrai.exe. Extract the entire downloaded ZIP first.
    pause
    exit /b 1
)
cd /d "%~dp0Builds\Windows-ChestLoot"
start "" "%~dp0Builds\Windows-ChestLoot\NongTrai.exe"
