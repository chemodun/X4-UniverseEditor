using System;
using System.IO;
using System.Text.Json;

namespace ChemGateBuilder.AvaloniaApp.Services
{
  public class AppSettings
  {
    public string? X4DataFolder { get; set; }
    public string? X4GameFolder { get; set; }
    public bool DirectMode { get; set; }
    public bool LoadModsData { get; set; }
    public string? X4DataVersion { get; set; }
    public bool X4DataVersionOverride { get; set; }
    public int SelectedTabIndex { get; set; }
    public bool GatesActiveByDefault { get; set; } = true;
    public int GatesMinimalDistanceBetween { get; set; } = 10;
  }

  public class SettingsService
  {
    private readonly string _settingsDir;
    private readonly string _settingsPath;
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public AppSettings Current { get; private set; } = new();

    public SettingsService(string appName = "ChemGateBuilder")
    {
      var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
      _settingsDir = Path.Combine(appData, appName);
      _settingsPath = Path.Combine(_settingsDir, "settings.json");
    }

    public void Load()
    {
      try
      {
        if (File.Exists(_settingsPath))
        {
          var json = File.ReadAllText(_settingsPath);
          var loaded = JsonSerializer.Deserialize<AppSettings>(json, Options);
          if (loaded != null)
            Current = loaded;
        }
      }
      catch
      {
        // ignore and continue with defaults
        Current = new AppSettings();
      }
    }

    public void Save()
    {
      try
      {
        Directory.CreateDirectory(_settingsDir);
        var json = JsonSerializer.Serialize(Current, Options);
        File.WriteAllText(_settingsPath, json);
      }
      catch
      {
        // ignore save failures for now
      }
    }
  }
}
