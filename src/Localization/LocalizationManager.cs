using System.Collections.Concurrent;
using System.ComponentModel;
using System.Globalization;
using System.Text.Json;

namespace Localization;

/// <summary>
/// Provides runtime translation lookup with simple JSON catalogs that integrate well with Crowdin exports.
/// </summary>
public sealed class LocalizationManager : INotifyPropertyChanged
{
  private readonly ConcurrentDictionary<string, IReadOnlyDictionary<string, string>> _catalogs = new(StringComparer.OrdinalIgnoreCase);
  private readonly object _languageLock = new();
  private string _currentLanguage = "en";
  private string _defaultLanguage = "en";

  public static LocalizationManager Shared { get; } = new();

  public event PropertyChangedEventHandler? PropertyChanged;
  public event EventHandler<LanguageChangedEventArgs>? LanguageChanged;

  public string CurrentLanguage
  {
    get => _currentLanguage;
    private set => _currentLanguage = value;
  }

  public string DefaultLanguage
  {
    get => _defaultLanguage;
    private set => _defaultLanguage = value;
  }

  public IReadOnlyCollection<string> AvailableLanguages => _catalogs.Keys.ToList().AsReadOnly();

  public string this[string key] => Translate(key);

  public void LoadFromDirectory(string directoryPath, string searchPattern = "*.json", string? defaultLanguage = null)
  {
    if (string.IsNullOrWhiteSpace(directoryPath) || !Directory.Exists(directoryPath))
    {
      return;
    }

    foreach (string file in Directory.EnumerateFiles(directoryPath, searchPattern, SearchOption.TopDirectoryOnly))
    {
      string language = Path.GetFileNameWithoutExtension(file);
      if (string.IsNullOrWhiteSpace(language))
      {
        continue;
      }

      var entries = TranslationFileLoader.Load(file);
      RegisterLanguage(language, entries, overwrite: true, suppressNotification: true);
    }

    if (!string.IsNullOrWhiteSpace(defaultLanguage))
    {
      DefaultLanguage = NormalizeLanguageCode(defaultLanguage);
    }

    if (!_catalogs.ContainsKey(DefaultLanguage) && _catalogs.Count > 0)
    {
      DefaultLanguage = _catalogs.Keys.First();
    }

    if (!_catalogs.ContainsKey(CurrentLanguage))
    {
      CurrentLanguage = DefaultLanguage;
    }

    NotifyLanguageChanged(CurrentLanguage, CurrentLanguage);
  }

  public void RegisterLanguage(
    string languageCode,
    IReadOnlyDictionary<string, string> translations,
    bool overwrite = true,
    bool suppressNotification = false
  )
  {
    if (string.IsNullOrWhiteSpace(languageCode) || translations == null)
    {
      return;
    }

    string normalized = NormalizeLanguageCode(languageCode);
    _catalogs.AddOrUpdate(
      normalized,
      _ => new Dictionary<string, string>(translations, StringComparer.OrdinalIgnoreCase),
      (_, existing) =>
        overwrite ? new Dictionary<string, string>(translations, StringComparer.OrdinalIgnoreCase) : Merge(existing, translations)
    );

    if (!suppressNotification)
    {
      NotifyLanguageChanged(CurrentLanguage, CurrentLanguage);
    }
  }

  public bool TrySetLanguage(string languageCode)
  {
    if (string.IsNullOrWhiteSpace(languageCode))
    {
      return false;
    }

    string normalized = NormalizeLanguageCode(languageCode);
    if (!_catalogs.ContainsKey(normalized))
    {
      return false;
    }

    lock (_languageLock)
    {
      string oldLanguage = CurrentLanguage;
      CurrentLanguage = normalized;
      NotifyLanguageChanged(oldLanguage, normalized);
    }

    return true;
  }

  public string Translate(string key, string? fallback = null)
  {
    if (string.IsNullOrWhiteSpace(key))
    {
      return string.Empty;
    }

    string normalizedKey = key.Trim();

    if (TryGetValue(_currentLanguage, normalizedKey, out string value))
    {
      return value;
    }

    if (!string.Equals(_currentLanguage, _defaultLanguage, StringComparison.OrdinalIgnoreCase))
    {
      if (TryGetValue(_defaultLanguage, normalizedKey, out value))
      {
        return value;
      }
    }

    return fallback ?? normalizedKey;
  }

  public bool TryTranslate(string languageCode, string key, out string value)
  {
    if (string.IsNullOrWhiteSpace(languageCode) || string.IsNullOrWhiteSpace(key))
    {
      value = string.Empty;
      return false;
    }

    return TryGetValue(languageCode, key.Trim(), out value);
  }

  public string GetLanguageDisplayName(string languageCode, string? fallback = null)
  {
    if (string.IsNullOrWhiteSpace(languageCode))
    {
      return fallback ?? string.Empty;
    }

    if (TryTranslate(languageCode, "language.name", out string value) && !string.IsNullOrWhiteSpace(value))
    {
      return value;
    }

    try
    {
      var culture = CultureInfo.GetCultureInfo(languageCode);
      if (!string.IsNullOrWhiteSpace(culture.NativeName))
      {
        return culture.NativeName;
      }
    }
    catch (CultureNotFoundException)
    {
      // Ignore and fall back
    }

    return fallback ?? languageCode;
  }

  private static Dictionary<string, string> Merge(
    IReadOnlyDictionary<string, string> existing,
    IReadOnlyDictionary<string, string> additions
  )
  {
    var merged = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    foreach (var pair in existing)
    {
      merged[pair.Key] = pair.Value;
    }

    foreach (var pair in additions)
    {
      merged[pair.Key] = pair.Value;
    }

    return merged;
  }

  private bool TryGetValue(string languageCode, string key, out string value)
  {
    value = string.Empty;

    if (string.IsNullOrWhiteSpace(languageCode) || string.IsNullOrWhiteSpace(key))
    {
      return false;
    }

    string normalizedLanguage = NormalizeLanguageCode(languageCode);
    string normalizedKey = key.Trim();

    if (_catalogs.TryGetValue(normalizedLanguage, out IReadOnlyDictionary<string, string>? catalog))
    {
      if (catalog.TryGetValue(normalizedKey, out string? localized))
      {
        value = localized ?? string.Empty;
        return true;
      }
    }

    return false;
  }

  private string NormalizeLanguageCode(string languageCode)
  {
    return languageCode.Trim().Replace('_', '-');
  }

  private void NotifyLanguageChanged(string oldLanguage, string newLanguage)
  {
    OnPropertyChanged("Item[]");
    LanguageChanged?.Invoke(this, new LanguageChangedEventArgs(oldLanguage, newLanguage));
  }

  private void OnPropertyChanged(string propertyName)
  {
    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
  }
}

public sealed class LanguageChangedEventArgs(string oldLanguage, string newLanguage) : EventArgs
{
  public string OldLanguage { get; } = oldLanguage;
  public string NewLanguage { get; } = newLanguage;
}

internal static class TranslationFileLoader
{
  public static IReadOnlyDictionary<string, string> Load(string filePath)
  {
    using FileStream stream = File.OpenRead(filePath);
    using JsonDocument document = JsonDocument.Parse(stream);
    var flattened = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    FlattenElement(document.RootElement, string.Empty, flattened);
    return flattened;
  }

  private static void FlattenElement(JsonElement element, string prefix, IDictionary<string, string> target)
  {
    switch (element.ValueKind)
    {
      case JsonValueKind.Object:
        foreach (JsonProperty property in element.EnumerateObject())
        {
          string childPrefix = string.IsNullOrEmpty(prefix) ? property.Name : $"{prefix}.{property.Name}";
          FlattenElement(property.Value, childPrefix, target);
        }
        break;
      case JsonValueKind.Array:
        for (int i = 0; i < element.GetArrayLength(); i++)
        {
          string childPrefix = string.IsNullOrEmpty(prefix) ? i.ToString() : $"{prefix}[{i}]";
          FlattenElement(element[i], childPrefix, target);
        }
        break;
      case JsonValueKind.String:
        target[prefix] = element.GetString() ?? string.Empty;
        break;
      case JsonValueKind.Number:
      case JsonValueKind.True:
      case JsonValueKind.False:
        target[prefix] = element.ToString();
        break;
      case JsonValueKind.Null:
        target[prefix] = string.Empty;
        break;
      default:
        break;
    }
  }
}
