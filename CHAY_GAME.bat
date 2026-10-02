@echo off
set "GAME_DIR=%~dp0Builds\Windows-NumberMemory"
if not exist "%GAME_DIR%\NongTrai.exe" (
    echo Missing NongTrai.exe. Keep the full Windows build folder beside this launcher.
    pause
    exit /b 1
)
cd /d "%GAME_DIR%"
start "" "%GAME_DIR%\NongTrai.exe"
