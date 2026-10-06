# Copyright HThinh.yy. Rebuild the single portable CPU plugin for both targets.
param([string]$Cache=(Join-Path $env:LOCALAPPDATA 'NongTraiNativeChat'),[string]$AndroidPlayer='D:\Unity\Editors\6000.3.22f1\Editor\Data\PlaybackEngines\AndroidPlayer',[ValidateSet('Both','Windows','Android')][string]$Target='Both')
$ErrorActionPreference='Stop'
$project=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
New-Item -ItemType Directory -Path $Cache -Force | Out-Null
$cmake=Join-Path $AndroidPlayer 'SDK/cmake/3.22.1/bin/cmake.exe'
$ninja=Join-Path $AndroidPlayer 'SDK/cmake/3.22.1/bin/ninja.exe'
function Fetch([string]$Url,[string]$File,[string]$Hash) {
    if(!(Test-Path -LiteralPath $File)) {
        & curl.exe --fail --location --retry 3 --output ($File+'.download') $Url
        if($LASTEXITCODE){throw 'Download failed'}
        if((Get-FileHash -LiteralPath ($File+'.download')).Hash -ne $Hash){throw 'Download checksum mismatch'}
        Move-Item -LiteralPath ($File+'.download') -Destination $File
    }
    if((Get-FileHash -LiteralPath $File).Hash -ne $Hash){throw 'Archive checksum mismatch'}
}
$source=Join-Path $Cache 'llama.cpp-8c32d9d96d9ae345a0150cae8572859e9aafea0b'
if(!(Test-Path -LiteralPath (Join-Path $source 'include/llama.h'))) {
    $zip=Join-Path $Cache 'llama-b7199.zip'
    Fetch 'https://codeload.github.com/ggml-org/llama.cpp/zip/8c32d9d96d9ae345a0150cae8572859e9aafea0b' $zip '4cfda1876db48557a5a003dfb4d7e530f795393bc6bab1c7922c26047209d8f7'
    Expand-Archive -LiteralPath $zip -DestinationPath $Cache
}
function BuildPlugin([string]$Name,[string[]]$Options,[string]$Library) {
    $build=Join-Path $Cache ('build-'+$Name)
    & $cmake -S (Join-Path $project 'Tools/NativeChat') -B $build -G Ninja "-DCMAKE_MAKE_PROGRAM=$ninja" "-DLLAMA_SOURCE=$source" '-DCMAKE_BUILD_TYPE=Release' @Options
    if($LASTEXITCODE){throw ($Name+' CMake configure failed')}
    & $cmake --build $build --target farm_chat --parallel 3
    if($LASTEXITCODE){throw ($Name+' native build failed')}
    $destination=Join-Path $project ('Assets/Farm/Plugins/'+$Name)
    New-Item -ItemType Directory -Path $destination -Force | Out-Null
    $fileName=if($Name -eq 'Windows'){'farm_chat.dll'}else{$Library}
    Copy-Item -LiteralPath (Join-Path $build $Library) -Destination (Join-Path $destination $fileName)
}
if($Target -ne 'Android') {
    $compiler=Join-Path $Cache 'llvm-mingw-20260922-ucrt-x86_64'
    if(!(Test-Path -LiteralPath (Join-Path $compiler 'bin/x86_64-w64-mingw32-clang.exe'))) {
        $zip=Join-Path $Cache 'llvm-mingw.zip'
        Fetch 'https://github.com/mstorsjo/llvm-mingw/releases/download/20260922/llvm-mingw-20260922-ucrt-x86_64.zip' $zip 'e3ad77d117a4bea19a7a3b333341824d79a5a371004a10e25b8504e7b3047666'
        Expand-Archive -LiteralPath $zip -DestinationPath $Cache
    }
    $compiler=$compiler.Replace('\','/')
    Copy-Item -LiteralPath (Join-Path $compiler 'LICENSE.TXT') -Destination (Join-Path $project 'Backend/licenses/LLVM-Apache-Exceptions.txt')
    BuildPlugin 'Windows' @('-DCMAKE_SYSTEM_NAME=Windows',"-DCMAKE_C_COMPILER=$compiler/bin/x86_64-w64-mingw32-clang.exe","-DCMAKE_CXX_COMPILER=$compiler/bin/x86_64-w64-mingw32-clang++.exe") 'libfarm_chat.dll'
}
if($Target -ne 'Windows') {
    BuildPlugin 'Android' @("-DCMAKE_TOOLCHAIN_FILE=$AndroidPlayer/NDK/build/cmake/android.toolchain.cmake",'-DANDROID_ABI=arm64-v8a','-DANDROID_PLATFORM=android-26','-DANDROID_STL=c++_static') 'libfarm_chat.so'
}
$licenses=Join-Path $project 'Backend/licenses'
Copy-Item -LiteralPath (Join-Path $source 'LICENSE') -Destination (Join-Path $licenses 'llama.cpp-MIT.txt')
Write-Output 'Local chat plugins built. Model is packaged by the Unity build hook.'
