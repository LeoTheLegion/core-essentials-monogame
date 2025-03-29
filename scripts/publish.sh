#!/bin/bash

# Extract the version from CoreEssentials\CoreEssentials.csproj
VERSION="$(sed -n 's|.*<Version>\([^<]*\)</Version>.*|\1|p' CoreEssentials/CoreEssentials.csproj)"

# Run "dotnet pack" specifying Release configuration
dotnet pack CoreEssentials/CoreEssentials.csproj --configuration Release

# Push the resulting NuGet package to the GitHub feed using the extracted version
dotnet nuget push "CoreEssentials/bin/Release/CoreEssentials-MonoGame.${VERSION}.nupkg" \
  --api-key "$GITHUB_NUGET_TOKEN" --source "github"
