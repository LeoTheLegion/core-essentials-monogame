#!/usr/bin/env pwsh

dotnet test CoreEssentials.Tests/
$testExit = $LASTEXITCODE

# Always surface a per-file coverage report, even when the 80% threshold gate fails.
& (Join-Path $PSScriptRoot 'coverage-report.ps1')

exit $testExit
