# Copyright HThinh.yy. Local owner-review packages; never publish/replace a release.
param([Parameter(Mandatory=$true)][string]$BuildSourceCommit,[string]$Label=(Get-Date -Format 'yyyyMMdd-HHmm'))
$ErrorActionPreference='Stop'
$project=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
Set-Location -LiteralPath $project
$windows=Join-Path $project 'Builds/Windows-LocalAI'
$apk=Join-Path $project 'Builds/Android-LocalAI/NongTrai.apk'
$output=Join-Path $project ('DongGoi/LocalAI/'+$Label)
if(Test-Path -LiteralPath $output){throw 'Package label already exists; choose a new label.'}
$spec=Get-Content -LiteralPath Backend/LOCAL-MODEL.json -Raw -Encoding utf8 | ConvertFrom-Json
$model=Join-Path $project ('Backend/models/'+$spec.file)
foreach($path in @($model,$apk,(Join-Path $windows 'NongTrai.exe'),(Join-Path $windows ('NongTrai_Data/StreamingAssets/FarmAI/'+$spec.file)))){if(!(Test-Path -LiteralPath $path)){throw ('Missing package input '+$path)}}
if((Get-FileHash -LiteralPath $model).Hash -ne $spec.sha256){throw 'Model checksum mismatch'}
New-Item -ItemType Directory -Path $output -Force | Out-Null
Add-Type -AssemblyName System.IO.Compression
function WriteZip([string]$File,[object[]]$Entries) {
    $stream=[IO.File]::Open($File,[IO.FileMode]::CreateNew)
    $zip=[IO.Compression.ZipArchive]::new($stream,[IO.Compression.ZipArchiveMode]::Create)
    try {
        foreach($item in $Entries){
            # Quantized weights already have high entropy. Store without recompressing.
            $level=if($item.Name.EndsWith('.gguf')){[IO.Compression.CompressionLevel]::NoCompression}else{[IO.Compression.CompressionLevel]::Optimal}
            $entry=$zip.CreateEntry($item.Name.Replace('\','/'),$level)
            $input=[IO.File]::OpenRead($item.Path);$target=$entry.Open()
            try{$input.CopyTo($target)}finally{$target.Dispose();$input.Dispose()}
        }
    }finally{$zip.Dispose();$stream.Dispose()}
}
$runtime=@(Get-ChildItem -LiteralPath $windows -Recurse -File | Where-Object{$_.FullName -notmatch 'DoNotShip|BackUpThis|\.log$'})
$entries=@($runtime|ForEach-Object{@{Path=$_.FullName;Name='NongTrai-Windows/'+$_.FullName.Substring($windows.Length+1)}})
foreach($file in @('CHAY_GAME.bat','CHAY_GAME_CO_TRO_LY.bat','CHAY_GAME_KHONG_TRO_LY.bat','Docs/LOCAL_CHAT.md','COPYRIGHT.md')){$entries+=@{Path=(Join-Path $project $file);Name='NongTrai-Windows/'+$file}}
$winZip=Join-Path $output "NongTrai-Windows-LocalAI-$Label.zip"
WriteZip $winZip $entries
$files=@(& git -c core.quotepath=false ls-files | Where-Object{$_ -notmatch '^(Builds/|DongGoi/|Evidence/.*\.mp4$)'})
$entries=@($files|ForEach-Object{@{Path=(Join-Path $project $_);Name='NongTrai-Unity/'+$_}})
$entries+=@{Path=$model;Name=('NongTrai-Unity/Backend/models/'+$spec.file)}
$entries+=@{Path=(Join-Path $project 'Backend/models/Qwen3-LICENSE');Name='NongTrai-Unity/Backend/models/Qwen3-LICENSE'}
$sourceZip=Join-Path $output "NongTrai-Unity-LocalAI-$Label.zip"
WriteZip $sourceZip $entries
$apkCopy=Join-Path $output "NongTrai-Android-LocalAI-$Label.apk"
Copy-Item -LiteralPath $apk -Destination $apkCopy
$packages=@($winZip,$apkCopy,$sourceZip)|ForEach-Object{[ordered]@{file=[IO.Path]::GetFileName($_);bytes=(Get-Item -LiteralPath $_).Length;sha256=(Get-FileHash -LiteralPath $_).Hash.ToLowerInvariant()}}
$manifest=[ordered]@{branch=(& git branch --show-current).Trim();build_source_commit=$BuildSourceCommit;source_git_commit=(& git rev-parse HEAD).Trim();model=$spec;packages=$packages;publication='local only, waiting for owner approval';physical_android='not verified';runtime_acceptance='owner test pending'}
$manifest|ConvertTo-Json -Depth 6|Set-Content -LiteralPath (Join-Path $output 'MANIFEST.json') -Encoding utf8
$packages|ForEach-Object{$_.sha256+'  '+$_.file}|Set-Content -LiteralPath (Join-Path $output 'SHA256SUMS.txt') -Encoding ascii
Write-Output ($manifest|ConvertTo-Json -Depth 6)
