$ErrorActionPreference = 'Stop'
$dll = Join-Path $PSScriptRoot 'dist/Phantasms-Arsenal.dll'
$bundle = Join-Path $PSScriptRoot "Bundles/Phantasm's Arsenal_1.0.0.nobp"
$assembly = [Reflection.Assembly]::LoadFile($dll)
$names = @($assembly.GetManifestResourceNames())
if ($names.Count -ne 1 -or $names[0] -ne 'Arsenal.Weapons.nobp') { throw 'Unexpected embedded resources' }
$stream = $assembly.GetManifestResourceStream('Arsenal.Weapons.nobp')
$sha = [Security.Cryptography.SHA256]::Create()
try { $embedded = [BitConverter]::ToString($sha.ComputeHash($stream)).Replace('-','') } finally { $stream.Dispose(); $sha.Dispose() }
$source = (Get-FileHash -LiteralPath $bundle -Algorithm SHA256).Hash
if ($source -ne $embedded) { throw 'Embedded bundle hash mismatch' }
$report = [ordered]@{ dll_sha256=(Get-FileHash -LiteralPath $dll).Hash; source_bundle_sha256=$source; embedded_bundle_sha256=$embedded; resource='Arsenal.Weapons.nobp'; package_verified=$true; mission_test='not performed' }
$report | ConvertTo-Json | Set-Content (Join-Path $PSScriptRoot 'verification/package.json')
"$($report.dll_sha256)  Phantasms-Arsenal.dll" | Set-Content (Join-Path $PSScriptRoot 'dist/SHA256.txt')
Write-Output 'PASS: embedded Blueprinter bundle matches source byte-for-byte'
