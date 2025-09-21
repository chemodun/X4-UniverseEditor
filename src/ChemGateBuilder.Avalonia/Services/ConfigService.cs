using System;
using System.IO;
using System.Reflection;
using System.Text.Json;

namespace ChemGateBuilder.AvaloniaApp.Services
{
  // WPF-compatible configuration models
  public class ModeConfig
  {
    public bool DirectMode { get; set; } = false;
  }

  public class EditConfig
  {
    public bool GatesActiveByDefault { get; set; } = true;
    public int GatesMinimalDistanceBetween { get; set; } = 10;
  }

  public class MapConfig
  {
    public double MapColorsOpacity { get; set; } = 0.5;
    public bool NonStandardUniverse { get; set; } = false;
    public string NonStandardUniverseId { get; set; } = "";
  }

  public class DataConfig
  {
    public string X4GameFolder { get; set; } = "";
    public string X4DataExtractedPath { get; set; } = ".";
    public bool X4DataVersionOverride { get; set; } = false;
    public int X4DataVersion { get; set; } = 710;
    public bool LoadModsData { get; set; } = false;
  }

  public class LoggingConfig
  {
    public string LogLevel { get; set; } = "Warning";
    public bool LogToFile { get; set; } = false;
  }

  public class AppConfig
  {
    public ModeConfig Mode { get; set; } = new();
    public EditConfig Edit { get; set; } = new();
    public MapConfig Map { get; set; } = new();
    public DataConfig Data { get; set; } = new();
    public LoggingConfig Logging { get; set; } = new();
  }

  public static class ConfigService
  {
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    private static string GetPrimaryConfigPath() => Path.Combine(AppContext.BaseDirectory, "ChemGateBuilder.json");

    private static string GetFallbackConfigPath()
    {
      var name = Assembly.GetExecutingAssembly().GetName().Name ?? "ChemGateBuilder.Avalonia";
      return Path.Combine(AppContext.BaseDirectory, name + ".json");
    }

    public static AppConfig Load()
    {
      try
      {
        var primary = GetPrimaryConfigPath();
        var fallback = GetFallbackConfigPath();
        var path = File.Exists(primary) ? primary : (File.Exists(fallback) ? fallback : primary);
        if (File.Exists(path))
        {
          var json = File.ReadAllText(path);
          var cfg = JsonSerializer.Deserialize<AppConfig>(json, Options);
          if (cfg != null)
          {
            // Normalize null sections for resilience
            cfg.Mode ??= new ModeConfig();
            cfg.Data ??= new DataConfig();
            cfg.Edit ??= new EditConfig();
            cfg.Map ??= new MapConfig();
            cfg.Logging ??= new LoggingConfig();
            return cfg;
          }
        }
      }
      catch
      {
        // ignore and fall back to defaults
      }
      return new AppConfig();
    }

    public static void Save(AppConfig config)
    {
      try
      {
        var path = GetPrimaryConfigPath();
        var json = JsonSerializer.Serialize(config, Options);
        File.WriteAllText(path, json);
      }
      catch
      {
        // ignore save failures for now
      }
    }
  }
}
