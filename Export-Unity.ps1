param([string]$BlueprinterProject = 'F:\NCMod\Blueprinter-Editor')
$ErrorActionPreference = 'Stop'
$source = Join-Path $BlueprinterProject 'Assets/Blueprinter/Mods/PhantasmsArsenal'
$destination = Join-Path $PSScriptRoot 'UnityAssets/PhantasmsArsenal'
New-Item -ItemType Directory -Path $destination -Force | Out-Null
foreach ($item in Get-ChildItem -LiteralPath $source -Force) {
    Copy-Item -LiteralPath $item.FullName -Destination $destination -Recurse -Force
}
Copy-Item -LiteralPath "$source.meta" -Destination (Join-Path $PSScriptRoot 'UnityAssets/PhantasmsArsenal.meta') -Force
