@echo off
title Nong Trai - Backend PC
cd /d "%~dp0"
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0Tools\Start-Assistant.ps1" -NoGame
if errorlevel 1 pause
