#!/usr/bin/env pwsh
# Reports per-file line coverage from the latest Coverlet Cobertura report.
# Intended to run right after `dotnet test` (see test.ps1 / test.sh), which collects
# coverage and enforces the 80% gate. This ranks source files by how many lines are
# still uncovered, so you can see exactly where to add tests next.

$ErrorActionPreference = 'Stop'

# Compress a set of line numbers into Jest-style ranges, e.g. 38,39,40,55 -> "38-40, 55".
function ConvertTo-LineRanges($nums) {
    if (-not $nums) { return '' }
    $sorted = @($nums | Sort-Object -Unique)
    if ($sorted.Count -eq 0) { return '' }
    $parts = [System.Collections.Generic.List[string]]::new()
    for ($i = 0; $i -lt $sorted.Count;) {
        $start = $sorted[$i]
        $end   = $start
        $j     = $i + 1
        while ($j -lt $sorted.Count -and $sorted[$j] -eq $end + 1) { $end = $sorted[$j]; $j++ }
        if ($end -gt $start) { $parts.Add("$start-$end") } else { $parts.Add("$start") }
        $i = $j
    }
    return ($parts -join ', ')
}

$repoRoot  = Split-Path -Parent $PSScriptRoot
$cobertura = Join-Path $repoRoot (Join-Path 'CoreEssentials.Tests' (Join-Path 'coverage' 'coverage.cobertura.xml'))

if (-not (Test-Path -LiteralPath $cobertura)) {
    Write-Host "No coverage report found at $cobertura" -ForegroundColor Red
    Write-Host 'Run the tests first (e.g. ./scripts/test.ps1) to generate one.' -ForegroundColor Yellow
    exit 1
}

try {
    $xml = [xml](Get-Content -LiteralPath $cobertura)
}
catch {
    Write-Host "Could not parse $cobertura : $_" -ForegroundColor Red
    exit 1
}

$root       = $xml.coverage
$linesHit   = [int]$root.'lines-covered'
$linesValid = [int]$root.'lines-valid'
$lineRate   = [double]$root.'line-rate'
$threshold  = 80

Write-Host ''
Write-Host '=== CoreEssentials line coverage (Cobertura) ===' -ForegroundColor Cyan
Write-Host ('Overall : {0:N2}%  ({1}/{2} lines)  [target {3}%]' -f ($lineRate * 100), $linesHit, $linesValid, $threshold)

# Aggregate covered/total per source file. A line counts as covered if any class
# that maps to the same filename records a hit for it (deduped by line number).
$byFile = @{}
foreach ($cls in @($xml.SelectNodes('//class'))) {
    $fname = $cls.GetAttribute('filename')
    if (-not $fname) { continue }
    # The playground is manual integration testing; never let it surface in the unit-coverage report.
    if ($fname -like 'CoreEssentials.Playground*') { continue }
    if (-not $byFile.ContainsKey($fname)) { $byFile[$fname] = @{} }
    foreach ($ln in @($cls.SelectNodes('lines/line'))) {
        $num   = [int]$ln.GetAttribute('number')
        $isHit = ([int]$ln.GetAttribute('hits')) -gt 0
        if (-not ($byFile[$fname].ContainsKey($num))) { $byFile[$fname][$num] = $false }
        if ($isHit) { $byFile[$fname][$num] = $true }
    }
}

$rows = foreach ($fname in $byFile.Keys) {
    $lines   = $byFile[$fname]
    $total   = $lines.Count
    # Split line numbers into covered vs. uncovered so we can print Jest-style ranges.
    $coveredNums   = @($lines.Keys | Where-Object { $lines[$_] })
    $uncoveredNums = @($lines.Keys | Where-Object { -not $lines[$_] })
    $covered       = $coveredNums.Count
    [PSCustomObject]@{
        # Short leaf name keeps the numeric columns visible; full path is retained in `Path`.
        Name      = (Split-Path -Leaf $fname)
        Path      = ($fname -replace '^/','')
        Covered   = $covered
        Total     = $total
        Uncovered = $total - $covered
        Pct       = if ($total) { [math]::Round(100.0 * $covered / $total, 1) } else { 0.0 }
        'Uncovered Line #s' = (ConvertTo-LineRanges $uncoveredNums)
    }
}

$gaps = @($rows | Where-Object { $_.Uncovered -gt 0 } | Sort-Object Uncovered -Descending)

Write-Host ''
Write-Host ('Files with uncovered lines: {0} (most impactful to test first)' -f $gaps.Count) -ForegroundColor Cyan
if ($gaps.Count -eq 0) {
    Write-Host 'No uncovered lines.' -ForegroundColor Green
}
else {
    $gaps | Select-Object Name, Covered, Total, Uncovered, Pct, 'Uncovered Line #s', Path | Format-Table -AutoSize -Wrap
}

$below = @($rows | Where-Object { $_.Pct -lt $threshold })
Write-Host ('Files below {0}% line coverage: {1}' -f $threshold, $below.Count) -ForegroundColor Yellow
if ($below.Count -gt 0) {
    $below | Sort-Object Pct | Select-Object Name, Covered, Total, Uncovered, Pct, 'Uncovered Line #s', Path | Format-Table -AutoSize -Wrap
}

$deltaNeeded = [math]::Ceiling($linesValid * ($threshold / 100)) - $linesHit
if ($deltaNeeded -gt 0) {
    Write-Host ('Coverage is below target. Cover ~{0} more lines to reach {1}%.' -f $deltaNeeded, $threshold) -ForegroundColor Red
}
else {
    Write-Host 'Coverage meets the target.' -ForegroundColor Green
}
