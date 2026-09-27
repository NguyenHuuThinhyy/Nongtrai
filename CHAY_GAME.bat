@echo off
if not exist "%~dp0Builds\Windows-BowPhysics\NongTrai.exe" (
    echo Missing Builds\Windows-BowPhysics\NongTrai.exe. Extract the entire downloaded ZIP first.
    pause
    exit /b 1
)
cd /d "%~dp0Builds\Windows-BowPhysics"
start "" "%~dp0Builds\Windows-BowPhysics\NongTrai.exe"
