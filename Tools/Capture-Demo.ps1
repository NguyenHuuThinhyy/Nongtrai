param([string]$Ffmpeg='ffmpeg', [string]$Game='Builds/Windows-Rubric/NongTrai.exe')
$ErrorActionPreference='Stop'
$project=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$exe=[IO.Path]::GetFullPath((Join-Path $project $Game))
$log=Join-Path $project 'Logs/demo-pc.log'
New-Item -ItemType Directory -Path (Split-Path $log -Parent) -Force | Out-Null
$p=Start-Process -FilePath $exe -ArgumentList @('-farmDemo','-farmTouch','-screen-width','1280','-screen-height','720','-screen-fullscreen','0','-logFile',('"'+$log+'"')) -WindowStyle Hidden -PassThru
$p.WaitForExit()
if($p.ExitCode -ne 0){throw 'Demo failed; see Logs/demo-pc.log. Start a local diagnostic backend/model first.'}
$line=Get-Content -LiteralPath $log | Where-Object {$_ -match 'FARM_DEMO_OK frames=\d+ folder=(.+) •'} | Select-Object -Last 1
if(!$line){throw 'Demo completion marker missing'}
$line -match 'folder=(.+) •' | Out-Null
$frames=$Matches[1].Trim()
$output=Join-Path $project 'Evidence/Rubric/demo-pc-automated.mp4'
# Frames are deliberately 5 fps; this video is evidence of flow, not an FPS benchmark.
& $Ffmpeg -y -framerate 5 -i (Join-Path $frames 'frame-%05d.png') -c:v libx264 -pix_fmt yuv420p -movflags +faststart $output
if($LASTEXITCODE -ne 0){throw 'ffmpeg encoding failed'}
Write-Output "PC demo: $output"
