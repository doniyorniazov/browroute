#!/bin/bash

# BrowRoute Build Script for macOS
# Builds the .NET application and creates a macOS .app bundle

set -e

echo "🌐 Building BrowRoute for macOS..."

# Configuration
PROJECT_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
APP_NAME="BrowRoute"
BUNDLE_ID="com.doniyorniazov.browroute"
OUTPUT_DIR="$PROJECT_ROOT/build"
APP_BUNDLE="$OUTPUT_DIR/$APP_NAME.app"
PUBLISH_DIR="$PROJECT_ROOT/src/BrowRoute.App/bin/Release/net9.0/osx-arm64/publish"

# Clean previous build
echo "Cleaning previous build..."
rm -rf "$OUTPUT_DIR"
mkdir -p "$OUTPUT_DIR"

# Build and publish .NET app
echo "Building .NET application..."
cd "$PROJECT_ROOT"
dotnet publish src/BrowRoute.App/BrowRoute.App.csproj \
    -c Release \
    -r osx-arm64 \
    --self-contained \
    -p:PublishSingleFile=true \
    -p:PublishTrimmed=true \
    -p:TrimMode=partial

# Create .app bundle structure
echo "Creating .app bundle..."
mkdir -p "$APP_BUNDLE/Contents/MacOS"
mkdir -p "$APP_BUNDLE/Contents/Resources"

# Copy executable
cp "$PUBLISH_DIR/BrowRoute.App" "$APP_BUNDLE/Contents/MacOS/$APP_NAME"
chmod +x "$APP_BUNDLE/Contents/MacOS/$APP_NAME"

# Copy configuration
if [ -f "$PROJECT_ROOT/config.example.json" ]; then
    cp "$PROJECT_ROOT/config.example.json" "$APP_BUNDLE/Contents/Resources/config.json"
fi

# Create Info.plist
cat > "$APP_BUNDLE/Contents/Info.plist" << EOF
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>CFBundleName</key>
    <string>$APP_NAME</string>
    <key>CFBundleDisplayName</key>
    <string>$APP_NAME</string>
    <key>CFBundleIdentifier</key>
    <string>$BUNDLE_ID</string>
    <key>CFBundleVersion</key>
    <string>1.0.0</string>
    <key>CFBundleShortVersionString</key>
    <string>1.0.0</string>
    <key>CFBundlePackageType</key>
    <string>APPL</string>
    <key>CFBundleExecutable</key>
    <string>$APP_NAME</string>
    <key>LSMinimumSystemVersion</key>
    <string>13.0</string>
    <key>NSHighResolutionCapable</key>
    <true/>
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
    <key>NSAppleEventsUsageDescription</key>
    <string>BrowRoute needs to control other browsers to open URLs with the correct profile.</string>
</dict>
</plist>
EOF

echo "✓ Build complete: $APP_BUNDLE"
echo ""
echo "To install:"
echo "  1. Copy $APP_NAME.app to /Applications"
echo "  2. Open System Settings → Desktop & Dock"
echo "  3. Set $APP_NAME as default web browser"
echo ""
echo "Or run: cp -r \"$APP_BUNDLE\" /Applications/"
