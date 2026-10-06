# HThinh.yy. Compatibility entry point: services run locally.
param([switch]$DownloadModel,[switch]$Lan)
& (Join-Path $PSScriptRoot 'Start-Assistant.ps1') -NoGame -Lan:$Lan
