<#
.SYNOPSIS
    Installs ThemeExtrasNG to Playnite and restarts Playnite if running.
#>
[CmdletBinding()]
param(
    [switch]$NoRestart = $false
)

$ErrorActionPreference = "Stop"

$scriptDir = $PSScriptRoot
$sourceDir = Join-Path $scriptDir "source\bin\Release\net462"
$targetDir = Join-Path $env:APPDATA "Playnite\Extensions\goover_ThemeExtrasNG_Plugin"
$legacyTargetDir = Join-Path $env:APPDATA "Playnite\Extensions\felixkmh_Extras_Plugin"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "    ThemeExtrasNG Extension Installer   " -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# 1. Build project in Release mode
Write-Host "Building ThemeExtrasNG in Release mode..." -ForegroundColor Yellow
dotnet build (Join-Path $scriptDir "source\Extras.sln") -c Release

# 2. Check if Playnite is running and stop it
$playniteProcesses = Get-Process -Name "Playnite.DesktopApp", "Playnite.FullscreenApp" -ErrorAction SilentlyContinue
$wasRunning = $false
if ($playniteProcesses) {
    $wasRunning = $true
    Write-Host "Playnite is running. Closing Playnite to update files..." -ForegroundColor Yellow
    $playniteProcesses | Stop-Process -Force
    Start-Sleep -Seconds 1
}

# 3. Clean up legacy extension directory to prevent duplicates
if (Test-Path $legacyTargetDir) {
    Write-Host "Removing legacy felixkmh_Extras_Plugin from Extensions..." -ForegroundColor Yellow
    Remove-Item -Path $legacyTargetDir -Recurse -Force -ErrorAction SilentlyContinue
}

# 4. Clean and deploy to ThemeExtrasNG Extensions directory
Write-Host "Installing to: $targetDir" -ForegroundColor Green
if (-not (Test-Path $targetDir)) {
    New-Item -ItemType Directory -Path $targetDir -Force | Out-Null
} else {
    Get-ChildItem -Path $targetDir -Recurse | Remove-Item -Force -Recurse -ErrorAction SilentlyContinue
}

Copy-Item -Path "$sourceDir\*" -Destination $targetDir -Recurse -Force
Write-Host "ThemeExtrasNG installed successfully to Playnite Extensions!" -ForegroundColor Green

# 5. Create .pext extension package
$distDir = Join-Path $scriptDir "dist"
if (-not (Test-Path $distDir)) { New-Item -ItemType Directory -Path $distDir -Force | Out-Null }
$builtPext = Join-Path $scriptDir "source\bin\goover_ThemeExtrasNG_Plugin_1_0_0.pext"
$pextTarget = Join-Path $distDir "ThemeExtrasNG_1.0.0.pext"
$pextStandard = Join-Path $distDir "goover_ThemeExtrasNG_Plugin_1_0_0.pext"

if (Test-Path $builtPext) {
    Copy-Item $builtPext -Destination $pextTarget -Force
} else {
    $tempZip = Join-Path $distDir "temp_package.zip"
    if (Test-Path $tempZip) { Remove-Item $tempZip -Force }
    Compress-Archive -Path "$sourceDir\*" -DestinationPath $tempZip -Force
    Move-Item $tempZip $pextTarget -Force
}

Copy-Item $pextTarget -Destination $pextStandard -Force
Copy-Item $pextTarget -Destination (Join-Path $scriptDir "ThemeExtrasNG_1.0.0.pext") -Force
Write-Host "Created extension package: $pextTarget" -ForegroundColor Green

# 6. Restart Playnite if it was running
if ($wasRunning -and -not $NoRestart) {
    Write-Host "Relaunching Playnite..." -ForegroundColor Cyan
    $playniteExe = Join-Path $env:LOCALAPPDATA "Playnite\Playnite.DesktopApp.exe"
    if (Test-Path $playniteExe) {
        Start-Process $playniteExe
    }
}

Write-Host "Done!" -ForegroundColor Green
