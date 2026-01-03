# Swift Implementation Guide

## Prerequisites
- macOS 13.0+ (Ventura or later)
- Xcode 15+
- Swift 5.9+

## Quick Start with Swift

### 1. Create Xcode Project
```bash
# Open Xcode
# File → New → Project
# macOS → App
# Product Name: BrowRoute
# Interface: SwiftUI
# Language: Swift
```

### 2. Register as Default Browser

Add to `Info.plist`:
```xml
<key>CFBundleURLTypes</key>
<array>
    <dict>
        <key>CFBundleURLName</key>
        <string>Web Browser</string>
        <key>CFBundleURLSchemes</key>
        <array>
            <string>http</string>
            <string>https</string>
        </array>
        <key>CFBundleTypeRole</key>
        <string>Viewer</string>
    </dict>
</array>
```

### 3. Project Structure
```
BrowRoute/
├── BrowRouteApp.swift           # Main app entry
├── Models/
│   ├── Rule.swift              # Rule data model
│   ├── Browser.swift           # Browser configuration
│   ├── Team.swift              # Team configuration
│   └── Config.swift            # Configuration model
├── Services/
│   ├── URLHandlerService.swift # URL interception
│   ├── RuleEngine.swift        # Pattern matching
│   ├── ConfigManager.swift     # Config loading
│   └── BrowserLauncher.swift   # Browser opening
├── Views/
│   ├── MenuBarView.swift       # Menu bar UI
│   ├── PreferencesView.swift   # Settings UI
│   └── RuleEditorView.swift    # Rule editing UI
└── Resources/
    └── config.json             # Default config
```

## Key Implementation Files

### BrowRouteApp.swift
```swift
import SwiftUI

@main
struct BrowRouteApp: App {
    @NSApplicationDelegateAdaptor(AppDelegate.self) var appDelegate
    
    var body: some Scene {
        MenuBarExtra("BrowRoute", systemImage: "globe") {
            MenuBarView()
        }
        .menuBarExtraStyle(.window)
    }
}

class AppDelegate: NSObject, NSApplicationDelegate {
    let urlHandler = URLHandlerService()
    
    func application(_ application: NSApplication, open urls: [URL]) {
        for url in urls {
            urlHandler.handleURL(url)
        }
    }
    
    func applicationDidFinishLaunching(_ notification: Notification) {
        // Register as default browser
        LSSetDefaultHandlerForURLScheme("http" as CFString, 
                                       "com.yourcompany.browroute" as CFString)
        LSSetDefaultHandlerForURLScheme("https" as CFString, 
                                       "com.yourcompany.browroute" as CFString)
    }
}
```

### URLHandlerService.swift
```swift
import Foundation

class URLHandlerService {
    private let ruleEngine = RuleEngine()
    private let browserLauncher = BrowserLauncher()
    
    func handleURL(_ url: URL) {
        // Get matching rule
        guard let match = ruleEngine.match(url: url) else {
            // Open in default browser
            browserLauncher.openDefault(url: url)
            return
        }
        
        // Open in matched browser with profile
        browserLauncher.open(url: url, 
                           browser: match.browser, 
                           profile: match.profile)
    }
}
```

### RuleEngine.swift
```swift
import Foundation

struct RuleMatch {
    let browser: String
    let profile: String?
}

class RuleEngine {
    private var rules: [Rule] = []
    private let configManager = ConfigManager()
    
    init() {
        loadRules()
    }
    
    func match(url: URL) -> RuleMatch? {
        // Sort rules by priority (descending)
        let sortedRules = rules.sorted { $0.priority > $1.priority }
        
        for rule in sortedRules {
            if matchesRule(url: url, rule: rule) {
                return RuleMatch(browser: rule.browser, 
                               profile: rule.profile)
            }
        }
        
        return nil
    }
    
    private func matchesRule(url: URL, rule: Rule) -> Bool {
        let host = url.host ?? ""
        let path = url.path
        
        // Check patterns
        for pattern in rule.patterns {
            if matchesPattern(host: host, path: path, pattern: pattern) {
                // Check additional conditions if present
                if let conditions = rule.conditions {
                    return matchesConditions(url: url, conditions: conditions)
                }
                return true
            }
        }
        
        return false
    }
    
    private func matchesPattern(host: String, path: String, pattern: String) -> Bool {
        let fullURL = host + path
        
        // Convert glob pattern to regex
        let regexPattern = pattern
            .replacingOccurrences(of: ".", with: "\\.")
            .replacingOccurrences(of: "*", with: ".*")
        
        guard let regex = try? NSRegularExpression(pattern: regexPattern) else {
            return false
        }
        
        let range = NSRange(fullURL.startIndex..., in: fullURL)
        return regex.firstMatch(in: fullURL, range: range) != nil
    }
    
    private func matchesConditions(url: URL, conditions: Conditions) -> Bool {
        // Implement condition matching logic
        // Check queryContains, pathContains, etc.
        return true
    }
    
    private func loadRules() {
        rules = configManager.loadConfig().rules
    }
}
```

### BrowserLauncher.swift
```swift
import AppKit

class BrowserLauncher {
    func open(url: URL, browser: String, profile: String?) {
        let config = ConfigManager().loadConfig()
        
        guard let browserConfig = config.browsers[browser] else {
            openDefault(url: url)
            return
        }
        
        let appURL = URL(fileURLWithPath: browserConfig.executablePath)
        
        if browserConfig.supportsProfiles, let profile = profile {
            // Open with profile
            openWithProfile(url: url, appURL: appURL, profile: profile, 
                          method: browserConfig.profileSwitchMethod)
        } else {
            // Open without profile
            NSWorkspace.shared.open([url], 
                                   withApplicationAt: appURL,
                                   configuration: NSWorkspace.OpenConfiguration())
        }
    }
    
    private func openWithProfile(url: URL, appURL: URL, 
                                profile: String, method: String?) {
        switch method {
        case "applescript":
            openWithAppleScript(url: url, appURL: appURL, profile: profile)
        case "cli-argument":
            openWithCLI(url: url, appPath: appURL.path, profile: profile)
        default:
            // Fallback to default
            NSWorkspace.shared.open([url], 
                                   withApplicationAt: appURL,
                                   configuration: NSWorkspace.OpenConfiguration())
        }
    }
    
    private func openWithAppleScript(url: URL, appURL: URL, profile: String) {
        // Use AppleScript for profile switching
        let script = """
        tell application "\(appURL.lastPathComponent.replacingOccurrences(of: ".app", with: ""))"
            activate
            open location "\(url.absoluteString)" with profile "\(profile)"
        end tell
        """
        
        if let appleScript = NSAppleScript(source: script) {
            appleScript.executeAndReturnError(nil)
        }
    }
    
    private func openWithCLI(url: URL, appPath: String, profile: String) {
        let task = Process()
        task.executableURL = URL(fileURLWithPath: appPath + "/Contents/MacOS/Dia")
        task.arguments = ["--profile=\(profile)", url.absoluteString]
        try? task.run()
    }
    
    func openDefault(url: URL) {
        NSWorkspace.shared.open(url)
    }
}
```

## Alternative: .NET Implementation

If you prefer .NET, here's the approach:

### Technology Stack
- **.NET 8.0+** with native AOT
- **Avalonia UI** for cross-platform UI (or AppKit bindings)
- **P/Invoke** for macOS APIs

### Key Considerations
1. Use `LSSetDefaultHandlerForURLScheme` via P/Invoke
2. Implement URL handler using `NSApplication` delegates via Objective-C bridge
3. Package as `.app` bundle with proper Info.plist

### Pros of .NET Approach
- Familiar C# syntax
- Better debugging tools
- Easier testing with xUnit
- Potential Windows/Linux support later

### Cons of .NET Approach
- Larger app bundle size (~60MB vs ~5MB Swift)
- More complex macOS API integration
- Less community support for macOS-specific features

## Recommended Approach

**For Production: Use Swift**
- Native performance
- Smaller footprint
- Better macOS integration
- Easier App Store distribution

**For Rapid Prototyping: Use .NET**
- Faster development if you know C#
- Better suited if you want cross-platform later
- Good testing infrastructure

## Next Steps

1. **Choose your stack** (Swift recommended)
2. **Set up project** using the structure above
3. **Implement core services** (URLHandler, RuleEngine, ConfigManager, BrowserLauncher)
4. **Test with real URLs** from your teams
5. **Build menu bar UI** for configuration
6. **Package and distribute**

Would you like me to:
1. Generate the full Swift Xcode project?
2. Create a .NET version instead?
3. Build a specific component in detail?
