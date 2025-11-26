using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml.Linq;
using Localization;
using SharedWindows;
using Utilities.Logging;
using X4DataLoader;
using X4DataLoader.Helpers;
using X4Map;
using X4Map.Converters;

namespace ClusterRelocationService
{
  public class AppConfig
  {
    public ModeConfig Mode { get; set; } = new ModeConfig();
    public MapConfig Map { get; set; } = new MapConfig();
    public DataConfig Data { get; set; } = new DataConfig();
    public RelocationConfig Relocation { get; set; } = new RelocationConfig();
    public LoggingConfig Logging { get; set; } = new LoggingConfig();
    public OtherConfig Other { get; set; } = new OtherConfig();
  }

  public class ModeConfig
  {
    public bool DirectMode { get; set; } = false;
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
    public int X4DataVersion { get; set; } = 760;
    public bool LoadModsData { get; set; } = true;
  }

  public class RelocationConfig
  {
    public bool ExportClustersIsEnabled { get; set; } = false;
    public bool FullMessIsEnabled { get; set; } = false;
  }

  public class LoggingConfig : INotifyPropertyChanged
  {
    private string _logLevel = "Warning";
    private bool _logToFile = false;

    public string LogLevel
    {
      get => _logLevel;
      set
      {
        if (_logLevel != value)
        {
          _logLevel = value;
          OnPropertyChanged(nameof(LogLevel));
        }
      }
    }

    public bool LogToFile
    {
      get => _logToFile;
      set
      {
        if (_logToFile != value)
        {
          _logToFile = value;
          OnPropertyChanged(nameof(LogToFile));
        }
      }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }

  public class OtherConfig
  {
    private bool _checkForUpdatesOnStartUp = false;
    private string _languageCode = "en";
    public bool CheckForUpdatesOnStartUp
    {
      get => _checkForUpdatesOnStartUp;
      set
      {
        if (_checkForUpdatesOnStartUp != value)
        {
          _checkForUpdatesOnStartUp = value;
          OnPropertyChanged(nameof(CheckForUpdatesOnStartUp));
        }
      }
    }

    public string? LanguageCode
    {
      get => _languageCode;
      set
      {
        string newValue = string.IsNullOrWhiteSpace(value) ? _languageCode : value.Trim();
        if (!string.Equals(_languageCode, newValue, StringComparison.OrdinalIgnoreCase))
        {
          _languageCode = newValue;
          OnPropertyChanged(nameof(LanguageCode));
        }
      }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }

  public sealed record LanguageOption(string Code, string DisplayName);

  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window, INotifyPropertyChanged
  {
    private readonly string _configFileName;
    private readonly ObservableCollection<LanguageOption> _availableLanguages = new();
    private string _selectedLanguageCode = LocalizationManager.Shared.CurrentLanguage;
    private string? _preferredLanguageFromConfig;

    private static string T(string key, string? fallback = null)
    {
      return LocalizationManager.Shared.Translate(key, fallback ?? key);
    }

    private static string TF(string key, params object[] args)
    {
      string template = LocalizationManager.Shared.Translate(key, key);
      return string.Format(CultureInfo.CurrentCulture, template, args);
    }

    private bool _directMode = true;
    public bool DirectMode
    {
      get => _directMode;
      set
      {
        if (_directMode != value)
        {
          if (_hasBeenInitialized && Galaxy.Sectors.Count > 0)
          {
            var choice = MessageBox.Show(
              T("dialog.reloadX4.unsavedChanges.body"),
              T("dialog.reloadX4.title"),
              MessageBoxButton.YesNo,
              MessageBoxImage.Warning
            );
            if (choice != MessageBoxResult.Yes)
            {
              return; // User chose not to proceed
            }
          }
          _directMode = value;
          OnPropertyChanged(nameof(DirectMode));
          OnPropertyChanged(nameof(NoDirectMode));
          OnPropertyChanged(nameof(ExtractedVisibility));
          OnPropertyChanged(nameof(GameFolderVisibility));
          SaveConfiguration();
          if (_hasBeenInitialized)
          {
            ClusterRelocationServiceMod = new("", X4UniverseId);
            LoadX4DataInBackgroundStart();
          }
        }
      }
    }
    public bool NoDirectMode => !DirectMode;
    public Visibility ExtractedVisibility => DirectMode ? Visibility.Collapsed : Visibility.Visible;
    public Visibility GameFolderVisibility => DirectMode ? Visibility.Visible : Visibility.Collapsed;
    private string _x4GameFolder = "";
    public string X4GameFolder
    {
      get => _x4GameFolder;
      set
      {
        if (_x4GameFolder != value)
        {
          _x4GameFolder = value;
          OnPropertyChanged(nameof(X4GameFolder));
          OnPropertyChanged(nameof(X4GameFolderPath));
          SaveConfiguration();
        }
      }
    }

    public ObservableCollection<LanguageOption> AvailableLanguages => _availableLanguages;

    public string SelectedLanguageCode
    {
      get => _selectedLanguageCode;
      set
      {
        string normalized = string.IsNullOrWhiteSpace(value) ? LocalizationManager.Shared.DefaultLanguage : value.Trim();

        if (!_availableLanguages.Any(option => option.Code.Equals(normalized, StringComparison.OrdinalIgnoreCase)))
        {
          normalized = LocalizationManager.Shared.DefaultLanguage;
        }

        if (!string.Equals(_selectedLanguageCode, normalized, StringComparison.OrdinalIgnoreCase))
        {
          _selectedLanguageCode = normalized;
          OnPropertyChanged(nameof(SelectedLanguageCode));
          LocalizationManager.Shared.TrySetLanguage(_selectedLanguageCode);
          SaveConfiguration();
        }
      }
    }

    private bool _checkForUpdatesOnStartUp = false;
    public bool CheckForUpdatesOnStartUp
    {
      get => _checkForUpdatesOnStartUp;
      set
      {
        if (_checkForUpdatesOnStartUp != value)
        {
          _checkForUpdatesOnStartUp = value;
          OnPropertyChanged(nameof(CheckForUpdatesOnStartUp));
          SaveConfiguration();
        }
      }
    }

    private readonly List<GameFilesStructureItem> X4DataStructure = [];

    private readonly List<ProcessingOrderItem> X4PDataProcessingOrder =
    [
      new ProcessingOrderItem("translations", ""),
      new ProcessingOrderItem("colors", ""),
      new ProcessingOrderItem("galaxy", "clusters"),
      new ProcessingOrderItem("clusters", ""),
      new ProcessingOrderItem("mapDefaults", ""),
      new ProcessingOrderItem("sectors", ""),
      new ProcessingOrderItem("zones", ""),
      new ProcessingOrderItem("races", ""),
      new ProcessingOrderItem("factions", ""),
      new ProcessingOrderItem("modules", ""),
      new ProcessingOrderItem("modulegroups", ""),
      new ProcessingOrderItem("constructionplans", ""),
      new ProcessingOrderItem("stationgroups", ""),
      new ProcessingOrderItem("stations", ""),
      new ProcessingOrderItem("god", ""),
      new ProcessingOrderItem("sechighways", ""),
      new ProcessingOrderItem("zonehighways", ""),
      new ProcessingOrderItem("galaxy", "gates"),
    ];
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
          SaveConfiguration();
        }
      }
    }

    List<GameFile> _baseGameFiles = [];

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
          SaveConfiguration();
          if (!value && Galaxy != null && Galaxy.Version != 0 && Galaxy.Version != X4DataVersion)
          {
            X4DataVersion = Galaxy.Version;
          }
        }
      }
    }

    private int _x4DataVersion = 760;
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
          _clusterRelocationServiceMod.SetGameVersion(_x4DataVersion);
          SaveConfiguration();
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

    private bool _loadModsData = true;
    public bool LoadModsData
    {
      get => _loadModsData;
      set
      {
        if (_loadModsData != value)
        {
          if (_hasBeenInitialized && Galaxy.Sectors.Count > 0)
          {
            var choice = MessageBox.Show(
              T("dialog.reloadX4.unsavedChanges.body"),
              T("dialog.reloadX4.title"),
              MessageBoxButton.YesNo,
              MessageBoxImage.Warning
            );
            if (choice != MessageBoxResult.Yes)
            {
              return; // User chose not to proceed
            }
          }
          _loadModsData = value;
          OnPropertyChanged(nameof(LoadModsData));
          SaveConfiguration();
          if (_hasBeenInitialized)
          {
            LoadX4DataInBackgroundStart();
          }
        }
      }
    }

    bool _isFullReloadNeeded = false;

    public ObservableCollection<string> X4DataVersions { get; set; } = ["7.10", "7.50", "7.60"];

    private string _x4UniverseId = DataLoader.DefaultUniverseId;
    public string X4UniverseId
    {
      get => _x4UniverseId;
      set
      {
        if (_x4UniverseId != value)
        {
          _x4UniverseId = value;
          OnPropertyChanged(nameof(X4UniverseId));
          X4UniverseIdIsDefault = value == DataLoader.DefaultUniverseId;
          SaveConfiguration();
        }
      }
    }
    private bool _x4UniverseIdIsDefault = true;
    public bool X4UniverseIdIsDefault
    {
      get => _x4UniverseIdIsDefault;
      set
      {
        if (_x4UniverseIdIsDefault != value)
        {
          _x4UniverseIdIsDefault = value;
          OnPropertyChanged(nameof(X4UniverseIdIsDefault));
          if (value)
          {
            X4UniverseId = DataLoader.DefaultUniverseId;
          }
        }
      }
    }

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
          SaveConfiguration();
        }
      }
    }

    private bool _exportClustersIsEnabled = false;
    public bool ExportClustersIsEnabled
    {
      get => _exportClustersIsEnabled;
      set
      {
        if (_exportClustersIsEnabled != value)
        {
          _exportClustersIsEnabled = value;
          OnPropertyChanged(nameof(ExportClustersIsEnabled));
          UpdateIsModCanBeSavedAs();
          SaveConfiguration();
        }
      }
    }

    private bool _fullMessIsEnabled = false;
    public bool FullMessIsEnabled
    {
      get => _fullMessIsEnabled;
      set
      {
        if (_fullMessIsEnabled != value)
        {
          _fullMessIsEnabled = value;
          OnPropertyChanged(nameof(FullMessIsEnabled));
          SaveConfiguration();
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
          SaveConfiguration();
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
          SaveConfiguration();
        }
      }
    }

    public StatusBarMessage StatusBar { get; set; } = new();

    public ObservableCollection<RelocatedCluster> RelocatedClusters { get; private set; } = new();

    public ObservableCollection<OverlaidClusterInfo> OverlaidClusters { get; } = new();

    private RelocatedCluster? _relocatedClusterCurrent = null;
    public RelocatedCluster? RelocatedClusterCurrent
    {
      get => _relocatedClusterCurrent;
      set
      {
        if (_relocatedClusterCurrent != value)
        {
          _relocatedClusterCurrent = value;
          OnPropertyChanged(nameof(RelocatedClusterCurrent));
          IsRelocationCanBeCancelled = value != null && !GalaxyMapViewer.IsRelocatedClusterOriginalLocationOccupied(value);
          GalaxyMapViewer.OnRelocatedClusterCurrent(value);
        }
      }
    }

    private RelocatedClustersMod _clusterRelocationServiceMod = new();
    public RelocatedClustersMod ClusterRelocationServiceMod
    {
      get => _clusterRelocationServiceMod;
      set
      {
        if (_clusterRelocationServiceMod != value)
        {
          _clusterRelocationServiceMod = value;
          OnPropertyChanged(nameof(ClusterRelocationServiceMod));
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
          // Save As should be enabled whenever Save is enabled OR the mod has any content loaded
          UpdateIsModCanBeSavedAs();
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
          OnPropertyChanged(nameof(IsModCanBeSavedAs));
        }
      }
    }

    private bool _hasRelocated = false;
    public bool HasRelocated
    {
      get => _hasRelocated;
      set
      {
        if (_hasRelocated != value)
        {
          _hasRelocated = value;
          OnPropertyChanged(nameof(HasRelocated));
          UpdateIsModCanBeSavedAs();
        }
      }
    }

    private void UpdateIsModCanBeSavedAs()
    {
      IsModCanBeSavedAs = IsModCanBeSaved || (HasRelocated && NoClusterInRelocationInProcess);
    }

    private void UpdateOverlaidClustersList()
    {
      if (Galaxy == null || Galaxy.Clusters.Count == 0)
      {
        OverlaidClusters.Clear();
        return;
      }

      List<List<Cluster>> groups = [];

      foreach (Cluster cluster in Galaxy.Clusters)
      {
        if (cluster == null)
        {
          continue;
        }

        Position? position = cluster.Position;
        if (position == null)
        {
          continue;
        }

        List<Cluster>? matchingGroup = null;
        foreach (List<Cluster> existingGroup in groups)
        {
          if (existingGroup.Any(existing => GalaxyMapViewer.ShareSameCell(existing.Position, position)))
          {
            matchingGroup = existingGroup;
            break;
          }
        }

        if (matchingGroup == null)
        {
          matchingGroup = [];
          groups.Add(matchingGroup);
        }

        matchingGroup.Add(cluster);
      }

      var overlayInfos = groups
        .Where(group => group.Count > 1)
        .SelectMany(group =>
        {
          return group.Select(cluster =>
          {
            List<Cluster> others = group.Where(other => !ReferenceEquals(other, cluster)).ToList();
            bool isRelocated = RelocatedClusters.Any(relocated =>
              relocated?.Cluster != null && StringHelper.EqualsIgnoreCase(relocated.Cluster.Macro, cluster.Macro)
            );
            bool isVisibleOnMap = false;
            GalaxyMapCluster? mapCluster = GalaxyMapViewer.GetClusterByMacro(cluster.Macro);
            if (mapCluster is GalaxyMapClusterForClusterRelocation relocationCluster)
            {
              isVisibleOnMap = relocationCluster.Cluster != null && ReferenceEquals(relocationCluster.Cluster, cluster);
            }
            return new OverlaidClusterInfo(cluster, others, isRelocated, isVisibleOnMap);
          });
        })
        .OrderBy(info => info.X)
        .ThenBy(info => info.Z)
        .ThenBy(info => info.Macro, StringComparer.OrdinalIgnoreCase)
        .ToList();

      OverlaidClusters.Clear();
      foreach (OverlaidClusterInfo info in overlayInfos)
      {
        OverlaidClusters.Add(info);
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
            return T("status.folder.placeholder");
          }
        }
        else
        {
          return T("status.folder.placeholder");
        }
      }
    }
    public string X4GameFolderPath
    {
      get
      {
        if (Directory.Exists(X4GameFolder))
        {
          if (ValidateX4GameFolder(X4GameFolder, out _))
          {
            return System.IO.Path.GetFullPath(X4GameFolder);
          }
          else
          {
            return T("status.folder.placeholder");
          }
        }
        else
        {
          return T("status.folder.placeholder");
        }
      }
    }

    private void HandleValidationError(string message)
    {
      // Optional: Log the validation message for debugging
      Log.Error($"Validation Error: {message}");

      StatusBar.SetStatusMessage(message, StatusMessageType.Error);
    }

    // Galaxy and Sectors
    public Galaxy Galaxy { get; private set; } = new();

    public bool IsDataLoaded
    {
      get => Galaxy.Sectors.Count > 0;
    }

    private bool _isRelocationCanBeDeleted = false;
    public bool IsRelocationCanBeCancelled
    {
      get => _isRelocationCanBeDeleted;
      set
      {
        if (_isRelocationCanBeDeleted != value)
        {
          _isRelocationCanBeDeleted = value;
          if (!value && RelocatedClusterCurrent != null)
          {
            StatusBar.SetStatusMessage(T("status.relocation.originalLocationInUse"), StatusMessageType.Warning);
          }
          OnPropertyChanged(nameof(IsRelocationCanBeCancelled));
        }
      }
    }

    bool _noClusterInRelocationInProcess = true;
    public bool NoClusterInRelocationInProcess
    {
      get => _noClusterInRelocationInProcess;
      set
      {
        if (_noClusterInRelocationInProcess != value)
        {
          _noClusterInRelocationInProcess = value;
          OnPropertyChanged(nameof(NoClusterInRelocationInProcess));
        }
      }
    }

    public bool IsModCanBeCreated => IsDataLoaded & RelocatedClusters.Count > 0;

    private int _selectedTabIndex = -1;
    public int SelectedTabIndex
    {
      get => _selectedTabIndex;
      set
      {
        if (_selectedTabIndex != value)
        {
          _selectedTabIndex = value;
          OnPropertyChanged(nameof(SelectedTabIndex));
        }
      }
    }

    private readonly BitmapImage _appIcon;

    private readonly BackgroundWorker _backgroundWorker;

    private bool _isBusy = false;
    public bool IsBusy
    {
      get => _isBusy;
      set
      {
        if (_isBusy != value)
        {
          _isBusy = value;
          OnPropertyChanged(nameof(IsBusy));
        }
      }
    }

    private string _busyMessage = "";
    public string BusyMessage
    {
      get => _busyMessage;
      set
      {
        if (_busyMessage != value)
        {
          _busyMessage = value;
          OnPropertyChanged(nameof(BusyMessage));
          StatusBar.SetStatusMessage(value, StatusMessageType.Info);
        }
      }
    }
    private bool _hasBeenInitialized = false;

    private GalaxyMapClusterForClusterRelocation? _markedForRelocation = null;
    private Cluster? _markedForRelocationOverlaid = null;
    private Visibility _optionsVisibilityState = Visibility.Hidden;
    private string _optionsVisibilitySymbol = "CircleLeft";
    private double _optionsWidth = 10;

    public Visibility OptionsVisibilityState
    {
      get => _optionsVisibilityState;
      set
      {
        _optionsVisibilityState = value;
        if (value == Visibility.Visible)
        {
          OptionsWidth = double.NaN;
          OptionsVisibilitySymbol = "CircleRight";
        }
        else
        {
          OptionsWidth = 10;
          OptionsVisibilitySymbol = "CircleLeft";
        }
        OnPropertyChanged(nameof(OptionsVisibilityState));
      }
    }

    public string OptionsVisibilitySymbol
    {
      get => _optionsVisibilitySymbol;
      set
      {
        _optionsVisibilitySymbol = value;
        OnPropertyChanged(nameof(OptionsVisibilitySymbol));
      }
    }

    public double OptionsWidth
    {
      get => _optionsWidth;
      set
      {
        _optionsWidth = value;
        OnPropertyChanged(nameof(OptionsWidth));
      }
    }

    // Constructor
    public MainWindow()
    {
      _configFileName = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.json";
      LoadConfiguration();
      InitializeLanguageOptions(_preferredLanguageFromConfig);
      InitializeComponent();
      DataContext = this;
      X4DataStructure.AddRange(
        [
          new GameFilesStructureItem(id: "translations", folder: "t", ["0001-l044.xml", "0001.xml"], MatchingModes.Exact, false),
          new GameFilesStructureItem(id: "colors", folder: "libraries", ["colors.xml"]),
          new GameFilesStructureItem(id: "mapDefaults", folder: "libraries", ["mapdefaults.xml"]),
          new GameFilesStructureItem(id: "clusters", folder: $"maps/{X4UniverseId}", ["clusters.xml"], MatchingModes.Suffix),
          new GameFilesStructureItem(id: "sectors", folder: $"maps/{X4UniverseId}", ["sectors.xml"], MatchingModes.Suffix),
          new GameFilesStructureItem(id: "zones", folder: $"maps/{X4UniverseId}", ["zones.xml"], MatchingModes.Suffix),
          new GameFilesStructureItem(id: "races", folder: "libraries", ["races.xml"]),
          new GameFilesStructureItem(id: "factions", folder: "libraries", ["factions.xml"]),
          new GameFilesStructureItem(id: "modules", folder: "libraries", ["modules.xml"]),
          new GameFilesStructureItem(id: "modulegroups", folder: "libraries", ["modulegroups.xml"]),
          new GameFilesStructureItem(id: "constructionplans", folder: "libraries", ["constructionplans.xml"]),
          new GameFilesStructureItem(id: "stationgroups", folder: "libraries", ["stationgroups.xml"]),
          new GameFilesStructureItem(id: "stations", folder: "libraries", ["stations.xml"]),
          new GameFilesStructureItem(id: "god", folder: "libraries", ["god.xml"]),
          new GameFilesStructureItem(id: "sechighways", folder: $"maps/{X4UniverseId}", ["sechighways.xml"], MatchingModes.Suffix),
          new GameFilesStructureItem(id: "zonehighways", folder: $"maps/{X4UniverseId}", ["zonehighways.xml"], MatchingModes.Suffix),
          new GameFilesStructureItem(id: "galaxy", folder: $"maps/{X4UniverseId}", ["galaxy.xml"]),
          new GameFilesStructureItem(id: "patchactions", folder: "libraries", ["patchactions.xml"]),
        ]
      );
      Assembly assembly = Assembly.GetExecutingAssembly();
      AssemblyName assemblyName = assembly.GetName();
      _appIcon = Icon as BitmapImage ?? new BitmapImage();
      Title = $"{Title} v{assemblyName.Version}";
      _clusterRelocationServiceMod.SetGameVersion(X4DataVersion);

      // Subscribe to Validation Errors
      TextBoxExtensions.OnValidationError += HandleValidationError;

      _backgroundWorker = new BackgroundWorker { WorkerReportsProgress = true, WorkerSupportsCancellation = false };
      GalaxyMapViewer.Connect(Galaxy!, GalaxyMapCanvas, MapColorsOpacity);

      GalaxyMapViewer.OnPressedCluster += GalaxyMapViewer_OnPressedMouseButton;
      GalaxyMapViewer.OnPressedSector += GalaxyMapViewer_OnPressedMouseButton;
      GalaxyMapViewer.OnPressedCell += GalaxyMapViewer_OnPressedMouseButton;
      GalaxyMapViewer.OnPressedSectorItem += GalaxyMapViewer_OnPressedMouseButton;

      GalaxyMapViewer.OnRightPressedSector += GalaxyMapViewer_OnPressedMouseButton;
      GalaxyMapViewer.OnRightPressedCluster += GalaxyMapViewer_OnPressedMouseButton;
      GalaxyMapViewer.OnRightPressedCell += GalaxyMapViewer_OnPressedMouseButton;
      GalaxyMapViewer.OnRightPressedSectorItem += GalaxyMapViewer_OnPressedMouseButton;

      GalaxyMapViewer.OnRelocationMessage += GalaxyMapViewer_OnRelocationMessage;
      RelocatedClusters.CollectionChanged += RelocatedClusters_CollectionChanged;

      GalaxyMapViewer.RefreshGalaxyData();
      UpdateOverlaidClustersList();
      Dispatcher.BeginInvoke(
        DispatcherPriority.Loaded,
        new Action(() =>
        {
          X4DataNotLoadedCheckAndWarning();
        })
      );
    }

    private void LoadConfiguration()
    {
      if (File.Exists(_configFileName))
      {
        var jsonString = File.ReadAllText(_configFileName);
        var config = JsonSerializer.Deserialize<AppConfig>(jsonString);

        if (config != null)
        {
          // Be resilient to older or manually edited configs where sections may be null
          var mode = config.Mode ?? new ModeConfig();
          var data = config.Data ?? new DataConfig();
          var map = config.Map ?? new MapConfig();
          var relocation = config.Relocation ?? new RelocationConfig();
          var logging = config.Logging ?? new LoggingConfig();
          var other = config.Other ?? new OtherConfig();

          DirectMode = mode.DirectMode;

          X4DataFolder = data.X4DataExtractedPath ?? ".";
          X4DataVersionOverride = data.X4DataVersionOverride;
          if (X4DataVersionOverride)
          {
            X4DataVersion = data.X4DataVersion;
          }
          if (!string.IsNullOrEmpty(data.X4GameFolder))
          {
            X4GameFolder = data.X4GameFolder;
          }
          LoadModsData = data.LoadModsData;

          if (map.NonStandardUniverse && !string.IsNullOrEmpty(map.NonStandardUniverseId))
          {
            X4UniverseId = map.NonStandardUniverseId;
          }
          MapColorsOpacity = map.MapColorsOpacity;

          ExportClustersIsEnabled = relocation.ExportClustersIsEnabled;
          FullMessIsEnabled = relocation.FullMessIsEnabled;

          LogLevel = logging.LogLevel ?? "Warning";
          LogToFile = logging.LogToFile;

          CheckForUpdatesOnStartUp = other.CheckForUpdatesOnStartUp;
          _preferredLanguageFromConfig = other.LanguageCode;
        }
      }
      else
      {
        // Set default values if the config file does not exist
        X4DataFolder = ".";
        LogLevel = "Warning";
        LogToFile = false;
      }
    }

    // Cache and reuse options for config serialization
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private void SaveConfiguration()
    {
      var config = new AppConfig
      {
        Mode = new ModeConfig { DirectMode = DirectMode },
        Data = new DataConfig
        {
          X4DataExtractedPath = X4DataFolder,
          X4DataVersionOverride = X4DataVersionOverride,
          LoadModsData = LoadModsData,
        },
        Map = new MapConfig { MapColorsOpacity = MapColorsOpacity },
        Relocation = new RelocationConfig { ExportClustersIsEnabled = ExportClustersIsEnabled, FullMessIsEnabled = FullMessIsEnabled },
        Logging = new LoggingConfig { LogLevel = LogLevel, LogToFile = LogToFile },
        Other = new OtherConfig { CheckForUpdatesOnStartUp = CheckForUpdatesOnStartUp, LanguageCode = SelectedLanguageCode },
      };
      if (X4UniverseId != DataLoader.DefaultUniverseId)
      {
        config.Map.NonStandardUniverse = true;
        config.Map.NonStandardUniverseId = X4UniverseId;
      }
      else
      {
        config.Map.NonStandardUniverse = false;
        config.Map.NonStandardUniverseId = "";
      }
      App.ConfigureNLog(config.Logging);
      if (X4DataVersionOverride)
      {
        config.Data.X4DataVersion = X4DataVersion;
      }
      if (!String.IsNullOrEmpty(X4GameFolder))
      {
        config.Data.X4GameFolder = X4GameFolder;
      }
      var jsonString = JsonSerializer.Serialize(config, JsonOptions);
      File.WriteAllText(_configFileName, jsonString);
    }

    private void InitializeLanguageOptions(string? preferredLanguageCode)
    {
      var manager = LocalizationManager.Shared;
      _availableLanguages.Clear();

      var orderedLanguages = manager.AvailableLanguages.OrderBy(code => code, StringComparer.OrdinalIgnoreCase);
      foreach (string code in orderedLanguages)
      {
        string displayName = manager.GetLanguageDisplayName(code, code);
        _availableLanguages.Add(new LanguageOption(code, displayName));
      }

      if (_availableLanguages.Count == 0)
      {
        _selectedLanguageCode = manager.DefaultLanguage;
        OnPropertyChanged(nameof(SelectedLanguageCode));
        return;
      }

      string target = preferredLanguageCode ?? manager.CurrentLanguage;
      if (
        string.IsNullOrWhiteSpace(target)
        || !_availableLanguages.Any(option => option.Code.Equals(target, StringComparison.OrdinalIgnoreCase))
      )
      {
        target = manager.CurrentLanguage;
      }

      _selectedLanguageCode = target;
      OnPropertyChanged(nameof(SelectedLanguageCode));
      manager.TrySetLanguage(target);
    }

    private void X4DataNotLoadedCheckAndWarning()
    {
      // Validate the loaded X4DataFolder
      if (DirectMode && !ValidateX4GameFolder(X4GameFolder, out string errorMessage))
      {
        StatusBar.SetStatusMessage(errorMessage, StatusMessageType.Error);
        // Prompt the user to select a valid folder
        _ = MessageBox.Show(
          T("dialog.gameFolderNotSet.body"),
          T("dialog.gameFolderNotSet.title"),
          MessageBoxButton.OK,
          MessageBoxImage.Warning
        );
        StatusBar.SetStatusMessage(T("status.gameFolder.selectValid"), StatusMessageType.Warning);
        // Show the ribbon tab options
        RibbonMain.SelectedTabItem = (Fluent.RibbonTabItem)RibbonMain.FindName("RibbonTabConfiguration")!;
      }
      else if (!DirectMode && !ValidateX4DataFolder(X4DataFolder, out string errorMessageData))
      {
        StatusBar.SetStatusMessage(errorMessageData, StatusMessageType.Error);
        // Prompt the user to select a valid folder
        _ = MessageBox.Show(
          T("dialog.dataFolderNotSet.body"),
          T("dialog.dataFolderNotSet.title"),
          MessageBoxButton.OK,
          MessageBoxImage.Warning
        );
        StatusBar.SetStatusMessage(T("status.dataFolder.selectValid"), StatusMessageType.Warning);
        // Show the ribbon tab options
        RibbonMain.SelectedTabItem = (Fluent.RibbonTabItem)RibbonMain.FindName("RibbonTabConfiguration")!;
      }
      else
      {
        LoadX4DataInBackgroundStart();
      }
    }

    private async void LoadX4DataInBackgroundStart()
    {
      IsBusy = true;
      _clusterRelocationServiceMod = new("", X4UniverseId);
      _clusterRelocationServiceMod.SetGameVersion(_x4DataVersion);
      await ResetRelocations();
      BusyMessage = T("busy.data.preparing");
      Galaxy.Clear();
      OverlaidClusters.Clear();
      _backgroundWorker.DoWork -= LoadX4DataInBackground;
      _backgroundWorker.DoWork += LoadX4DataInBackground;
      _backgroundWorker.ProgressChanged -= LoadX4DataInBackgroundProgressChanged;
      _backgroundWorker.ProgressChanged += LoadX4DataInBackgroundProgressChanged;
      _backgroundWorker.RunWorkerCompleted -= LoadX4DataInBackgroundCompleted;
      _backgroundWorker.RunWorkerCompleted += LoadX4DataInBackgroundCompleted;
      _backgroundWorker.RunWorkerAsync();
    }

    private void LoadX4DataInBackground(object? sender, DoWorkEventArgs e)
    {
      LoadX4Data();
    }

    private void LoadX4DataInBackgroundProgressChanged(object? sender, ProgressChangedEventArgs e)
    {
      if (e.UserState is string progressText)
      {
        BusyMessage = TF("busy.data.processingFile", progressText);
        StatusBar.SetStatusMessage(BusyMessage, StatusMessageType.Info, true);
      }
    }

    private async void LoadX4DataInBackgroundCompleted(object? sender, RunWorkerCompletedEventArgs e)
    {
      _backgroundWorker.Dispose();
      _isFullReloadNeeded = false;
      if (e.Error != null)
      {
        StatusBar.SetStatusMessage(TF("status.error.loadingData", e.Error.Message), StatusMessageType.Error);
        IsBusy = false;
        _hasBeenInitialized = true;
        return;
      }
      _baseGameFiles = GameFile.CloneList(Galaxy.GameFiles, true);
      var sectors = Galaxy.GetSectors();
      // Ensure detected version appears in the versions list, formatted as major.minor (minor 2 digits) and sorted
      if (Galaxy.Version != 0)
      {
        string detectedVersion = $"{Galaxy.Version / 100}.{Galaxy.Version % 100:D2}";
        if (!X4DataVersions.Contains(detectedVersion))
        {
          var sorted = X4DataVersions
            .Concat([detectedVersion])
            .Distinct()
            .Select(s =>
            {
              var parts = (s ?? string.Empty).Split('.');
              int major = 0;
              int minor = 0;
              if (parts.Length > 0)
              {
                _ = int.TryParse(parts[0], out major);
              }
              if (parts.Length > 1)
              {
                _ = int.TryParse(parts[1], out minor);
              }
              return (major, minor);
            })
            .OrderBy(v => v.major)
            .ThenBy(v => v.minor)
            .Select(v => $"{v.major}.{v.minor:D2}")
            .ToList();
          X4DataVersions.Clear();
          foreach (var v in sorted)
          {
            X4DataVersions.Add(v);
          }
        }
      }
      if (!X4DataVersionOverride && Galaxy.Version != 0 && Galaxy.Version != X4DataVersion)
      {
        X4DataVersion = Galaxy.Version;
      }
      OnPropertyChanged(nameof(IsDataLoaded));
      BusyMessage = T("busy.data.loaded");
      RibbonMain.SelectedTabItem = (Fluent.RibbonTabItem)RibbonMain.FindName("RibbonTabMod")!;
      GalaxyMapViewer.RefreshGalaxyData();
      GalaxyMapViewer.UpdateMap();
      if (DirectMode)
      {
        await ModLoadAsync();
        ButtonLoadMod.Header = T("ribbon.mod.reload");
      }
      else
      {
        ButtonLoadMod.Header = T("ribbon.mod.load");
        await AssignRelocatedClusters();
      }
      if (RelocatedClusters.Count == 0)
      {
        _clusterRelocationServiceMod.SetGameVersion(X4DataVersion);
      }
      if (!_hasBeenInitialized)
      {
        if (CheckForUpdatesOnStartUp)
        {
          UpdateChecker.onCheckUpdatePressedAsync(this, StatusBar, Assembly.GetExecutingAssembly(), true);
        }
        _hasBeenInitialized = true;
      }
      UpdateOverlaidClustersList();
      IsBusy = false;
    }

    private async Task AssignRelocatedClusters()
    {
      RelocatedClusters.Clear();
      RelocatedClusterCurrent = null;
      var queue = new Queue<RelocatedCluster>(ClusterRelocationServiceMod.RelocatedClustersList);
      int maxPasses = queue.Count + 2; // Prevent infinite loop
      int passes = 0;

      while (queue.Count > 0 && passes < maxPasses)
      {
        int itemsThisPass = queue.Count;
        for (int i = 0; i < itemsThisPass; i++)
        {
          var rc = queue.Dequeue();
          var occupied = GalaxyMapViewer.IsRelocatedClusterTargetLocationOccupied(rc);
          if (occupied && passes > 2)
          {
            queue.Enqueue(rc); // retry later
          }
          else
          {
            GalaxyMapClusterForClusterRelocation? targetCluster = GalaxyMapViewer.GetTargetClusterForRelocated(rc);
            GalaxyMapClusterForClusterRelocation? currentCluster = GalaxyMapViewer.GetMapClusterForRelocated(rc);
            if (targetCluster == null)
            {
              Log.Error($"Could not find target or current cluster for relocation: {rc.Cluster.Id}");
              continue; // skip this one
            }
            if (occupied && currentCluster != null)
            {
              Log.Warn($"Target location still occupied for relocation: {rc.Cluster.Id}. Resetting position.");
              GalaxyMapClusterForClusterRelocation? interimCluster = GalaxyMapViewer.GetRandomFreeCluster();
              if (interimCluster == null)
              {
                Log.Error($"Could not find interim cluster for relocation: {rc.Cluster.Id}");
                continue; // skip this one
              }
              rc.ResetPosition();
              RelocatedCluster? relocatedToInterim = GalaxyMapViewer.RelocateCluster(currentCluster, interimCluster);
              if (relocatedToInterim != null)
              {
                relocatedToInterim.ReAssignCluster(rc.Cluster, targetCluster.Position);
                queue.Enqueue(relocatedToInterim); // retry later
                await Task.Delay(5);
                await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Render);
              }
              continue; // skip this one
            }
            if (currentCluster != null)
            {
              rc.ResetPosition();
              RelocatedCluster? relocated = GalaxyMapViewer.RelocateCluster(currentCluster, targetCluster);
              if (relocated != null)
              {
                RelocatedClusters.Add(relocated);
                await Task.Delay(5);
                await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Render);
              }
            }
            else
            {
              rc.ResetPosition();
              RelocatedCluster relocated = new(rc.Cluster);
              relocated.Cluster.SetPosition(targetCluster.Position);
              GalaxyMapViewer.GalaxyMapClusterReassign(targetCluster, relocated.Cluster);
              targetCluster.IsRelocated = true;
              if (relocated != null)
              {
                ClusterRelocationServiceMod.RelocatedClustersList.Remove(rc);
                ClusterRelocationServiceMod.RelocatedClustersList.Add(relocated);
                RelocatedClusters.Add(relocated);
                await Task.Delay(5);
                await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Render);
              }
            }
          }
        }
        passes++;
      }
      GalaxyMapViewer.ReCheckCovers();
      // Any still in queue after maxPasses could not be canceled
      if (queue.Count > 0)
      {
        string message = TF("status.error.relocationAssignmentFailed", queue.Count);
        Log.Warn(message);
        StatusBar.SetStatusMessage(message, StatusMessageType.Warning);
        foreach (var rc in queue)
        {
          Log.Warn($"Resetting relocation: {rc.Cluster.Name}");
          rc.ResetPosition();
        }
      }
      if (RelocatedClusters.Count > 0)
      {
        RelocatedClusterCurrent = RelocatedClusters[0];
      }
      NoClusterInRelocationInProcess = true;
      UpdateOverlaidClustersList();
    }

    private static bool ValidateX4DataFolder(string folderPath, out string errorMessage)
    {
      return DataLoader.ValidateDataFolder(folderPath, out errorMessage);
    }

    private static bool ValidateX4GameFolder(string folderPath, out string errorMessage)
    {
      if (string.IsNullOrEmpty(folderPath))
      {
        errorMessage = T("error.gameFolder.notSet");
        return false;
      }
      if (!Directory.Exists(folderPath))
      {
        errorMessage = T("error.gameFolder.missing");
        return false;
      }
      if (!File.Exists(Path.Combine(folderPath, X4DataExtractionWindow.X4Executable)))
      {
        errorMessage = T("error.gameFolder.missingExe");
        return false;
      }
      errorMessage = "";
      return true;
    }

    private void SelectX4DataFolder_Click(object? sender, RoutedEventArgs? e)
    {
      if (Galaxy.Sectors.Count > 0)
      {
        MessageBoxResult confirm = MessageBox.Show(
          T("dialog.reloadX4.confirm.body"),
          T("dialog.reloadX4.title"),
          MessageBoxButton.YesNo,
          MessageBoxImage.Warning
        );
        if (confirm == MessageBoxResult.No)
        {
          return;
        }
      }
      else
      {
        MessageBoxResult confirm = MessageBox.Show(
          T("dialog.selectX4DataFolder.prompt.body"),
          T("dialog.selectX4DataFolder.prompt.title"),
          MessageBoxButton.YesNo,
          MessageBoxImage.Warning
        );
        if (confirm == MessageBoxResult.No)
        {
          RibbonMain.SelectedTabItem = (Fluent.RibbonTabItem)RibbonMain.FindName("RibbonTabX4Data")!;
          return;
        }
      }
      var dialog = new Microsoft.Win32.OpenFolderDialog { Title = T("dialog.selectX4DataFolder.browser.title") };
      if (!string.IsNullOrEmpty(X4DataFolder))
      {
        dialog.InitialDirectory = !string.IsNullOrEmpty(X4DataFolder) && X4DataFolder != "." ? $"{X4DataFolder}\\" : ".";
      }
      else
      {
        dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer);
      }
      bool? result = dialog.ShowDialog();

      if (result == true && !string.IsNullOrWhiteSpace(dialog.FolderName))
      {
        string selectedPath = dialog.FolderName;
        if (ValidateX4DataFolder(selectedPath, out string errorMessage))
        {
          X4DataFolder = selectedPath;
          StatusBar.SetStatusMessage(T("status.x4data.folderSet"), StatusMessageType.Info);
          LoadX4DataInBackgroundStart();
        }
        else
        {
          StatusBar.SetStatusMessage(errorMessage, StatusMessageType.Error);
          MessageBox.Show(errorMessage, T("dialog.invalidFolder.title"), MessageBoxButton.OK, MessageBoxImage.Error);
          // Optionally, prompt again
        }
      }
      else
      {
        StatusBar.SetStatusMessage(T("status.folderSelectionCancelled"), StatusMessageType.Warning);
      }
    }

    private void SelectX4GameFolder_Click(object? sender, RoutedEventArgs? e)
    {
      if (_hasBeenInitialized && Galaxy.Sectors.Count > 0)
      {
        var choice = MessageBox.Show(
          T("dialog.reloadX4.unsavedChanges.body"),
          T("dialog.reloadX4.title"),
          MessageBoxButton.YesNo,
          MessageBoxImage.Warning
        );
        if (choice != MessageBoxResult.Yes)
        {
          return; // User chose not to proceed
        }
      }
      var dialog = new Microsoft.Win32.OpenFileDialog
      {
        InitialDirectory = string.IsNullOrEmpty(X4GameFolder)
          ? Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)
          : X4GameFolder,
        Filter = $"{T("dialog.x4Executable.filterDescription")}|{X4DataExtractionWindow.X4Executable}",
        Title = T("dialog.x4Executable.title"),
      };

      bool? result = dialog.ShowDialog();
      if (result != true || string.IsNullOrWhiteSpace(dialog.FileName))
      {
        Log.Warn("No game folder selected");
        return;
      }
      try
      {
        string selectedPath = Path.GetDirectoryName(dialog.FileName) ?? "";
        if (ValidateX4GameFolder(selectedPath, out string errorMessage))
        {
          X4GameFolder = selectedPath;
          StatusBar.SetStatusMessage(T("status.x4game.folderSet"), StatusMessageType.Info);
          LoadX4DataInBackgroundStart();
        }
        else
        {
          StatusBar.SetStatusMessage(errorMessage, StatusMessageType.Error);
          MessageBox.Show(errorMessage, T("dialog.invalidFolder.title"), MessageBoxButton.OK, MessageBoxImage.Error);
          // Optionally, prompt again
        }
      }
      catch (Exception ex)
      {
        Log.Error("Error selecting game folder", ex);
        return;
      }
    }

    private void LoadX4Data()
    {
      DataLoader dataLoader = new();
      dataLoader.X4DataLoadingEvent += (sender, e) =>
      {
        if (e.ProcessingFile != null)
        {
          _backgroundWorker.ReportProgress(0, e.ProcessingFile);
        }
      };
      if (DirectMode)
      {
        dataLoader.LoadData(
          Galaxy,
          X4GameFolder,
          X4DataStructure,
          X4PDataProcessingOrder,
          LoadModsData,
          true,
          [RelocatedClustersMod.ModId]
        );
      }
      else
      {
        dataLoader.LoadData(Galaxy, X4DataFolder, X4DataStructure, X4PDataProcessingOrder, LoadModsData);
      }
    }

    private void RelocatedClusters_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
      GalaxyMapViewer.OnUpdateRelocatedClusters(RelocatedClusters, e);
      IsModCanBeSaved = ClusterRelocationServiceMod.IsModChanged(RelocatedClusters);
      HasRelocated = ClusterRelocationServiceMod != null && RelocatedClusters.Count > 0;
      UpdateOverlaidClustersList();
    }

    private void OverlaidClustersDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
      if (e.ChangedButton != MouseButton.Left)
      {
        return;
      }

      if (sender is not DataGrid dataGrid)
      {
        return;
      }

      if (!TryGetDataGridRow(e.OriginalSource, out DataGridRow? row) || row?.DataContext is not OverlaidClusterInfo clusterInfo)
      {
        return;
      }

      if (string.IsNullOrWhiteSpace(clusterInfo.Macro))
      {
        return;
      }

      GalaxyMapViewer.CenterOnCluster(clusterInfo.Macro);
      e.Handled = true;
    }

    private static bool TryGetDataGridRow(object source, out DataGridRow? row)
    {
      row = null;
      if (source is not DependencyObject dependencyObject)
      {
        return false;
      }

      DependencyObject current = dependencyObject;
      while (current != null && current is not DataGridRow)
      {
        current = VisualTreeHelper.GetParent(current);
      }

      row = current as DataGridRow;
      return row != null;
    }

    public void ButtonRelocationCancel_Click(object sender, RoutedEventArgs e)
    {
      if (RelocatedClusterCurrent != null)
      {
        RelocatedClusters.Remove(RelocatedClusterCurrent);
      }
    }

    public async void ButtonRelocationFullMess_Click(object sender, RoutedEventArgs e)
    {
      if (Galaxy == null || Galaxy.Sectors.Count == 0)
      {
        StatusBar.SetStatusMessage(T("status.error.galaxyNotLoaded"), StatusMessageType.Error);
        return;
      }
      if (!FullMessIsEnabled)
      {
        StatusBar.SetStatusMessage(T("status.warning.fullMessDisabled"), StatusMessageType.Warning);
        return;
      }
      MessageBoxResult confirm = MessageBox.Show(
        T("dialog.fullMess.confirm.body"),
        T("dialog.fullMess.confirm.title"),
        MessageBoxButton.YesNo,
        MessageBoxImage.Warning
      );
      if (confirm == MessageBoxResult.No)
      {
        return;
      }
      BusyMessage = T("busy.fullMess.creating");
      IsBusy = true;
      await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Render);

      try
      {
        await MakeFullMessAsync();
      }
      catch (Exception ex)
      {
        Log.Warn($"Error exporting PNG: {ex}");
        MessageBox.Show(TF("dialog.error.exportPng", ex.Message), T("dialog.error.title"), MessageBoxButton.OK, MessageBoxImage.Error);
      }
      finally
      {
        IsBusy = false;
      }
    }

    public void ButtonNewMod_Click(object sender, RoutedEventArgs e)
    {
      ClusterRelocationServiceMod = new("", X4UniverseId);
      _clusterRelocationServiceMod.SetGameVersion(X4DataVersion);
      RelocatedClusters.Clear();
      RelocatedClusterCurrent = null;
      IsModCanBeSaved = false;
    }

    public async void ButtonLoadMod_Click(object sender, RoutedEventArgs e)
    {
      if (_markedForRelocation != null)
      {
        _markedForRelocation.IsMarkedForRelocation = false;
        _markedForRelocation = null;
      }
      if (Galaxy == null || Galaxy.Sectors.Count == 0)
      {
        StatusBar.SetStatusMessage(T("status.error.galaxyNotLoaded"), StatusMessageType.Error);
        return;
      }
      if (!DirectMode && !LoadModsData)
      {
        StatusBar.SetStatusMessage(T("status.error.modLoadingDisabled"), StatusMessageType.Error);
        return;
      }
      if (ClusterRelocationServiceMod.IsModChanged(RelocatedClusters))
      {
        MessageBoxResult confirm = MessageBox.Show(
          T("dialog.loadMod.unsavedChanges.body"),
          T("dialog.loadMod.unsavedChanges.title"),
          MessageBoxButton.YesNo,
          MessageBoxImage.Warning
        );
        if (confirm == MessageBoxResult.No)
        {
          return;
        }
      }
      if (_isFullReloadNeeded)
      {
        MessageBoxResult confirm = MessageBox.Show(
          T("dialog.loadMod.reloadRequired.body"),
          T("dialog.loadMod.reloadRequired.title"),
          MessageBoxButton.YesNo,
          MessageBoxImage.Warning
        );
        if (confirm == MessageBoxResult.No)
        {
          return;
        }
        LoadX4DataInBackgroundStart();
        return; // Will load mod after data load completes
      }
      BusyMessage = T("busy.mod.loading");
      IsBusy = true;
      await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Render);

      try
      {
        await ModLoadAsync();
      }
      catch (Exception ex)
      {
        Log.Warn($"Error loading mod: {ex}");
        MessageBox.Show(TF("dialog.error.loadMod", ex.Message), T("dialog.error.title"), MessageBoxButton.OK, MessageBoxImage.Error);
      }
      finally
      {
        IsBusy = false;
      }
    }

    // Removed ModLoadTask/ModLoadProcedureAsync wrappers (synchronous invokes caused deadlocks)

    private async Task ModLoadAsync()
    {
      if (Galaxy == null)
      {
        StatusBar.SetStatusMessage(T("status.error.galaxyNotLoaded"), StatusMessageType.Error);
        return;
      }
      string path = string.Empty;
      if (DirectMode)
      {
        path = Path.Combine(X4GameFolder, "extensions", RelocatedClustersMod.ModFolder);
        if (!(Directory.Exists(path) && File.Exists(Path.Combine(path, "content.xml"))))
        {
          string message = T("status.error.modFolderMissing");
          StatusBar.SetStatusMessage(message, StatusMessageType.Error);
          Log.Error(message);
          if (ClusterRelocationServiceMod == null)
          {
            ClusterRelocationServiceMod = new(path, X4UniverseId);
            _clusterRelocationServiceMod.SetGameVersion(_x4DataVersion);
            await AssignRelocatedClusters();
            IsModCanBeSaved = false;
          }
          else
          {
            ClusterRelocationServiceMod.ModFolderPath = path;
          }
          UpdateIsModCanBeSavedAs();
          return;
        }
      }
      RelocatedClustersMod newMod = new(path, X4UniverseId);
      newMod.SetGameVersion(_x4DataVersion);
      await ResetRelocations();
      if (newMod.LoadData(Galaxy, _baseGameFiles, ClusterRelocationServiceMod))
      {
        ClusterRelocationServiceMod = newMod;
        StatusBar.SetStatusMessage(T("status.info.modDataLoaded"), StatusMessageType.Info);
        UpdateIsModCanBeSavedAs(); // Enable Save As for non-empty mods immediately after load
      }
      else
      {
        if (File.Exists(Path.Combine(path, "ext_01.cat")))
        {
          ClusterRelocationServiceMod = new(path, X4UniverseId);
          _clusterRelocationServiceMod.SetGameVersion(_x4DataVersion);
          StatusBar.SetStatusMessage(T("status.info.steamStubFound"), StatusMessageType.Info);
        }
        else
        {
          StatusBar.SetStatusMessage(T("status.error.modDataLoadFailed"), StatusMessageType.Error);
          Log.Warn("Mod data could not be loaded.");
        }
        UpdateIsModCanBeSavedAs();
      }
      await AssignRelocatedClusters();
    }

    private async Task ResetRelocations()
    {
      if (RelocatedClusters.Count == 0)
      {
        return;
      }
      var queue = new Queue<RelocatedCluster>(RelocatedClusters);
      int maxPasses = queue.Count; // Prevent infinite loop
      int passes = 0;

      while (queue.Count > 0 && passes < maxPasses)
      {
        int itemsThisPass = queue.Count;
        for (int i = 0; i < itemsThisPass; i++)
        {
          var rc = queue.Dequeue();
          if (GalaxyMapViewer.IsRelocatedClusterOriginalLocationOccupied(rc))
          {
            GalaxyMapClusterForClusterRelocation? current = GalaxyMapViewer.GetMapClusterForRelocated(rc);
            GalaxyMapClusterForClusterRelocation? interim = GalaxyMapViewer.GetRandomFreeCluster();
            if (current != null && interim != null)
            {
              rc = GalaxyMapViewer.RelocateCluster(current, interim, rc);
            }
            if (rc != null)
            {
              queue.Enqueue(rc); // retry later
            }
          }
          else
          {
            RelocatedClusters.Remove(rc);
            await Task.Delay(5);
            await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Render);
          }
        }
        passes++;
      }

      // Any still in queue after maxPasses could not be canceled
      if (queue.Count > 0)
      {
        // Log or notify about unresolved relocations
        string message = TF("status.error.relocationCancelFailed", queue.Count);
        Log.Warn(message);
        StatusBar.SetStatusMessage(message, StatusMessageType.Warning);
      }
      RelocatedClusters.Clear();
      RelocatedClusterCurrent = null;
      IsModCanBeSaved = ClusterRelocationServiceMod.IsModChanged(RelocatedClusters);
    }

    private async Task MakeFullMessAsync()
    {
      if (!Dispatcher.CheckAccess())
      {
        await Dispatcher.InvokeAsync(async () => await MakeFullMessAsync());
        return;
      }
      if (_markedForRelocation != null)
      {
        _markedForRelocation.IsMarkedForRelocation = false;
        _markedForRelocation = null;
      }
      await ResetRelocations();
      await GalaxyMapViewer.MakeFullMess(Galaxy, RelocatedClusters);
      IsModCanBeSaved = ClusterRelocationServiceMod.IsModChanged(RelocatedClusters);
    }

    public void ButtonSaveMod_Click(object sender, RoutedEventArgs e)
    {
      if (RelocatedClusters.Count > 0)
      {
        _clusterRelocationServiceMod.SetGameVersion(X4DataVersion);
        bool saved = _clusterRelocationServiceMod.SaveData(Galaxy, RelocatedClusters);
        StatusBar.SetStatusMessage(
          saved ? T("status.info.modDataSaved") : T("status.error.modDataSaveFailed"),
          saved ? StatusMessageType.Info : StatusMessageType.Error
        );
        IsModCanBeSaved = !saved || _clusterRelocationServiceMod.IsModChanged(RelocatedClusters);
      }
    }

    public void ButtonSaveModAs_Click(object sender, RoutedEventArgs e)
    {
      if (RelocatedClusters.Count > 0)
      {
        _clusterRelocationServiceMod.SetGameVersion(X4DataVersion);
        bool saved = _clusterRelocationServiceMod.SaveData(Galaxy, RelocatedClusters);
        StatusBar.SetStatusMessage(
          saved ? T("status.info.modDataSaved") : T("status.error.modDataSaveFailed"),
          saved ? StatusMessageType.Info : StatusMessageType.Error
        );
        IsModCanBeSaved = !saved || _clusterRelocationServiceMod.IsModChanged(RelocatedClusters);
      }
    }

    public void ButtonExportClusters_Click(object sender, RoutedEventArgs e)
    {
      var dialog = new Microsoft.Win32.OpenFolderDialog { Title = T("dialog.exportClusters.title") };
      dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
      bool? folderSelect = dialog.ShowDialog();
      if (!folderSelect == true || string.IsNullOrWhiteSpace(dialog.FolderName))
      {
        return;
      }
      string galaxyPath = Path.Combine(dialog.FolderName, "galaxy.xml");
      if (File.Exists(galaxyPath))
      {
        MessageBoxResult confirm = MessageBox.Show(
          T("dialog.exportClusters.overwrite.body"),
          T("dialog.exportClusters.overwrite.title"),
          MessageBoxButton.YesNo,
          MessageBoxImage.Warning
        );
        if (confirm == MessageBoxResult.No)
        {
          return;
        }
      }
      XElement diffElement = new("diff");
      XElement addElement = new("add", new XAttribute("sel", $"/macros/macro[@name='{Galaxy.Name}']/connections"));
      foreach (RelocatedCluster cluster in RelocatedClusters)
      {
        if (cluster.Cluster != null && cluster.Cluster.Position != null)
        {
          XElement connection = new XElement(
            "connection",
            new XAttribute("name", cluster.Cluster.PositionId),
            new XAttribute("ref", "clusters")
          );
          XElement offset = new("offset");
          XElement position = new(
            "position",
            new XAttribute("x", $"{cluster.Cluster.Position.X:F0}"),
            new XAttribute("y", $"{cluster.Cluster.Position.Y:F0}"),
            new XAttribute("z", $"{cluster.Cluster.Position.Z:F0}")
          );
          offset.Add(position);
          XElement macro = new("macro", new XAttribute("name", cluster.Cluster.Macro), new XAttribute("ref", "galaxy"));
          connection.Add(offset);
          connection.Add(macro);
          addElement.Add(connection);
        }
      }
      diffElement.Add(addElement);
      XDocument docGalaxy = new(new XDeclaration("1.0", "utf-8", null), diffElement);
      docGalaxy.Save(galaxyPath);
    }

    public void ButtonExtractX4Data_Click(object sender, RoutedEventArgs e)
    {
      X4DataExtractionWindow extractionWindow = new(_appIcon, X4UniverseId, X4GameFolder, X4DataFolder, LoadModsData) { Owner = this };
      if (extractionWindow.ShowDialog() == true)
      {
        if (!string.IsNullOrEmpty(extractionWindow.GameFolder) && Directory.Exists(extractionWindow.GameFolder))
        {
          X4GameFolder = extractionWindow.GameFolder;
        }
        string extractedDataFolder = extractionWindow.ExtractedDataFolder;
        if (!string.IsNullOrEmpty(extractedDataFolder))
        {
          X4DataFolder = extractedDataFolder;
          LoadX4DataInBackgroundStart();
        }
      }
    }

    public void ButtonReloadX4Data_Click(object sender, RoutedEventArgs e)
    {
      if (_hasBeenInitialized && Galaxy.Sectors.Count > 0)
      {
        var choice = MessageBox.Show(
          T("dialog.reloadX4.unsavedChanges.body"),
          T("dialog.reloadX4.title"),
          MessageBoxButton.YesNo,
          MessageBoxImage.Warning
        );
        if (choice != MessageBoxResult.Yes)
        {
          return; // User chose not to proceed
        }
      }
      LoadX4DataInBackgroundStart();
    }

    public void ButtonCheckUpdate_Click(object sender, RoutedEventArgs e)
    {
      UpdateChecker.onCheckUpdatePressedAsync(this, StatusBar, Assembly.GetExecutingAssembly());
    }

    public void ButtonAbout_Click(object sender, RoutedEventArgs e)
    {
      AboutWindow aboutWindow = new(_appIcon, Assembly.GetExecutingAssembly()) { Owner = this };
      aboutWindow.ShowDialog();
    }

    public void ButtonReadme_Click(object sender, RoutedEventArgs e)
    {
      string readmePath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "README.html");
      string imagesPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "docs", "images");
      if (File.Exists(readmePath) && Directory.Exists(imagesPath))
      {
        // Process.Start(new ProcessStartInfo(readmePath) { UseShellExecute = true });
        try
        {
          var htmlViewer = new SharedWindows.HtmlViewerWindow(readmePath, _appIcon) { Owner = this };

          // Subscribe to error events
          htmlViewer.ErrorOccurred += HtmlViewer_ErrorOccurred;

          htmlViewer.ShowDialog();
        }
        catch (System.Exception ex)
        {
          // Handle any exceptions that occur before the window is shown
          HandleHtmlViewerError(ex, "Failed to open HTML viewer");
        }
      }
      else
      {
        Log.Error("Documentation file not found.");
        StatusBar.SetStatusMessage(T("status.error.documentationMissing"), StatusMessageType.Error);
      }
    }

    private void HtmlViewer_ErrorOccurred(object? sender, SharedWindows.HtmlViewerErrorEventArgs e)
    {
      // Handle the error from the HTML viewer
      HandleHtmlViewerError(e.Exception, e.ErrorMessage);
    }

    private void HandleHtmlViewerError(Exception? exception, string errorMessage)
    {
      string fullMessage = exception != null ? $"{errorMessage}: {exception.Message}" : errorMessage;

      Log.Error($"HTML Viewer Error: {fullMessage}");
      StatusBar.SetStatusMessage($"HTML Viewer Error: {errorMessage}", StatusMessageType.Error);

      TryOpenWithDefaultBrowser();
    }

    private void TryOpenWithDefaultBrowser()
    {
      try
      {
        string readmePath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "README.html");
        string imagesPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "docs", "images");
        if (File.Exists(readmePath) && Directory.Exists(imagesPath))
        {
          Process.Start(new ProcessStartInfo(readmePath) { UseShellExecute = true });
          StatusBar.SetStatusMessage(T("status.info.documentationOpened"), StatusMessageType.Info);
        }
      }
      catch (System.Exception ex)
      {
        Log.Error($"Failed to open file with default browser: {ex.Message}");
        StatusBar.SetStatusMessage(T("status.error.documentationOpenFailed"), StatusMessageType.Error);
      }
    }

    private void GalaxyMapViewer_OnRelocationMessage(object? sender, RelocationMessageEventArgs e)
    {
      // if (e.MessageType == StatusMessageType.Error)
      // {
      //   StatusBar.SetStatusMessage(e.Message, e.MessageType);
      // }
      // else
      // {
      BusyMessage = e.Message;
      // }
    }

    private void GalaxyMapViewer_OnPressedMouseButton(object? sender, EventArgs e)
    {
      GalaxyMapCluster? cell = null;
      if (e is ClusterEventArgs || e is SectorEventArgs || e is SectorItemEventArgs)
      {
        if (e is SectorItemEventArgs sectorItemEventArgs && sectorItemEventArgs.PressedSector != null)
        {
          Log.Debug($"Selected sector item in sector: {sectorItemEventArgs.PressedSector.Macro}");
          cell = GalaxyMapViewer.GetClusterBySectorMacro(sectorItemEventArgs.PressedSector.Macro);
        }
        if (e is SectorEventArgs sectorEventArgs && sectorEventArgs.PressedSector != null)
        {
          Log.Debug($"Selected sector: {sectorEventArgs.PressedSector.Name}");
          cell = GalaxyMapViewer.GetClusterBySectorMacro(sectorEventArgs.PressedSector.Macro);
        }
        else if (e is ClusterEventArgs clusterEventArgs && clusterEventArgs.PressedCluster != null)
        {
          Log.Debug($"Selected cluster: {clusterEventArgs.PressedCluster.Name}");
          cell = GalaxyMapViewer.GetClusterByMacro(clusterEventArgs.PressedCluster.Macro);
        }
        if (cell != null && cell is GalaxyMapClusterForClusterRelocation mapCell && mapCell.Cluster != null)
        {
          var contextMenu = new ContextMenu();
          var headerItem = new MenuItem
          {
            Header = TF("context.clusterHeader", mapCell.Cluster.Name ?? string.Empty),
            FontWeight = FontWeights.Bold,
            IsEnabled = false,
          };
          if (mapCell.Cluster.Sectors.Count == 1 && mapCell.Cluster.Sectors[0].Name != mapCell.Cluster.Name)
          {
            headerItem.Header += "\n" + TF("context.sectorHeader", mapCell.Cluster.Sectors[0].Name);
          }

          contextMenu.Items.Add(headerItem);
          contextMenu.Items.Add(new Separator());
          var menuItem = new MenuItem
          {
            Header =
              mapCell.IsMarkedForRelocation && _markedForRelocationOverlaid == null
                ? T("context.unmarkRelocation")
                : T("context.markRelocation"),
          };
          menuItem.Click += (s, args) => ContextMenuMarkClusterForRelocation(mapCell);
          contextMenu.Items.Add(menuItem);
          if (mapCell.IsRelocated)
          {
            RelocatedCluster? currentClusterRelocated = RelocatedClusters.FirstOrDefault(rc =>
              StringHelper.EqualsIgnoreCase(rc.Cluster.Macro, mapCell.Cluster.Macro)
            );
            if (currentClusterRelocated != null)
            {
              contextMenu.Items.Add(new Separator());
              bool isOccupied = GalaxyMapViewer.IsRelocatedClusterOriginalLocationOccupied(currentClusterRelocated);
              var menuItemRelocation = new MenuItem { Header = T("context.cancelRelocation"), IsEnabled = !isOccupied };
              menuItemRelocation.Click += (s, args) => ContextMenuRelocationDelete(currentClusterRelocated);
              contextMenu.Items.Add(menuItemRelocation);
              if (isOccupied)
              {
                StatusBar.SetStatusMessage(T("status.relocation.originalLocationInUse"), StatusMessageType.Warning);
              }
            }
          }
          if (mapCell.IsCovers)
          {
            List<Cluster> overlaidClusters = GalaxyMapViewer.GetOverlaidClusters(mapCell);
            if (overlaidClusters.Count > 0)
            {
              contextMenu.Items.Add(new Separator());
              var coversHeader = new MenuItem
              {
                Header = TF("context.underlyingClusters", overlaidClusters.Count),
                FontWeight = FontWeights.Bold,
                IsEnabled = false,
              };
              contextMenu.Items.Add(coversHeader);
              contextMenu.Items.Add(new Separator());
              foreach (var overlaidCluster in overlaidClusters)
              {
                var headerOfOverlaidItem = new MenuItem
                {
                  Header = TF("context.clusterHeader", overlaidCluster.Name ?? string.Empty),
                  FontWeight = FontWeights.Bold,
                  IsEnabled = false,
                };
                if (overlaidCluster.Sectors.Count == 1 && overlaidCluster.Sectors[0].Name != overlaidCluster.Name)
                {
                  headerOfOverlaidItem.Header += "\n" + TF("context.sectorHeader", overlaidCluster.Sectors[0].Name);
                }

                contextMenu.Items.Add(headerOfOverlaidItem);
                contextMenu.Items.Add(new Separator());
                menuItem = new MenuItem
                {
                  Header =
                    mapCell.IsMarkedForRelocation && _markedForRelocationOverlaid == overlaidCluster
                      ? T("context.unmarkRelocation")
                      : T("context.markRelocation"),
                };
                menuItem.Click += (s, args) => ContextMenuMarkClusterForRelocation(mapCell, overlaidCluster);
                contextMenu.Items.Add(menuItem);
              }
            }
          }
          contextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.MousePoint;
          contextMenu.IsOpen = true;
        }
      }
      else
      {
        if (e is CellEventArgs cellEventArgs && cellEventArgs.PressedCell != null)
        {
          Log.Debug($"Selected cell: {cellEventArgs.PressedCell.OriginalX}, {cellEventArgs.PressedCell.OriginalZ}");
          cell = cellEventArgs.PressedCell;
          if (_markedForRelocation != null)
          {
            var contextMenu = new ContextMenu();
            contextMenu.Items.Add(
              new MenuItem
              {
                Header = TF("context.mapCell", cell.OriginalX, cell.OriginalZ),
                FontWeight = FontWeights.Bold,
                IsEnabled = false,
              }
            );
            contextMenu.Items.Add(new Separator());
            var menuItem = new MenuItem { Header = T("context.relocateHere") };
            menuItem.Click += (s, args) => ContextMenuRelocateMarkedCluster(cell as GalaxyMapClusterForClusterRelocation);
            contextMenu.Items.Add(menuItem);
            contextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.MousePoint;
            contextMenu.IsOpen = true;
          }
        }
      }
    }

    public void ContextMenuMarkClusterForRelocation(GalaxyMapClusterForClusterRelocation? cluster, Cluster? overlaid = null)
    {
      if (cluster == null || cluster.Cluster == null || Galaxy == null)
      {
        return;
      }
      if (_markedForRelocation == cluster && cluster.IsMarkedForRelocation && cluster.IsCovers)
      {
        if (_markedForRelocationOverlaid != null || overlaid != null)
        {
          if (overlaid != _markedForRelocationOverlaid)
          {
            BusyMessage =
              $"Cluster \"{RelocatedCluster.GetClusterName(_markedForRelocationOverlaid != null ? _markedForRelocationOverlaid : cluster.Cluster)}\" unmarked for relocation.";
            _markedForRelocationOverlaid = overlaid;
            BusyMessage =
              $"Cluster \"{RelocatedCluster.GetClusterName(_markedForRelocationOverlaid != null ? _markedForRelocationOverlaid : cluster.Cluster)}\" marked for relocation. Please define a target.";
            return;
          }
        }
      }
      if (cluster.IsMarkedForRelocation)
      {
        GalaxyMapViewer.ShowEmptyClusterPlaces.IsChecked = false;
        cluster.IsMarkedForRelocation = false;
        Cluster clusterForName = cluster.Cluster;
        if (_markedForRelocationOverlaid != null)
        {
          clusterForName = _markedForRelocationOverlaid;
        }
        _markedForRelocation = null;
        _markedForRelocationOverlaid = null;
        NoClusterInRelocationInProcess = true;
        if (cluster.Cluster != null)
        {
          BusyMessage = $"Cluster \"{RelocatedCluster.GetClusterName(clusterForName)}\" unmarked for relocation.";
        }
      }
      else
      {
        if (_markedForRelocation != null)
        {
          _markedForRelocation.IsMarkedForRelocation = false;
        }
        GalaxyMapViewer.ShowEmptyClusterPlaces.IsChecked = true;
        cluster.IsMarkedForRelocation = true;
        _markedForRelocation = cluster;
        _markedForRelocationOverlaid = overlaid;
        NoClusterInRelocationInProcess = false;
        if (cluster.Cluster != null)
        {
          BusyMessage =
            $"Cluster \"{RelocatedCluster.GetClusterName(_markedForRelocationOverlaid ?? cluster.Cluster)}\" marked for relocation. Please define a target.";
        }
      }
    }

    public void ContextMenuRelocationDelete(RelocatedCluster? cluster)
    {
      if (Galaxy == null || cluster == null || cluster.Cluster == null)
      {
        return;
      }
      if (_markedForRelocation != null)
      {
        _markedForRelocation.IsMarkedForRelocation = false;
        _markedForRelocation = null;
      }
      BusyMessage = $"Cancelling Relocation of \"{RelocatedCluster.GetClusterName(cluster.Cluster)}\"...";
      RelocatedClusters.Remove(cluster);
    }

    public void ContextMenuRelocateMarkedCluster(GalaxyMapClusterForClusterRelocation? targetCell)
    {
      if (targetCell == null || Galaxy == null || _markedForRelocation == null || _markedForRelocation.Cluster == null)
      {
        return;
      }
      BusyMessage = $"Relocating Cluster \"{RelocatedCluster.GetClusterName(_markedForRelocation.Cluster)}\" ...";
      _markedForRelocation.IsMarkedForRelocation = false;
      GalaxyMapViewer.ShowEmptyClusterPlaces.IsChecked = false;
      List<Cluster> overlaidClusters = GalaxyMapViewer.GetOverlaidClusters(_markedForRelocation);
      _isFullReloadNeeded = overlaidClusters.Count > 0;
      if (_markedForRelocationOverlaid == null)
      {
        RelocatedCluster? currentClusterRelocated = RelocatedClusters.FirstOrDefault(rc =>
          StringHelper.EqualsIgnoreCase(rc.Cluster.Macro, _markedForRelocation.Cluster.Macro)
        );
        RelocatedCluster? relocatedCluster = GalaxyMapViewer.RelocateCluster(_markedForRelocation, targetCell, currentClusterRelocated);
        if (currentClusterRelocated != null)
        {
          IsModCanBeSaved = _clusterRelocationServiceMod.IsModChanged(RelocatedClusters);
        }
        if (relocatedCluster != null)
        {
          RelocatedClusterCurrent = null;
          if (!RelocatedClusters.Contains(relocatedCluster))
          {
            RelocatedClusters.Add(relocatedCluster);
            RelocatedClusterCurrent = RelocatedClusters.Last();
          }
          else
          {
            RelocatedClusterCurrent = relocatedCluster;
          }
        }
        if (_markedForRelocation.IsCovers)
        {
          if (overlaidClusters.Count > 0)
          {
            GalaxyMapViewer.GalaxyMapClusterReassign(_markedForRelocation, overlaidClusters[0]);
          }
        }
      }
      else
      {
        Position position = targetCell.Position;
        if (position != null)
        {
          RelocatedCluster clusterRelocated = new(_markedForRelocationOverlaid);
          _markedForRelocationOverlaid.SetPosition(position);
          RelocatedClusters.Add(clusterRelocated);
          RelocatedClusterCurrent = RelocatedClusters.Last();
          GalaxyMapViewer.GalaxyMapClusterReassign(targetCell, _markedForRelocationOverlaid);
          targetCell.IsRelocated = true;
          _markedForRelocationOverlaid = null;
        }
      }
      GalaxyMapViewer.IsCovers(_markedForRelocation);
      _markedForRelocation = null;
      NoClusterInRelocationInProcess = true;
      UpdateOverlaidClustersList();
    }

    private async void ExportPngButton_Click(object sender, RoutedEventArgs e)
    {
      var dialog = new Microsoft.Win32.SaveFileDialog
      {
        Filter = T("dialog.exportPng.filter"),
        Title = T("dialog.exportPng.title"),
        FileName = T("dialog.exportPng.defaultFileName"),
      };
      if (dialog.ShowDialog() != true)
        return;

      BusyMessage = T("busy.map.exporting");
      IsBusy = true;

      await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Render);

      try
      {
        await Task.Run(() => GalaxyMapViewer.ExportToPng(GalaxyMapCanvas, dialog.FileName));

        MessageBox.Show(T("dialog.success.exportPng"), T("dialog.success.title"), MessageBoxButton.OK, MessageBoxImage.Information);
      }
      catch (Exception ex)
      {
        Log.Warn($"Error exporting PNG: {ex}");
        MessageBox.Show(TF("dialog.error.exportPng", ex.Message), T("dialog.error.title"), MessageBoxButton.OK, MessageBoxImage.Error);
      }
      finally
      {
        IsBusy = false;
      }
    }

    private void ButtonOptionsVisibility_Click(object sender, RoutedEventArgs e)
    {
      OptionsVisibilityState = OptionsVisibilityState == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;
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

  class WindowHelper
  {
    public static double GetWindowLeft(Window window)
    {
      if (window.WindowState == WindowState.Maximized)
      {
        var leftField = typeof(Window).GetField(
          "_actualLeft",
          System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
        );
        if (leftField?.GetValue(window) is double value)
        {
          return value;
        }
        throw new InvalidOperationException("_actualLeft field is null or not a double.");
      }
      else
        return window.Left;
    }

    public static double GetWindowTop(Window window)
    {
      if (window.WindowState == WindowState.Maximized)
      {
        var topField = typeof(Window).GetField(
          "_actualTop",
          System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
        );
        if (topField?.GetValue(window) is double value)
        {
          return value;
        }
        throw new InvalidOperationException("_actualTop field is null or not a double.");
      }
      else
        return window.Top;
    }
  }
}
