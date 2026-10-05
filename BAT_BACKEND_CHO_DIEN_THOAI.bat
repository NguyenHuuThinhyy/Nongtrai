@echo off
title Nong Trai - Backend cho dien thoai
cd /d "%~dp0"
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0Tools\Start-Assistant.ps1" -NoGame -Lan
if errorlevel 1 pause
