#!/usr/bin/env pwsh

# Extract the version from CoreEssentials\CoreEssentials.csproj
$csproj = Get-Content -Path "$PSScriptRoot/../CoreEssentials/CoreEssentials.csproj" -Raw
if ($csproj -match '<Version>([^<]+)</Version>') {
    $VERSION = $Matches[1]
} else {
    Write-Error "Could not find <Version> in CoreEssentials.csproj"
    exit 1
}

# Run "dotnet pack" specifying Release configuration
dotnet pack "$PSScriptRoot/../CoreEssentials/CoreEssentials.csproj" --configuration Release

# Push the resulting NuGet package to the GitHub feed using the extracted version
dotnet nuget push "`"$PSScriptRoot/../CoreEssentials/bin/Release/CoreEssentials-MonoGame.${VERSION}.nupkg`"" `
    --api-key "$env:GITHUB_NUGET_TOKEN" --source "github"
