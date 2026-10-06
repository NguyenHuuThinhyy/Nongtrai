param([string]$UnityExe='D:\Unity\Editors\6000.3.22f1\Editor\Unity.exe',[string]$Output='Builds/Android/NongTrai.apk')
$ErrorActionPreference='Stop'
$project=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
New-Item -ItemType Directory -Path (Join-Path $project 'Logs') -Force | Out-Null
$running=Get-CimInstance Win32_Process -Filter "Name='Unity.exe'" | Where-Object {$_.CommandLine -and $_.CommandLine.IndexOf($project,[StringComparison]::OrdinalIgnoreCase) -ge 0}
if($running){throw 'Close the Unity Editor using this project before building.'}
$target=if([IO.Path]::IsPathRooted($Output)){[IO.Path]::GetFullPath($Output)}else{[IO.Path]::GetFullPath((Join-Path $project $Output))}
if(!$target.StartsWith($project+'\',[StringComparison]::OrdinalIgnoreCase)){throw 'APK output must be inside the project.'}
$log=Join-Path $project 'Logs/build-android.log'
$previousOutput=$env:FARM_ANDROID_OUTPUT
try {
    $env:FARM_ANDROID_OUTPUT=$target
    $arguments=@('-batchmode','-nographics','-quit','-projectPath',('"'+$project+'"'),'-buildTarget','Android','-executeMethod','NongTrai.Editor.FarmTechnologyBuild.BuildAndroid','-logFile',('"'+$log+'"'))
    $process=Start-Process -FilePath $UnityExe -ArgumentList $arguments -WindowStyle Hidden -PassThru
    $process.WaitForExit()
    if($process.ExitCode -ne 0 -or !(Select-String -LiteralPath $log -Pattern 'FARM_BUILD_RESULT Succeeded' -Quiet) -or !(Test-Path -LiteralPath $target)){throw "Android build failed; see $log (exit $($process.ExitCode))"}
    Write-Output "Android build: $target"
} finally {$env:FARM_ANDROID_OUTPUT=$previousOutput}
