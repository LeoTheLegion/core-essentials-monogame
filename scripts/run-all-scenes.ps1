#!/usr/bin/env pwsh
#
# Scene smoke-run harness.
#
# Launches every data-driven scene (any Content XML whose root element is <Scene>) for a fixed
# number of seconds, then lets the game auto-exit. A scene "passes" when its process exits cleanly
# (exit code 0); a non-zero exit means it threw while booting or running and is reported as failed.
#
# Usage:
#   ./scripts/run-all-scenes.ps1                 # run each scene for 5 seconds
#   ./scripts/run-all-scenes.ps1 -Seconds 8      # run each scene for 8 seconds
#   ./scripts/run-all-scenes.ps1 -Scenes HomeScene.xml,PhysicsEntityScene.xml
#   ./scripts/run-all-scenes.ps1 -NoFocusPause   # keep audio playing if the window loses focus
#
param(
    [double]$Seconds = 5.0,
    [string[]]$Scenes = @(),
    [switch]$NoFocusPause = $false
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$contentDir = Join-Path $repoRoot "CoreEssentials.Playground/Content"
$project = Join-Path $repoRoot "CoreEssentials.Playground"

if (-not (Test-Path $contentDir)) {
    Write-Error "Content directory not found: $contentDir"
}

# Discover scenes: any XML file whose first non-empty line contains a <Scene element.
function Get-SceneFiles {
    Get-ChildItem -Path $contentDir -Filter *.xml | ForEach-Object {
        $head = (Get-Content $_.FullName -TotalCount 3) -join ' '
        if ($head -match '<Scene') { $_.Name }
    }
}

$sceneFiles = if ($Scenes.Count -gt 0) { $Scenes } else { Get-SceneFiles }

if ($sceneFiles.Count -eq 0) {
    Write-Error "No scenes found to run."
}

Write-Host "Building playground..." -ForegroundColor Cyan
dotnet build $project --nologo -v q
if ($LASTEXITCODE -ne 0) {
    Write-Error "Build failed; aborting smoke-run."
}

$results = @()
foreach ($scene in $sceneFiles) {
    Write-Host "`n=== Running scene: $scene (for $Seconds s) ===" -ForegroundColor Cyan

    $runArgs = @("--scene", $scene, "--run-for", $Seconds)
    if ($NoFocusPause) { $runArgs += "--no-focus-pause" }
    $output = & dotnet run --project $project --no-build --nologo -- $runArgs 2>&1
    $code = $LASTEXITCODE

    if ($code -eq 0) {
        $status = "PASS"
        Write-Host "[$status] $scene" -ForegroundColor Green
    } else {
        $status = "FAIL"
        Write-Host "[$status] $scene (exit code $code)" -ForegroundColor Red
        Write-Host $output
    }

    $results += [pscustomobject]@{ Scene = $scene; Status = $status; ExitCode = $code }
}

Write-Host "`n=================== Smoke-Run Summary ===================" -ForegroundColor Cyan
$results | Format-Table -AutoSize
$failed = @($results | Where-Object { $_.Status -eq "FAIL" })

if ($failed.Count -gt 0) {
    Write-Host ("{0} of {1} scene(s) FAILED." -f $failed.Count, $results.Count) -ForegroundColor Red
    exit 1
} else {
    Write-Host ("All {0} scene(s) passed." -f $results.Count) -ForegroundColor Green
    exit 0
}
