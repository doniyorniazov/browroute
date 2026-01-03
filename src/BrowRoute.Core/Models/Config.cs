using System.Text.Json.Serialization;

namespace BrowRoute.Core.Models;

public class Config
{
  [JsonPropertyName("version")]
  public string Version { get; set; } = "1.0";

  [JsonPropertyName("defaultBrowser")]
  public string DefaultBrowser { get; set; } = "Brave";

  [JsonPropertyName("browsers")]
  public Dictionary<string, BrowserConfig> Browsers { get; set; } = new();

  [JsonPropertyName("personalSites")]
  public List<string> PersonalSites { get; set; } = new();

  [JsonPropertyName("teams")]
  public List<Team> Teams { get; set; } = new();

  [JsonPropertyName("rules")]
  public List<Rule> Rules { get; set; } = new();
}

public class BrowserConfig
{
  [JsonPropertyName("bundleId")]
  public string BundleId { get; set; } = string.Empty;

  [JsonPropertyName("executablePath")]
  public string ExecutablePath { get; set; } = string.Empty;

  [JsonPropertyName("supportsProfiles")]
  public bool SupportsProfiles { get; set; }

  [JsonPropertyName("profileSwitchMethod")]
  public string? ProfileSwitchMethod { get; set; }

  [JsonPropertyName("profilePathTemplate")]
  public string? ProfilePathTemplate { get; set; }
}

public class Team
{
  [JsonPropertyName("name")]
  public string Name { get; set; } = string.Empty;

  [JsonPropertyName("cloud")]
  public string? Cloud { get; set; }

  [JsonPropertyName("domains")]
  public List<string> Domains { get; set; } = new();

  [JsonPropertyName("githubOrgs")]
  public List<string> GithubOrgs { get; set; } = new();

  [JsonPropertyName("githubRepoPatterns")]
  public List<string> GithubRepoPatterns { get; set; } = new();

  [JsonPropertyName("browser")]
  public string Browser { get; set; } = string.Empty;

  [JsonPropertyName("profile")]
  public string? Profile { get; set; }

  [JsonPropertyName("additionalDomains")]
  public List<string> AdditionalDomains { get; set; } = new();
}

public class Rule
{
  [JsonPropertyName("name")]
  public string Name { get; set; } = string.Empty;

  [JsonPropertyName("priority")]
  public int Priority { get; set; }

  [JsonPropertyName("browser")]
  public string Browser { get; set; } = string.Empty;

  [JsonPropertyName("profile")]
  public string? Profile { get; set; }

  [JsonPropertyName("patterns")]
  public List<string> Patterns { get; set; } = new();

  [JsonPropertyName("patternType")]
  public string PatternType { get; set; } = "glob";

  [JsonPropertyName("conditions")]
  public Conditions? Conditions { get; set; }
}

public class Conditions
{
  [JsonPropertyName("queryContains")]
  public List<string>? QueryContains { get; set; }

  [JsonPropertyName("pathContains")]
  public List<string>? PathContains { get; set; }

  [JsonPropertyName("pathMatches")]
  public string? PathMatches { get; set; }

  [JsonPropertyName("or")]
  public Conditions? Or { get; set; }

  [JsonPropertyName("and")]
  public Conditions? And { get; set; }
}

public class RuleMatch
{
  public string Browser { get; set; } = string.Empty;
  public string? Profile { get; set; }
  public string RuleName { get; set; } = string.Empty;
  public int Priority { get; set; }
}
