param([string]$GameDir = 'F:\Games\Nuclear.Option.v0.34.1', [string]$BlueprinterProject = 'F:\NCMod\Blueprinter-Editor')
$ErrorActionPreference = 'Stop'
Push-Location $PSScriptRoot
try {
    dotnet build Runtime/Arsenal.csproj -c Release "-p:GameDir=$GameDir" "-p:BlueprinterProject=$BlueprinterProject"
    if ($LASTEXITCODE -ne 0) { throw 'Runtime build failed' }
    Copy-Item Runtime/bin/Release/net472/Phantasms-Arsenal.dll dist/Phantasms-Arsenal.dll -Force
    & "$PSScriptRoot/Verify.ps1"
} finally { Pop-Location }
