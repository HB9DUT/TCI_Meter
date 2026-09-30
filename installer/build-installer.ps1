# Copyright © 2026, HB9DUT
# SPDX-License-Identifier: GPL-3.0-or-later
#
# Publishes the app and builds the Inno Setup installer: installer\Output\TciMeter-Setup-<version>.exe
# Requires the .NET 8 SDK and Inno Setup 6.

$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent

# version from the project file
[xml]$proj = Get-Content "$root\TciMeter.csproj"
$version = ($proj.Project.PropertyGroup.Version | Where-Object { $_ } | Select-Object -First 1)
if (-not $version) { throw 'No <Version> found in TciMeter.csproj' }

# locate the Inno Setup compiler
$iscc = @(
    "$env:LOCALAPPDATA\Programs\Inno Setup 6\ISCC.exe",
    "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
    "$env:ProgramFiles\Inno Setup 6\ISCC.exe"
) | Where-Object { Test-Path $_ } | Select-Object -First 1
if (-not $iscc) { throw 'Inno Setup 6 (ISCC.exe) not found.' }

# self-contained single file: no .NET runtime needed on the target machine
$out = "$root\publish\installer"
if (Test-Path $out) { Remove-Item $out -Recurse -Force }
dotnet publish "$root\TciMeter.csproj" -c Release -r win-x64 --self-contained true `
    -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true `
    -o $out
if ($LASTEXITCODE -ne 0) { throw 'dotnet publish failed' }

& $iscc "/DAppVersion=$version" "$PSScriptRoot\TciMeter.iss"
if ($LASTEXITCODE -ne 0) { throw 'Inno Setup compile failed' }

Get-Item "$PSScriptRoot\Output\TciMeter-Setup-$version.exe" |
    Select-Object Name, @{n = 'MB'; e = { [math]::Round($_.Length / 1MB, 1) } }
