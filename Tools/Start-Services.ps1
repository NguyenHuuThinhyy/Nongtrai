param([switch]$DownloadModel)
$ErrorActionPreference='Stop'
$backend=Join-Path $PSScriptRoot '../Backend'
Push-Location $backend
try{
    if(!(Get-Command docker -ErrorAction SilentlyContinue)){throw 'Install and start Docker Desktop (WSL2/Linux containers) first.'}
    if(!(Test-Path -LiteralPath '.env')){
        $key=[Guid]::NewGuid().ToString('N')
        $config=(Get-Content -LiteralPath '.env.example' -Raw).Replace('change-this-pairing-code',$key)
        [IO.File]::WriteAllText((Join-Path (Get-Location) '.env'),$config)
        Write-Output 'Created Backend/.env. Read FARM_PAIRING_KEY locally for the game connection screen.'
    }
    docker compose up -d --build --wait --wait-timeout 180
    if($LASTEXITCODE -ne 0){throw 'Compose startup failed'}
    if($DownloadModel){docker compose exec -T ollama ollama pull qwen3:1.7b;if($LASTEXITCODE -ne 0){throw 'Model download failed'}}
    docker compose ps
    Invoke-RestMethod -Uri 'http://127.0.0.1:8000/health' | ConvertTo-Json
}finally{Pop-Location}
