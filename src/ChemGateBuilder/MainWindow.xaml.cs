using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
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

namespace ChemGateBuilder
{
  public class AppConfig
  {
    public ModeConfig Mode { get; set; } = new ModeConfig();
    public EditConfig Edit { get; set; } = new EditConfig();
    public MapConfig Map { get; set; } = new MapConfig();
    public DataConfig Data { get; set; } = new DataConfig();
    public LoggingConfig Logging { get; set; } = new LoggingConfig();
  }

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

  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window, INotifyPropertyChanged
  {
    private readonly string _configFileName;

    private bool _directMode = true;
    public bool DirectMode
    {
      get => _directMode;
      set
      {
        if (_directMode != value)
        {
          _directMode = value;
          OnPropertyChanged(nameof(DirectMode));
          OnPropertyChanged(nameof(NoDirectMode));
          OnPropertyChanged(nameof(ExtractedVisibility));
          OnPropertyChanged(nameof(GameFolderVisibility));
          SaveConfiguration();
          LoadX4DataInBackgroundStart();
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
          _chemGateKeeperMod.SetGameVersion(_x4DataVersion);
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
          SaveConfiguration();
        }
      }
    }

    public ObservableCollection<string> X4DataVersions { get; set; } = ["7.10", "7.50"];

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
    private bool _gatesActiveByDefault = true;
    public bool GatesActiveByDefault
    {
      get => _gatesActiveByDefault;
      set
      {
        if (_gatesActiveByDefault != value)
        {
          _gatesActiveByDefault = value;
          OnPropertyChanged(nameof(GatesActiveByDefault));
          GatesConnectionCurrent?.SetGateStatusDefaults(value);
          SaveConfiguration();
        }
      }
    }

    private int _gatesMinimalDistanceBetween = 10;
    public int GatesMinimalDistanceBetween
    {
      get => _gatesMinimalDistanceBetween;
      set
      {
        if (_gatesMinimalDistanceBetween != value)
        {
          _gatesMinimalDistanceBetween = value;
          OnPropertyChanged(nameof(GatesMinimalDistanceBetween));
          SaveConfiguration();
          GatesConnectionCurrent?.UpdateDataFlags();
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

    private readonly string _gateMacroDefault = "props_gates_anc_gate_macro";
    private static readonly string GalaxyConnectionPrefix = "Chem_Gate";

    private GalaxyConnectionData? _currentGalaxyConnection;
    public GalaxyConnectionData? CurrentGalaxyConnection
    {
      get => _currentGalaxyConnection;
      set
      {
        if (_currentGalaxyConnection != value)
        {
          _currentGalaxyConnection = value;
          OnPropertyChanged(nameof(CurrentGalaxyConnection));
          IsGateCanBeCreated = IsNowGateCanBeCreated;
          IsGateCanBeDeleted = IsNowGateCanBeDeleted;
          ButtonSaveContent = IsNowGateCanBeCreated ? "Update" : "Add";
          if (value != null && GatesConnectionCurrent != null)
          {
            GatesConnectionCurrent.SetDefaultsFromReference(value, AllSectors);
            GateConnectionReset();
          }
        }
      }
    }

    private ChemGateKeeper _chemGateKeeperMod = new();
    public ChemGateKeeper ChemGateKeeperMod
    {
      get => _chemGateKeeperMod;
      set
      {
        if (_chemGateKeeperMod != value)
        {
          _chemGateKeeperMod = value;
          OnPropertyChanged(nameof(ChemGateKeeperMod));
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

    private void UpdateIsModCanBeSavedAs()
    {
      bool hasContent = ChemGateKeeperMod != null && ChemGateKeeperMod.GalaxyConnections.Count > 0;
      IsModCanBeSavedAs = IsModCanBeSaved || (hasContent && IsGateCanBeCreated);
    }

    // Master sector list
    public ObservableCollection<SectorsListItem> AllSectors { get; } = [];

    // CollectionViewSources for filtering
    public CollectionViewSource SectorsDirectViewSource { get; } = new CollectionViewSource();
    public CollectionViewSource SectorsOppositeViewSource { get; } = new CollectionViewSource();

    public ObservableCollection<string> GateMacros { get; } = [];

    // GatesConnectionCurrent Property
    private GatesConnectionData? _gatesConnectionCurrent;
    public GatesConnectionData? GatesConnectionCurrent
    {
      get => _gatesConnectionCurrent;
      set
      {
        if (_gatesConnectionCurrent != value)
        {
          _gatesConnectionCurrent = value;
          OnPropertyChanged(nameof(GatesConnectionCurrent));
        }
      }
    }

    // Other properties

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
            return "Please set it!";
          }
        }
        else
        {
          return "Please set it!";
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
      get => AllSectors.Count > 0;
    }

    private bool _isGateCanBeDeleted = false;
    public bool IsGateCanBeDeleted
    {
      get => _isGateCanBeDeleted;
      set
      {
        if (_isGateCanBeDeleted != value)
        {
          _isGateCanBeDeleted = value;
          OnPropertyChanged(nameof(IsGateCanBeDeleted));
        }
      }
    }
    private bool _isGateCanBeCreated = false;
    public bool IsGateCanBeCreated
    {
      get => _isGateCanBeCreated;
      set
      {
        if (_isGateCanBeCreated != value)
        {
          _isGateCanBeCreated = value;
          OnPropertyChanged(nameof(IsGateCanBeCreated));
          UpdateIsModCanBeSavedAs();
        }
      }
    }
    public bool IsNowGateCanBeDeleted => IsDataLoaded & ChemGateKeeperMod.GalaxyConnections.Count > 0 && CurrentGalaxyConnection != null;
    public bool IsNowGateCanBeCreated => IsDataLoaded & ChemGateKeeperMod.GalaxyConnections.Count > 0 && CurrentGalaxyConnection != null;

    bool _changingGalaxyConnectionIsPossible = false;
    public bool ChangingGalaxyConnectionIsPossible
    {
      get => _changingGalaxyConnectionIsPossible;
      set
      {
        if (_changingGalaxyConnectionIsPossible != value)
        {
          _changingGalaxyConnectionIsPossible = value;
          IsGateCanBeCreated = IsNowGateCanBeCreated && value;
          IsGateCanBeDeleted = IsNowGateCanBeDeleted && value;
          OnPropertyChanged(nameof(ChangingGalaxyConnectionIsPossible));
        }
        bool isModChanged = ChemGateKeeperMod.IsModChanged();
        if (value && IsGateCanBeDeleted)
        {
          IsModCanBeSaved = isModChanged;
        }
        else
        {
          IsModCanBeSaved = false;
        }
      }
    }

    public bool IsModCanBeCreated => IsDataLoaded & ChemGateKeeperMod.GalaxyConnections.Count > 0;

    private string _buttonSaveContent = "Add";
    public string ButtonSaveContent
    {
      get => _buttonSaveContent;
      set
      {
        if (_buttonSaveContent != value)
        {
          _buttonSaveContent = value;
          OnPropertyChanged(nameof(ButtonSaveContent));
        }
      }
    }

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
        }
      }
    }

    // Constructor
    public MainWindow()
    {
      _configFileName = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.json";
      LoadConfiguration();
      InitializeComponent();
      DataContext = this;
      GatesConnectionCurrent = new GatesConnectionData(GatesActiveByDefault, _gateMacroDefault);
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
      _chemGateKeeperMod.SetGameVersion(X4DataVersion);
      GatesConnectionCurrent.SetMapsCanvasAndHexagons(SectorDirectCanvas, SectorDirectHexagon, SectorOppositeCanvas, SectorOppositeHexagon);

      // Initialize CollectionViewSource filters
      SectorsDirectViewSource.Filter += SectorsDirect_Filter;
      SectorsOppositeViewSource.Filter += SectorsOpposite_Filter;

      SectorsDirectViewSource.Source = AllSectors;
      SectorsOppositeViewSource.Source = AllSectors;

      // Subscribe to Validation Errors
      TextBoxExtensions.OnValidationError += HandleValidationError;

      _backgroundWorker = new BackgroundWorker { WorkerReportsProgress = true, WorkerSupportsCancellation = false };

      GateMacros.Add(_gateMacroDefault);

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
          DirectMode = config.Mode.DirectMode;
          X4DataFolder = config.Data.X4DataExtractedPath;
          X4DataVersionOverride = config.Data.X4DataVersionOverride;
          if (X4DataVersionOverride)
          {
            X4DataVersion = config.Data.X4DataVersion;
          }
          if (!String.IsNullOrEmpty(config.Data.X4GameFolder))
          {
            X4GameFolder = config.Data.X4GameFolder;
          }
          LoadModsData = config.Data.LoadModsData;
          GatesActiveByDefault = config.Edit.GatesActiveByDefault;
          GatesMinimalDistanceBetween = config.Edit.GatesMinimalDistanceBetween;
          if (config.Map.NonStandardUniverse && !String.IsNullOrEmpty(config.Map.NonStandardUniverseId))
          {
            X4UniverseId = config.Map.NonStandardUniverseId;
          }
          MapColorsOpacity = config.Map.MapColorsOpacity;
          LogLevel = config.Logging.LogLevel;
          LogToFile = config.Logging.LogToFile;
        }
      }
      else
      {
        // Set default values if the config file does not exist
        X4DataFolder = ".";
        GatesActiveByDefault = true;
        GatesMinimalDistanceBetween = 10;
        LogLevel = "Warning";
        LogToFile = false;
      }
    }

    private readonly JsonSerializerOptions _jsonSerializerOptions = new() { WriteIndented = true };

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
        Edit = new EditConfig { GatesActiveByDefault = GatesActiveByDefault, GatesMinimalDistanceBetween = GatesMinimalDistanceBetween },
        Map = new MapConfig { MapColorsOpacity = MapColorsOpacity },
        Logging = new LoggingConfig { LogLevel = LogLevel, LogToFile = LogToFile },
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
      var jsonString = JsonSerializer.Serialize(config, _jsonSerializerOptions);
      File.WriteAllText(_configFileName, jsonString);
    }

    private void X4DataNotLoadedCheckAndWarning()
    {
      // Validate the loaded X4DataFolder
      if (DirectMode && !ValidateX4GameFolder(X4GameFolder, out string errorMessage))
      {
        StatusBar.SetStatusMessage(errorMessage, StatusMessageType.Error);
        // Prompt the user to select a valid folder
        _ = MessageBox.Show(
          "The X4 Game folder is not set. Please set it via Configuration -> X4 Game Folder!",
          "Invalid or missing X4 Game Folder",
          MessageBoxButton.OK,
          MessageBoxImage.Warning
        );
        StatusBar.SetStatusMessage("Please select a valid X4 Game folder to proceed.", StatusMessageType.Warning);
        // Show the ribbon tab options
        RibbonMain.SelectedTabItem = (Fluent.RibbonTabItem)RibbonMain.FindName("RibbonTabConfiguration")!;
      }
      else if (!DirectMode && !ValidateX4DataFolder(X4DataFolder, out string errorMessageData))
      {
        StatusBar.SetStatusMessage(errorMessageData, StatusMessageType.Error);
        // Prompt the user to select a valid folder
        _ = MessageBox.Show(
          "The X4 Data folder is not set. Please set it via Configuration -> X4 Data Folder!",
          "Invalid or missing X4 Data Folder",
          MessageBoxButton.OK,
          MessageBoxImage.Warning
        );
        StatusBar.SetStatusMessage("Please select a valid X4 Data folder to proceed.", StatusMessageType.Warning);
        // Show the ribbon tab options
        RibbonMain.SelectedTabItem = (Fluent.RibbonTabItem)RibbonMain.FindName("RibbonTabConfiguration")!;
      }
      else
      {
        LoadX4DataInBackgroundStart();
      }
    }

    private void LoadX4DataInBackgroundStart()
    {
      IsBusy = true;
      BusyMessage = "Preparing to load X4 data...";
      AllSectors.Clear();
      Galaxy.Clear();
      _backgroundWorker.DoWork += LoadX4DataInBackground;
      _backgroundWorker.ProgressChanged += LoadX4DataInBackgroundProgressChanged;
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
        string message = $"Processing file: {progressText} ...";
        StatusBar.SetStatusMessage(message, StatusMessageType.Info, true);
        BusyMessage = message;
      }
    }

    private void LoadX4DataInBackgroundCompleted(object? sender, RunWorkerCompletedEventArgs e)
    {
      if (e.Error != null)
      {
        StatusBar.SetStatusMessage("Error loading X4 data: " + e.Error.Message, StatusMessageType.Error);
        BusyMessage = "Error loading X4 data.";
        IsBusy = false;
      }
      _backgroundWorker.Dispose();
      var sectors = Galaxy.GetSectors();

      foreach (var sector in sectors.Values.OrderBy(s => s.Name))
      {
        var sectorListItem = new SectorsListItem
        {
          Name = sector.Name,
          Source = sector.Source,
          Macro = sector.Macro,
          Selectable = true,
        };
        AllSectors.Add(sectorListItem);
      }
      SectorsDirectViewSource.View.Refresh();
      SectorsOppositeViewSource.View.Refresh();
      if (!X4DataVersionOverride && Galaxy.Version != 0 && Galaxy.Version != X4DataVersion)
      {
        X4DataVersion = Galaxy.Version;
      }
      OnPropertyChanged(nameof(IsDataLoaded));
      GateConnectionReset();
      StatusBar.SetStatusMessage("X4 data loaded successfully!", StatusMessageType.Info);
      BusyMessage = "X4 data loaded successfully!";
      IsBusy = false;
      RibbonMain.SelectedTabItem = (Fluent.RibbonTabItem)RibbonMain.FindName("RibbonTabMod")!;
      if (DirectMode)
      {
        string modPath = Path.Combine(X4GameFolder, "extensions", ChemGateKeeper.ModId);
        if (Directory.Exists(modPath) && File.Exists(Path.Combine(modPath, "content.xml")))
        {
          ModLoad(modPath);
        }
        else
        {
          ChemGateKeeper newMod = new(modPath, X4UniverseId);
          ChemGateKeeperMod = newMod;
          IsModCanBeSaved = false;
        }
      }
    }

    private static bool ValidateX4DataFolder(string folderPath, out string errorMessage)
    {
      return DataLoader.ValidateDataFolder(folderPath, out errorMessage);
    }

    private static bool ValidateX4GameFolder(string folderPath, out string errorMessage)
    {
      if (string.IsNullOrEmpty(folderPath))
      {
        errorMessage = "The X4 Program folder is not set.";
        return false;
      }
      if (!Directory.Exists(folderPath))
      {
        errorMessage = "The X4 Program folder does not exist.";
        return false;
      }
      if (!File.Exists(Path.Combine(folderPath, X4DataExtractionWindow.X4Executable)))
      {
        errorMessage = "The X4 Program folder does not contain the X4 executable file.";
        return false;
      }
      errorMessage = "";
      return true;
    }

    private void SelectX4DataFolder_Click(object? sender, RoutedEventArgs? e)
    {
      if (AllSectors.Count > 0)
      {
        MessageBoxResult confirm = MessageBox.Show(
          "Are you really want to reload the X4 Data?\n\nAny unsaved changes will be lost!",
          "Reload X4 Data",
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
          "Do you have an extracted X4 data folder?\n\nIf not, you can cancel and extract the data first.",
          "Select X4 Data Folder",
          MessageBoxButton.YesNo,
          MessageBoxImage.Warning
        );
        if (confirm == MessageBoxResult.No)
        {
          RibbonMain.SelectedTabItem = (Fluent.RibbonTabItem)RibbonMain.FindName("RibbonTabX4Data")!;
          return;
        }
      }
      var dialog = new Microsoft.Win32.OpenFolderDialog
      {
        Title = "Please select the folder where the X4 extracted data files are located.",
      };
      // Microsoft.Win32.OpenFolderDialog does not support RootFolder/SelectedPath in the same way.
      bool? result = dialog.ShowDialog();

      if (result == true && !string.IsNullOrWhiteSpace(dialog.FolderName))
      {
        string selectedPath = dialog.FolderName;
        if (ValidateX4DataFolder(selectedPath, out string errorMessage))
        {
          X4DataFolder = selectedPath;
          StatusBar.SetStatusMessage("X4 Data folder set successfully.", StatusMessageType.Info);
          LoadX4DataInBackgroundStart();
        }
        else
        {
          StatusBar.SetStatusMessage(errorMessage, StatusMessageType.Error);
          MessageBox.Show(errorMessage, "Invalid Folder", MessageBoxButton.OK, MessageBoxImage.Error);
          // Optionally, prompt again
        }
      }
      else
      {
        StatusBar.SetStatusMessage("Folder selection was canceled or invalid.", StatusMessageType.Warning);
      }
    }

    private void SelectX4GameFolder_Click(object? sender, RoutedEventArgs? e)
    {
      var dialog = new Microsoft.Win32.OpenFileDialog
      {
        InitialDirectory = string.IsNullOrEmpty(X4GameFolder)
          ? Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)
          : X4GameFolder,
        Filter = $"X4 Foundations|{X4DataExtractionWindow.X4Executable}",
        Title = "Select the X4: Foundations executable",
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
          StatusBar.SetStatusMessage("X4 Game folder set successfully.", StatusMessageType.Info);
          LoadX4DataInBackgroundStart();
        }
        else
        {
          StatusBar.SetStatusMessage(errorMessage, StatusMessageType.Error);
          MessageBox.Show(errorMessage, "Invalid Folder", MessageBoxButton.OK, MessageBoxImage.Error);
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
        dataLoader.LoadData(Galaxy, X4GameFolder, X4DataStructure, X4PDataProcessingOrder, LoadModsData, true, [ChemGateKeeper.ModId]);
      }
      else
      {
        dataLoader.LoadData(Galaxy, X4DataFolder, X4DataStructure, X4PDataProcessingOrder, LoadModsData);
      }
    }

    // Filter methods
    private void SectorsDirect_Filter(object sender, FilterEventArgs e)
    {
      if (e.Item is SectorsListItem sector && sector != null && sector?.Macro != null)
      {
        sector.Selectable = true;
        // Exclude the sector selected in Opposite ComboBox
        if (GatesConnectionCurrent?.SectorOpposite != null)
        {
          if (sector.Macro == GatesConnectionCurrent.SectorOpposite.Macro)
          {
            sector.Selectable = false;
          }
          else if (
            GatesConnectionCurrent.SectorOppositeExistingObjectsMacros != null
              && GatesConnectionCurrent.SectorOppositeExistingObjectsMacros.Contains(sector.Macro)
            || (
              GatesConnectionCurrent.SectorOpposite.Macro != null
              && GetOppositeSectorsMacrosFromMod(sector.Macro).Contains(GatesConnectionCurrent.SectorOpposite.Macro)
            )
          )
          {
            sector.Selectable = false;
          }
          // e.Accepted = false;
        }
        e.Accepted = true;
      }
      else
      {
        e.Accepted = false;
      }
    }

    private void SectorsOpposite_Filter(object sender, FilterEventArgs e)
    {
      if (e.Item is SectorsListItem sector && sector != null && sector?.Macro != null)
      {
        sector.Selectable = true;
        // Exclude the sector selected in Direct ComboBox
        if (GatesConnectionCurrent?.SectorDirect != null)
        {
          if (sector.Macro == GatesConnectionCurrent.SectorDirect.Macro)
          {
            sector.Selectable = false;
          }
          else if (
            GatesConnectionCurrent.SectorDirectExistingObjectsMacros != null
              && GatesConnectionCurrent.SectorDirectExistingObjectsMacros.Contains(sector.Macro)
            || (
              GatesConnectionCurrent.SectorDirect.Macro != null
              && GetOppositeSectorsMacrosFromMod(sector.Macro).Contains(GatesConnectionCurrent.SectorDirect.Macro)
            )
          )
          {
            sector.Selectable = false;
          }
        }
        e.Accepted = true;
      }
      else
      {
        e.Accepted = false;
      }
    }

    private List<string> GetOppositeSectorsMacrosFromMod(string sectorMacro)
    {
      List<string> oppositeSectorsMacros = [];
      foreach (var connection in ChemGateKeeperMod.GalaxyConnections)
      {
        if (connection.Connection?.PathDirect?.Sector?.Macro == null || connection.Connection?.PathOpposite?.Sector?.Macro == null)
        {
          continue;
        }
        if (_currentGalaxyConnection == connection)
        {
          continue;
        }
        if (connection.Connection.PathDirect.Sector.Macro == sectorMacro)
        {
          oppositeSectorsMacros.Add(connection.Connection.PathOpposite.Sector.Macro);
        }
        else if (connection.Connection.PathOpposite.Sector.Macro == sectorMacro)
        {
          oppositeSectorsMacros.Add(connection.Connection.PathDirect.Sector.Macro);
        }
      }
      return oppositeSectorsMacros;
    }

    private Dictionary<string, List<ObjectInSector>> GetObjectsFromModAndNew(bool onlyMod = false)
    {
      Dictionary<string, List<ObjectInSector>> sectorObjects = [];
      foreach (var connection in ChemGateKeeperMod.GalaxyConnections)
      {
        if (connection.Connection?.PathDirect?.Sector?.Macro == null || connection.Connection?.PathOpposite?.Sector?.Macro == null)
        {
          continue;
        }
        foreach (
          var modSectorMacro in new string[]
          {
            connection.Connection.PathDirect.Sector.Macro,
            connection.Connection.PathOpposite.Sector.Macro,
          }
        )
        {
          bool isDirect = modSectorMacro == connection.Connection.PathDirect.Sector.Macro;
          ObjectInSector newObject = new()
          {
            Active = isDirect ? connection.GateDirectActive : connection.GateOppositeActive,
            Info = isDirect ? connection.SectorOppositeName : connection.SectorDirectName,
            X = isDirect ? connection.GateDirectX : connection.GateOppositeX,
            Y = isDirect ? connection.GateDirectY : connection.GateOppositeY,
            Z = isDirect ? connection.GateDirectZ : connection.GateOppositeZ,
            Angle = isDirect ? connection.GateDirectAngle : connection.GateOppositeAngle,
            Id = connection.Connection.Name,
            From = "mod",
            Type = "gate",
          };
          if (sectorObjects.TryGetValue(modSectorMacro, out List<ObjectInSector>? value))
          {
            value.Add(newObject);
          }
          else
          {
            sectorObjects[modSectorMacro] = [newObject];
          }
        }
      }
      if (GatesConnectionCurrent != null && !onlyMod)
      {
        foreach (string gateType in new string[] { "GateDirect", "GateOpposite" })
        {
          bool isDirect = gateType == "GateDirect";
          SectorsListItem? sector = isDirect ? GatesConnectionCurrent.SectorDirect : GatesConnectionCurrent.SectorOpposite;
          if (sector == null)
          {
            continue;
          }
          ObjectInSector? newObject = GatesConnectionCurrent.UpdateCurrentGateOnMap(gateType, null, true);
          if (newObject != null && sector.Macro != null)
          {
            if (sectorObjects.TryGetValue(sector.Macro, out List<ObjectInSector>? value))
            {
              value.Add(newObject);
            }
            else
            {
              sectorObjects[sector.Macro] = [newObject];
            }
          }
        }
      }
      return sectorObjects;
    }

    public List<ObjectInSector> GetObjectsInSectorFromMod(string sectorMacro)
    {
      return GetObjectsFromModAndNew(true).GetValueOrDefault(sectorMacro) ?? [];
    }

    private List<string> GetConnectionNamesFromMod()
    {
      return ChemGateKeeperMod.GalaxyConnections.Select(c => c.Connection.Name).ToList();
    }

    private void ButtonSectorDirectSelectFromMap_Click(object sender, RoutedEventArgs e)
    {
      GalaxyMapWindow clusterMapWindow = new(this, SectorsDirectViewSource, GetObjectsFromModAndNew(), GetConnectionNamesFromMod());
      clusterMapWindow.ShowDialog();
      if (clusterMapWindow.SelectedSector != null && GatesConnectionCurrent != null)
      {
        GatesConnectionCurrent.SectorDirect = AllSectors.FirstOrDefault(s => s.Macro == clusterMapWindow.SelectedSector.Macro);
      }
    }

    private void ButtonSectorOppositeSelectFromMap_Click(object sender, RoutedEventArgs e)
    {
      GalaxyMapWindow clusterMapWindow = new(this, SectorsOppositeViewSource, GetObjectsFromModAndNew(), GetConnectionNamesFromMod());
      clusterMapWindow.ShowDialog();
      if (clusterMapWindow.SelectedSector != null && GatesConnectionCurrent != null)
      {
        GatesConnectionCurrent.SectorOpposite = AllSectors.FirstOrDefault(s => s.Macro == clusterMapWindow.SelectedSector.Macro);
      }
    }

    // Handle Canvas Size Changed to adjust Hexagon Size
    private void SectorDirectCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
    {
      GatesConnectionCurrent?.SectorDirectMap.OnSizeChanged(e.NewSize.Width, e.NewSize.Height);
    }

    private void SectorOppositeCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
    {
      GatesConnectionCurrent?.SectorOppositeMap.OnSizeChanged(e.NewSize.Width, e.NewSize.Height);
    }

    private void SectorDirectMapItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      if (GatesConnectionCurrent?.SectorDirectMap != null)
      {
        GatesConnectionCurrent.SectorDirectMap.MouseLeftButtonDown(sender, e);
        this.Cursor = Cursors.Hand;
      }
    }

    private void SectorOppositeMapItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      if (GatesConnectionCurrent?.SectorOppositeMap != null)
      {
        GatesConnectionCurrent.SectorOppositeMap.MouseLeftButtonDown(sender, e);
        this.Cursor = Cursors.Hand;
      }
    }

    private void SectorDirectMapItem_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
      if (GatesConnectionCurrent?.SectorDirectMap != null)
      {
        GatesConnectionCurrent.SectorDirectMap.MouseRightButtonDown(sender, e);
        this.Cursor = Cursors.Hand;
      }
    }

    private void SectorOppositeMapItem_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
      if (GatesConnectionCurrent?.SectorOppositeMap != null)
      {
        GatesConnectionCurrent.SectorOppositeMap.MouseRightButtonDown(sender, e);
        this.Cursor = Cursors.Hand;
      }
    }

    private void SectorDirectMapItem_MouseMove(object sender, MouseEventArgs e)
    {
      if (GatesConnectionCurrent?.SectorDirectMap != null)
      {
        GatesConnectionCurrent.SectorDirectMap.MouseMove(
          sender,
          e,
          GatesConnectionCurrent.GateDirect.Coordinates,
          GatesConnectionCurrent.GateDirect.Rotation
        );
      }
    }

    private void SectorOppositeMapItem_MouseMove(object sender, MouseEventArgs e)
    {
      if (GatesConnectionCurrent?.SectorOppositeMap != null)
      {
        GatesConnectionCurrent.SectorOppositeMap.MouseMove(
          sender,
          e,
          GatesConnectionCurrent.GateOpposite.Coordinates,
          GatesConnectionCurrent.GateOpposite.Rotation
        );
      }
    }

    private void SectorDirectMapItem_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
      this.Cursor = Cursors.Arrow;
      if (GatesConnectionCurrent?.SectorDirectMap != null)
      {
        ObjectInSector? objectData = GatesConnectionCurrent.SectorDirectMap.MouseLeftButtonUp(sender, e);
        if (objectData != null)
        {
          if (GatesConnectionCurrent.SectorDirectSelectedObject == objectData)
          {
            GatesConnectionCurrent.SectorDirectSelectedObject = null;
          }
          else
          {
            GatesConnectionCurrent.SectorDirectSelectedObject = objectData;
          }
        }
      }
    }

    private void SectorOppositeMapItem_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
      this.Cursor = Cursors.Arrow;
      if (GatesConnectionCurrent?.SectorOppositeMap != null)
      {
        ObjectInSector? objectData = GatesConnectionCurrent.SectorOppositeMap.MouseLeftButtonUp(sender, e);
        if (objectData != null)
        {
          if (GatesConnectionCurrent.SectorOppositeSelectedObject == objectData)
          {
            GatesConnectionCurrent.SectorOppositeSelectedObject = null;
          }
          else
          {
            GatesConnectionCurrent.SectorOppositeSelectedObject = objectData;
          }
        }
      }
    }

    private void SectorDirectMapItem_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
    {
      this.Cursor = Cursors.Arrow;
      if (GatesConnectionCurrent?.SectorDirectMap != null)
      {
        GatesConnectionCurrent.SectorDirectMap.MouseRightButtonUp(sender, e);
      }
    }

    private void SectorOppositeMapItem_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
    {
      this.Cursor = Cursors.Arrow;
      if (GatesConnectionCurrent?.SectorOppositeMap != null)
      {
        GatesConnectionCurrent.SectorOppositeMap.MouseRightButtonUp(sender, e);
      }
    }

    public void ButtonSectorDirectMapExpand_Click(object sender, RoutedEventArgs e)
    {
      ButtonSectorMapExpand_Click(sender, e, true);
    }

    public void ButtonSectorOppositeMapExpand_Click(object sender, RoutedEventArgs e)
    {
      ButtonSectorMapExpand_Click(sender, e, false);
    }

    public void ButtonSectorMapExpand_Click(object _, RoutedEventArgs e, bool isDirect)
    {
      if (GatesConnectionCurrent != null)
      {
        Log.Debug($"[ButtonSectorDirectMapExpand_Click] Direct: ");
        string sectorName = isDirect ? GatesConnectionCurrent.SectorDirect?.Name ?? "" : GatesConnectionCurrent.SectorOpposite?.Name ?? "";
        SectorMapExpandedWindow sectorMapExpandedWindow = new(
          this,
          sectorName,
          isDirect ? GatesConnectionCurrent.SectorDirectMap : GatesConnectionCurrent.SectorOppositeMap,
          MapColorsOpacity
        );
        sectorMapExpandedWindow.ShowDialog(); // Modal
        if (isDirect)
        {
          GatesConnectionCurrent.SectorDirectMap.InternalSizeKm = sectorMapExpandedWindow.SectorMapExpanded.InternalSizeKm;
          GatesConnectionCurrent.GateDirect.Coordinates.SetFrom(sectorMapExpandedWindow.NewGateCoordinates);
          GatesConnectionCurrent.GateDirect.Rotation.Pitch = sectorMapExpandedWindow.NewGateRotation.Pitch;
        }
        else
        {
          GatesConnectionCurrent.SectorOppositeMap.InternalSizeKm = sectorMapExpandedWindow.SectorMapExpanded.InternalSizeKm;
          GatesConnectionCurrent.GateOpposite.Coordinates.SetFrom(sectorMapExpandedWindow.NewGateCoordinates);
          GatesConnectionCurrent.GateOpposite.Rotation.Pitch = sectorMapExpandedWindow.NewGateRotation.Pitch;
        }
      }
    }

    public void ButtonSave_Click(object sender, RoutedEventArgs e)
    {
      if (GatesConnectionCurrent != null)
      {
        // GatesConnectionCurrent.Save();
        Log.Debug($"[ButtonSave_Click] GatesConnectionCurrent: {GatesConnectionCurrent}");
        if (Galaxy == null)
        {
          StatusBar.SetStatusMessage("Error: Galaxy data is not loaded.", StatusMessageType.Error);
          return;
        }
        if (GatesConnectionCurrent.SectorDirect == null || GatesConnectionCurrent.SectorOpposite == null)
        {
          StatusBar.SetStatusMessage("Error: Both sectors must be selected.", StatusMessageType.Error);
          return;
        }
        Sector? sectorDirect = Galaxy.GetSectorByMacro(GatesConnectionCurrent.SectorDirect.Macro);
        Sector? sectorOpposite = Galaxy.GetSectorByMacro(GatesConnectionCurrent.SectorOpposite.Macro);
        if (sectorDirect == null || sectorOpposite == null)
        {
          StatusBar.SetStatusMessage("Error: Sectors not found.", StatusMessageType.Error);
          return;
        }
        string sectorDirectId = sectorDirect.Id;
        string sectorOppositeId = sectorOpposite.Id;
        string uniqueId = $"c_{sectorDirectId}_g_{sectorOppositeId}_b";
        string galaxyConnectionId = $"{GalaxyConnectionPrefix}_{sectorDirectId}_to_{sectorOppositeId}";
        string gateDirectId = $"connection_{GalaxyConnectionPrefix}_{sectorDirectId}_to_{sectorOppositeId}";
        string gateOppositeId = $"connection_{GalaxyConnectionPrefix}_{sectorOppositeId}_to_{sectorDirectId}";
        string zoneDirectId = $"Zone_{GalaxyConnectionPrefix}_{uniqueId}_{sectorDirect.Id}";
        string zoneOppositeId = $"Zone_{GalaxyConnectionPrefix}_{uniqueId}_{sectorOpposite.Id}";
        GateConnection gateDirect = new();
        GateData gateData = GatesConnectionCurrent.GateDirect;
        ObjectCoordinates position = gateData.Position;
        Position gatePosition = new(position.X, position.Y, position.Z);
        ObjectCoordinates coordinates = gateData.Coordinates;
        Position zonePosition = new(coordinates.X * 1000, coordinates.Y * 1000, coordinates.Z * 1000);
        gateDirect.Create(
          gateDirectId,
          gatePosition,
          gateData.Rotation.ToQuaternion(),
          new Dictionary<string, string> { ["gateMacro"] = gateData.GateMacro, ["isActive"] = gateData.Active ? "true" : "false" }
        );
        Zone zoneDirect = new();
        zoneDirect.Create(
          $"{zoneDirectId}_macro",
          new Dictionary<string, Connection> { [gateDirectId] = gateDirect },
          zonePosition,
          $"{zoneDirectId}_connection"
        );
        GateConnection gateOpposite = new();
        gateData = GatesConnectionCurrent.GateOpposite;
        position = gateData.Position;
        gatePosition = new Position(position.X, position.Y, position.Z);
        coordinates = gateData.Coordinates;
        zonePosition = new Position(coordinates.X * 1000, coordinates.Y * 1000, coordinates.Z * 1000);
        gateOpposite.Create(
          gateOppositeId,
          gatePosition,
          gateData.Rotation.ToQuaternion(),
          new Dictionary<string, string> { ["gateMacro"] = gateData.GateMacro, ["isActive"] = gateData.Active ? "true" : "false" }
        );
        Zone zoneOpposite = new();
        zoneOpposite.Create(
          $"{zoneOppositeId}_macro",
          new Dictionary<string, Connection> { [gateOppositeId] = gateOpposite },
          zonePosition,
          $"{zoneOppositeId}_connection"
        );
        Cluster? clusterDirect = Galaxy.GetClusterById(sectorDirect.ClusterId);
        Cluster? clusterOpposite = Galaxy.GetClusterById(sectorOpposite.ClusterId);
        if (clusterDirect == null || clusterOpposite == null)
        {
          StatusBar.SetStatusMessage("Error: Clusters not found.", StatusMessageType.Error);
          return;
        }
        GalaxyConnection galaxyConnection = new();
        galaxyConnection.Create(
          galaxyConnectionId,
          clusterDirect,
          sectorDirect,
          zoneDirect,
          gateDirect,
          clusterOpposite,
          sectorOpposite,
          zoneOpposite,
          gateOpposite
        );
        if (CurrentGalaxyConnection != null)
        {
          CurrentGalaxyConnection.Update(galaxyConnection, GatesConnectionCurrent);
          GatesConnectionCurrent.SetDefaultsFromReference(CurrentGalaxyConnection, AllSectors);
          GateConnectionReset();
        }
        else
        {
          GalaxyConnectionData newConnection = new(galaxyConnection, GatesConnectionCurrent);
          ChemGateKeeperMod.GalaxyConnections.Add(newConnection);
          CurrentGalaxyConnection = newConnection;
        }
      }
    }

    public void GateConnectionReset()
    {
      if (GatesConnectionCurrent != null)
      {
        GatesConnectionCurrent.Reset();
        SectorsDirectViewSource.View.Refresh();
        SectorsOppositeViewSource.View.Refresh();
      }
    }

    public void ButtonReset_Click(object sender, RoutedEventArgs e)
    {
      GateConnectionReset();
    }

    public void ButtonGateNew_Click(object sender, RoutedEventArgs e)
    {
      if (GatesConnectionCurrent != null)
      {
        GatesConnectionCurrent.ResetToInitial(GatesActiveByDefault, _gateMacroDefault);
        CurrentGalaxyConnection = null;
        GateConnectionReset();
      }
    }

    public void ButtonGateDelete_Click(object sender, RoutedEventArgs e)
    {
      if (GatesConnectionCurrent != null)
      {
        if (CurrentGalaxyConnection != null)
        {
          int index = ChemGateKeeperMod.GalaxyConnections.IndexOf(CurrentGalaxyConnection);
          ChemGateKeeperMod.GalaxyConnections.Remove(CurrentGalaxyConnection);
          if (ChemGateKeeperMod.GalaxyConnections.Count > 0)
          {
            if (index >= ChemGateKeeperMod.GalaxyConnections.Count)
            {
              index = ChemGateKeeperMod.GalaxyConnections.Count - 1;
            }
            CurrentGalaxyConnection = ChemGateKeeperMod.GalaxyConnections[index];
          }
          else
          {
            CurrentGalaxyConnection = null;
            GatesConnectionCurrent.ResetToInitial(GatesActiveByDefault, _gateMacroDefault);
          }
        }
      }
    }

    public void ButtonNewMod_Click(object sender, RoutedEventArgs e)
    {
      ChemGateKeeperMod = new("", X4UniverseId);
      _chemGateKeeperMod.SetGameVersion(X4DataVersion);
      ChemGateKeeperMod.GalaxyConnections.Clear();
      GatesConnectionCurrent?.ResetToInitial(GatesActiveByDefault, _gateMacroDefault);
      SectorsDirectViewSource.View.Refresh();
      SectorsOppositeViewSource.View.Refresh();
      CurrentGalaxyConnection = null;
      IsModCanBeSaved = false;
    }

    public void ButtonLoadMod_Click(object sender, RoutedEventArgs e)
    {
      ModLoad();
    }

    public void ModLoad(string path = "")
    {
      if (Galaxy == null)
      {
        StatusBar.SetStatusMessage("Error: Galaxy data is not loaded.", StatusMessageType.Error);
        return;
      }
      ChemGateKeeper newMod = new(path, X4UniverseId);
      newMod.SetGameVersion(_x4DataVersion);
      if (newMod.LoadData(Galaxy, ChemGateKeeperMod))
      {
        ChemGateKeeperMod = newMod;
        SectorsDirectViewSource.View.Refresh();
        SectorsOppositeViewSource.View.Refresh();
        if (ChemGateKeeperMod.GalaxyConnections.Count > 0)
        {
          CurrentGalaxyConnection = ChemGateKeeperMod.GalaxyConnections[0];
        }
        else
        {
          CurrentGalaxyConnection = null;
        }
        StatusBar.SetStatusMessage("Mod data loaded successfully.", StatusMessageType.Info);
        UpdateIsModCanBeSavedAs(); // Enable Save As for non-empty mods immediately after load
      }
      else
      {
        StatusBar.SetStatusMessage("Error: Mod data could not be loaded.", StatusMessageType.Error);
        Log.Warn("Mod data could not be loaded.");
        UpdateIsModCanBeSavedAs();
      }
    }

    public void ButtonSaveMod_Click(object sender, RoutedEventArgs e)
    {
      if (ChemGateKeeperMod.GalaxyConnections.Count > 0)
      {
        IsModCanBeSaved = !ChemGateKeeperMod.SaveData(Galaxy);
      }
    }

    public void ButtonSaveModAs_Click(object sender, RoutedEventArgs e)
    {
      if (ChemGateKeeperMod.GalaxyConnections.Count > 0)
      {
        IsModCanBeSaved = !ChemGateKeeperMod.SaveData(Galaxy, true);
      }
    }

    public void ButtonExtractX4Data_Click(object sender, RoutedEventArgs e)
    {
      X4DataExtractionWindow extractionWindow = new(_appIcon, X4GameFolder, X4DataFolder, LoadModsData) { Owner = this };
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
      LoadX4DataInBackgroundStart();
    }

    public void ButtonAbout_Click(object sender, RoutedEventArgs e)
    {
      Dictionary<string, string> informationalLinks = new()
      {
        { "GitHub", "https://github.com/chemodun/X4-UniverseEditor" },
        { "EGOSOFT Forum", "https://forum.egosoft.com/viewtopic.php?p=5262362" },
        { "Nexus", "https://www.nexusmods.com/x4foundations/mods/1587/" },
      };

      AssemblyInfo assemblyInfo = AssemblyInfo.GetAssemblyInfo(Assembly.GetExecutingAssembly());
      AboutWindow aboutWindow = new(_appIcon, assemblyInfo, informationalLinks) { Owner = this };
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
        StatusBar.SetStatusMessage("Error: Documentation file not found.", StatusMessageType.Error);
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
          StatusBar.SetStatusMessage("Documentation opened in default browser.", StatusMessageType.Info);
        }
      }
      catch (System.Exception ex)
      {
        Log.Error($"Failed to open file with default browser: {ex.Message}");
        StatusBar.SetStatusMessage("Failed to open documentation.", StatusMessageType.Error);
      }
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
