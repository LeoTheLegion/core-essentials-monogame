# usage: ./scripts/uncovered-lines.ps1 <type-simple-name>   e.g. EntitySystem
# prints uncovered line numbers for each coverage class whose SIMPLE type name matches.
# (coverage.cobertura.xml names classes by C# *type*, not file path.)
param(
    [Parameter(Mandatory=$true)][string]$Name,
    [string]$Xml = (Join-Path $PSScriptRoot '..' 'CoreEssentials.Tests/coverage/coverage.cobertura.xml')
)
if (-not (Test-Path $Xml)) { Write-Error "coverage file not found: $Xml"; exit 1 }
[xml]$doc = Get-Content $Xml
$pat = '(^|\.)' + [regex]::Escape($Name) + '(/.*)?$'
foreach ($c in $doc.SelectNodes('//class')) {
    if ($c.Name -match $pat) {
        # <line> elements are nested under methods/method/lines, so query all descendants.
        $miss = @($c.SelectNodes('.//line') | Where-Object { [int]$_.GetAttribute('hits') -eq 0 } | ForEach-Object { [int]$_.GetAttribute('number') })
        Write-Output "$($c.Name) [$($miss.Count) lines]: $($miss -join ', ')"
    }
}
