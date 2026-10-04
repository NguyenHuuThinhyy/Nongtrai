param([string]$UnityExe='D:\Unity\Editors\6000.3.22f1\Editor\Unity.exe',[string]$Output='Builds/Android/NongTrai.apk')
$ErrorActionPreference='Stop'
$project=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
New-Item -ItemType Directory -Path (Join-Path $project 'Logs') -Force | Out-Null
$env:FARM_ANDROID_OUTPUT=$Output
$arguments=@('-batchmode','-nographics','-quit','-projectPath',('"'+$project+'"'),'-buildTarget','Android','-executeMethod','NongTrai.Editor.FarmTechnologyBuild.BuildAndroid','-logFile',('"'+(Join-Path $project 'Logs/build-android.log')+'"'))
$process=Start-Process -FilePath $UnityExe -ArgumentList $arguments -WindowStyle Hidden -PassThru
$process.WaitForExit()
if($process.ExitCode -ne 0){throw "Android build failed; see Logs/build-android.log (exit $($process.ExitCode))"}
Write-Output "Android build: $Output"
