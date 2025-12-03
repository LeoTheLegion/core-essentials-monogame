# Contributing to CoreEssentials-MonoGame

Thank you for your interest in contributing to CoreEssentials-MonoGame! This document provides guidelines and information for contributors.

## Development Setup

### Prerequisites
- .NET 8.0 SDK or later
- Git
- A code editor (Visual Studio, VS Code, or Rider recommended)

### Getting Started

1. Fork the repository
2. Clone your fork:
   ```bash
   git clone https://github.com/YOUR_USERNAME/core-essentials-monogame.git
   cd core-essentials-monogame
   ```

3. Create a feature branch:
   ```bash
   git checkout -b feature/your-feature-name
   ```

## Building the Project

### Build the library
```bash
dotnet build CoreEssentials/CoreEssentials.csproj -c Release
```

### Run tests (Windows only)
```bash
bash ./scripts/test.sh
# or
dotnet test CoreEssentials.Tests/CoreEssentials.Tests.csproj
```

**Note:** Tests require Windows due to MonoGame.Framework.WindowsDX dependency. They cannot be run on Linux/macOS.

### Pack the NuGet package
```bash
dotnet pack CoreEssentials/CoreEssentials.csproj -c Release
```

## Development Workflow

1. **Create a feature branch** following the naming convention:
   - `feature/xxx` for new features
   - `bugfix/xxx` for bug fixes
   
2. **Make your changes** with proper:
   - Code implementation
   - Unit tests (when applicable)
   - XML documentation comments
   - Updates to relevant documentation in `docs/`

3. **Test your changes** locally on Windows if possible

4. **Commit your changes** with descriptive messages:
   ```bash
   git commit -m "Add feature: description of what you added"
   ```

5. **Push to your fork**:
   ```bash
   git push origin feature/your-feature-name
   ```

6. **Create a Pull Request** on GitHub

## Pull Request Guidelines

- Ensure your code builds without errors
- Add tests for new functionality when applicable
- Update documentation for new features
- Follow the existing code style and conventions
- Keep changes focused and atomic (one feature/fix per PR)
- Reference any related issues in the PR description

## Continuous Integration

The repository uses GitHub Actions for CI/CD:

- **CI Workflow** (`.github/workflows/ci.yml`):
  - Runs on every push to `main`/`master` and on pull requests
  - Builds the CoreEssentials library
  - Creates NuGet package
  - Uploads build artifacts

- **Release Workflow** (`.github/workflows/release.yml`):
  - Triggers on version tags (e.g., `v0.13.0`)
  - Builds and packs the library
  - Publishes to GitHub Packages
  - Creates a GitHub Release

## Release Process

Releases are automated via GitHub Actions. To create a new release:

### 1. Update the Version

Edit `CoreEssentials/CoreEssentials.csproj` and update the version number:

```xml
<Version>0.14.0</Version>
```

### 2. Commit and Push

```bash
git add CoreEssentials/CoreEssentials.csproj
git commit -m "Bump version to 0.14.0"
git push origin main
```

### 3. Create and Push a Tag

```bash
git tag v0.14.0
git push origin v0.14.0
```

### 4. Automated Release

GitHub Actions will automatically:
- ✅ Build the library in Release mode
- ✅ Pack the NuGet package
- ✅ Publish to GitHub Packages
- ✅ Create a GitHub Release with auto-generated changelog
- ✅ Attach `.nupkg` and `.snupkg` files to the release

### 5. Monitor the Release

- Check the [Actions tab](https://github.com/LeoTheLegion/core-essentials-monogame/actions) to monitor the workflow
- Once complete, verify the release at [Releases](https://github.com/LeoTheLegion/core-essentials-monogame/releases)
- The package will be available at [GitHub Packages](https://github.com/LeoTheLegion/core-essentials-monogame/packages)

## Manual Release (Legacy)

If you need to publish manually for any reason:

```bash
# Build and pack
bash ./scripts/publish.sh

# The script handles:
# - Extracting version from .csproj
# - Running dotnet pack
# - Pushing to GitHub Packages (requires GITHUB_NUGET_TOKEN env var)
```

## Code Style

- Use C# naming conventions (PascalCase for public members, camelCase for private)
- Add XML documentation comments for all public APIs
- Keep methods focused and concise
- Follow existing patterns in the codebase

## Documentation

- Update `docs/` files when adding new features
- Keep code examples up to date
- Add inline comments for complex logic only

## Questions or Issues?

- Check existing [Issues](https://github.com/LeoTheLegion/core-essentials-monogame/issues)
- Create a new issue for bugs or feature requests
- Join discussions in pull requests

## License

By contributing, you agree that your contributions will be licensed under the MIT License.

---

Thank you for contributing to CoreEssentials-MonoGame! 🎮
