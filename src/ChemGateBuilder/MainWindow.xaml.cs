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

        public event PropertyChangedEventHandler PropertyChanged;

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
        private string _x4DataFolder;
        private bool _gatesActiveByDefault;
        private int _gatesMinimalDistanceBetween;
        private int _sectorRadius = 400;
        private string _logLevel;
        private bool _logToFile;
        private string _statusMessage;
        private string _gateMacroDefault = "props_gates_anc_gate_macro";
        public static Logger _logger = LogManager.GetCurrentClassLogger();
        public static string GalaxyConnectionPrefix = "Chem_Gate";
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
        private GalaxyConnectionData _currentGalaxyConnection;
        public GalaxyConnectionData CurrentGalaxyConnection
        {
            get => _currentGalaxyConnection;
            set
            {
                if (_currentGalaxyConnection != value)
                {
                    _currentGalaxyConnection = value;
                    OnPropertyChanged(nameof(CurrentGalaxyConnection));
                }
            }
        }
        public ObservableCollection<GalaxyConnectionData> GalaxyConnections { get; } = new ObservableCollection<GalaxyConnectionData>();

        // Master sector list
        public ObservableCollection<SectorItem> AllSectors { get; } = new ObservableCollection<SectorItem>();

        // CollectionViewSources for filtering
        public CollectionViewSource SectorsDirectViewSource { get; } = new CollectionViewSource();
        public CollectionViewSource SectorsOppositeViewSource { get; } = new CollectionViewSource();

        public ObservableCollection<string> GateMacros { get; } = new ObservableCollection<string>();
        // GatesConnectionCurrent Property
        private GatesConnectionData _gatesConnectionCurrent;
        public GatesConnectionData GatesConnectionCurrent
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

        public bool GatesActiveByDefault
        {
            get => _gatesActiveByDefault;
            set
            {
                if (_gatesActiveByDefault != value && value != null)
                {
                    _gatesActiveByDefault = value;
                    OnPropertyChanged(nameof(GatesActiveByDefault));
                    if (GatesConnectionCurrent != null) GatesConnectionCurrent.SetDefaults(value);
                    SaveConfiguration();
                }
            }
        }

        public int GatesMinimalDistanceBetween
        {
            get => _gatesMinimalDistanceBetween;
            set
            {
                if (_gatesMinimalDistanceBetween != value && value != null)
                {
                    _gatesMinimalDistanceBetween = value;
                    OnPropertyChanged(nameof(GatesMinimalDistanceBetween));
                    SaveConfiguration();
                }
            }
        }

        public int SectorRadius
        {
            get => _sectorRadius;
            set
            {
                if (_sectorRadius != value && value != null)
                {
                    _sectorRadius = value;
                    OnPropertyChanged(nameof(SectorRadius));
                    SaveConfiguration();
                    if (value != null && value > 0 && GatesConnectionCurrent != null)
                    {
                        GatesConnectionCurrent.SetSectorMapInternalSize(value);
                    }
                }
            }
        }
        public int SectorRadiusNegative => -SectorRadius;
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

        public bool LogToFile
        {
            get => _logToFile;
            set
            {
                if (_logToFile != value && value != null)
                {
                    _logToFile = value;
                    OnPropertyChanged(nameof(LogToFile));
                    SaveConfiguration();
                }
            }
        }

        private void HandleValidationError(string message)
        {
            // Optional: Log the validation message for debugging
            // _logger.LogError($"Validation Error: {message}");

            StatusMessage = message;
        }

        // Galaxy and Sectors
        public Galaxy Galaxy { get; private set; }

        public bool IsDataLoaded => AllSectors.Count > 0;
        public bool IsGateCanBeDeleted => IsDataLoaded & GalaxyConnections.Count > 0 && CurrentGalaxyConnection != null;
        public bool IsGateCanBeCreated => IsDataLoaded & GalaxyConnections.Count > 0 && CurrentGalaxyConnection != null;
        public bool IsModCanBeSaved => IsDataLoaded & GalaxyConnections.Count > 0;
        public bool IsModCanBeCreated => IsDataLoaded & GalaxyConnections.Count > 0;
        // Constructor
        public MainWindow()
        {
            _logger = LogManager.GetCurrentClassLogger();
            _configFileName = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.json";
            GatesConnectionCurrent = new GatesConnectionData(GatesActiveByDefault, _gateMacroDefault);
            LoadConfiguration();
            InitializeComponent();
            DataContext = this;
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

        private void SelectX4DataFolder_Click(object sender, RoutedEventArgs e)
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
            if (e.Item is SectorItem sector)
            {
                sector.Selectable = true;
                // Exclude the sector selected in Opposite ComboBox
                if (GatesConnectionCurrent.SectorOpposite != null)
                {
                    if (sector.Macro == GatesConnectionCurrent.SectorOpposite.Macro)
                    {
                        sector.Selectable = false;
                    }
                    else if (GatesConnectionCurrent.SectorOppositeExistingConnectionsMacros != null && GatesConnectionCurrent.SectorOppositeExistingConnectionsMacros.Contains(sector.Macro))
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
            if (e.Item is SectorItem sector)
            {
                sector.Selectable = true;
                // Exclude the sector selected in Direct ComboBox
                if (GatesConnectionCurrent.SectorDirect != null)
                {
                    if (sector.Macro == GatesConnectionCurrent.SectorDirect.Macro)
                    {
                        sector.Selectable = false;
                    }
                    else if (GatesConnectionCurrent.SectorDirectExistingConnectionsMacros != null && GatesConnectionCurrent.SectorDirectExistingConnectionsMacros.Contains(sector.Macro))
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
            SectorMapItem_MouseLeftButtonDown(sender, e, GatesConnectionCurrent.SectorDirectMap);
        }
        private void SectorOppositeMapItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SectorMapItem_MouseLeftButtonDown(sender, e, GatesConnectionCurrent.SectorOppositeMap);
        }
        private void SectorMapItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e, SectorMap sectorMap)
        {
            if (sender is Ellipse ellipse && ellipse.DataContext is SectorMapItem item)
            {
                sectorMap.SelectedItem = item;
                if (item.IsNew) // Only allow dragging for "new" gates
                {
                    sectorMap.IsDragging = true;

                    // Get the mouse position relative to the gate
                    sectorMap.MouseOffset = e.GetPosition(ellipse);

                    // Capture the mouse to receive MouseMove events even if the cursor leaves the ellipse
                    ellipse.CaptureMouse();
                }
                _logger.Debug($"[MouseLeftButtonDown] Direct: {sectorMap == GatesConnectionCurrent.SectorDirectMap}, Selected Item: {sectorMap.SelectedItem?.ConnectionData.Id}, IsDragging: {sectorMap.IsDragging}, MouseOffset: {sectorMap.MouseOffset}");
            }
        }

        private void SectorDirectMapItem_MouseMove(object sender, MouseEventArgs e){
            SectorMapItem_MouseMove(sender, e, GatesConnectionCurrent.SectorDirectMap);
        }
        private void SectorOppositeMapItem_MouseMove(object sender, MouseEventArgs e)
        {
            SectorMapItem_MouseMove(sender, e, GatesConnectionCurrent.SectorOppositeMap);
        }
        private void SectorMapItem_MouseMove(object sender, MouseEventArgs e, SectorMap sectorMap)
        {
            if (sectorMap.IsDragging && sectorMap.SelectedItem != null)
            {
                _logger.Debug($"[MouseMove] Direct: {sectorMap == GatesConnectionCurrent.SectorDirectMap}, Selected Item: {sectorMap.SelectedItem?.ConnectionData.Id}, IsDragging: {sectorMap.IsDragging}, MouseOffset: {sectorMap.MouseOffset}, sender: {sender}, isEllipse: {sender is Ellipse}, ellipse.Parent: {((Ellipse)sender).Parent}");
                if (sender is Ellipse ellipse)
                {
                    // Get the current mouse position relative to the SectorCanvas
                    Point mousePosition = e.GetPosition(ellipse);
                    if (sectorMap == GatesConnectionCurrent.SectorDirectMap)
                    {
                        mousePosition = e.GetPosition(SectorDirectCanvas);
                    }
                    else
                    {
                        mousePosition = e.GetPosition(SectorOppositeCanvas);
                    }
                    // Calculate new position by subtracting the offset
                    double newX = mousePosition.X - sectorMap.MouseOffset.X;
                    double newY = mousePosition.Y - sectorMap.MouseOffset.Y;
                    // Account the size of the item
                    Point newPoint = new Point(newX + sectorMap.SelectedItem.ItemSizePx / 2 , newY + sectorMap.SelectedItem.ItemSizePx / 2);
                    // Check if the new position is inside the hexagon
                    bool isInside = false;
                    if (sectorMap == GatesConnectionCurrent.SectorDirectMap)
                    {
                        isInside = SectorDirectHexagon.RenderedGeometry.FillContains(newPoint);
                    }
                    else
                    {
                        isInside = SectorOppositeHexagon.RenderedGeometry.FillContains(newPoint);
                    }
                    _logger.Debug($"[MouseMove] IsInside: {isInside}");
                    if (isInside)
                    {
                        // Update the SectorMapItem's coordinates
                        sectorMap.SelectedItem.X = newX;
                        sectorMap.SelectedItem.Y = newY;
                        if (sectorMap == GatesConnectionCurrent.SectorDirectMap)
                        {
                            sectorMap.SelectedItem.UpdateInternalCoordinates(GatesConnectionCurrent.GateDirect.Coordinates);
                        }
                        else
                        {
                            sectorMap.SelectedItem.UpdateInternalCoordinates(GatesConnectionCurrent.GateOpposite.Coordinates);
                        }
                        _logger.Debug($"[MouseMove] New X: {newX}, New Y: {newY}");
                    }
                }
            }
        }

        private void SectorDirectMapItem_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            SectorMapItem_MouseLeftButtonUp(sender, e, GatesConnectionCurrent.SectorDirectMap, GatesConnectionCurrent, true);
        }
        private void SectorOppositeMapItem_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            SectorMapItem_MouseLeftButtonUp(sender, e, GatesConnectionCurrent.SectorOppositeMap, GatesConnectionCurrent, false);
        }
        private void SectorMapItem_MouseLeftButtonUp(object sender, MouseButtonEventArgs e, SectorMap sectorMap, GatesConnectionData connectionData, bool isDirect)
        {
            if (sectorMap.SelectedItem != null)
            {
                sectorMap.IsDragging = false;
                sectorMap.SelectedItem = null;

                if (sender is Ellipse ellipse && ellipse.DataContext is SectorMapItem item)
                {
                    ellipse.ReleaseMouseCapture();
                    if (connectionData != null)
                    {
                        if (isDirect)
                        {
                            if (connectionData.SectorDirectSelectedConnection == item.ConnectionData)
                            {
                                connectionData.SectorDirectSelectedConnection = null;
                            }
                            else if (!item.IsNew)
                            {
                                connectionData.SectorDirectSelectedConnection = item.ConnectionData;
                            }
                        }
                        else
                        {
                            if (connectionData.SectorOppositeSelectedConnection == item.ConnectionData)
                            {
                                connectionData.SectorOppositeSelectedConnection = null;
                            }
                            else if (!item.IsNew)
                            {
                                connectionData.SectorOppositeSelectedConnection = item.ConnectionData;
                            }
                        }
                    }
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
                _logger.Debug($"[ButtonSave_Click] GatesConnectionCurrent: {GatesConnectionCurrent}");
                Sector sectorDirect = Galaxy.GetSectorByMacro(GatesConnectionCurrent.SectorDirect.Macro);
                Sector sectorOpposite = Galaxy.GetSectorByMacro(GatesConnectionCurrent.SectorOpposite.Macro);
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
                Cluster clusterDirect = Galaxy.GetClusterById(sectorDirect.ClusterId);
                Cluster clusterOpposite = Galaxy.GetClusterById(sectorOpposite.ClusterId);
                GalaxyConnection galaxyConnection = new GalaxyConnection();
                galaxyConnection.Create(galaxyConnectionId, clusterDirect, sectorDirect, zoneDirect, gateDirect, clusterOpposite, sectorOpposite, zoneOpposite, gateOpposite);
                if (CurrentGalaxyConnection != null)
                {
                    CurrentGalaxyConnection.Update(galaxyConnection, GatesConnectionCurrent);
                }
                else
                {
                    GalaxyConnectionData newConnection = new GalaxyConnectionData(galaxyConnection, GatesConnectionCurrent);
                    GalaxyConnections.Add(newConnection);
                    CurrentGalaxyConnection = newConnection;
                }
                GatesConnectionCurrent.SetDefaultsFromReference(CurrentGalaxyConnection);
                GatesConnectionCurrent.Reset();
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}