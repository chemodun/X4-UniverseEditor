using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using SharedWindows;
using Utilities.Logging;
using X4DataLoader;
using X4Map;
using X4Map.Converters;

namespace GalaxyEditor
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window, INotifyPropertyChanged
  {
    private string _x4DataFolder = ".";
    public string X4DataFolder
    {
      get => _x4DataFolder;
      set
      {
        if (_x4DataFolder != value && value != null)
        {
          _x4DataFolder = value;
          OnPropertyChanged(nameof(X4DataFolder));
          OnPropertyChanged(nameof(X4DataFolderPath));
          // SaveConfiguration();
        }
      }
    }

    public Galaxy? Galaxy { get; private set; }

    public bool IsDataLoaded
    {
      get =>
        false /* AllSectors.Count > 0 */
      ;
    }

    private bool _isModCanBeLoaded = true;
    public bool IsModCanBeLoaded
    {
      get => _isModCanBeLoaded;
      set
      {
        if (_isModCanBeLoaded != value)
        {
          _isModCanBeLoaded = value;
          OnPropertyChanged(nameof(IsModCanBeLoaded));
        }
      }
    }
    private bool _isModCanBeSaved = false;
    public bool IsModCanBeSaved
    {
      get => _isModCanBeSaved;
      set
      {
        if (_isModCanBeSaved != value)
        {
          _isModCanBeSaved = value;
          OnPropertyChanged(nameof(IsModCanBeSaved));
        }
      }
    }

    private bool _isModCanBeSavedAs = false;
    public bool IsModCanBeSavedAs
    {
      get => _isModCanBeSavedAs;
      set
      {
        if (_isModCanBeSavedAs != value)
        {
          _isModCanBeSavedAs = value;
        }
      }
    }

    public string X4DataFolderPath
    {
      get
      {
        if (Directory.Exists(X4DataFolder))
        {
          if (ValidateX4DataFolder(X4DataFolder, out _))
          {
            return System.IO.Path.GetFullPath(X4DataFolder);
          }
          else
          {
            return "Please set it!";
          }
        }
        else
        {
          return "Please set it!";
        }
      }
    }
    private bool _x4DataVersionOverride = false;
    public bool X4DataVersionOverride
    {
      get => _x4DataVersionOverride;
      set
      {
        if (_x4DataVersionOverride != value)
        {
          _x4DataVersionOverride = value;
          OnPropertyChanged(nameof(X4DataVersionOverride));
          // SaveConfiguration();
          if (!value && Galaxy != null && Galaxy.Version != 0 && Galaxy.Version != X4DataVersion)
          {
            X4DataVersion = Galaxy.Version;
          }
        }
      }
    }
    public ObservableCollection<string> X4DataVersions { get; set; } = ["7.10", "7.50"];
    private int _x4DataVersion = 710;
    public int X4DataVersion
    {
      get => _x4DataVersion;
      set
      {
        if (_x4DataVersion != value)
        {
          _x4DataVersion = value;
          OnPropertyChanged(nameof(X4DataVersion));
          OnPropertyChanged(nameof(X4DataVersionString));
          // _chemGateKeeperMod.SetGameVersion(_x4DataVersion);
          // SaveConfiguration();
        }
      }
    }
    public string X4DataVersionString
    {
      get => $"{_x4DataVersion / 100}.{_x4DataVersion % 100:D2}";
      set
      {
        string[] versionParts = value?.Split('.') ?? [];
        if (versionParts.Length == 2 && int.TryParse(versionParts[0], out int major) && int.TryParse(versionParts[1], out int minor))
        {
          int version = major * 100 + minor;
          if (version != _x4DataVersion)
          {
            X4DataVersion = version;
          }
        }
      }
    }
    private bool _loadModsData = false;
    public bool LoadModsData
    {
      get => _loadModsData;
      set
      {
        if (_loadModsData != value)
        {
          _loadModsData = value;
          OnPropertyChanged(nameof(LoadModsData));
          // SaveConfiguration();
        }
      }
    }

    private int _sectorRadius = 400;
    public int SectorRadius
    {
      get => _sectorRadius;
      set
      {
        if (_sectorRadius != value)
        {
          _sectorRadius = value;
          OnPropertyChanged(nameof(SectorRadius));
          // SaveConfiguration();
          // if (value > 0 && GatesConnectionCurrent != null)
          // {
          //   GatesConnectionCurrent.SetSectorMapInternalSize(value);
          // }
        }
      }
    }
    public int SectorRadiusNegative => -SectorRadius;

    private double _mapColorsOpacity = 0.5;
    public double MapColorsOpacity
    {
      get => _mapColorsOpacity;
      set
      {
        if (_mapColorsOpacity != value)
        {
          _mapColorsOpacity = value;
          OnPropertyChanged(nameof(MapColorsOpacity));
          // SaveConfiguration();
        }
      }
    }
    private string _logLevel = "Warning";
    public string LogLevel
    {
      get => _logLevel;
      set
      {
        if (_logLevel != value && value != null)
        {
          _logLevel = value;
          OnPropertyChanged(nameof(LogLevel));
          // SaveConfiguration();
        }
      }
    }

    private bool _logToFile = false;
    public bool LogToFile
    {
      get => _logToFile;
      set
      {
        if (_logToFile != value)
        {
          _logToFile = value;
          OnPropertyChanged(nameof(LogToFile));
          // SaveConfiguration();
        }
      }
    }

    public StatusBarMessage StatusBar { get; set; } = new();

    public MainWindow()
    {
      InitializeComponent();
    }

    private static bool ValidateX4DataFolder(string folderPath, out string errorMessage)
    {
      return X4Galaxy.ValidateDataFolder(folderPath, out errorMessage);
    }

    public void ButtonNewMod_Click(object sender, RoutedEventArgs e) { }

    public void ButtonLoadMod_Click(object sender, RoutedEventArgs e) { }

    public void ButtonSaveMod_Click(object sender, RoutedEventArgs e) { }

    public void ButtonSaveModAs_Click(object sender, RoutedEventArgs e) { }

    public void ButtonExtractX4Data_Click(object sender, RoutedEventArgs e) { }

    public void SelectX4DataFolder_Click(object sender, RoutedEventArgs e) { }

    public void ButtonReadme_Click(object sender, RoutedEventArgs e) { }

    public void ButtonAbout_Click(object sender, RoutedEventArgs e)
    {
      Dictionary<string, string> informationalLinks = new() { { "GitHub", "https://github.com/chemodun/X4-UniverseEditor" } };
      var bitmapImage = Icon as BitmapImage;
      AssemblyInfo assemblyInfo = AssemblyInfo.GetAssemblyInfo(Assembly.GetExecutingAssembly());
      AboutWindow aboutWindow = new(bitmapImage!, assemblyInfo, informationalLinks) { Owner = this };
      aboutWindow.ShowDialog();
    }

    public void ButtonExit_Click(object sender, RoutedEventArgs e)
    {
      Application.Current.Shutdown();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
