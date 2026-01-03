# .NET BrowRoute Implementation

This document provides a quick overview of the .NET implementation.

## Quick Start

```bash
# Build the project
dotnet build

# Run interactive mode
dotnet run --project src/BrowRoute.App/BrowRoute.App.csproj -- interactive

# Test a URL
dotnet run --project src/BrowRoute.App/BrowRoute.App.csproj -- test https://github.com/SilkRoadProfessionals/pcbe-api
```

## Architecture

### Core Components

1. **Models** (`BrowRoute.Core/Models/Config.cs`)
   - `Config`: Main configuration model
   - `Rule`: URL routing rule
   - `Team`: Team configuration
   - `BrowserConfig`: Browser settings
   - `RuleMatch`: Match result

2. **Services**
   - `ConfigManager`: Loads and validates configuration from JSON
   - `RuleEngine`: Pattern matching and rule evaluation
   - `BrowserLauncher`: Opens URLs in browsers with profile support
   - `URLHandlerService`: Main URL routing coordinator

3. **Platform** (`BrowRoute.Core/Platform/`)
   - `URLHandlerService`: macOS URL handling and registration
   - `MacOSInterop`: P/Invoke for native macOS APIs (partial implementation)

### Data Flow

```
URL Input
   ↓
URLHandlerService.HandleURL()
   ↓
RuleEngine.Match() → Find matching rule
   ↓
BrowserLauncher.Open() → Launch browser
   ↓
Process.Start() / AppleScript
```

## Key Features

### Pattern Matching

The `RuleEngine` supports three pattern types:

```csharp
// Glob (default) - wildcards
"patterns": ["github.com/*", "*.example.com/*"]

// Regex - full regex support
"patternType": "regex",
"patterns": ["github\\.com/[a-z]+/.*"]

// Exact - exact match only
"patternType": "exact",
"patterns": ["youtube.com"]
```

### Browser Profile Switching

Supports three methods:

1. **AppleScript** - For browsers with AppleScript support
2. **CLI Arguments** - Pass `--profile=name` to browser
3. **URL Parameters** - Add profile to URL (browser-specific)

### Configuration Hot Reload

```csharp
configManager.WatchConfig(() => {
    Console.WriteLine("Config changed, reloading...");
    ruleEngine.LoadRules();
});
```

## Testing

### Manual Testing

```bash
# Test personal sites (should route to Brave)
dotnet run --project src/BrowRoute.App -- test https://youtube.com

# Test team GitHub (should route to Dia with profile)
dotnet run --project src/BrowRoute.App -- test https://github.com/SilkRoadProfessionals/pcbe-api

# Test Azure (should route to Dia with profile)
dotnet run --project src/BrowRoute.App -- test "https://portal.azure.com?subscription=pcbe-prod"
```

### Interactive Mode

```bash
dotnet run --project src/BrowRoute.App -- interactive
```

## Building for Distribution

See [BUILD.md](BUILD.md) for detailed build instructions.

Quick build:
```bash
./scripts/build-macos.sh
```

## Configuration

Configuration file location:
- macOS: `~/Library/Application Support/BrowRoute/config.json`
- Default: Created from `config.example.json` on first run

## Next Steps

1. ✅ Build and test CLI version
2. ⬜ Register as default browser
3. ⬜ Add GUI (optional - Avalonia/MAUI)
4. ⬜ Add unit tests
5. ⬜ Package for distribution
6. ⬜ Code signing and notarization

## Limitations

Current limitations in .NET implementation:

1. **URL Registration**: Requires proper .app bundle with Info.plist (build script creates this)
2. **Profile Switching**: Dia browser profile switching needs browser-specific implementation
3. **macOS APIs**: Some features require Objective-C interop via P/Invoke

## Extending

### Adding a New Browser

1. Update `config.json`:
```json
{
  "browsers": {
    "Chrome": {
      "bundleId": "com.google.Chrome",
      "executablePath": "/Applications/Google Chrome.app",
      "supportsProfiles": true,
      "profileSwitchMethod": "cli-argument"
    }
  }
}
```

2. Add rules targeting the browser:
```json
{
  "rules": [
    {
      "name": "Work Gmail",
      "priority": 80,
      "browser": "Chrome",
      "profile": "Work",
      "patterns": ["mail.google.com/*"]
    }
  ]
}
```

### Custom Pattern Matching

Extend `RuleEngine.cs` to add new pattern types:

```csharp
private bool MatchesRule(Uri url, string host, string path, string fullUrl, Rule rule)
{
    switch (rule.PatternType)
    {
        case "custom":
            return CustomMatcher(url, rule);
        // ... existing cases
    }
}
```

## Performance Tips

1. **Rule Priority**: Higher priority rules are evaluated first - put common rules at top
2. **Pattern Specificity**: More specific patterns are faster than broad wildcards
3. **Config Caching**: Configuration is cached until file changes

## Troubleshooting

### Common Issues

**"Cannot find browser executable"**
- Check browser paths in config
- Use `ls -la /Applications/` to verify installation

**"No rule matched"**
- Use `test` command to debug pattern matching
- Check pattern syntax (glob vs regex)

**Config not loading**
- Verify JSON syntax: `cat config.json | python3 -m json.tool`
- Check file permissions

## Resources

- [BUILD.md](BUILD.md) - Detailed build instructions
- [ARCHITECTURE.md](../ARCHITECTURE.md) - System architecture
- [config.example.json](../config.example.json) - Example configuration
