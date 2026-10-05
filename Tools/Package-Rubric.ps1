# © HThinh.yy. Run only after builds and smoke checks pass.
param([Parameter(Mandatory=$true)][string]$BuildSourceCommit,[string]$Label='20261005')
$ErrorActionPreference='Stop'
$project=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
Set-Location -LiteralPath $project
$sourceCommit=(& git rev-parse HEAD).Trim()
& git cat-file -e ($BuildSourceCommit+'^{commit}')
if($LASTEXITCODE -ne 0){throw 'BuildSourceCommit is not a local commit'}
$windows=Join-Path $project 'Builds/Windows-Rubric'
$apk=Join-Path $project 'Builds/Android/NongTrai.apk'
foreach($path in @((Join-Path $windows 'NongTrai.exe'),(Join-Path $windows 'UnityPlayer.dll'),(Join-Path $windows 'NongTrai_Data/globalgamemanagers'),$apk)){
    if(!(Test-Path -LiteralPath $path -PathType Leaf)){throw "Missing build: $path"}
}
$output=Join-Path $project 'DongGoi'
New-Item -ItemType Directory -Path $output -Force | Out-Null
Add-Type -AssemblyName System.IO.Compression
function Write-Package([string]$destination,[object[]]$entries){
    if(Test-Path -LiteralPath $destination){throw "Package already exists: $destination"}
    $stream=[IO.File]::Open($destination,[IO.FileMode]::CreateNew)
    $archive=[IO.Compression.ZipArchive]::new($stream,[IO.Compression.ZipArchiveMode]::Create)
    try{
        foreach($item in $entries){
            $entry=$archive.CreateEntry($item.Name.Replace('\','/'),[IO.Compression.CompressionLevel]::Optimal)
            $input=[IO.File]::OpenRead($item.Path);$entryStream=$entry.Open()
            try{$input.CopyTo($entryStream)}finally{$entryStream.Dispose();$input.Dispose()}
        }
    }finally{$archive.Dispose();$stream.Dispose()}
}
$runtimeFiles=Get-ChildItem -LiteralPath $windows -File -Recurse | Where-Object {$_.FullName -notmatch 'DoNotShip|\.png$|\.log$'}
$windowsEntries=@($runtimeFiles | ForEach-Object {@{Path=$_.FullName;Name='NongTrai-Windows/'+$_.FullName.Substring($windows.Length+1)}})
# The playable package must include the assistant, not just a UI pointing at an absent server.
$companionFiles=@(& git ls-files | Where-Object {$_ -match '^Backend/(app/|licenses/|launcher\.py$|run\.py$|requirements\.txt$|MODEL-MANIFEST\.json$|Dockerfile$|compose\.yaml$|\.dockerignore$|\.env\.example$)' -or $_ -in @('Tools/Start-Assistant.ps1','Tools/Start-Services.ps1','CHAY_GAME.bat','CHAY_GAME_CO_TRO_LY.bat','CHAY_GAME_KHONG_TRO_LY.bat','BAT_BACKEND_PC.bat','BAT_BACKEND_CHO_DIEN_THOAI.bat','Docs/ASSISTANT_START.md')})
$windowsEntries+=@($companionFiles | ForEach-Object {@{Path=(Join-Path $project $_);Name='NongTrai-Windows/'+$_}})
$windowsZip=Join-Path $output "NongTrai-Windows-Rubric-$Label.zip"
Write-Package $windowsZip $windowsEntries
# Archive only versioned source. Runtime, package archives and secrets are excluded.
$sourceFiles=@(& git ls-files | Where-Object {$_ -notmatch '^(Builds/|DongGoi/|Evidence/.*\.mp4$)'})
$sourceEntries=@($sourceFiles | ForEach-Object {@{Path=(Join-Path $project $_);Name='NongTrai-Unity/'+$_}})
$sourceZip=Join-Path $output "NongTrai-Unity-Rubric-$Label.zip"
Write-Package $sourceZip $sourceEntries
$apkCopy=Join-Path $output "NongTrai-Android-Rubric-$Label.apk"
Copy-Item -LiteralPath $apk -Destination $apkCopy
$packages=@($windowsZip,$apkCopy,$sourceZip) | ForEach-Object {
    $file=Get-Item -LiteralPath $_
    @{file=$file.Name;bytes=$file.Length;sha256=(Get-FileHash -LiteralPath $_ -Algorithm SHA256).Hash.ToLowerInvariant()}
}
$manifest=[ordered]@{
    release="rubric-preview-$Label";release_url="https://github.com/NguyenHuuThinhyy/Nongtrai/releases/tag/rubric-preview-$Label"
    unity='6000.3.22f1';save_version=22;feature_branch='codex/rubric-mobile-ar-ai-cloud'
    source_git_commit=$sourceCommit;build_source_commit=$BuildSourceCommit
    runtime_bytes=($runtimeFiles | Measure-Object Length -Sum).Sum
    farm_scene_sha256=(Get-FileHash -LiteralPath 'Assets/Farm/Scenes/Farm.unity' -Algorithm SHA256).Hash.ToLowerInvariant()
    packages=@($packages);physical_android_ar_cloud_acceptance='pending';user_merge_approval='pending'
}
$manifest | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $output 'RELEASE-MANIFEST.json') -Encoding utf8
$packages | ForEach-Object {$_.sha256+'  '+$_.file} | Set-Content -LiteralPath (Join-Path $output 'SHA256SUMS.txt') -Encoding ascii
Write-Output ($manifest | ConvertTo-Json -Depth 6)
