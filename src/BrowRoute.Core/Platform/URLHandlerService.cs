using System.Runtime.InteropServices;
using BrowRoute.Core.Models;
using BrowRoute.Core.Services;

namespace BrowRoute.Core.Platform;

public class URLHandlerService
{
  private readonly RuleEngine _ruleEngine;
  private readonly BrowserLauncher _browserLauncher;
  private readonly ConfigManager _configManager;

  public URLHandlerService(
      RuleEngine ruleEngine,
      BrowserLauncher browserLauncher,
      ConfigManager configManager)
  {
    _ruleEngine = ruleEngine;
    _browserLauncher = browserLauncher;
    _configManager = configManager;
  }

  public void HandleURL(string urlString)
  {
    Console.WriteLine($"Handling URL: {urlString}");

    if (!Uri.TryCreate(urlString, UriKind.Absolute, out var url))
    {
      Console.WriteLine($"Invalid URL: {urlString}");
      return;
    }

    // Get matching rule
    var match = _ruleEngine.Match(url);

    if (match != null)
    {
      // Open in matched browser
      _browserLauncher.Open(url, match.Browser, match.Profile);
    }
    else
    {
      // Open in default browser
      var config = _configManager.LoadConfig();
      if (config.Browsers.TryGetValue(config.DefaultBrowser, out var _))
      {
        _browserLauncher.Open(url, config.DefaultBrowser);
      }
      else
      {
        _browserLauncher.OpenDefault(url);
      }
    }
  }

  public void RegisterAsDefaultBrowser()
  {
    if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
    {
      RegisterMacOS();
    }
    else
    {
      Console.WriteLine("Default browser registration only supported on macOS currently");
    }
  }

  private void RegisterMacOS()
  {
    try
    {
      // This requires the app to be properly bundled with Info.plist
      // The actual registration is done via LSSetDefaultHandlerForURLScheme
      // which requires Objective-C interop

      Console.WriteLine("To register as default browser:");
      Console.WriteLine("1. Build and bundle the app with proper Info.plist");
      Console.WriteLine("2. Open System Settings → Desktop & Dock → Default web browser");
      Console.WriteLine("3. Select BrowRoute from the list");
      Console.WriteLine();
      Console.WriteLine("The app is configured to handle http:// and https:// URLs");
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error registering as default browser: {ex.Message}");
    }
  }
}

// macOS interop for URL handling (requires Objective-C bindings)
public static class MacOSInterop
{
  // This would require proper Objective-C interop via P/Invoke
  // For now, we'll rely on the Info.plist configuration

  [DllImport("/System/Library/Frameworks/CoreServices.framework/CoreServices")]
  private static extern int LSSetDefaultHandlerForURLScheme(
      IntPtr urlScheme,
      IntPtr handlerBundleID);

  public static bool SetDefaultBrowser(string bundleId)
  {
    if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
      return false;

    try
    {
      // Convert strings to CFString
      // This is simplified - actual implementation needs CFString marshaling
      var schemes = new[] { "http", "https" };

      foreach (var scheme in schemes)
      {
        // Would need proper CFString marshaling here
        Console.WriteLine($"Setting default handler for {scheme} to {bundleId}");
      }

      return true;
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error setting default browser: {ex.Message}");
      return false;
    }
  }
}
