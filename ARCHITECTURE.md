# BrowRoute - Service Architecture

## Overview
BrowRoute is a smart browser routing application for macOS that acts as a default browser and intelligently routes URLs to appropriate browsers and profiles based on configurable rules.

## Technology Stack Recommendation

### Primary Recommendation: Swift (Native macOS)
**Pros:**
- Native macOS integration with NSWorkspace and URL handlers
- Best performance and user experience
- Direct access to macOS APIs for browser profile management
- Can register as default browser easily
- Lower resource footprint

**Cons:**
- macOS only (but that's your target)
- Requires learning Swift if not familiar

### Alternative 1: .NET 8+ (C#)
**Pros:**
- You're familiar with .NET
- Cross-platform potential
- Good performance with native AOT compilation
- Can use P/Invoke for macOS APIs

**Cons:**
- Heavier runtime
- More complex for deep macOS integration
- Limited native UI frameworks (would need Avalonia or .NET MAUI)

### Alternative 2: Python + PyObjC
**Pros:**
- Quick prototyping
- PyObjC provides macOS API access
- Easy configuration parsing

**Cons:**
- Slower performance
- Requires bundling Python runtime
- Less polished for production apps

**Recommendation: Swift** for production-ready app, or **.NET** if you want to leverage existing expertise.

## Architecture Components

```
┌─────────────────────────────────────────────────────────┐
│                     BrowRoute App                        │
└─────────────────────────────────────────────────────────┘
                            │
        ┌───────────────────┼───────────────────┐
        │                   │                   │
        ▼                   ▼                   ▼
┌──────────────┐   ┌─────────────────┐  ┌─────────────┐
│  URL Handler │   │  Rule Engine    │  │  Config     │
│   Service    │   │                 │  │  Manager    │
└──────────────┘   └─────────────────┘  └─────────────┘
        │                   │                   │
        └───────────────────┼───────────────────┘
                            ▼
                ┌───────────────────────┐
                │  Browser Launcher     │
                │  Service              │
                └───────────────────────┘
                            │
        ┌───────────────────┼───────────────────┐
        ▼                   ▼                   ▼
┌──────────────┐   ┌─────────────────┐  ┌─────────────┐
│    Brave     │   │   Dia Browser   │  │   Comet     │
│   Browser    │   │   (Profiles)    │  │             │
└──────────────┘   └─────────────────┘  └─────────────┘
```

## Core Components

### 1. URL Handler Service
**Responsibility:** Register and intercept URL open requests
- Registers app as default browser handler
- Receives URL open events from macOS
- Passes URLs to Rule Engine

**Technical Implementation:**
- Register custom URL scheme handler (`http://`, `https://`)
- Implement `NSApplicationDelegate` methods
- Handle `application(_:openURLs:)` callbacks

### 2. Rule Engine
**Responsibility:** Match URLs against configured rules
- Pattern matching (domain, path, query parameters)
- Priority-based rule evaluation
- Team/organization identification
- Profile selection logic

**Rule Matching Logic:**
```
Input: URL
↓
1. Parse URL (domain, path, params)
↓
2. Check Personal Rules (exact domain match)
   → If match: return Brave
↓
3. Check Research Rules
   → If match: return Comet
↓
4. Check Team-Specific Rules (pattern match)
   → Azure: Check subscription/tenant ID in URL
   → GitHub: Check organization in path
   → Other: Domain-based routing
↓
5. Return default browser if no match
```

### 3. Configuration Manager
**Responsibility:** Load, validate, and manage routing rules
- Load configuration from JSON/YAML file
- Validate rule syntax
- Hot-reload configuration changes
- Provide UI for editing rules

**Configuration Schema:**
```json
{
  "version": "1.0",
  "defaultBrowser": "Brave",
  "rules": [
    {
      "name": "Personal Sites",
      "priority": 100,
      "browser": "Brave",
      "patterns": ["youtube.com/*", "instagram.com/*", "facebook.com/*"]
    },
    {
      "name": "Research",
      "priority": 90,
      "browser": "Comet",
      "patterns": ["*.research/*", "perplexity.ai/*"]
    },
    {
      "name": "Benevia Azure",
      "priority": 80,
      "browser": "Dia",
      "profile": "Benevia",
      "patterns": [
        "portal.azure.com/*",
        "dev.azure.com/*"
      ],
      "conditions": {
        "queryContains": ["silkroadprofessionals.com"],
        "or": {
          "pathContains": ["/benevia", "/BEN-"]
        }
      }
    },
    {
      "name": "Benevia GitHub",
      "priority": 80,
      "browser": "Dia",
      "profile": "Benevia",
      "patterns": ["github.com/beneviasoftware/*"]
    }
  ],
  "teams": [
    {
      "name": "Benevia",
      "cloud": "Azure",
      "domains": ["silkroadprofessionals.com"],
      "githubOrgs": ["beneviasoftware"],
      "browser": "Dia",
      "profile": "Benevia"
    },
    {
      "name": "DoorLink",
      "cloud": "Azure",
      "domains": ["premierellc.com"],
      "githubOrgs": ["doniyorniazov"],
      "browser": "Dia",
      "profile": "DoorLink"
    },
    {
      "name": "PCBE",
      "cloud": "Azure",
      "domains": ["silkroadprofessionals.com"],
      "githubOrgs": ["SilkRoadProfessionals"],
      "githubRepoPatterns": ["SilkRoadProfessionals/pcbe*"],
      "browser": "Dia",
      "profile": "PCBE"
    },
    {
      "name": "Rosewood",
      "cloud": "GCP",
      "domains": ["gmail.com"],
      "githubOrgs": ["rosewood-org"],
      "browser": "Dia",
      "profile": "Rosewood"
    },
    {
      "name": "Run3D",
      "cloud": "AWS",
      "domains": ["gmail.com"],
      "githubOrgs": ["run3d-org"],
      "browser": "Dia",
      "profile": "Run3D"
    },
    {
      "name": "Silk Road Professionals",
      "domains": ["app.ninety.io", "silkroadprofessionals.com"],
      "githubOrgs": ["doniyorniazov"],
      "browser": "Dia",
      "profile": "SilkRoad"
    }
  ],
  "browsers": {
    "Brave": {
      "bundleId": "com.brave.Browser",
      "executablePath": "/Applications/Brave Browser.app",
      "supportsProfiles": false
    },
    "Dia": {
      "bundleId": "com.dia.browser",
      "executablePath": "/Applications/Dia.app",
      "supportsProfiles": true,
      "profileSwitchMethod": "url-parameter"
    },
    "Comet": {
      "bundleId": "ai.perplexity.comet",
      "executablePath": "/Applications/Comet.app",
      "supportsProfiles": false
    }
  }
}
```

### 4. Browser Launcher Service
**Responsibility:** Open URLs in specified browsers with profiles
- Launch browser applications
- Pass profile parameters if supported
- Handle browser-specific URL opening methods
- Fallback to default browser on error

**Browser Integration Methods:**
- **Standard:** Use `NSWorkspace.open(URL, withApplicationAt:)`
- **Dia Browser:** Use profile switching via AppleScript or URL parameters
- **Profile Management:** Learn and store profile switching mechanisms per browser

### 5. UI Components (Optional but Recommended)

#### Menu Bar App
- Lives in menu bar
- Quick access to configuration
- Shows current routing rules
- Manual browser selection override
- Stats/logging viewer

#### Preferences Window
- Visual rule editor
- Test URL against rules
- Browser detection
- Profile discovery
- Import/export configuration

## Data Flow

### URL Opening Flow
```
1. User clicks link/opens URL
   ↓
2. macOS sends URL to BrowRoute (default handler)
   ↓
3. URL Handler receives URL
   ↓
4. Rule Engine evaluates URL
   ↓
5. Configuration Manager provides matching rule
   ↓
6. Browser Launcher opens URL in target browser
   ↓
7. Log event (optional)
```

### Configuration Update Flow
```
1. User edits config file / uses UI
   ↓
2. Configuration Manager detects change
   ↓
3. Validate new configuration
   ↓
4. Reload rules into Rule Engine
   ↓
5. Notify user of successful reload
```

## Module Structure

```
BrowRoute/
├── BrowRouteApp/                 # Main application
│   ├── AppDelegate.swift         # App lifecycle & URL handling
│   ├── Models/
│   │   ├── Rule.swift           # Rule data models
│   │   ├── Browser.swift        # Browser configuration
│   │   └── Team.swift           # Team configuration
│   ├── Services/
│   │   ├── URLHandlerService.swift
│   │   ├── RuleEngine.swift
│   │   ├── ConfigManager.swift
│   │   ├── BrowserLauncher.swift
│   │   └── PatternMatcher.swift
│   ├── UI/
│   │   ├── MenuBarController.swift
│   │   ├── PreferencesWindow.swift
│   │   └── RuleEditor.swift
│   └── Resources/
│       └── default-config.json
├── BrowRouteTests/
└── BrowRoute.xcodeproj
```

## Advanced Features

### Phase 1 (MVP)
- [x] Register as default browser
- [x] Basic pattern matching (domain exact match)
- [x] Static configuration file
- [x] Launch Brave, Dia, Comet browsers
- [x] Basic menu bar presence

### Phase 2 (Enhanced)
- [ ] Regex pattern matching
- [ ] Profile auto-detection for Dia
- [ ] URL testing tool in UI
- [ ] Rule priority and conflict resolution
- [ ] Import/export configuration
- [ ] Multiple configuration profiles

### Phase 3 (Advanced)
- [ ] Machine learning-based URL classification
- [ ] Automatic team detection from URL content
- [ ] Browser session management
- [ ] URL history and analytics
- [ ] Cloud config sync
- [ ] Browser automation (close duplicate tabs)

## Security Considerations

1. **Configuration Validation:**
   - Validate all patterns to prevent regex DoS
   - Sanitize paths and bundle IDs
   - Verify browser executables exist

2. **Privacy:**
   - All routing happens locally
   - No URL logging unless explicitly enabled
   - User controls all data

3. **Sandbox Limitations:**
   - May need to disable sandboxing for browser launching
   - Request appropriate entitlements

## Distribution

### Development
- Build and run from Xcode
- Configuration in `~/Library/Application Support/BrowRoute/`

### Production
- Notarized .app bundle
- Installer package (.pkg) with setup wizard
- Auto-update mechanism (Sparkle framework)
- Homebrew cask for easy installation

## Performance Targets

- URL routing decision: < 10ms
- Application launch: < 500ms
- Memory footprint: < 50MB
- CPU usage (idle): < 0.1%

## Error Handling

1. **Rule Match Failure:** Fall back to default browser
2. **Browser Not Found:** Prompt user to configure browser path
3. **Configuration Error:** Load last known good config + notify user
4. **Profile Not Found:** Open in browser without profile switching

## Testing Strategy

1. **Unit Tests:**
   - Rule matching logic
   - Pattern matching
   - Configuration parsing

2. **Integration Tests:**
   - URL handling workflow
   - Browser launching
   - Profile switching

3. **Manual Testing:**
   - Test each team's URLs
   - Verify profile switching in Dia
   - Test edge cases (malformed URLs)

## Next Steps

1. Set up Swift Xcode project
2. Implement URL handler registration
3. Build basic rule engine with pattern matching
4. Create configuration loader
5. Implement browser launcher
6. Build menu bar UI
7. Add preferences window
8. Test with real URLs from each team
9. Package and distribute
