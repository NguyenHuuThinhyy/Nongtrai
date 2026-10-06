@echo off
set "GAME_DIR=%~dp0Builds\Windows-LocalAI"
if exist "%~dp0NongTrai.exe" set "GAME_DIR=%~dp0"
if not exist "%GAME_DIR%\NongTrai.exe" (
    echo Missing NongTrai.exe. Extract the complete Windows package.
    pause
    exit /b 1
)
cd /d "%GAME_DIR%"
start "" "%GAME_DIR%\NongTrai.exe"
