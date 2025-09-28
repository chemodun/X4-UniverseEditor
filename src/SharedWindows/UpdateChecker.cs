using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Utilities.Logging;

namespace SharedWindows
{
  public static class UpdateChecker
  {
    private static HttpClient CreateGitHubHttpClient()
    {
      var handler = new HttpClientHandler
      {
        AllowAutoRedirect = true,
        AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli,
        UseCookies = false,
      };
      var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(15) };
      client.DefaultRequestHeaders.UserAgent.ParseAdd(
        "X4-UniverseEditor-UpdateChecker/1.0 (+https://github.com/chemodun/X4-UniverseEditor)"
      );
      client.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");
      client.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip, deflate, br");
      return client;
    }

    public static async Task<string?> GetLatestComponentVersionAsync(string githubRepoUrl, string componentId, CancellationToken ct)
    {
      if (!TryParseOwnerRepo(githubRepoUrl, out string owner, out string repo))
      {
        return null;
      }

      // Fetch releases (not generic tags), as requested. Consider drafts/prereleases excluded.
      string api = $"https://api.github.com/repos/{owner}/{repo}/releases?per_page=100";
      try
      {
        using var http = CreateGitHubHttpClient();
        using var resp = await http.GetAsync(api, ct).ConfigureAwait(false);
        if (!resp.IsSuccessStatusCode)
        {
          return null;
        }
        using var stream = await resp.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
        using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct).ConfigureAwait(false);
        if (doc.RootElement.ValueKind != JsonValueKind.Array)
        {
          return null;
        }
        string? bestVersion = null;
        Version? best = null;
        foreach (var rel in doc.RootElement.EnumerateArray())
        {
          bool draft = rel.TryGetProperty("draft", out var pDraft) && pDraft.GetBoolean();
          bool prerelease = rel.TryGetProperty("prerelease", out var pPre) && pPre.GetBoolean();
          if (draft || prerelease)
            continue; // consider only stable releases

          if (!rel.TryGetProperty("tag_name", out var tagProp))
            continue;
          var tag = tagProp.GetString() ?? string.Empty;
          if (TryExtractComponentVersionFromTag(tag, componentId, out var verStr))
          {
            if (VersionTryParseFlexible(verStr, out var ver))
            {
              if (best == null || CompareVersionsSafe(ver, best) > 0)
              {
                best = ver;
                bestVersion = verStr;
              }
            }
          }
        }
        return bestVersion;
      }
      catch
      {
        return null;
      }
    }

    private static bool TryParseOwnerRepo(string url, out string owner, out string repo)
    {
      owner = string.Empty;
      repo = string.Empty;
      try
      {
        var uri = new Uri(url);
        if (!string.Equals(uri.Host, "github.com", StringComparison.OrdinalIgnoreCase))
          return false;
        var segments = uri.AbsolutePath.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length < 2)
          return false;
        owner = segments[0];
        repo = segments[1];
        return true;
      }
      catch
      {
        return false;
      }
    }

    private static bool TryExtractComponentVersionFromTag(string tag, string componentName, out string version)
    {
      version = string.Empty;
      if (string.IsNullOrWhiteSpace(tag) || string.IsNullOrWhiteSpace(componentName))
        return false;

      // Primary pattern from release-please config: include-component-in-tag=true, include-v-in-tag=true, tag-separator="@"
      // Accept both orders: "v1.2.3@Component" or "Component@v1.2.3"
      var parts = tag.Split('@');
      if (parts.Length == 2)
      {
        var a = parts[0].Trim();
        var b = parts[1].Trim();
        if (string.Equals(a, componentName, StringComparison.OrdinalIgnoreCase))
        {
          version = b.TrimStart('v', 'V');
          return true;
        }
        if (string.Equals(b, componentName, StringComparison.OrdinalIgnoreCase))
        {
          version = a.TrimStart('v', 'V');
          return true;
        }
      }

      // Fallback: If component appears in tag, extract a version-like token near it.
      // Example: "ChemGateBuilder-v1.2.3" or "v1.2.3-ChemGateBuilder"
      if (tag.IndexOf(componentName, StringComparison.OrdinalIgnoreCase) >= 0)
      {
        var m = Regex.Match(tag, "\\b[vV]?\\d+(?:[\\._]\\d+){0,3}\\b");
        if (m.Success)
        {
          version = m.Value.Trim().TrimStart('v', 'V');
          return true;
        }
      }

      return false;
    }

    public static bool VersionTryParseFlexible(string text, out Version version)
    {
      version = new Version(0, 0, 0, 0);
      if (string.IsNullOrWhiteSpace(text))
        return false;
      string cleaned = text.Trim().TrimStart('v', 'V').Replace('_', '.');
      var parts = cleaned.Split('.', StringSplitOptions.RemoveEmptyEntries);
      if (parts.Length == 0)
        return false;
      int[] nums = parts.Take(4).Select(p => int.TryParse(new string(p.TakeWhile(char.IsDigit).ToArray()), out var n) ? n : 0).ToArray();
      while (nums.Length < 2)
        nums = nums.Append(0).ToArray();
      while (nums.Length < 4)
        nums = nums.Append(0).ToArray();
      try
      {
        version = new Version(nums[0], nums[1], nums[2], nums[3]);
        return true;
      }
      catch
      {
        return false;
      }
    }

    public static int CompareVersionsSafe(Version a, Version b)
    {
      int c = a.Major.CompareTo(b.Major);
      if (c != 0)
        return c;
      c = a.Minor.CompareTo(b.Minor);
      if (c != 0)
        return c;
      c = a.Build.CompareTo(b.Build);
      if (c != 0)
        return c;
      return a.Revision.CompareTo(b.Revision);
    }

    public static async void onCheckUpdatePressedAsync(
      Window mainWindow,
      StatusBarMessage statusBar,
      Assembly assembly,
      bool onStartUp = false
    )
    {
      if (onStartUp)
      {
        await Task.Delay(2000);
      }
      statusBar.SetStatusMessage("Checking for utility updates ...", StatusMessageType.Info);
      try
      {
        if (assembly == null)
        {
          statusBar.SetStatusMessage("No assembly provided for update check.", StatusMessageType.Error);
          return;
        }

        // Grab all AssemblyMetadata attributes
        var metadata = assembly.GetCustomAttributes<AssemblyMetadataAttribute>().ToDictionary(a => a.Key, a => a.Value);
        if (metadata == null || metadata.Count == 0)
        {
          statusBar.SetStatusMessage("No assembly metadata found.", StatusMessageType.Warning);
          return;
        }
        // Now you can safely look up your values
        string githubUrl = metadata.TryGetValue("RepositoryUrl", out string? repo) ? repo ?? string.Empty : string.Empty;
        string nexusModsUrl = metadata.TryGetValue("NexusModsUrl", out string? nexus) ? nexus ?? string.Empty : string.Empty;
        string componentId = metadata.TryGetValue("SubProject", out string? sub) ? sub ?? string.Empty : string.Empty;

        if (string.IsNullOrWhiteSpace(githubUrl) || string.IsNullOrWhiteSpace(componentId) || string.IsNullOrWhiteSpace(nexusModsUrl))
        {
          statusBar.SetStatusMessage("No related URL or component ID found in assembly metadata.", StatusMessageType.Warning);
          return;
        }
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        string? latestVersionStr = await GetLatestComponentVersionAsync(githubUrl, componentId, cts.Token).ConfigureAwait(true);

        var current = assembly.GetName().Version ?? new Version(0, 0, 0, 0);
        string currentStr = $"{current.Major}.{current.Minor}.{current.Build}".TrimEnd('.');

        if (!string.IsNullOrWhiteSpace(latestVersionStr))
        {
          if (VersionTryParseFlexible(latestVersionStr!, out var latest))
          {
            int cmp = CompareVersionsSafe(current, latest);
            if (cmp < 0)
            {
              statusBar.SetStatusMessage($"New version of {componentId} available: {latestVersionStr}", StatusMessageType.Info);
              var choice = MessageBox.Show(
                mainWindow,
                $"A new version of {componentId} is available (current: {currentStr}, latest: {latestVersionStr}).\n\nOpen the Nexus Mods page now?",
                "Update available",
                MessageBoxButton.YesNo,
                MessageBoxImage.Information
              );
              if (choice == MessageBoxResult.Yes)
              {
                try
                {
                  Process.Start(new ProcessStartInfo(nexusModsUrl) { UseShellExecute = true });
                }
                catch (Exception openEx)
                {
                  Log.Error($"Failed to open Nexus Mods link: {nexusModsUrl}", openEx);
                  statusBar.SetStatusMessage("Failed to open Nexus page.", StatusMessageType.Error);
                }
              }
            }
            else
            {
              statusBar.SetStatusMessage($"You are on the latest version of {componentId}.", StatusMessageType.Info);
              if (!onStartUp) // avoid bothering user on startup
              {
                MessageBox.Show(
                  mainWindow,
                  $"You are on the latest version ({currentStr}).",
                  "Up to date",
                  MessageBoxButton.OK,
                  MessageBoxImage.Information
                );
              }
            }
          }
          else
          {
            statusBar.SetStatusMessage($"Latest version of {componentId}: {latestVersionStr}", StatusMessageType.Info);
          }
        }
        else
        {
          statusBar.SetStatusMessage($"Could not determine the latest version of {componentId}.", StatusMessageType.Warning);
          if (!onStartUp) // avoid bothering user on startup
          {
            MessageBox.Show(
              mainWindow,
              $"Could not determine the latest version of {componentId}.",
              "Check update",
              MessageBoxButton.OK,
              MessageBoxImage.Warning
            );
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("Failed to check updates.", ex);
        statusBar.SetStatusMessage("Failed to check updates.", StatusMessageType.Error);
        if (!onStartUp) // avoid bothering user on startup
        {
          MessageBox.Show(mainWindow, $"Failed to check updates.\\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
      }
    }
  }
}
