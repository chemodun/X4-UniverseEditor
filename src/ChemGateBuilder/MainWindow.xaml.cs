using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Threading;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Shapes;
using X4DataLoader;
using System.Windows.Input;
using System.Windows.Controls;
using NLog;
using Utilities.Logging;

using System.Windows.Markup;

namespace ChemGateBuilder
{
    public class AppConfig
    {
        public EditConfig Edit { get; set; } = new EditConfig();
        public MapConfig Map { get; set; } = new MapConfig();
        public DataConfig Data { get; set; } = new DataConfig();
        public LoggingConfig Logging { get; set; } = new LoggingConfig();

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
        public string X4DataExtractedPath { get; set; } = ".";
        public bool X4DataVersionOverride { get; set; } = false;
        public int X4DataVersion { get; set; } = 710;
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
                    OnPropertyChanged(nameof(X4DataFolderStatus));
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

        public ObservableCollection<string> X4DataVersions { get; set; } = ["7.10", "7.50"];

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
                    if (value > 0 && GatesConnectionCurrent != null)
                    {
                        GatesConnectionCurrent.SetSectorMapInternalSize(value);
                    }
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

        private string _statusMessage = "Ready";
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                if (_statusMessage != value)
                {
                    _statusMessage = value;
                    OnPropertyChanged(nameof(StatusMessage));
                    StartStatusMessageTimer();
                }
            }
        }

        private StatusMessageType _statusMessageType = StatusMessageType.Info;
        public StatusMessageType StatusMessageType
        {
            get => _statusMessageType;
            set
            {
                if (_statusMessageType != value)
                {
                    _statusMessageType = value;
                    OnPropertyChanged(nameof(StatusMessageType));
                }
            }
        }

        // Timer to hide the StatusMessage
        private DispatcherTimer? _statusMessageTimer;
        private readonly TimeSpan _statusMessageDisplayDuration = TimeSpan.FromSeconds(5); // Adjust as needed

        private void StartStatusMessageTimer()
        {
            // Stop existing timer if any
            _statusMessageTimer?.Stop();

            if (string.IsNullOrEmpty(_statusMessage))
                return;

            // Initialize and start the timer
            _statusMessageTimer = new DispatcherTimer
            {
                Interval = _statusMessageDisplayDuration
            };
            _statusMessageTimer.Tick += StatusMessageTimer_Tick;
            _statusMessageTimer.Start();
        }

        private void StatusMessageTimer_Tick(object? sender, EventArgs e)
        {
            _statusMessageTimer?.Stop();
            StatusMessage = string.Empty; // Clear the message
            StatusMessageType = StatusMessageType.Info; // Reset the message type
        }

        // Method to set status message with type
        public void SetStatusMessage(string message, StatusMessageType messageType)
        {
            StatusMessage = message;
            StatusMessageType = messageType;
        }

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
                        GatesConnectionCurrent.Reset();
                    }
                }
            }
        }

        public ObservableCollection<GalaxyConnectionData> GalaxyConnections { get; } = [];

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
                }
            }
        }

        // Master sector list
        public ObservableCollection<SectorItem> AllSectors { get; } = [];

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

        public string X4DataFolderStatus
        {
            get
            {
                if (Directory.Exists(X4DataFolder))
                {
                    if (ValidateX4DataFolder(X4DataFolder, out string errorMessage))
                    {
                        return System.IO.Path.GetFullPath(X4DataFolder);
                    }
                    else
                    {
                        return errorMessage;
                    }
                }
                else
                {
                    return $"Error: Folder does not exist ({X4DataFolder})";
                }
            }
        }


        private void HandleValidationError(string message)
        {
            // Optional: Log the validation message for debugging
            Log.Error($"Validation Error: {message}");

            SetStatusMessage(message, StatusMessageType.Error);
        }

        // Galaxy and Sectors
        public Galaxy? Galaxy { get; private set; }

        public bool IsDataLoaded => AllSectors.Count > 0;

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
                }
            }
        }
        public bool IsNowGateCanBeDeleted => IsDataLoaded & GalaxyConnections.Count > 0 && CurrentGalaxyConnection != null;
        public bool IsNowGateCanBeCreated => IsDataLoaded & GalaxyConnections.Count > 0 && CurrentGalaxyConnection != null;

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
                bool isModChanged = ChemGateKeeperMod.IsModChanged(GalaxyConnections);
                if (value && IsGateCanBeDeleted)
                {
                    IsModCanBeSaved =isModChanged;
                } else
                {
                    IsModCanBeSaved = false;
                }
            }
        }

        public bool IsModCanBeCreated => IsDataLoaded & GalaxyConnections.Count > 0;

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

        public FactionColors FactionColors = new();

        // Constructor
        public MainWindow()
        {
            _configFileName = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.json";
            GatesConnectionCurrent = new GatesConnectionData(GatesActiveByDefault, _gateMacroDefault);
            LoadConfiguration();
            InitializeComponent();
            DataContext = this;
            _chemGateKeeperMod.SetGameVersion(X4DataVersion);
            GatesConnectionCurrent.SetMapsCanvasAndHexagons(SectorDirectCanvas, SectorDirectHexagon, SectorOppositeCanvas, SectorOppositeHexagon);
            GatesConnectionCurrent.Reset();

            // Initialize CollectionViewSource filters
            SectorsDirectViewSource.Filter += SectorsDirect_Filter;
            SectorsOppositeViewSource.Filter += SectorsOpposite_Filter;

            SectorsDirectViewSource.Source = AllSectors;
            SectorsOppositeViewSource.Source = AllSectors;

            // Subscribe to Validation Errors
            TextBoxExtensions.OnValidationError += HandleValidationError;

            // Validate the loaded X4DataFolder
            if (!ValidateX4DataFolder(X4DataFolder, out string errorMessage))
            {
                SetStatusMessage(errorMessage, StatusMessageType.Error);
                // Prompt the user to select a valid folder
                SelectX4DataFolder();
            }

            // Load sectors if the folder is valid
            if (ValidateX4DataFolder(X4DataFolder, out errorMessage))
            {
                SetStatusMessage("X4 Data folder validated successfully.", StatusMessageType.Info);
                LoadX4Data();
                GateMacros.Add(_gateMacroDefault);
            }

            // Optionally, set default selections
            if (AllSectors.Count > 0)
            {
                GatesConnectionCurrent.SectorDirect = null;
            }
            if (AllSectors.Count > 1)
            {
                GatesConnectionCurrent.SectorOpposite = null;
            }
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
                    GatesActiveByDefault = config.Edit.GatesActiveByDefault;
                    GatesMinimalDistanceBetween = config.Edit.GatesMinimalDistanceBetween;
                    MapColorsOpacity = config.Map.MapColorsOpacity;
                    SectorRadius = config.Map.SectorRadius;
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
                Data = new DataConfig {
                    X4DataExtractedPath = X4DataFolder,
                    X4DataVersionOverride = X4DataVersionOverride
                },
                Edit = new EditConfig
                {
                    GatesActiveByDefault = GatesActiveByDefault,
                    GatesMinimalDistanceBetween = GatesMinimalDistanceBetween,
                },
                Map = new MapConfig
                {
                    MapColorsOpacity = MapColorsOpacity,
                    SectorRadius = SectorRadius
                },
                Logging = new LoggingConfig
                {
                    LogLevel = LogLevel,
                    LogToFile = LogToFile
                }
            };
            if (X4DataVersionOverride)
            {
                config.Data.X4DataVersion = X4DataVersion;
            }

            var jsonString = JsonSerializer.Serialize(config, _jsonSerializerOptions);
            File.WriteAllText(_configFileName, jsonString);
        }

        private static bool ValidateX4DataFolder(string folderPath, out string errorMessage)
        {
            string subfolderPath = System.IO.Path.Combine(folderPath, "t");
            string filePath = System.IO.Path.Combine(subfolderPath, "0001-l044.xml");

            if (Directory.Exists(subfolderPath) && File.Exists(filePath) && new FileInfo(filePath).Length > 0)
            {
                errorMessage = string.Empty;
                return true;
            }
            else
            {
                errorMessage = $"Error: Folder does not contain required X4 data ({folderPath})";
                return false;
            }
        }

        private void SelectX4DataFolder_Click(object? sender, RoutedEventArgs? e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.SelectedPath))
            {
                string selectedPath = dialog.SelectedPath;
                if (ValidateX4DataFolder(selectedPath, out string errorMessage))
                {
                    X4DataFolder = selectedPath;
                    LoadX4Data();
                    SetStatusMessage("X4 Data folder set successfully.", StatusMessageType.Info);
                }
                else
                {
                    SetStatusMessage(errorMessage, StatusMessageType.Error);
                    MessageBox.Show(errorMessage, "Invalid Folder", MessageBoxButton.OK, MessageBoxImage.Error);
                    // Optionally, prompt again
                }
            }
            else
            {
                SetStatusMessage("Folder selection was canceled or invalid.", StatusMessageType.Warning);
            }
        }

        // Method to programmatically invoke folder selection
        private void SelectX4DataFolder()
        {
            SelectX4DataFolder_Click(null, null);
        }

        private void ButtonLoadX4Data_Click(object sender, RoutedEventArgs e)
        {
            if (AllSectors.Count > 0)
            {
                MessageBoxResult result = MessageBox.Show("Are you sure you want to reload the X4 data? Any unsaved changes will be lost.", "Reload X4 Data", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.No)
                {
                    return;
                }
            }
            LoadX4Data();
        }

        private void LoadX4Data()
        {
            AllSectors.Clear();

            Galaxy = X4Galaxy.LoadData(X4DataFolder);
            var sectors = Galaxy.GetSectors();

            foreach (var sector in sectors.Values.OrderBy(s => s.Name))
            {
                var sectorItem = new SectorItem
                {
                    Name = sector.Name,
                    Source = sector.Source,
                    Macro = sector.Macro,
                    Selectable = true
                };
                AllSectors.Add(sectorItem);
            }
            SectorsDirectViewSource.View.Refresh();
            SectorsOppositeViewSource.View.Refresh();
            if (!X4DataVersionOverride && Galaxy.Version != 0 && Galaxy.Version != X4DataVersion)
            {
                X4DataVersion = Galaxy.Version;
            }
            FactionColors.Load(Galaxy.Factions, Galaxy.MappedColors);
            GatesConnectionCurrent?.SetColors(FactionColors);
            SetStatusMessage("X4 data loaded successfully.", StatusMessageType.Info);
        }

        // Filter methods
        private void SectorsDirect_Filter(object sender, FilterEventArgs e)
        {
            if (e.Item is SectorItem sector && sector != null && sector?.Macro != null)
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
                        GatesConnectionCurrent.SectorOppositeExistingConnectionsMacros != null &&
                        GatesConnectionCurrent.SectorOppositeExistingConnectionsMacros.Contains(sector.Macro) ||
                        (
                            GatesConnectionCurrent.SectorOpposite.Macro != null &&
                            GetOppositeSectorsMacrosFromMod(sector.Macro).Contains(GatesConnectionCurrent.SectorOpposite.Macro)
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
            if (e.Item is SectorItem sector && sector != null && sector?.Macro != null)
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
                        GatesConnectionCurrent.SectorDirectExistingConnectionsMacros != null &&
                        GatesConnectionCurrent.SectorDirectExistingConnectionsMacros.Contains(sector.Macro) ||
                        (
                            GatesConnectionCurrent.SectorDirect.Macro != null &&
                            GetOppositeSectorsMacrosFromMod(sector.Macro).Contains(GatesConnectionCurrent.SectorDirect.Macro)
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
            foreach (var connection in GalaxyConnections)
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

        public List<SectorConnectionData> GetSectorConnectionsFromMod(string sectorMacro)
        {
            List<SectorConnectionData> sectorConnections = [];
            foreach (var connection in GalaxyConnections)
            {
                if (connection.Connection?.PathDirect?.Sector?.Macro == null || connection.Connection?.PathOpposite?.Sector?.Macro == null)
                {
                    continue;
                }
                foreach (var modSectorMacro in new string[] { connection.Connection.PathDirect.Sector.Macro, connection.Connection.PathOpposite.Sector.Macro })
                {
                    if (modSectorMacro != sectorMacro)
                    {
                        continue;
                    }
                    bool isDirect = modSectorMacro == connection.Connection.PathDirect.Sector.Macro;
                    SectorConnectionData newConnection = new()
                    {
                        Active = isDirect ? connection.GateDirectActive : connection.GateOppositeActive,
                        ToSector = isDirect ? connection.SectorOppositeName : connection.SectorDirectName,
                        X = isDirect ? connection.GateDirectX : connection.GateOppositeX,
                        Y = isDirect ? connection.GateDirectY : connection.GateOppositeY,
                        Z = isDirect ? connection.GateDirectZ : connection.GateOppositeZ,
                        Id = connection.Connection.Name,
                        From = "mod",
                        Type = "gate"
                    };
                    sectorConnections.Add(newConnection);
                }
            }
            return sectorConnections;
        }

        private void ButtonSectorDirectSelectFromMap_Click(object sender, RoutedEventArgs e)
        {
            GalaxyMapWindow clusterMapWindow = new(this);
            clusterMapWindow.ShowDialog();
            if (clusterMapWindow.SelectedSector != null && GatesConnectionCurrent != null)
            {
                GatesConnectionCurrent.SectorDirect = AllSectors.FirstOrDefault(s => s.Macro == clusterMapWindow.SelectedSector.Macro);
            }
        }

        private void ButtonSectorOppositeSelectFromMap_Click(object sender, RoutedEventArgs e)
        {
            GalaxyMapWindow clusterMapWindow = new(this);
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

        private void SectorDirectMapItem_MouseMove(object sender, MouseEventArgs e){
            if (GatesConnectionCurrent?.SectorDirectMap != null)
            {
                GatesConnectionCurrent.SectorDirectMap.MouseMove(sender, e, GatesConnectionCurrent.GateDirect.Coordinates);
            }
        }

        private void SectorOppositeMapItem_MouseMove(object sender, MouseEventArgs e)
        {
            if (GatesConnectionCurrent?.SectorOppositeMap != null)
            {
                GatesConnectionCurrent.SectorOppositeMap.MouseMove(sender, e, GatesConnectionCurrent.GateOpposite.Coordinates);
            }
        }

        private void SectorDirectMapItem_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.Cursor = Cursors.Arrow;
            if (GatesConnectionCurrent?.SectorDirectMap != null)
            {
                SectorConnectionData? connectionData = GatesConnectionCurrent.SectorDirectMap.MouseLeftButtonUp(sender, e);
                if (connectionData != null)
                {
                    if (GatesConnectionCurrent.SectorDirectSelectedConnection == connectionData)
                    {
                        GatesConnectionCurrent.SectorDirectSelectedConnection = null;
                    }
                    else
                    {
                        GatesConnectionCurrent.SectorDirectSelectedConnection = connectionData;
                    }
                }
            }
        }
        private void SectorOppositeMapItem_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.Cursor = Cursors.Arrow;
            if (GatesConnectionCurrent?.SectorOppositeMap != null)
            {
                SectorConnectionData? connectionData = GatesConnectionCurrent.SectorOppositeMap.MouseLeftButtonUp(sender, e);
                if (connectionData != null)
                {
                    if (GatesConnectionCurrent.SectorOppositeSelectedConnection == connectionData)
                    {
                        GatesConnectionCurrent.SectorOppositeSelectedConnection = null;
                    }
                    else
                    {
                        GatesConnectionCurrent.SectorOppositeSelectedConnection = connectionData;
                    }
                }
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

        public void ButtonSectorMapExpand_Click(object sender, RoutedEventArgs e, bool isDirect)
        {
            if (GatesConnectionCurrent != null)
            {
                Log.Debug($"[ButtonSectorDirectMapExpand_Click] Direct: ");
                SectorMapExpandedWindow sectorMapExpandedWindow = new(SectorRadius, FactionColors)
                {
                    // Set the owner to the main window for proper window behavior
                    Owner = this
                };
                var minSize = Math.Min(this.ActualWidth, this.ActualHeight) * 0.9;
                sectorMapExpandedWindow.Width = minSize;
                sectorMapExpandedWindow.Height = minSize;
                sectorMapExpandedWindow.Left = GetWindowLeft() + (this.ActualWidth - minSize) / 2;
                sectorMapExpandedWindow.Top = GetWindowTop() + (this.ActualHeight - minSize) / 2;
                SectorMap sectorMap = isDirect ? GatesConnectionCurrent.SectorDirectMap : GatesConnectionCurrent.SectorOppositeMap;
                sectorMapExpandedWindow.SetMapItems([.. sectorMap.Items]);
                sectorMapExpandedWindow.SectorMapExpanded.OwnerColor = sectorMap.OwnerColor;
                string sectorName = isDirect ? GatesConnectionCurrent.SectorDirect?.Name ?? "" : GatesConnectionCurrent.SectorOpposite?.Name ?? "";
                sectorMapExpandedWindow.SectorMapExpanded.InternalSizeKm = isDirect ? GatesConnectionCurrent.SectorDirectMap.InternalSizeKm : GatesConnectionCurrent.SectorOppositeMap.InternalSizeKm;
                sectorMapExpandedWindow.Title = $"Map of {sectorName}";
                sectorMapExpandedWindow.ShowDialog(); // Modal
                if (isDirect)
                {
                    GatesConnectionCurrent.SectorDirectMap.InternalSizeKm = sectorMapExpandedWindow.SectorMapExpanded.InternalSizeKm;
                    GatesConnectionCurrent.GateDirect.Coordinates = sectorMapExpandedWindow.NewGateCoordinates;
                }
                else
                {
                    GatesConnectionCurrent.SectorOppositeMap.InternalSizeKm = sectorMapExpandedWindow.SectorMapExpanded.InternalSizeKm;
                    GatesConnectionCurrent.GateOpposite.Coordinates = sectorMapExpandedWindow.NewGateCoordinates;
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
                    SetStatusMessage("Error: Galaxy data is not loaded.", StatusMessageType.Error);
                    return;
                }
                if (GatesConnectionCurrent.SectorDirect == null || GatesConnectionCurrent.SectorOpposite == null)
                {
                    SetStatusMessage("Error: Both sectors must be selected.", StatusMessageType.Error);
                    return;
                }
                Sector? sectorDirect = Galaxy.GetSectorByMacro(GatesConnectionCurrent.SectorDirect.Macro);
                Sector? sectorOpposite = Galaxy.GetSectorByMacro(GatesConnectionCurrent.SectorOpposite.Macro);
                if (sectorDirect == null || sectorOpposite == null)
                {
                    SetStatusMessage("Error: Sectors not found.", StatusMessageType.Error);
                    return;
                }
                string uniqueId = $"{sectorDirect.ClusterId:D3}{sectorDirect.Id:D3}{sectorOpposite.ClusterId:D3}{sectorOpposite.Id:D3}";
                string sectorDirectId = $"cl_{sectorDirect.ClusterId:D3}_sect_{sectorDirect.Id:D3}";
                string sectorOppositeId = $"cl_{sectorOpposite.ClusterId:D3}_sect_{sectorOpposite.Id:D3}";
                string galaxyConnectionId = $"{GalaxyConnectionPrefix}_{sectorDirectId}_to_{sectorOppositeId}";
                string gateDirectId = $"connection_{GalaxyConnectionPrefix}_{sectorDirectId}_to_{sectorOppositeId}";
                string gateOppositeId = $"connection_{GalaxyConnectionPrefix}_{sectorOppositeId}_to_{sectorDirectId}";
                string zoneDirectId = $"Zone_{GalaxyConnectionPrefix}_{uniqueId}_Cluster_{sectorDirect.ClusterId:D2}_Sector_{sectorDirect.Id:D3}";
                string zoneOppositeId = $"Zone_{GalaxyConnectionPrefix}_{uniqueId}_Cluster_{sectorOpposite.ClusterId:D2}_Sector_{sectorOpposite.Id:D3}";
                GateConnection gateDirect = new();
                GateData gateData = GatesConnectionCurrent.GateDirect;
                Coordinates position = gateData.Position;
                Position gatePosition = new(position.X, position.Y, position.Z);
                Coordinates coordinates = gateData.Coordinates;
                Position zonePosition = new(coordinates.X, coordinates.Y, coordinates.Z);
                gateDirect.Create(gateDirectId, gatePosition, gateData.Rotation.ToQuaternion(), new Dictionary<string, string>
                {
                    ["gateMacro"] = gateData.GateMacro,
                    ["isActive"] = gateData.Active ? "true" : "false"
                });
                Zone zoneDirect = new();
                zoneDirect.Create($"{zoneDirectId}_macro", new Dictionary<string, Connection>
                {
                    [gateDirectId] = gateDirect
                }, zonePosition, $"{zoneDirectId}_connection");
                GateConnection gateOpposite = new();
                gateData = GatesConnectionCurrent.GateOpposite;
                position = gateData.Position;
                gatePosition = new Position(position.X, position.Y, position.Z);
                coordinates = gateData.Coordinates;
                zonePosition = new Position(coordinates.X, coordinates.Y, coordinates.Z);
                gateOpposite.Create(gateOppositeId, gatePosition, gateData.Rotation.ToQuaternion(), new Dictionary<string, string>
                {
                    ["gateMacro"] = gateData.GateMacro,
                    ["isActive"] = gateData.Active ? "true" : "false"
                });
                Zone zoneOpposite = new();
                zoneOpposite.Create($"{zoneOppositeId}_macro", new Dictionary<string, Connection>
                {
                    [gateOppositeId] = gateOpposite
                }, zonePosition, $"{zoneOppositeId}_connection");
                Cluster? clusterDirect = Galaxy.GetClusterById(sectorDirect.ClusterId);
                Cluster? clusterOpposite = Galaxy.GetClusterById(sectorOpposite.ClusterId);
                if (clusterDirect == null || clusterOpposite == null)
                {
                    SetStatusMessage("Error: Clusters not found.", StatusMessageType.Error);
                    return;
                }
                GalaxyConnection galaxyConnection = new();
                galaxyConnection.Create(galaxyConnectionId, clusterDirect, sectorDirect, zoneDirect, gateDirect, clusterOpposite, sectorOpposite, zoneOpposite, gateOpposite);
                if (CurrentGalaxyConnection != null)
                {
                    CurrentGalaxyConnection.Update(galaxyConnection, GatesConnectionCurrent);
                    GatesConnectionCurrent.SetDefaultsFromReference(CurrentGalaxyConnection, AllSectors);
                    GatesConnectionCurrent.Reset();
                }
                else
                {
                    GalaxyConnectionData newConnection = new(galaxyConnection, GatesConnectionCurrent);
                    GalaxyConnections.Add(newConnection);
                    CurrentGalaxyConnection = newConnection;
                }
            }
        }

        public void ButtonReset_Click(object sender, RoutedEventArgs e)
        {
            if (GatesConnectionCurrent != null)
            {
                GatesConnectionCurrent.Reset();
                SectorsDirectViewSource.View.Refresh();
                SectorsOppositeViewSource.View.Refresh();
            }
        }

        public void ButtonGateNew_Click(object sender, RoutedEventArgs e)
        {
            if (GatesConnectionCurrent != null)
            {
                GatesConnectionCurrent.ResetToInitial(GatesActiveByDefault, _gateMacroDefault);
                CurrentGalaxyConnection = null;
                SectorsDirectViewSource.View.Refresh();
                SectorsOppositeViewSource.View.Refresh();
            }
        }

        public void ButtonGateDelete_Click(object sender, RoutedEventArgs e)
        {
            if (GatesConnectionCurrent != null)
            {
                if (CurrentGalaxyConnection != null)
                {
                    int index = GalaxyConnections.IndexOf(CurrentGalaxyConnection);
                    GalaxyConnections.Remove(CurrentGalaxyConnection);
                    if (GalaxyConnections.Count > 0)
                    {
                        if (index >= GalaxyConnections.Count)
                        {
                            index = GalaxyConnections.Count - 1;
                        }
                        CurrentGalaxyConnection = GalaxyConnections[index];
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
            ChemGateKeeperMod = new();
            _chemGateKeeperMod.SetGameVersion(X4DataVersion);
            GalaxyConnections.Clear();
            GatesConnectionCurrent?.ResetToInitial(GatesActiveByDefault, _gateMacroDefault);
            SectorsDirectViewSource.View.Refresh();
            SectorsOppositeViewSource.View.Refresh();
            CurrentGalaxyConnection = null;
            IsModCanBeSaved = false;
        }

        public void ButtonLoadMod_Click(object sender, RoutedEventArgs e)
        {
            if (Galaxy == null)
            {
                SetStatusMessage("Error: Galaxy data is not loaded.", StatusMessageType.Error);
                return;
            }
            if (ChemGateKeeperMod.LoadData(Galaxy))
            {
                GalaxyConnections.Clear();
                foreach (var connection in ChemGateKeeperMod.Connections)
                {
                    GalaxyConnectionData newConnection = new(connection);
                    GalaxyConnections.Add(newConnection);
                }
                SectorsDirectViewSource.View.Refresh();
                SectorsOppositeViewSource.View.Refresh();
                if (GalaxyConnections.Count > 0)
                {
                    CurrentGalaxyConnection = GalaxyConnections[0];
                } else
                {
                    CurrentGalaxyConnection = null;
                }
                SetStatusMessage("Mod data loaded successfully.", StatusMessageType.Info);
            }
            else {
                SetStatusMessage("Error: Mod data could not be loaded.", StatusMessageType.Error);
                Log.Warn("Mod data could not be loaded.");
            }
        }

        public void ButtonSaveMod_Click(object sender, RoutedEventArgs e)
        {
            if (GalaxyConnections.Count > 0)
            {
                ChemGateKeeperMod.SaveData(GalaxyConnections);
                IsModCanBeSaved = false;
            }
        }


        public void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        public double GetWindowLeft()
        {
            if (WindowState == WindowState.Maximized)
            {
                var leftField = typeof(Window).GetField("_actualLeft", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (leftField?.GetValue(this) is double value)
                {
                    return value;
                }
                throw new InvalidOperationException("_actualLeft field is null or not a double.");
            }
            else
                return Left;
        }
        public double GetWindowTop()
        {
            if (WindowState == WindowState.Maximized)
            {
                var topField = typeof(Window).GetField("_actualTop", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (topField?.GetValue(this) is double value)
                {
                    return value;
                }
                throw new InvalidOperationException("_actualTop field is null or not a double.");
            }
            else
                return Top;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum StatusMessageType
    {
        Info,
        Warning,
        Error
    }
}
