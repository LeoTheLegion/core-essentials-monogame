#!/usr/bin/env pwsh
#
# Scene smoke-run harness.
#
# Launches every scene registered in the scene manifest (CoreEssentials.Playground/Content/scenes.xml,
# the <GameScenes> list, in order) for a fixed number of seconds, then lets the game auto-exit. A
# scene "passes" when its process exits cleanly (exit code 0); a non-zero exit means it threw while
# booting or running and is reported as failed. The manifest is authoritative — consistent with the
# core's enforcement: a missing manifest aborts, and <Scene> files on disk that are not registered
# anywhere in the manifest are surfaced as a warning (and skipped).
#
# Usage:
#   ./scripts/run-all-scenes.ps1                 # run each registered scene for 5 seconds
#   ./scripts/run-all-scenes.ps1 -Seconds 8      # run each registered scene for 8 seconds
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
$manifestPath = Join-Path $contentDir "scenes.xml"

if (-not (Test-Path $contentDir)) {
    Write-Error "Content directory not found: $contentDir"
}

# The scene manifest is the authoritative list of scenes — the core enforces it, so a missing file
# aborts the harness rather than falling back to globbing.
if (-not (Test-Path $manifestPath)) {
    Write-Error "Scene manifest not found at $manifestPath. Create it (see docs/SceneManifest.md) — the core requires it for name-based scene loads."
}

# The registered game scenes: <GameScenes> entries in scenes.xml, in order.
function Get-RegisteredScenes {
    [xml]$manifest = Get-Content $manifestPath
    @($manifest.Scenes.GameScenes.Scene) | ForEach-Object { $_.Name }
}

# Warning pass: <Scene>-rooted XML files on disk that are not registered anywhere in the manifest
# (neither as a game scene nor a loading screen). These are skipped but surfaced.
function Get-UnregisteredSceneFiles {
    [xml]$manifest = Get-Content $manifestPath
    $registered = @{}
    foreach ($s in @($manifest.Scenes.GameScenes.Scene)) { $registered[$s.Name] = $true }
    foreach ($l in @($manifest.Scenes.LoadingScenes.LoadingScene)) {
        if ($null -ne $l) { $registered[$l.Name] = $true }
    }

    Get-ChildItem -Path $contentDir -Filter *.xml | ForEach-Object {
        $head = (Get-Content $_.FullName -TotalCount 3) -join ' '
        if ($head -match '<Scene' -and -not $registered.ContainsKey($_.Name)) { $_.Name }
    }
}

$sceneFiles = if ($Scenes.Count -gt 0) { $Scenes } else { Get-RegisteredScenes }

if ($sceneFiles.Count -eq 0) {
    Write-Error "No scenes registered in the manifest at $manifestPath."
}

$unregistered = @(Get-UnregisteredSceneFiles)
if ($unregistered.Count -gt 0) {
    Write-Warning ("Scene files on disk are not registered in scenes.xml (skipped): {0}" -f ($unregistered -join ', '))
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
