# Copyright HThinh.yy. Desktop launcher, no administrator rights required.
[CmdletBinding()]
param([switch]$NoGame,[switch]$Lan,[switch]$Mute,[switch]$PrepareOnly,[switch]$VerifyModel)
$ErrorActionPreference='Stop'
[Net.ServicePointManager]::SecurityProtocol=[Net.SecurityProtocolType]::Tls12
$project=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$backend=Join-Path $project 'Backend'
$cache=Join-Path $env:LOCALAPPDATA 'NongTraiAssistant'
New-Item -ItemType Directory -Path $cache -Force | Out-Null
$settingsFile=Join-Path $backend 'runtime.local.json'
$settings=if(Test-Path -LiteralPath $settingsFile){Get-Content -LiteralPath $settingsFile -Raw | ConvertFrom-Json}else{$null}
$pythonExe=if($settings -and $settings.python){$settings.python}else{Join-Path $cache 'Python312/python.exe'}
$ollamaExe=if($settings -and $settings.ollama){$settings.ollama}else{Join-Path $cache 'Ollama/ollama.exe'}

function Download-Verified([string]$Url,[string]$Destination,[string]$Sha256){
    if(!(Test-Path -LiteralPath $Destination)){
        $pending=$Destination+'.download'
        Write-Host ('Downloading '+[IO.Path]::GetFileName($Destination)+' ...')
        if(Get-Command curl.exe -ErrorAction SilentlyContinue){
            & curl.exe --fail --location --retry 3 --output $pending $Url
            if($LASTEXITCODE -ne 0){throw 'Download failed. Check Internet and run again.'}
        }else{Invoke-WebRequest -UseBasicParsing -Uri $Url -OutFile $pending}
        if($Sha256 -and (Get-FileHash -LiteralPath $pending -Algorithm SHA256).Hash -ne $Sha256){throw 'Download checksum mismatch.'}
        Move-Item -LiteralPath $pending -Destination $Destination
    }
    if($Sha256 -and (Get-FileHash -LiteralPath $Destination -Algorithm SHA256).Hash -ne $Sha256){throw ('Checksum mismatch: '+$Destination)}
}

if(!(Test-Path -LiteralPath $pythonExe)){
    if($settings -and $settings.python){throw 'Python path in Backend/runtime.local.json no longer exists.'}
    $archive=Join-Path $cache 'python-3.12.10-embed-amd64.zip'
    Download-Verified 'https://www.python.org/ftp/python/3.12.10/python-3.12.10-embed-amd64.zip' $archive '4acbed6dd1c744b0376e3b1cf57ce906f9dc9e95e68824584c8099a63025a3c3'
    Expand-Archive -LiteralPath $archive -DestinationPath (Split-Path -Parent $pythonExe) -Force
    $pth=Join-Path (Split-Path -Parent $pythonExe) 'python312._pth'
    [IO.File]::WriteAllText($pth,[IO.File]::ReadAllText($pth).Replace('#import site','import site'))
}
& $pythonExe -c "import importlib.util,sys; sys.exit(0 if all(importlib.util.find_spec(n) for n in ('fastapi','uvicorn','httpx','pydantic','paho')) else 1)"
if($LASTEXITCODE -ne 0){
    $pip=Join-Path $cache 'pip.pyz'
    Download-Verified 'https://bootstrap.pypa.io/pip/pip.pyz' $pip ''
    & $pythonExe $pip install --disable-pip-version-check -r (Join-Path $backend 'requirements.txt')
    if($LASTEXITCODE -ne 0){throw 'Backend dependency installation failed.'}
}
if(!(Test-Path -LiteralPath $ollamaExe)){
    if($settings -and $settings.ollama){throw 'Ollama path in Backend/runtime.local.json no longer exists.'}
    Write-Host 'First setup: download Ollama runtime (~1.91 GB); model downloads separately (~1.36 GB).'
    $archive=Join-Path $cache 'ollama-windows-amd64-0.12.3.zip'
    Download-Verified 'https://github.com/ollama/ollama/releases/download/v0.12.3/ollama-windows-amd64.zip' $archive 'c440fcda7fb1f4677f16c159192aa9ad05aae6b010ee652b82ecc184d08e5549'
    Expand-Archive -LiteralPath $archive -DestinationPath (Split-Path -Parent $ollamaExe) -Force
}
if(!(Test-Path -LiteralPath $ollamaExe)){throw 'Ollama archive did not contain ollama.exe.'}
if($PrepareOnly){Write-Output 'Runtime and Python imports ready. No services/game started.';return}
if(!$NoGame -and (Get-Process NongTrai -ErrorAction SilentlyContinue)){throw 'Close the currently running game, then start CHAY_GAME.bat again to load the new connection.'}
$arguments=@('-X','utf8',(Join-Path $backend 'launcher.py'),'--ollama',$ollamaExe)
if($settings -and $settings.models){$arguments+=@('--models',$settings.models)}
if($NoGame){$arguments+='--no-game'}
if($Lan){$arguments+='--lan'}
if($Mute){$arguments+='--mute'}
if($VerifyModel){$arguments+='--verify-model'}
& $pythonExe @arguments
if($LASTEXITCODE -ne 0){throw 'Assistant not ready. Read the error above and Backend/.runtime logs.'}
