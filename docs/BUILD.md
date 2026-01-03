# BrowRoute .NET Build & Deployment Guide

## Prerequisites

- .NET 9.0 SDK
- macOS 13.0+ (Ventura or later) for macOS builds
- Optional: Visual Studio 2022, VS Code, or Rider

## Project Structure

```
BrowRoute/
├── BrowRoute.sln                 # Solution file
├── src/
│   ├── BrowRoute.App/           # Main console application
│   │   ├── Program.cs           # Entry point and CLI
│   │   └── BrowRoute.App.csproj
│   └── BrowRoute.Core/          # Core library
│       ├── Models/              # Data models
│       ├── Services/            # Business logic
│       ├── Platform/            # Platform-specific code
│       └── BrowRoute.Core.csproj
├── scripts/
│   └── build-macos.sh          # macOS build script
├── config.example.json         # Example configuration
└── README.md
```

## Building

### Quick Build (Development)

```bash
# Build the solution
dotnet build

# Run the application
dotnet run --project src/BrowRoute.App/BrowRoute.App.csproj

# Run with arguments
dotnet run --project src/BrowRoute.App/BrowRoute.App.csproj -- test https://github.com/SilkRoadProfessionals/pcbe-api
```

### Release Build (Single Executable)

```bash
# For macOS (Apple Silicon)
dotnet publish src/BrowRoute.App/BrowRoute.App.csproj \
    -c Release \
    -r osx-arm64 \
    --self-contained \
    -p:PublishSingleFile=true \
    -p:PublishTrimmed=true

# For macOS (Intel)
dotnet publish src/BrowRoute.App/BrowRoute.App.csproj \
    -c Release \
    -r osx-x64 \
    --self-contained \
    -p:PublishSingleFile=true \
    -p:PublishTrimmed=true
```

### macOS .app Bundle

```bash
# Make build script executable
chmod +x scripts/build-macos.sh

# Run build script
./scripts/build-macos.sh
```

This creates a proper macOS application bundle at `build/BrowRoute.app`.

## Installation

### Option 1: Copy to Applications

```bash
# After building
cp -r build/BrowRoute.app /Applications/

# Set as default browser
# System Settings → Desktop & Dock → Default web browser → BrowRoute
```

### Option 2: Use From Build Directory

```bash
# Create symlink
ln -s "$(pwd)/build/BrowRoute.app/Contents/MacOS/BrowRoute" /usr/local/bin/browroute

# Now you can use it from terminal
browroute test https://youtube.com
```

## Configuration

### Initial Setup

1. Copy example configuration:
```bash
mkdir -p ~/Library/Application\ Support/BrowRoute
cp config.example.json ~/Library/Application\ Support/BrowRoute/config.json
```

2. Edit configuration:
```bash
# Using your favorite editor
code ~/Library/Application\ Support/BrowRoute/config.json
# or
vim ~/Library/Application\ Support/BrowRoute/config.json
```

3. Update browser paths in config to match your installation:
```json
{
  "browsers": {
    "Brave": {
      "executablePath": "/Applications/Brave Browser.app"
    },
    "Dia": {
      "executablePath": "/Applications/Dia.app"
    }
  }
}
```

## Usage

### Command Line Interface

```bash
# Test URL routing
browroute test https://github.com/SilkRoadProfessionals/pcbe-api

# Open URL (actually launches browser)
browroute open https://youtube.com

# Show configuration
browroute config

# Interactive mode
browroute interactive

# Reload configuration
browroute reload

# Show help
browroute --help
```

### Interactive Mode

```bash
browroute interactive

> test https://portal.azure.com
✓ Rule matched!
  Rule: PCBE - Azure
  Browser: Dia
  Profile: PCBE

> config
Configuration Summary
=====================
...

> exit
```

## Testing

### Manual Testing

```bash
# Test personal sites
browroute test https://youtube.com
# Should match "Personal Sites" → Brave

# Test GitHub org routing
browroute test https://github.com/SilkRoadProfessionals/pcbe-api
# Should match "PCBE - GitHub" → Dia (PCBE profile)

browroute test https://github.com/beneviasoftware/erp
# Should match "Benevia - GitHub" → Dia (Benevia profile)

# Test Azure routing
browroute test "https://portal.azure.com?account=pcbe"
# Should match "PCBE - Azure" → Dia (PCBE profile)
```

### Unit Tests (TODO)

```bash
# Add test project
dotnet new xunit -n BrowRoute.Tests -o tests/BrowRoute.Tests
dotnet sln add tests/BrowRoute.Tests/BrowRoute.Tests.csproj
dotnet add tests/BrowRoute.Tests reference src/BrowRoute.Core/BrowRoute.Core.csproj

# Run tests
dotnet test
```

## Registering as Default Browser

### macOS

1. Build and install the .app bundle:
```bash
./scripts/build-macos.sh
cp -r build/BrowRoute.app /Applications/
```

2. Open System Settings:
   - Go to **Desktop & Dock**
   - Scroll to **Default web browser**
   - Select **BrowRoute**

3. The Info.plist is configured to handle `http://` and `https://` URLs

### Troubleshooting

If BrowRoute doesn't appear in the default browser list:

1. Check the .app bundle is valid:
```bash
/System/Library/Frameworks/CoreServices.framework/Frameworks/LaunchServices.framework/Support/lsregister -f /Applications/BrowRoute.app
```

2. Restart your Mac

3. Check console for errors:
```bash
log show --predicate 'process == "BrowRoute"' --last 1h
```

## Development

### IDE Setup

**Visual Studio Code:**
```bash
# Install C# Dev Kit extension
code --install-extension ms-dotnettools.csdevkit

# Open workspace
code BrowRoute.sln
```

**Visual Studio 2022:**
- Open `BrowRoute.sln`
- Set `BrowRoute.App` as startup project
- Press F5 to debug

**JetBrains Rider:**
- Open `BrowRoute.sln`
- Right-click `BrowRoute.App` → Set as Startup Project
- Run/Debug

### Hot Reload

```bash
# Run with hot reload
dotnet watch --project src/BrowRoute.App/BrowRoute.App.csproj
```

### Debug Configuration

Add to `.vscode/launch.json`:
```json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": "BrowRoute",
      "type": "coreclr",
      "request": "launch",
      "preLaunchTask": "build",
      "program": "${workspaceFolder}/src/BrowRoute.App/bin/Debug/net9.0/BrowRoute.App.dll",
      "args": ["test", "https://github.com/SilkRoadProfessionals/pcbe-api"],
      "cwd": "${workspaceFolder}/src/BrowRoute.App",
      "console": "integratedTerminal"
    }
  ]
}
```

## Advanced Features

### Profile Auto-Detection

To support automatic profile detection in Dia browser:

1. Update `BrowserLauncher.cs` with Dia-specific profile discovery
2. Use AppleScript to query available profiles
3. Cache profile list for performance

### Menu Bar App (Future)

Consider using:
- **Avalonia UI** for cross-platform GUI
- **dotnet-maui** for native macOS UI
- **Eto.Forms** for lightweight GUI

## Troubleshooting

### "Cannot find browser executable"

Check browser paths in config match your installation:
```bash
ls -la /Applications/Brave\ Browser.app
ls -la /Applications/Dia.app
```

### "No rule matched" for expected URLs

Test pattern matching:
```bash
browroute test <your-url>
```

Check patterns use correct syntax:
- Glob: `github.com/*` (matches any path)
- Regex: Enable with `"patternType": "regex"`

### Config not loading

Check config file location:
```bash
cat ~/Library/Application\ Support/BrowRoute/config.json
```

Validate JSON syntax:
```bash
cat ~/Library/Application\ Support/BrowRoute/config.json | python3 -m json.tool
```

## Performance

Expected performance:
- URL routing: < 5ms
- App startup: < 500ms
- Memory: ~30-50MB (self-contained) or ~100MB (framework-dependent)

## Packaging for Distribution

### Create Installer (TODO)

```bash
# Using pkgbuild
pkgbuild --root build/BrowRoute.app \
         --identifier com.doniyorniazov.browroute \
         --version 1.0.0 \
         --install-location /Applications/BrowRoute.app \
         BrowRoute-1.0.0.pkg
```

### Code Signing (TODO)

```bash
# Sign the app
codesign --force --deep --sign "Developer ID Application: Your Name" \
         build/BrowRoute.app

# Notarize for Gatekeeper
xcrun notarytool submit BrowRoute-1.0.0.pkg \
         --apple-id your@email.com \
         --team-id TEAMID \
         --password @keychain:AC_PASSWORD
```

## License

MIT License - See LICENSE file for details
