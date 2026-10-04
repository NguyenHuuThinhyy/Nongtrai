# © HThinh.yy. Tests are opt-in and run the built player.
param([ValidateSet('Full','Restaurant','Technology','ServicesLive')][string]$Suite='Full',[switch]$Touch)
$ErrorActionPreference='Stop'
$project=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$exe=Join-Path $project 'Builds/Windows-Rubric/NongTrai.exe'
New-Item -ItemType Directory -Path (Join-Path $project 'Logs') -Force | Out-Null
$log=Join-Path $project ('Logs/smoke-'+$Suite.ToLowerInvariant()+$(if($Touch){'-touch'})+'.log')
$arguments=@('-farmSmokeCheck','-screen-width','1280','-screen-height','720','-screen-fullscreen','0','-logFile',('"'+$log+'"'))
$marker=switch($Suite){
    'Full'{$arguments+='-farmArtCheck';'FARM_CROPS_SMOKE_OK'}
    'Restaurant'{$arguments+='-farmRestaurantOnly';'FARM_RESTAURANT_OK'}
    'Technology'{$arguments+='-farmTechnologyOnly';'FARM_TECHNOLOGY_OK'}
    'ServicesLive'{$arguments+=@('-farmTechnologyOnly','-farmServicesLive');'FARM_SERVICES_LIVE_OK'}
}
if($Touch){$arguments+='-farmTouch'}
$p=Start-Process -FilePath $exe -ArgumentList $arguments -WindowStyle Hidden -PassThru
$deadline=(Get-Date).AddMinutes(8)
while(!$p.HasExited){
    if((Get-Date) -gt $deadline -or ((Test-Path -LiteralPath $log) -and (Select-String -LiteralPath $log -Pattern 'InvalidOperationException|NullReferenceException|FARM_DEMO_ERROR' -Quiet))){
        # Only this verified child player is stopped; the user's other game is untouched.
        $p.Kill();throw "Smoke check failed or timed out. Read $log"
    }
    Start-Sleep -Milliseconds 250
}
if($p.ExitCode -ne 0 -or !(Select-String -LiteralPath $log -SimpleMatch $marker -Quiet)){throw "Smoke marker missing: $marker. Read $log"}
Write-Output "PASS $Suite $marker; log=$log"
