using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
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
        public DataConfig Data { get; set; } = new DataConfig();
        public LoggingConfig Logging { get; set; } = new LoggingConfig();

    }

    public class EditConfig
    {
        public bool GatesActiveByDefault { get; set; } = true;
        public int GatesMinimalDistanceBetween { get; set; } = 10;
        public int SectorRadius { get; set; } = 400;
    }

    public class DataConfig
    {
        public string X4DataExtractedPath { get; set; } = ".";
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
        private string _configFileName;

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
                    if (GatesConnectionCurrent != null) GatesConnectionCurrent.SetGateStatusDefaults(value);
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
                }
            }
        }

        private string _gateMacroDefault = "props_gates_anc_gate_macro";
        private static string GalaxyConnectionPrefix = "Chem_Gate";

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
                        GatesConnectionCurrent.SetDefaultsFromReference(value);
                        GatesConnectionCurrent.Reset();
                    }
                }
            }
        }

        public ObservableCollection<GalaxyConnectionData> GalaxyConnections { get; } = new ObservableCollection<GalaxyConnectionData>();

        private ChemGateKeeper _mod = new ChemGateKeeper();

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
        public ObservableCollection<SectorItem> AllSectors { get; } = new ObservableCollection<SectorItem>();

        // CollectionViewSources for filtering
        public CollectionViewSource SectorsDirectViewSource { get; } = new CollectionViewSource();
        public CollectionViewSource SectorsOppositeViewSource { get; } = new CollectionViewSource();

        public ObservableCollection<string> GateMacros { get; } = new ObservableCollection<string>();
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

            StatusMessage = message;
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
                if (value && IsGateCanBeDeleted)
                {
                    IsModCanBeSaved = _mod.IsModChanged(GalaxyConnections);
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


        // Constructor
        public MainWindow()
        {
            _configFileName = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.json";
            GatesConnectionCurrent = new GatesConnectionData(GatesActiveByDefault, _gateMacroDefault);
            LoadConfiguration();
            InitializeComponent();
            DataContext = this;
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
                StatusMessage = errorMessage;
                // Prompt the user to select a valid folder
                SelectX4DataFolder();
            }

            // Load sectors if the folder is valid
            if (ValidateX4DataFolder(X4DataFolder, out errorMessage))
            {
                StatusMessage = "X4 Data folder validated successfully.";
                LoadSectors();
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
                    GatesActiveByDefault = config.Edit.GatesActiveByDefault;
                    GatesMinimalDistanceBetween = config.Edit.GatesMinimalDistanceBetween;
                    SectorRadius = config.Edit.SectorRadius;
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

        private void SaveConfiguration()
        {
            var config = new AppConfig
            {
                Data = new DataConfig { X4DataExtractedPath = X4DataFolder },
                Edit = new EditConfig
                {
                    GatesActiveByDefault = GatesActiveByDefault,
                    GatesMinimalDistanceBetween = GatesMinimalDistanceBetween,
                    SectorRadius = SectorRadius
                },
                Logging = new LoggingConfig
                {
                    LogLevel = LogLevel,
                    LogToFile = LogToFile
                }
            };

            var jsonString = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_configFileName, jsonString);
        }

        private bool ValidateX4DataFolder(string folderPath, out string errorMessage)
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
                    LoadSectors();
                    StatusMessage = "X4 Data folder set successfully.";
                }
                else
                {
                    StatusMessage = errorMessage;
                    MessageBox.Show(errorMessage, "Invalid Folder", MessageBoxButton.OK, MessageBoxImage.Error);
                    // Optionally, prompt again
                }
            }
            else
            {
                StatusMessage = "Folder selection was canceled or invalid.";
            }
        }

        private void LoadSectors()
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

            StatusMessage = "X4 data loaded successfully.";
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

        // Method to programmatically invoke folder selection
        private void SelectX4DataFolder()
        {
            SelectX4DataFolder_Click(null, null);
        }

        private List<string> GetOppositeSectorsMacrosFromMod(string sectorMacro)
        {
            List<string> oppositeSectorsMacros = new List<string>();
            foreach (var connection in GalaxyConnections)
            {
                if (connection.Connection?.PathDirect?.Sector?.Macro == null || connection.Connection?.PathOpposite?.Sector?.Macro == null)
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
            List<SectorConnectionData> sectorConnections = new List<SectorConnectionData>();
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

        // Handle Canvas Size Changed to adjust Hexagon Size
        private void SectorDirectCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double newSize = Math.Min(e.NewSize.Width, e.NewSize.Height) ;//* 0.8; // 80% of smaller dimension
            if (GatesConnectionCurrent != null)
            {
                GatesConnectionCurrent.SectorDirectMap.OnSizeChanged(newSize);
            }
        }

        private void SectorOppositeCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double newSize = Math.Min(e.NewSize.Width, e.NewSize.Height) ;//* 0.8; // 80% of smaller dimension
            if (GatesConnectionCurrent != null)
            {
                GatesConnectionCurrent.SectorOppositeMap.OnSizeChanged(newSize);
            }
        }

        private void SectorDirectMapItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (GatesConnectionCurrent?.SectorDirectMap != null)
            {
                GatesConnectionCurrent.SectorDirectMap.MouseLeftButtonDown(sender, e);
            }
        }
        private void SectorOppositeMapItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (GatesConnectionCurrent?.SectorOppositeMap != null)
            {
                GatesConnectionCurrent.SectorOppositeMap.MouseLeftButtonDown(sender, e);
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
                SectorMapExpandedWindow sectorMapExpandedWindow = new SectorMapExpandedWindow(SectorRadius);

                // Set the owner to the main window for proper window behavior
                sectorMapExpandedWindow.Owner = this;



                // Center the new window horizontally relative to the main window
                double mainWindowLeft = this.Left;
                double mainWindowTop = this.Top;
                double mainWindowWidth = this.ActualWidth;
                double mainWindowHeight = this.ActualHeight;

                sectorMapExpandedWindow.Left = mainWindowLeft + mainWindowWidth * 0.05;
                sectorMapExpandedWindow.Top = mainWindowTop + mainWindowHeight * 0.05;
                sectorMapExpandedWindow.Width = mainWindowWidth * 0.9;
                sectorMapExpandedWindow.Height = mainWindowHeight * 0.9;
                SectorMap sectorMap = isDirect ? GatesConnectionCurrent.SectorDirectMap : GatesConnectionCurrent.SectorOppositeMap;
                sectorMapExpandedWindow.SetMapItems(sectorMap.Items.ToList());
                string sectorName = isDirect ? GatesConnectionCurrent.SectorDirect?.Name ?? "" : GatesConnectionCurrent.SectorOpposite?.Name ?? "";
                sectorMapExpandedWindow.Title = $"Map of {sectorName}";
                sectorMapExpandedWindow.ShowDialog(); // Modal
                if (isDirect)
                {
                    GatesConnectionCurrent.GateDirect.Coordinates = sectorMapExpandedWindow.NewGateCoordinates;
                }
                else
                {
                    GatesConnectionCurrent.GateOpposite.Coordinates = sectorMapExpandedWindow.NewGateCoordinates;
                }
            }
        }

        public void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        public void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            if (GatesConnectionCurrent != null)
            {
                // GatesConnectionCurrent.Save();
                Log.Debug($"[ButtonSave_Click] GatesConnectionCurrent: {GatesConnectionCurrent}");
                if (Galaxy == null)
                {
                    StatusMessage = "Error: Galaxy data is not loaded.";
                    return;
                }
                if (GatesConnectionCurrent.SectorDirect == null || GatesConnectionCurrent.SectorOpposite == null)
                {
                    StatusMessage = "Error: Both sectors must be selected.";
                    return;
                }
                Sector? sectorDirect = Galaxy.GetSectorByMacro(GatesConnectionCurrent.SectorDirect.Macro);
                Sector? sectorOpposite = Galaxy.GetSectorByMacro(GatesConnectionCurrent.SectorOpposite.Macro);
                if (sectorDirect == null || sectorOpposite == null)
                {
                    StatusMessage = "Error: Sectors not found.";
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
                GateConnection gateDirect = new GateConnection();
                GateData gateData = GatesConnectionCurrent.GateDirect;
                Coordinates position = gateData.Position;
                Position gatePosition = new Position(position.X, position.Y, position.Z);
                Coordinates coordinates = gateData.Coordinates;
                Position zonePosition = new Position(coordinates.X, coordinates.Y, coordinates.Z);
                gateDirect.Create(gateDirectId, gatePosition, gateData.Rotation.ToQuaternion(), new Dictionary<string, string>
                {
                    ["gateMacro"] = gateData.GateMacro,
                    ["isActive"] = gateData.Active ? "true" : "false"
                });
                Zone zoneDirect = new Zone();
                zoneDirect.Create($"{zoneDirectId}_macro", new Dictionary<string, Connection>
                {
                    [gateDirectId] = gateDirect
                }, zonePosition, $"{zoneDirectId}_connection");
                GateConnection gateOpposite = new GateConnection();
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
                Zone zoneOpposite = new Zone();
                zoneOpposite.Create($"{zoneOppositeId}_macro", new Dictionary<string, Connection>
                {
                    [gateOppositeId] = gateOpposite
                }, zonePosition, $"{zoneOppositeId}_connection");
                Cluster? clusterDirect = Galaxy.GetClusterById(sectorDirect.ClusterId);
                Cluster? clusterOpposite = Galaxy.GetClusterById(sectorOpposite.ClusterId);
                if (clusterDirect == null || clusterOpposite == null)
                {
                    StatusMessage = "Error: Clusters not found.";
                    return;
                }
                GalaxyConnection galaxyConnection = new GalaxyConnection();
                galaxyConnection.Create(galaxyConnectionId, clusterDirect, sectorDirect, zoneDirect, gateDirect, clusterOpposite, sectorOpposite, zoneOpposite, gateOpposite);
                if (CurrentGalaxyConnection != null)
                {
                    CurrentGalaxyConnection.Update(galaxyConnection, GatesConnectionCurrent);
                    GatesConnectionCurrent.SetDefaultsFromReference(CurrentGalaxyConnection);
                    GatesConnectionCurrent.Reset();
                }
                else
                {
                    GalaxyConnectionData newConnection = new GalaxyConnectionData(galaxyConnection, GatesConnectionCurrent);
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

        public void ButtonLoadMod_Click(object sender, RoutedEventArgs e)
        {
            if (Galaxy == null)
            {
                StatusMessage = "Error: Galaxy data is not loaded.";
                return;
            }
            GalaxyConnections.Clear();
            _mod.LoadData(Galaxy, GalaxyConnections);
            if (GalaxyConnections.Count > 0)
            {
                CurrentGalaxyConnection = GalaxyConnections[0];
            }
        }

        public void ButtonSaveMod_Click(object sender, RoutedEventArgs e)
        {
            if (GalaxyConnections.Count > 0)
            {
                _mod.SaveData(GalaxyConnections);
                IsModCanBeSaved = false;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}