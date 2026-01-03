using BrowRoute.Core.Models;
using BrowRoute.Core.Services;
using BrowRoute.Core.Platform;

namespace BrowRoute.App;

class Program
{
  static void Main(string[] args)
  {
    Console.WriteLine("🌐 BrowRoute - Intelligent Browser Router");
    Console.WriteLine("==========================================\n");

    // Initialize services
    var configManager = new ConfigManager();
    var ruleEngine = new RuleEngine(configManager);
    var browserLauncher = new BrowserLauncher(configManager);
    var urlHandler = new URLHandlerService(ruleEngine, browserLauncher, configManager);

    // Check command line arguments
    if (args.Length > 0)
    {
      HandleCommand(args, urlHandler, configManager, ruleEngine);
    }
    else
    {
      ShowHelp();
    }
  }

  static void HandleCommand(string[] args, URLHandlerService urlHandler,
                           ConfigManager configManager, RuleEngine ruleEngine)
  {
    var command = args[0].ToLowerInvariant();

    switch (command)
    {
      case "open":
      case "handle":
        if (args.Length < 2)
        {
          Console.WriteLine("Error: URL required");
          Console.WriteLine("Usage: browroute open <url>");
          return;
        }
        urlHandler.HandleURL(args[1]);
        break;

      case "test":
        if (args.Length < 2)
        {
          Console.WriteLine("Error: URL required");
          Console.WriteLine("Usage: browroute test <url>");
          return;
        }
        TestUrl(args[1], ruleEngine, configManager);
        break;

      case "register":
        urlHandler.RegisterAsDefaultBrowser();
        break;

      case "config":
        ShowConfig(configManager);
        break;

      case "reload":
        Console.WriteLine("Reloading configuration...");
        configManager.LoadConfig();
        ruleEngine.LoadRules();
        Console.WriteLine("✓ Configuration reloaded");
        break;

      case "interactive":
        RunInteractiveMode(urlHandler, ruleEngine, configManager);
        break;

      default:
        // If it looks like a URL, handle it
        if (command.StartsWith("http://") || command.StartsWith("https://"))
        {
          urlHandler.HandleURL(command);
        }
        else
        {
          Console.WriteLine($"Unknown command: {command}");
          ShowHelp();
        }
        break;
    }
  }

  static void TestUrl(string urlString, RuleEngine ruleEngine, ConfigManager configManager)
  {
    Console.WriteLine($"Testing URL: {urlString}\n");

    var match = ruleEngine.TestUrl(urlString);

    if (match != null)
    {
      Console.WriteLine("✓ Rule matched!");
      Console.WriteLine($"  Rule: {match.RuleName}");
      Console.WriteLine($"  Priority: {match.Priority}");
      Console.WriteLine($"  Browser: {match.Browser}");
      if (!string.IsNullOrEmpty(match.Profile))
        Console.WriteLine($"  Profile: {match.Profile}");
    }
    else
    {
      var config = configManager.LoadConfig();
      Console.WriteLine("✗ No rule matched");
      Console.WriteLine($"  Would open in default browser: {config.DefaultBrowser}");
    }
  }

  static void ShowConfig(ConfigManager configManager)
  {
    var config = configManager.LoadConfig();

    Console.WriteLine("Configuration Summary");
    Console.WriteLine("=====================\n");
    Console.WriteLine($"Version: {config.Version}");
    Console.WriteLine($"Default Browser: {config.DefaultBrowser}\n");

    Console.WriteLine($"Browsers ({config.Browsers.Count}):");
    foreach (var browser in config.Browsers)
    {
      Console.WriteLine($"  • {browser.Key}");
      Console.WriteLine($"    Path: {browser.Value.ExecutablePath}");
      Console.WriteLine($"    Profiles: {(browser.Value.SupportsProfiles ? "Yes" : "No")}");
    }

    Console.WriteLine($"\nTeams ({config.Teams.Count}):");
    foreach (var team in config.Teams)
    {
      Console.WriteLine($"  • {team.Name}");
      Console.WriteLine($"    Browser: {team.Browser}" +
                      (team.Profile != null ? $" ({team.Profile})" : ""));
      if (team.GithubOrgs.Count > 0)
        Console.WriteLine($"    GitHub: {string.Join(", ", team.GithubOrgs)}");
    }

    Console.WriteLine($"\nRules ({config.Rules.Count}):");
    foreach (var rule in config.Rules.OrderByDescending(r => r.Priority).Take(10))
    {
      Console.WriteLine($"  • {rule.Name} (priority: {rule.Priority})");
      Console.WriteLine($"    → {rule.Browser}" +
                      (rule.Profile != null ? $" ({rule.Profile})" : ""));
    }

    if (config.Rules.Count > 10)
      Console.WriteLine($"  ... and {config.Rules.Count - 10} more");
  }

  static void RunInteractiveMode(URLHandlerService urlHandler,
                                RuleEngine ruleEngine,
                                ConfigManager configManager)
  {
    Console.WriteLine("Interactive Mode - Enter URLs to test routing");
    Console.WriteLine("Commands: test <url>, open <url>, config, reload, exit\n");

    while (true)
    {
      Console.Write("> ");
      var input = Console.ReadLine()?.Trim();

      if (string.IsNullOrEmpty(input))
        continue;

      var parts = input.Split(' ', 2);
      var command = parts[0].ToLowerInvariant();

      if (command == "exit" || command == "quit" || command == "q")
      {
        Console.WriteLine("Goodbye!");
        break;
      }

      if (command == "config")
      {
        ShowConfig(configManager);
      }
      else if (command == "reload")
      {
        configManager.LoadConfig();
        ruleEngine.LoadRules();
        Console.WriteLine("✓ Configuration reloaded");
      }
      else if (command == "test" && parts.Length > 1)
      {
        TestUrl(parts[1], ruleEngine, configManager);
      }
      else if (command == "open" && parts.Length > 1)
      {
        urlHandler.HandleURL(parts[1]);
      }
      else if (input.StartsWith("http://") || input.StartsWith("https://"))
      {
        TestUrl(input, ruleEngine, configManager);
      }
      else
      {
        Console.WriteLine("Commands: test <url>, open <url>, config, reload, exit");
      }

      Console.WriteLine();
    }
  }

  static void ShowHelp()
  {
    Console.WriteLine("Usage:");
    Console.WriteLine("  browroute open <url>        Open URL with intelligent routing");
    Console.WriteLine("  browroute test <url>        Test which browser would open URL");
    Console.WriteLine("  browroute register          Show instructions to register as default browser");
    Console.WriteLine("  browroute config            Display current configuration");
    Console.WriteLine("  browroute reload            Reload configuration from disk");
    Console.WriteLine("  browroute interactive       Start interactive testing mode");
    Console.WriteLine();
    Console.WriteLine("Examples:");
    Console.WriteLine("  browroute open https://github.com/SilkRoadProfessionals/pcbe-api");
    Console.WriteLine("  browroute test https://youtube.com");
    Console.WriteLine();
  }
}
