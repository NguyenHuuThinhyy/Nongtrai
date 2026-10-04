param([string]$EditorDirectory='D:\Unity\Editors\6000.3.22f1', [string]$CacheDirectory='D:\GAME_NongTrai\Recovery\RubricTools\Android')
$ErrorActionPreference='Stop'
$ProgressPreference='SilentlyContinue'
$modules=Get-Content -LiteralPath (Join-Path $EditorDirectory 'modules.json') -Raw | ConvertFrom-Json -AsHashtable
$android=$modules | Where-Object {$_.id -eq 'android'}
if(!$android){throw 'Android module metadata missing'}
New-Item -ItemType Directory -Path $CacheDirectory -Force | Out-Null
$installer=Join-Path $CacheDirectory 'Unity-Android-Support.exe'
if(!(Test-Path -LiteralPath $installer)){Invoke-WebRequest -Uri $android.url -OutFile $installer}
$expected=[Convert]::ToHexString([Convert]::FromBase64String($android.integrity.Substring(4)))
if((Get-FileHash -LiteralPath $installer -Algorithm MD5).Hash -ne $expected){throw 'Unity Android installer checksum mismatch'}
if(!(Test-Path -LiteralPath (Join-Path $EditorDirectory 'Editor/Data/PlaybackEngines/AndroidPlayer/UnityEditor.Android.Extensions.dll'))){
    $process=Start-Process -FilePath $installer -ArgumentList '/S',("/D="+$EditorDirectory) -WindowStyle Hidden -Wait -PassThru
    if($process.ExitCode -ne 0){throw "Android installer failed: $($process.ExitCode)"}
}
function Install-ZipModule($entry){
    $id=$entry.id
    if($id -match 'platforms-(34|35|37)' -or $id -eq 'android-sdk-ndk-tools'){return}
    $destination=$entry.destination.Replace('{UNITY_PATH}',$EditorDirectory).Replace('/',[IO.Path]::DirectorySeparatorChar)
    $archive=Join-Path $CacheDirectory ($id.Replace('+','_')+'.zip')
    if(!(Test-Path -LiteralPath $archive)){Write-Output "Downloading $id";Invoke-WebRequest -Uri $entry.url -OutFile $archive}
    New-Item -ItemType Directory -Path $destination -Force | Out-Null
    Expand-Archive -LiteralPath $archive -DestinationPath $destination -Force
    if($entry.extractedPathRename){
        $from=$entry.extractedPathRename.from.Replace('{UNITY_PATH}',$EditorDirectory)
        $to=$entry.extractedPathRename.to.Replace('{UNITY_PATH}',$EditorDirectory)
        $root=[IO.Path]::GetFullPath($EditorDirectory)+[IO.Path]::DirectorySeparatorChar
        if(![IO.Path]::GetFullPath($from).StartsWith($root,[StringComparison]::OrdinalIgnoreCase) -or ![IO.Path]::GetFullPath($to).StartsWith($root,[StringComparison]::OrdinalIgnoreCase)){throw 'Module target outside editor'}
        if($to -eq $destination -and (Test-Path -LiteralPath $from)){
            Get-ChildItem -LiteralPath $from -Force | ForEach-Object {Move-Item -LiteralPath $_.FullName -Destination $destination -Force}
            Remove-Item -LiteralPath $from
        }elseif((Test-Path -LiteralPath $from) -and !(Test-Path -LiteralPath $to)){
            New-Item -ItemType Directory -Path (Split-Path $to -Parent) -Force | Out-Null
            Move-Item -LiteralPath $from -Destination $to
        }
    }
}
foreach($entry in $android.subModules){
    if($entry.id -eq 'android-sdk-ndk-tools'){foreach($child in $entry.subModules){Install-ZipModule $child}}
    else {Install-ZipModule $entry}
}
Write-Output 'ANDROID_SUPPORT_INSTALLED'
