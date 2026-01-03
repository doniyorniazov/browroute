using System.Text.Json;
using BrowRoute.Core.Models;

namespace BrowRoute.Core.Services;

public class ConfigManager
{
  private readonly string _configPath;
  private Config? _config;
  private DateTime _lastModified;

  public ConfigManager(string? configPath = null)
  {
    _configPath = configPath ?? GetDefaultConfigPath();
  }

  private static string GetDefaultConfigPath()
  {
    var appSupport = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    var configDir = Path.Combine(appSupport, "BrowRoute");
    Directory.CreateDirectory(configDir);
    return Path.Combine(configDir, "config.json");
  }

  public Config LoadConfig()
  {
    try
    {
      if (!File.Exists(_configPath))
      {
        Console.WriteLine($"Config not found at {_configPath}, creating default...");
        CreateDefaultConfig();
      }

      var fileInfo = new FileInfo(_configPath);

      // Check if we need to reload
      if (_config != null && fileInfo.LastWriteTime <= _lastModified)
      {
        return _config;
      }

      var json = File.ReadAllText(_configPath);
      _config = JsonSerializer.Deserialize<Config>(json, new JsonSerializerOptions
      {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip
      }) ?? throw new InvalidOperationException("Failed to deserialize config");

      _lastModified = fileInfo.LastWriteTime;

      Console.WriteLine($"Config loaded: {_config.Rules.Count} rules, {_config.Teams.Count} teams");
      return _config;
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error loading config: {ex.Message}");
      throw;
    }
  }

  public void SaveConfig(Config config)
  {
    var json = JsonSerializer.Serialize(config, new JsonSerializerOptions
    {
      WriteIndented = true,
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    });

    File.WriteAllText(_configPath, json);
    _config = config;
    _lastModified = File.GetLastWriteTime(_configPath);
  }

  private void CreateDefaultConfig()
  {
    var defaultConfig = new Config
    {
      Version = "1.0",
      DefaultBrowser = "Brave",
      Browsers = new Dictionary<string, BrowserConfig>
      {
        ["Brave"] = new()
        {
          BundleId = "com.brave.Browser",
          ExecutablePath = "/Applications/Brave Browser.app",
          SupportsProfiles = false
        },
        ["Dia"] = new()
        {
          BundleId = "app.dia.macos",
          ExecutablePath = "/Applications/Dia.app",
          SupportsProfiles = true,
          ProfileSwitchMethod = "url-parameter"
        }
      },
      PersonalSites = new List<string>
            {
                "youtube.com", "instagram.com", "facebook.com", "twitter.com", "reddit.com"
            },
      Teams = new List<Team>(),
      Rules = new List<Rule>
            {
                new()
                {
                    Name = "Personal Sites",
                    Priority = 100,
                    Browser = "Brave",
                    Patterns = new List<string> { "youtube.com/*", "instagram.com/*" }
                }
            }
    };

    SaveConfig(defaultConfig);
    Console.WriteLine($"Default config created at {_configPath}");
  }

  public bool ValidateConfig(Config config)
  {
    if (string.IsNullOrEmpty(config.Version))
      return false;

    if (string.IsNullOrEmpty(config.DefaultBrowser))
      return false;

    if (!config.Browsers.ContainsKey(config.DefaultBrowser))
      return false;

    foreach (var rule in config.Rules)
    {
      if (!config.Browsers.ContainsKey(rule.Browser))
        return false;
    }

    return true;
  }

  public void WatchConfig(Action onConfigChanged)
  {
    var watcher = new FileSystemWatcher(Path.GetDirectoryName(_configPath)!)
    {
      Filter = Path.GetFileName(_configPath),
      NotifyFilter = NotifyFilters.LastWrite
    };

    watcher.Changed += (sender, e) =>
    {
      Thread.Sleep(100); // Debounce
      Console.WriteLine("Config file changed, reloading...");
      LoadConfig();
      onConfigChanged();
    };

    watcher.EnableRaisingEvents = true;
  }
}
