$ErrorActionPreference = 'Stop'
# Official public Quaternius folders; only download FBX and the supplied license.
function Get-DriveEntries([string]$id) {
    $html = (Invoke-WebRequest -UseBasicParsing "https://drive.google.com/drive/folders/$id").Content
    foreach ($m in [regex]::Matches($html, '<tr data-selectable data-id="([^"]+)"(?:(?!</tr>).)*</tr>', 'Singleline')) {
        $label = [System.Net.WebUtility]::HtmlDecode([regex]::Match($m.Value, 'data-tooltip="([^"]+)"').Groups[1].Value)
        [pscustomobject]@{ Id=$m.Groups[1].Value; Label=$label }
    }
}
$packs = @(
    @{Name='FarmBuildings';Root='1gdZ39vcLML_ULU5sHirkKQ-gEkk458Ak';Fbx='1qO8aXJ9MHhn8D-ch8-AMEkzQS1nas2cg'},
    @{Name='Crops';Root='1uhbi-NWp7pwqOGtraBZurbphyxAvoABZ';Fbx='1r_WpDuffiJeQ3neYTvCEb0RI-yOB_afS'},
    @{Name='Animals';Root='1uJ3N5HfB7jKTseJUNQr3N4YaN0UuEtHk';Fbx='13cS1y5LTM8h6dNMQut7aO7zAxxdMgZA2'},
    @{Name='Enemies';Root='1VbJIslXPWK-1KybQN6yezZrfJcw608qe';Fbx='1hqIHvxWIQocc33E-Uubj4AxjETwhZahS'}
)
foreach ($pack in $packs) {
    $folder = New-Item -ItemType Directory -Force -Path (Join-Path $PSScriptRoot "../Assets/ThirdParty/VisualRedesign/Quaternius_$($pack.Name)")
    $entries = @(Get-DriveEntries $pack.Fbx) + @(Get-DriveEntries $pack.Root | Where-Object Label -like 'License.txt*')
    foreach ($entry in $entries) {
        if ($entry.Label -notmatch '^(.*\.(fbx|txt)) ') { Write-Output "Nested entry $($pack.Name): $($entry.Id) $($entry.Label)"; continue }
        $name = $Matches[1]; $target = Join-Path $folder.FullName $name
        if ($pack.Name -eq 'Animals' -and $name -notin @('Cow.fbx','Fox.fbx','Wolf.fbx','License.txt')) { continue }
        if ($pack.Name -eq 'Crops' -and $name -notmatch '^(Apple|BushBerries|Flower|License)') { continue }
        if ($pack.Name -eq 'Enemies' -and $name -notin @('Snake.fbx','License.txt')) { continue }
        if (Test-Path -LiteralPath $target) { continue }
        Invoke-WebRequest -UseBasicParsing "https://drive.google.com/uc?export=download&id=$($entry.Id)" -OutFile $target
        $size = (Get-Item -LiteralPath $target).Length
        Write-Output "$($pack.Name)/$name : $size bytes"
    }
}
