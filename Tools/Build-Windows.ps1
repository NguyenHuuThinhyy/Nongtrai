# Copyright (c) HThinh.yy. Build the saved Farm scene without regenerating it.
[CmdletBinding()]
param(
    [string]$UnityEditor,
    [string]$OutputDirectory = 'Builds/Windows-LocalAI'
)

$ErrorActionPreference = 'Stop'
$taskProjectRoot = (Resolve-Path -LiteralPath (Split-Path -Parent $PSScriptRoot)).Path
$versionLine = Get-Content -LiteralPath (Join-Path $taskProjectRoot 'ProjectSettings/ProjectVersion.txt') |
    Where-Object { $_ -match '^m_EditorVersion: ' } | Select-Object -First 1
$requiredVersion = $versionLine -replace '^m_EditorVersion: ', ''

if (-not $UnityEditor) {
    $candidates = @(
        $env:UNITY_EDITOR_PATH,
        "C:\Program Files\Unity\Hub\Editor\$requiredVersion\Editor\Unity.exe",
        "D:\Unity\Editors\$requiredVersion\Editor\Unity.exe"
    )
    $UnityEditor = $candidates | Where-Object { $_ -and (Test-Path -LiteralPath $_ -PathType Leaf) } | Select-Object -First 1
}
if (-not $UnityEditor -or -not (Test-Path -LiteralPath $UnityEditor -PathType Leaf)) {
    throw "Set -UnityEditor to the Unity $requiredVersion Editor/Unity.exe path. Install Windows Build Support in Unity Hub."
}
$UnityEditor = (Resolve-Path -LiteralPath $UnityEditor).Path
$actualVersion = (Get-Item -LiteralPath $UnityEditor).VersionInfo.ProductVersion
if (-not $actualVersion.StartsWith($requiredVersion)) {
    throw "Required Unity $requiredVersion; selected editor reports $actualVersion."
}
$running = Get-CimInstance Win32_Process -Filter "Name='Unity.exe'" | Where-Object {
    $_.CommandLine -and $_.CommandLine.IndexOf($taskProjectRoot, [StringComparison]::OrdinalIgnoreCase) -ge 0
}
if ($running) { throw 'Close the Unity Editor using this project before building.' }

$outputPath = if ([IO.Path]::IsPathRooted($OutputDirectory)) {
    [IO.Path]::GetFullPath($OutputDirectory)
} else {
    [IO.Path]::GetFullPath((Join-Path $taskProjectRoot $OutputDirectory))
}
if (-not $outputPath.StartsWith($taskProjectRoot + '\', [StringComparison]::OrdinalIgnoreCase)) {
    throw 'OutputDirectory must be inside this project, for example Builds/Windows.'
}
$logFolder = Join-Path $taskProjectRoot 'Logs'
New-Item -ItemType Directory -Path $logFolder -Force | Out-Null
$logPath = Join-Path $logFolder ('build-' + (Get-Date -Format 'yyyyMMdd-HHmmss') + '.log')
$previousOutput = $env:FARM_BUILD_OUTPUT
try {
    $env:FARM_BUILD_OUTPUT = Join-Path $outputPath 'NongTrai.exe'
    $arguments = @('-batchmode', '-nographics', '-quit', '-projectPath', ('"' + $taskProjectRoot + '"'), '-buildTarget', 'StandaloneWindows64',
        '-executeMethod', 'NongTrai.Editor.FarmTechnologyBuild.BuildWindows', '-logFile', ('"' + $logPath + '"'))
    # Wait only for the editor, not background processes inherited by its Windows job.
    $process = Start-Process -FilePath $UnityEditor -ArgumentList $arguments -PassThru -WindowStyle Hidden
    $process.WaitForExit()
    if ($process.ExitCode -ne 0 -or -not (Select-String -LiteralPath $logPath -Pattern 'FARM_BUILD_RESULT Succeeded' -Quiet)) {
        throw "Unity build failed. Read $logPath"
    }
    Write-Output ('Built: ' + (Join-Path $outputPath 'NongTrai.exe'))
    Write-Output ('Log: ' + $logPath)
} finally {
    $env:FARM_BUILD_OUTPUT = $previousOutput
}
