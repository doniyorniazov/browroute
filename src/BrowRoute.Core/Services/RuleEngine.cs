using System.Text.RegularExpressions;
using BrowRoute.Core.Models;

namespace BrowRoute.Core.Services;

public class RuleEngine
{
  private readonly ConfigManager _configManager;
  private List<Rule> _rules = new();

  public RuleEngine(ConfigManager configManager)
  {
    _configManager = configManager;
    LoadRules();
  }

  public void LoadRules()
  {
    var config = _configManager.LoadConfig();
    _rules = config.Rules.OrderByDescending(r => r.Priority).ToList();
    Console.WriteLine($"Loaded {_rules.Count} rules");
  }

  public RuleMatch? Match(Uri url)
  {
    var host = url.Host.ToLowerInvariant();
    var path = url.PathAndQuery.ToLowerInvariant();
    var fullUrl = $"{host}{path}";

    foreach (var rule in _rules)
    {
      if (MatchesRule(url, host, path, fullUrl, rule))
      {
        Console.WriteLine($"Matched rule: {rule.Name} (priority {rule.Priority})");
        return new RuleMatch
        {
          Browser = rule.Browser,
          Profile = rule.Profile,
          RuleName = rule.Name,
          Priority = rule.Priority
        };
      }
    }

    Console.WriteLine("No rule matched, using default");
    return null;
  }

  private bool MatchesRule(Uri url, string host, string path, string fullUrl, Rule rule)
  {
    // Check patterns first
    bool patternMatched = false;

    foreach (var pattern in rule.Patterns)
    {
      if (rule.PatternType == "regex")
      {
        if (MatchesRegex(fullUrl, pattern))
        {
          patternMatched = true;
          break;
        }
      }
      else if (rule.PatternType == "exact")
      {
        if (fullUrl == pattern.ToLowerInvariant())
        {
          patternMatched = true;
          break;
        }
      }
      else // glob (default)
      {
        if (MatchesGlob(fullUrl, pattern))
        {
          patternMatched = true;
          break;
        }
      }
    }

    if (!patternMatched)
      return false;

    // If pattern matched, check additional conditions
    if (rule.Conditions != null)
    {
      return MatchesConditions(url, rule.Conditions);
    }

    return true;
  }

  private bool MatchesGlob(string input, string pattern)
  {
    // Convert glob pattern to regex
    var regexPattern = "^" + Regex.Escape(pattern.ToLowerInvariant())
        .Replace("\\*", ".*")
        .Replace("\\?", ".") + "$";

    try
    {
      return Regex.IsMatch(input, regexPattern, RegexOptions.IgnoreCase);
    }
    catch
    {
      return false;
    }
  }

  private bool MatchesRegex(string input, string pattern)
  {
    try
    {
      return Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase);
    }
    catch
    {
      return false;
    }
  }

  private bool MatchesConditions(Uri url, Conditions conditions)
  {
    bool result = true;

    // Check queryContains
    if (conditions.QueryContains?.Count > 0)
    {
      var query = url.Query.ToLowerInvariant();
      result = conditions.QueryContains.Any(q => query.Contains(q.ToLowerInvariant()));
    }

    // Check pathContains
    if (result && conditions.PathContains?.Count > 0)
    {
      var path = url.PathAndQuery.ToLowerInvariant();
      result = conditions.PathContains.Any(p => path.Contains(p.ToLowerInvariant()));
    }

    // Check pathMatches
    if (result && !string.IsNullOrEmpty(conditions.PathMatches))
    {
      result = MatchesRegex(url.PathAndQuery, conditions.PathMatches);
    }

    // Handle OR conditions
    if (conditions.Or != null)
    {
      return result || MatchesConditions(url, conditions.Or);
    }

    // Handle AND conditions
    if (conditions.And != null)
    {
      return result && MatchesConditions(url, conditions.And);
    }

    return result;
  }

  public RuleMatch? TestUrl(string urlString)
  {
    if (!Uri.TryCreate(urlString, UriKind.Absolute, out var url))
    {
      Console.WriteLine($"Invalid URL: {urlString}");
      return null;
    }

    return Match(url);
  }
}
