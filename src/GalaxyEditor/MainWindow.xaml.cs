using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Diagnostics;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using SharedWindows;
using Utilities.Logging;
using X4DataLoader;
using X4Map;
using X4Map.Converters;

namespace GalaxyEditor
{
  public class AppConfig
  {
    public ModConfig Mod { get; set; } = new ModConfig();
    public EditConfig Edit { get; set; } = new EditConfig();
    public MapConfig Map { get; set; } = new MapConfig();
    public DataConfig Data { get; set; } = new DataConfig();
    public LoggingConfig Logging { get; set; } = new LoggingConfig();
  }

  public class ModConfig
  {
    public string LatestModPath { get; set; } = "";
    public bool AutoLoadLatestMod { get; set; } = false;
  }

  public class EditConfig
  {
    public bool GatesActiveByDefault { get; set; } = true;
    public int GatesMinimalDistanceBetween { get; set; } = 10;
  }

  public class MapConfig
  {
    public double MapColorsOpacity { get; set; } = 0.5;
    public int SectorRadius { get; set; } = 400;
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
          App.ConfigureNLog(this);
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
          App.ConfigureNLog(this);
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

    public Galaxy GalaxyData { get; private set; }

    public FactionColors FactionColors = new();

    public GalaxyReferencesHolder GalaxyReferences { get; private set; } = new();
    public bool IsDataLoaded
    {
      get => GalaxyData != null && GalaxyData.Clusters.Count > 0;
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
          SaveConfiguration();
        }
      }
    }
    private readonly List<string> ExtraNeededFilesMasks = [];
    private readonly List<GameFilesStructureItem> X4DataStructure =
    [
      new GameFilesStructureItem(id: "translations", folder: "t", ["0001-l044.xml", "0001.xml"]),
      new GameFilesStructureItem(id: "colors", folder: "libraries", ["colors.xml"]),
      new GameFilesStructureItem(id: "sounds", folder: "libraries", ["sound_library.xml"]),
      new GameFilesStructureItem(id: "icons", folder: "libraries", ["icons.xml"]),
      new GameFilesStructureItem(id: "mapDefaults", folder: "libraries", ["mapdefaults.xml"]),
      new GameFilesStructureItem(id: "clusters", folder: "maps/xu_ep2_universe", ["clusters.xml"], MatchingModes.Suffix),
      new GameFilesStructureItem(id: "sectors", folder: "maps/xu_ep2_universe", ["sectors.xml"], MatchingModes.Suffix),
      new GameFilesStructureItem(id: "zones", folder: "maps/xu_ep2_universe", ["zones.xml"], MatchingModes.Suffix),
      new GameFilesStructureItem(id: "races", folder: "libraries", ["races.xml"]),
      new GameFilesStructureItem(id: "factions", folder: "libraries", ["factions.xml"]),
      new GameFilesStructureItem(id: "modules", folder: "libraries", ["modules.xml"]),
      new GameFilesStructureItem(id: "modulegroups", folder: "libraries", ["modulegroups.xml"]),
      new GameFilesStructureItem(id: "constructionplans", folder: "libraries", ["constructionplans.xml"]),
      new GameFilesStructureItem(id: "stationgroups", folder: "libraries", ["stationgroups.xml"]),
      new GameFilesStructureItem(id: "stations", folder: "libraries", ["stations.xml"]),
      new GameFilesStructureItem(id: "god", folder: "libraries", ["god.xml"]),
      new GameFilesStructureItem(id: "sechighways", folder: "maps/xu_ep2_universe", ["sechighways.xml"], MatchingModes.Suffix),
      new GameFilesStructureItem(id: "zonehighways", folder: "maps/xu_ep2_universe", ["zonehighways.xml"], MatchingModes.Suffix),
      new GameFilesStructureItem(id: "galaxy", folder: "maps/xu_ep2_universe", ["galaxy.xml"]),
      new GameFilesStructureItem(id: "patchactions", folder: "libraries", ["patchactions.xml"]),
    ];
    private readonly List<ProcessingOrderItem> X4PDataProcessingOrder =
    [
      new ProcessingOrderItem("translations", ""),
      new ProcessingOrderItem("colors", ""),
      new ProcessingOrderItem("sounds", ""),
      new ProcessingOrderItem("icons", ""),
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
    public string X4DataFolderPath
    {
      get
      {
        if (Directory.Exists(X4DataFolder))
        {
          if (DataLoader.ValidateDataFolder(X4DataFolder, out _))
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
          SaveConfiguration();
          if (!value && GalaxyData != null && GalaxyData.Version != 0 && GalaxyData.Version != X4DataVersion)
          {
            X4DataVersion = GalaxyData.Version;
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
          SaveConfiguration();
          if (GalaxyMapViewer != null)
          {
            GalaxyMapViewer.SectorRadius = value;
            GalaxyMapViewer.UpdateMap();
          }
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
          SaveConfiguration();
          if (GalaxyMapViewer != null)
          {
            GalaxyMapViewer.MapColorsOpacity = value;
            GalaxyMapViewer.UpdateMap();
          }
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
          // SaveConfiguration();
        }
      }
    }

    private string _latestModPath = "";
    public string LatestModPath
    {
      get => _latestModPath;
      set
      {
        if (_latestModPath != value)
        {
          _latestModPath = value;
          OnPropertyChanged(nameof(LatestModPath));
          SaveConfiguration();
        }
      }
    }

    private bool _autoLoadLatestMod = false;
    public bool AutoLoadLatestMod
    {
      get => _autoLoadLatestMod;
      set
      {
        if (_autoLoadLatestMod != value)
        {
          _autoLoadLatestMod = value;
          OnPropertyChanged(nameof(AutoLoadLatestMod));
          SaveConfiguration();
        }
      }
    }

    private GalaxyMod? _currentMod;
    public GalaxyMod? CurrentMod
    {
      get => _currentMod;
      set
      {
        if (_currentMod != value)
        {
          _currentMod = value;
          OnPropertyChanged(nameof(CurrentMod));
          OnPropertyChanged(nameof(ModOptionsVisibility));
          OnPropertyChanged(nameof(WindowTitle));
          IsModCanBeSaved = false;
          IsModCanBeSavedAs = false;
          if (_currentMod != null)
          {
            _currentMod.PropertyChanged += (sender, e) =>
            {
              if (e.PropertyName == "Name" || e.PropertyName == "Id" || e.PropertyName == "Version")
              {
                OnPropertyChanged(nameof(WindowTitle));
              }
            };
            LatestModPath = _currentMod.Path;
            IsModCanBeSaved = true;
            IsModCanBeSavedAs = true;
          }
        }
      }
    }

    public bool ModIsLoaded => CurrentMod != null;
    public Visibility ModOptionsVisibility => ModIsLoaded ? Visibility.Visible : Visibility.Collapsed;
    public StatusBarMessage StatusBar { get; set; } = new();
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

    public CellItemInfo SelectedCellItemInfo
    {
      get => new(GalaxyMapViewer.SelectedMapCluster);
    }
    public ClusterItemInfo SelectedClusterItemInfo
    {
      get => new(GalaxyMapViewer.SelectedMapCluster?.Cluster);
    }
    public SectorItemInfo SelectedSectorItemInfo
    {
      get => new(GalaxyMapViewer.SelectedMapSector?.Sector);
    }
    private readonly AssemblyInfo _assemblyInfoData;
    private readonly BitmapImage _appIcon;
    private BackgroundWorker _backgroundWorker;

    public string WindowTitle
    {
      get
      {
        string title = $"{_assemblyInfoData.Product} - {_assemblyInfoData.Version}";
        if (CurrentMod != null)
        {
          title += $" | Mod: {CurrentMod.Name} v.{CurrentMod.Version} [{CurrentMod.Id}] ({CurrentMod.Path})";
        }
        return title;
      }
    }

    public MainWindow()
    {
      _configFileName = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.json";
      LoadConfiguration();
      InitializeComponent();
      DataContext = this;
      _assemblyInfoData = AssemblyInfo.GetAssemblyInfo(Assembly.GetExecutingAssembly());
      _appIcon = Icon as BitmapImage ?? new BitmapImage();
      // Title = $"{_assemblyInfoData.Product} - {_assemblyInfoData.Version}";
      GalaxyData = new Galaxy();
      Canvas galaxyCanvas = (Canvas)FindName("GalaxyMapCanvas");
      GalaxyMapViewer.Connect(GalaxyData, GalaxyMapCanvas, MapColorsOpacity, SectorRadius);
      GalaxyMapViewer.ShowEmptyClusterPlaces.IsChecked = true;
      GalaxyMapViewer.OnPressedSector += GalaxyMapViewer_OnPressedSector;
      GalaxyMapViewer.OnPressedCluster += GalaxyMapViewer_OnPressedCluster;
      GalaxyMapViewer.OnPressedCell += GalaxyMapViewer_OnPressedCell;
      GalaxyMapViewer.OnRightPressedSector += GalaxyMapViewer_OnRightPressedSector;
      GalaxyMapViewer.OnRightPressedCluster += GalaxyMapViewer_OnRightPressedCluster;
      GalaxyMapViewer.OnRightPressedCell += GalaxyMapViewer_OnRightPressedCell;
      _backgroundWorker = new BackgroundWorker { WorkerReportsProgress = true, WorkerSupportsCancellation = false };
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
          AutoLoadLatestMod = config.Mod.AutoLoadLatestMod;
          MapColorsOpacity = config.Map.MapColorsOpacity;
          SectorRadius = config.Map.SectorRadius;
          LogLevel = config.Logging.LogLevel;
          LogToFile = config.Logging.LogToFile;
          LatestModPath = config.Mod.LatestModPath;
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

    private readonly JsonSerializerOptions _jsonSerializerOptions = new() { WriteIndented = true };

    private void SaveConfiguration()
    {
      var config = new AppConfig
      {
        Mod = new ModConfig { LatestModPath = LatestModPath, AutoLoadLatestMod = AutoLoadLatestMod },
        Data = new DataConfig
        {
          X4DataExtractedPath = X4DataFolder,
          X4DataVersionOverride = X4DataVersionOverride,
          LoadModsData = LoadModsData,
        },
        Map = new MapConfig { MapColorsOpacity = MapColorsOpacity, SectorRadius = SectorRadius },
        Logging = new LoggingConfig { LogLevel = LogLevel, LogToFile = LogToFile },
      };
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

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
      X4DataNotLoadedCheckAndWarning();
    }

    private void X4DataNotLoadedCheckAndWarning()
    {
      // Validate the loaded X4DataFolder
      if (!DataLoader.ValidateDataFolder(X4DataFolder, out string errorMessage))
      {
        StatusBar.SetStatusMessage(errorMessage, StatusMessageType.Error);
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
        StatusBar.SetStatusMessage($"Processing file: {progressText} ...", StatusMessageType.Info, true);
      }
    }

    private void LoadX4DataInBackgroundCompleted(object? sender, RunWorkerCompletedEventArgs e)
    {
      if (e.Error != null)
      {
        StatusBar.SetStatusMessage("Error loading X4 data: " + e.Error.Message, StatusMessageType.Error);
      }
      _backgroundWorker.Dispose();
      // IsDataRefreshed = true;
      Dispatcher.BeginInvoke(
        new Action(() =>
        {
          GalaxyMapViewer.RefreshGalaxyData();
          if (AutoLoadLatestMod && LatestModPath != "" && Directory.Exists(LatestModPath))
          {
            ButtonLoadMod_Click(this, new RoutedEventArgs(Button.ClickEvent));
          }
        })
      );
      StatusBar.SetStatusMessage("X4 data loaded successfully!", StatusMessageType.Info);
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
      dataLoader.LoadData(GalaxyData, X4DataFolder, X4DataStructure, X4PDataProcessingOrder, LoadModsData);

      if (!X4DataVersionOverride && GalaxyData.Version != 0 && GalaxyData.Version != X4DataVersion)
      {
        X4DataVersion = GalaxyData.Version;
      }
      GalaxyMapViewer.FactionColors.Load(GalaxyData.Factions, GalaxyData.MappedColors);
      OnPropertyChanged(nameof(IsDataLoaded));
      GalaxyReferences = new GalaxyReferencesHolder(GalaxyData);
      StatusBar.SetStatusMessage("X4 data loaded successfully.", StatusMessageType.Info);
    }

    private void GalaxyMapViewer_OnPressedCell(object? sender, CellEventArgs e)
    {
      // Your code to run when the event is raised
      if (e.PressedCell != null)
      {
        string actionString = GalaxyMapViewer.SelectedMapCluster != null ? "Selected" : "Unselected";
        string message = $"{actionString} cell: {e.PressedCell.MapPosition.Column}, {e.PressedCell.MapPosition.Row}";
        Log.Debug(message);
        StatusBar.SetStatusMessage(message, StatusMessageType.Info);
        RibbonMain.SelectedTabItem = (Fluent.RibbonTabItem)RibbonMain.FindName("RibbonTabCell")!;
        // Show the cell details
      }
      OnPropertyChanged(nameof(SelectedSectorItemInfo));
      OnPropertyChanged(nameof(SelectedClusterItemInfo));
      OnPropertyChanged(nameof(SelectedCellItemInfo));
    }

    private void GalaxyMapViewer_OnPressedCluster(object? sender, ClusterEventArgs e)
    {
      // Your code to run when the event is raised
      if (e.PressedCluster != null)
      {
        string actionString = GalaxyMapViewer.SelectedMapCluster != null ? "Selected" : "Unselected";
        string message = $"{actionString} cluster: {e.PressedCluster.Name}";
        Log.Debug(message);
        StatusBar.SetStatusMessage(message, StatusMessageType.Info);
        RibbonMain.SelectedTabItem = (Fluent.RibbonTabItem)RibbonMain.FindName("RibbonTabCluster")!;
        // Show the sector details
      }
      OnPropertyChanged(nameof(SelectedSectorItemInfo));
      OnPropertyChanged(nameof(SelectedClusterItemInfo));
      OnPropertyChanged(nameof(SelectedCellItemInfo));
    }

    private void GalaxyMapViewer_OnPressedSector(object? sender, SectorEventArgs e)
    {
      // Your code to run when the event is raised
      if (e.PressedSector != null)
      {
        string actionString = GalaxyMapViewer.SelectedMapSector != null ? "Selected" : "Unselected";
        string message = $"{actionString} sector: {e.PressedSector.Name}";
        Log.Debug(message);
        StatusBar.SetStatusMessage(message, StatusMessageType.Info);
        RibbonMain.SelectedTabItem = (Fluent.RibbonTabItem)RibbonMain.FindName("RibbonTabSector")!;
        // Show the sector details
      }
      OnPropertyChanged(nameof(SelectedSectorItemInfo));
      OnPropertyChanged(nameof(SelectedClusterItemInfo));
      OnPropertyChanged(nameof(SelectedCellItemInfo));
    }

    private void GalaxyMapViewer_OnRightPressedCell(object? sender, CellEventArgs e)
    {
      // Your code to run when the event is raised
      if (e.PressedCell != null)
      {
        string message = $"Right button pressed on cell: {e.PressedCell.MapPosition.Column}, {e.PressedCell.MapPosition.Row}";
        Log.Debug(message);
        RibbonMain.SelectedTabItem = (Fluent.RibbonTabItem)RibbonMain.FindName("RibbonTabCell")!;
        OnPropertyChanged(nameof(SelectedSectorItemInfo));
        OnPropertyChanged(nameof(SelectedClusterItemInfo));
        OnPropertyChanged(nameof(SelectedCellItemInfo));
        HexagonContextMenu(null, null, e.PressedCell);
        // Show the cell details
      }
    }

    private void GalaxyMapViewer_OnRightPressedCluster(object? sender, ClusterEventArgs e)
    {
      // Your code to run when the event is raised
      if (e.PressedCluster != null)
      {
        string message = $"Right button pressed on cluster: {e.PressedCluster.Name}";
        Log.Debug(message);
        RibbonMain.SelectedTabItem = (Fluent.RibbonTabItem)RibbonMain.FindName("RibbonTabCluster")!;
        OnPropertyChanged(nameof(SelectedSectorItemInfo));
        OnPropertyChanged(nameof(SelectedClusterItemInfo));
        OnPropertyChanged(nameof(SelectedCellItemInfo));
        HexagonContextMenu(null, e.PressedCluster, null);
        // Show the sector details
      }
    }

    private void GalaxyMapViewer_OnRightPressedSector(object? sender, SectorEventArgs e)
    {
      // Your code to run when the event is raised
      if (e.PressedSector != null)
      {
        string message = $"Right button pressed on sector: {e.PressedSector.Name}";
        Log.Debug(message);
        RibbonMain.SelectedTabItem = (Fluent.RibbonTabItem)RibbonMain.FindName("RibbonTabSector")!;
        HexagonContextMenu(e.PressedSector, null, null);
        OnPropertyChanged(nameof(SelectedSectorItemInfo));
        OnPropertyChanged(nameof(SelectedClusterItemInfo));
        OnPropertyChanged(nameof(SelectedCellItemInfo));
        // MenuItem menuItem = new MenuItem { Header = "Action" };
        // Show the sector details
      }
    }

    public void HexagonContextMenu(Sector? sector, Cluster? cluster, GalaxyMapCluster? cell)
    {
      System.Windows.Forms.ContextMenuStrip contextMenu = new System.Windows.Forms.ContextMenuStrip();
      System.Windows.Forms.ToolStripMenuItem menuItem;
      if (sector != null)
      {
        contextMenu.Items.Add(
          new System.Windows.Forms.ToolStripLabel
          {
            Text = $"Sector {sector.Name}",
            Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold),
          }
        );
        contextMenu.Items.Add(new System.Windows.Forms.ToolStripSeparator());
        menuItem = new System.Windows.Forms.ToolStripMenuItem("Edit");
        menuItem.Click += EditSector_Click;
        contextMenu.Items.Add(menuItem);
        menuItem = new System.Windows.Forms.ToolStripMenuItem("Delete");
        menuItem.Click += DeleteSector_Click;
        contextMenu.Items.Add(menuItem);
        cluster = GalaxyMapViewer.SelectedMapCluster?.Cluster;
      }
      if (cluster != null)
      {
        if (contextMenu.Items.Count > 0)
        {
          contextMenu.Items.Add(new System.Windows.Forms.ToolStripSeparator());
        }
        contextMenu.Items.Add(
          new System.Windows.Forms.ToolStripLabel
          {
            Text = $"Cluster {cluster.Name}",
            Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold),
          }
        );
        contextMenu.Items.Add(new System.Windows.Forms.ToolStripSeparator());
        menuItem = new System.Windows.Forms.ToolStripMenuItem("Edit");
        menuItem.Click += EditCluster_Click;
        contextMenu.Items.Add(menuItem);
        menuItem = new System.Windows.Forms.ToolStripMenuItem("Delete");
        menuItem.Click += DeleteCluster_Click;
        contextMenu.Items.Add(menuItem);
        contextMenu.Items.Add(new System.Windows.Forms.ToolStripSeparator());
        menuItem = new System.Windows.Forms.ToolStripMenuItem("Add Sector");
        menuItem.Click += AddCluster_Click;
        contextMenu.Items.Add(menuItem);
      }
      if (cell != null)
      {
        if (contextMenu.Items.Count > 0)
        {
          contextMenu.Items.Add(new System.Windows.Forms.ToolStripSeparator());
        }
        contextMenu.Items.Add(
          new System.Windows.Forms.ToolStripLabel
          {
            Text = $"Cell [{cell.MapPosition.Column}, {cell.MapPosition.Row}]",
            Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold),
          }
        );
        contextMenu.Items.Add(new System.Windows.Forms.ToolStripSeparator());
        menuItem = new System.Windows.Forms.ToolStripMenuItem("Add Cluster");
        menuItem.Click += AddCluster_Click;
        contextMenu.Items.Add(menuItem);
      }
      contextMenu.Show(System.Windows.Forms.Cursor.Position);
    }

    public void EditSector_Click(object? sender, EventArgs e)
    {
      if (GalaxyMapViewer.SelectedMapSector != null)
      {
        MessageBox.Show($"Action triggered - Edit for {GalaxyMapViewer.SelectedMapSector.Sector?.Name}!");
      }
    }

    public void DeleteSector_Click(object? sender, EventArgs e)
    {
      if (GalaxyMapViewer.SelectedMapSector != null)
      {
        MessageBox.Show($"Action triggered - Delete for {GalaxyMapViewer.SelectedMapSector.Sector?.Name}!");
      }
    }

    public void EditCluster_Click(object? sender, EventArgs e)
    {
      if (GalaxyMapViewer.SelectedMapCluster != null)
      {
        if (CurrentMod == null)
        {
          MessageBox.Show("No mod loaded to edit the cluster!");
          return;
        }
        UnifyItemCluster? cluster = UnifyItemCluster.SearchById(CurrentMod.Clusters, GalaxyMapViewer.SelectedMapCluster.Cluster?.Id ?? "");
        ClusterEditWindow clusterEditWindow = new(
          _appIcon,
          cluster,
          GalaxyMapViewer.SelectedMapCluster.Cluster,
          null,
          GalaxyData,
          GalaxyReferences
        )
        {
          Owner = this,
        };
        if (clusterEditWindow.ShowDialog() == true)
        {
          if (cluster != null)
          {
            Log.Debug($"Updating cluster {cluster.Name} ...");
            cluster.UpdateFrom(clusterEditWindow.Cluster);
          }
          else
          {
            Log.Debug($"Adding cluster {clusterEditWindow.Cluster.Name} ...");
            CurrentMod.Clusters.Add(clusterEditWindow.Cluster);
          }
        }
      }
    }

    public void DeleteCluster_Click(object? sender, EventArgs e)
    {
      if (GalaxyMapViewer.SelectedMapCluster != null)
      {
        MessageBox.Show($"Action triggered - Delete for {GalaxyMapViewer.SelectedMapCluster.Cluster?.Name}!");
      }
    }

    public void AddCluster_Click(object? sender, EventArgs e)
    {
      if (GalaxyMapViewer.SelectedMapCluster != null)
      {
        if (CurrentMod == null)
        {
          MessageBox.Show("No mod loaded to edit the cluster!");
          return;
        }
        UnifyItemCluster? cluster = UnifyItemCluster.SearchByPosition(CurrentMod.Clusters, GalaxyMapViewer.SelectedMapCluster.Position);

        ClusterEditWindow clusterEditWindow = new(
          _appIcon,
          cluster,
          null,
          GalaxyMapViewer.SelectedMapCluster.Position,
          GalaxyData,
          GalaxyReferences
        )
        {
          Owner = this,
        };
        if (clusterEditWindow.ShowDialog() == true)
        {
          if (cluster != null)
          {
            Log.Debug($"Updating cluster {cluster.Name} ...");
            cluster.UpdateFrom(clusterEditWindow.Cluster);
          }
          else
          {
            Log.Debug($"Adding cluster {clusterEditWindow.Cluster.Name} ...");
            CurrentMod.Clusters.Add(clusterEditWindow.Cluster);
          }
        }
      }
    }

    public void ButtonNewMod_Click(object sender, RoutedEventArgs e)
    {
      GalaxyMod newMod = new();
      if (newMod.Create(GalaxyMapViewer.MapInfo, GalaxyMapViewer.GalaxyData))
      {
        CurrentMod = newMod;
      }
    }

    public void ButtonLoadMod_Click(object sender, RoutedEventArgs e)
    {
      GalaxyMod? mod = new();
      string modPath = string.Empty;
      if (sender == this && AutoLoadLatestMod && !string.IsNullOrEmpty(LatestModPath) && Directory.Exists(LatestModPath))
      {
        modPath = LatestModPath;
      }
      if (mod.Load(modPath))
      {
        CurrentMod = mod;
        RibbonMain.SelectedTabItem = (Fluent.RibbonTabItem)RibbonMain.FindName("RibbonTabMod")!;
        StatusBar.SetStatusMessage($"Mod {CurrentMod.Name} from {LatestModPath} loaded successfully!", StatusMessageType.Info);
      }
    }

    public void ButtonSaveMod_Click(object sender, RoutedEventArgs e) { }

    public void ButtonSaveModAs_Click(object sender, RoutedEventArgs e) { }

    public void ButtonExtractX4Data_Click(object sender, RoutedEventArgs e)
    {
      X4DataExtractionWindow extractionWindow = new(_appIcon, X4GameFolder, X4DataFolder, LoadModsData, ExtraNeededFilesMasks)
      {
        Owner = this,
      };
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

    public void SelectX4DataFolder_Click(object sender, RoutedEventArgs e)
    {
      if (GalaxyData != null && GalaxyData.Clusters.Count > 0)
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
      var dialog = new System.Windows.Forms.FolderBrowserDialog
      {
        Description = "Please select the folder where the X4 extracted data files are located.",
        ShowNewFolderButton = false,
      };
      if (GalaxyData != null && GalaxyData.Clusters.Count > 0)
      {
        dialog.SelectedPath = $"{X4DataFolder}\\";
      }
      else
      {
        dialog.SelectedPath = "";
        dialog.RootFolder = Environment.SpecialFolder.MyComputer; // Set the root folder to MyComputer
      }
      System.Windows.Forms.DialogResult result = dialog.ShowDialog();

      if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.SelectedPath))
      {
        string selectedPath = dialog.SelectedPath;
        if (DataLoader.ValidateDataFolder(selectedPath, out string errorMessage))
        {
          X4DataFolder = selectedPath;
          StatusBar.SetStatusMessage("X4 Data folder set successfully. Loading the data ...", StatusMessageType.Info, true);
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

    public void ButtonReadme_Click(object sender, RoutedEventArgs e) { }

    public void ButtonAbout_Click(object sender, RoutedEventArgs e)
    {
      Dictionary<string, string> informationalLinks = new() { { "GitHub", "https://github.com/chemodun/X4-UniverseEditor" } };
      AboutWindow aboutWindow = new(_appIcon, _assemblyInfoData, informationalLinks) { Owner = this };
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
