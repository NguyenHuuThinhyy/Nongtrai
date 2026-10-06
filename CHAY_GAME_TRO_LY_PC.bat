@echo off
title Nong Trai - Khoi dong tro ly va game
cd /d "%~dp0"
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0Tools\Start-Assistant.ps1"
if errorlevel 1 (
    echo.
    echo Tro ly chua san sang. Xem loi ben tren.
    echo De choi offline, mo CHAY_GAME_KHONG_TRO_LY.bat.
    pause
)
