using System.Diagnostics;
using System.Runtime.InteropServices;
using BrowRoute.Core.Models;

namespace BrowRoute.Core.Services;

public class BrowserLauncher
{
  private readonly ConfigManager _configManager;

  public BrowserLauncher(ConfigManager configManager)
  {
    _configManager = configManager;
  }

  public void Open(Uri url, string browser, string? profile = null)
  {
    var config = _configManager.LoadConfig();

    if (!config.Browsers.TryGetValue(browser, out var browserConfig))
    {
      Console.WriteLine($"Browser '{browser}' not found in config, opening with default");
      OpenDefault(url);
      return;
    }

    if (!File.Exists(browserConfig.ExecutablePath) &&
        !Directory.Exists(browserConfig.ExecutablePath))
    {
      Console.WriteLine($"Browser executable not found: {browserConfig.ExecutablePath}");
      OpenDefault(url);
      return;
    }

    try
    {
      if (browserConfig.SupportsProfiles && !string.IsNullOrEmpty(profile))
      {
        OpenWithProfile(url, browserConfig, profile);
      }
      else
      {
        OpenSimple(url, browserConfig);
      }

      Console.WriteLine($"Opened {url} in {browser}" +
                      (profile != null ? $" (profile: {profile})" : ""));
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error opening browser: {ex.Message}");
      OpenDefault(url);
    }
  }

  private void OpenSimple(Uri url, BrowserConfig browserConfig)
  {
    if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
    {
      // Use 'open' command on macOS
      Process.Start(new ProcessStartInfo
      {
        FileName = "open",
        Arguments = $"-a \"{browserConfig.ExecutablePath}\" \"{url}\"",
        UseShellExecute = false,
        CreateNoWindow = true
      });
    }
    else
    {
      // Fallback for other platforms
      Process.Start(new ProcessStartInfo
      {
        FileName = browserConfig.ExecutablePath,
        Arguments = url.ToString(),
        UseShellExecute = true
      });
    }
  }

  private void OpenWithProfile(Uri url, BrowserConfig browserConfig, string profile)
  {
    switch (browserConfig.ProfileSwitchMethod?.ToLowerInvariant())
    {
      case "applescript":
        OpenWithAppleScript(url, browserConfig, profile);
        break;

      case "cli-argument":
        OpenWithCliArgument(url, browserConfig, profile);
        break;

      case "url-parameter":
        OpenWithUrlParameter(url, browserConfig, profile);
        break;

      default:
        Console.WriteLine($"Unknown profile switch method: {browserConfig.ProfileSwitchMethod}");
        OpenSimple(url, browserConfig);
        break;
    }
  }

  private void OpenWithAppleScript(Uri url, BrowserConfig browserConfig, string profile)
  {
    if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
    {
      Console.WriteLine("AppleScript only supported on macOS");
      OpenSimple(url, browserConfig);
      return;
    }

    var appName = Path.GetFileNameWithoutExtension(browserConfig.ExecutablePath);
    var script = $@"
tell application ""{appName}""
    activate
    open location ""{url}"" with profile ""{profile}""
end tell";

    var scriptFile = Path.GetTempFileName() + ".scpt";
    File.WriteAllText(scriptFile, script);

    try
    {
      Process.Start(new ProcessStartInfo
      {
        FileName = "osascript",
        Arguments = scriptFile,
        UseShellExecute = false,
        CreateNoWindow = true
      })?.WaitForExit();
    }
    finally
    {
      File.Delete(scriptFile);
    }
  }

  private void OpenWithCliArgument(Uri url, BrowserConfig browserConfig, string profile)
  {
    if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
    {
      // For macOS .app bundles, find the actual executable
      var execPath = browserConfig.ExecutablePath;
      if (execPath.EndsWith(".app"))
      {
        var appName = Path.GetFileNameWithoutExtension(execPath);
        execPath = Path.Combine(execPath, "Contents", "MacOS", appName);
      }

      Process.Start(new ProcessStartInfo
      {
        FileName = execPath,
        Arguments = $"--profile=\"{profile}\" \"{url}\"",
        UseShellExecute = false,
        CreateNoWindow = true
      });
    }
    else
    {
      Process.Start(new ProcessStartInfo
      {
        FileName = browserConfig.ExecutablePath,
        Arguments = $"--profile=\"{profile}\" \"{url}\"",
        UseShellExecute = true
      });
    }
  }

  private void OpenWithUrlParameter(Uri url, BrowserConfig browserConfig, string profile)
  {
    // Some browsers support profile in URL (Dia might support this)
    // For now, fallback to simple open
    // TODO: Implement profile URL parameter support specific to Dia browser
    Console.WriteLine($"URL parameter profile switching not fully implemented yet");
    OpenSimple(url, browserConfig);
  }

  public void OpenDefault(Uri url)
  {
    try
    {
      if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
      {
        Process.Start(new ProcessStartInfo
        {
          FileName = "open",
          Arguments = $"\"{url}\"",
          UseShellExecute = false,
          CreateNoWindow = true
        });
      }
      else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
      {
        Process.Start(new ProcessStartInfo
        {
          FileName = url.ToString(),
          UseShellExecute = true
        });
      }
      else // Linux
      {
        Process.Start(new ProcessStartInfo
        {
          FileName = "xdg-open",
          Arguments = url.ToString(),
          UseShellExecute = false,
          CreateNoWindow = true
        });
      }
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error opening default browser: {ex.Message}");
    }
  }
}
