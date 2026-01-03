# BrowRoute 🌐

**Intelligent Browser Routing for macOS**

BrowRoute is a smart default browser replacement for macOS that automatically routes URLs to the right browser and profile based on configurable rules. Perfect for professionals managing multiple teams, cloud accounts, and GitHub organizations.

## Problem

Working with multiple teams means juggling:
- Multiple cloud provider accounts (Azure, AWS, GCP)
- Different email domains and GitHub organizations
- Various browser profiles for each team
- Personal vs. work browsing contexts

Opening links in the wrong browser or profile disrupts workflow and requires manual switching.

## Solution

BrowRoute acts as your default browser and intelligently routes each URL to the appropriate browser with the correct profile:

- **Personal sites** (YouTube, Instagram) → Brave Browser
- **Research** → Comet (Perplexity)
- **Team-specific work** → Dia Browser with correct profile
  - GitHub org detection
  - Cloud provider account matching
  - Domain-based routing

## Features

- ✅ **Smart URL Routing** - Pattern matching with wildcards and conditions
- ✅ **Multi-Browser Support** - Works with any browser (Brave, Dia, Chrome, Safari, etc.)
- ✅ **Profile Management** - Automatic profile switching for supported browsers
- ✅ **Team Configuration** - Define teams with cloud providers, domains, and GitHub orgs
- ✅ **Priority Rules** - Control rule evaluation order
- ✅ **Menu Bar App** - Quick access to settings and manual overrides
- ✅ **Fast & Lightweight** - Native macOS app with minimal resource usage

## Quick Start

### 1. Configuration

Copy `config.example.json` to `~/Library/Application Support/BrowRoute/config.json` and customize:

```json
{
  "teams": [
    {
      "name": "Your Team",
      "cloud": "Azure",
      "githubOrgs": ["your-org"],
      "browser": "Dia",
      "profile": "TeamProfile"
    }
  ]
}
```

### 2. Set as Default Browser

1. Open **System Settings** → **Desktop & Dock**
2. Scroll to "Default web browser"
3. Select **BrowRoute**

### 3. Open Any Link

BrowRoute will automatically route to the correct browser!

## Architecture

BrowRoute consists of four core services:

```
URL → URL Handler → Rule Engine → Browser Launcher
              ↓
        Config Manager
```

- **URL Handler** - Intercepts URLs from macOS
- **Rule Engine** - Matches URLs against configured rules
- **Config Manager** - Loads and validates configuration
- **Browser Launcher** - Opens URLs in target browsers with profiles

See [ARCHITECTURE.md](ARCHITECTURE.md) for detailed design.

## Implementation

Two implementation options:

### Option 1: Swift (Recommended)
- **Pros:** Native performance, small footprint, better macOS integration
- **Cons:** Swift-specific knowledge required
- **Best for:** Production app with App Store distribution

### Option 2: .NET 8+
- **Pros:** Familiar C# syntax, excellent debugging, cross-platform potential
- **Cons:** Larger bundle, more complex macOS integration
- **Best for:** Rapid prototyping or if you prefer C#

See [IMPLEMENTATION.md](IMPLEMENTATION.md) for code examples and setup guides.

## Configuration

### Example Rules

```json
{
  "rules": [
    {
      "name": "GitHub - Benevia",
      "priority": 90,
      "browser": "Dia",
      "profile": "Benevia",
      "patterns": ["github.com/beneviasoftware/*"]
    },
    {
      "name": "Azure - PCBE",
      "priority": 85,
      "browser": "Dia",
      "profile": "PCBE",
      "patterns": ["portal.azure.com/*"],
      "conditions": {
        "queryContains": ["pcbe"]
      }
    }
  ]
}
```

### Supported Pattern Types

- **Glob** (default): `youtube.com/*`, `*.github.io/*`
- **Regex**: Full regex support for complex patterns
- **Exact**: Exact domain/path matching

### Conditions

Add conditional matching:
- `queryContains` - Match URL parameters
- `pathContains` - Match path segments
- `pathMatches` - Regex path matching
- `or` / `and` - Combine conditions

## Project Structure

```
BrowRoute/
├── ARCHITECTURE.md           # System design and architecture
├── IMPLEMENTATION.md         # Implementation guide (Swift & .NET)
├── config.example.json       # Example configuration
├── config-schema.json        # JSON schema for validation
└── README.md                 # This file
```

## Roadmap

### Phase 1 (MVP)
- [x] Architecture design
- [x] Configuration schema
- [ ] Swift implementation
- [ ] Basic pattern matching
- [ ] Browser launching

### Phase 2 (Enhanced)
- [ ] Regex pattern support
- [ ] Profile auto-detection
- [ ] UI for rule editing
- [ ] Rule testing tool
- [ ] Import/export configs

### Phase 3 (Advanced)
- [ ] ML-based URL classification
- [ ] Browser session management
- [ ] URL history and analytics
- [ ] Cloud config sync

## Contributing

Contributions welcome! Please:
1. Read [ARCHITECTURE.md](ARCHITECTURE.md) for design principles
2. Follow Swift/SwiftUI best practices
3. Add tests for new features
4. Update documentation

## License

MIT License - See LICENSE file for details

## Support

- **Issues:** [GitHub Issues](https://github.com/doniyorniazov/browroute/issues)
- **Discussions:** [GitHub Discussions](https://github.com/doniyorniazov/browroute/discussions)

---

**Built with ❤️ for multi-team professionals**
