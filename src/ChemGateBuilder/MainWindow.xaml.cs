using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Data;
using X4DataLoader;

namespace ChemGateBuilder
{
    public class AppConfig
    {
        public EditConfig Edit { get; set; } = new EditConfig();
        public DataConfig Data { get; set; } = new DataConfig();
        public LoggingConfig Logging { get; set; } = new LoggingConfig();

        public GateData GateDataDirect { get; set; } = new GateData();
    }

    public class EditConfig
    {
        public bool GatesActiveByDefault { get; set; } = true;
        public int GatesMinimalDistanceBetween { get; set; } = 10;
    }

    public class DataConfig
    {
        public string X4DataExtractedPath { get; set; } = ".";
    }

    public class LoggingConfig
    {
        public string LogLevel { get; set; } = "Warning";
        public bool LogToFile { get; set; } = false;
    }

    public class SectorItem
    {
        public string Name { get; set; }
        public string Source { get; set; }
        public string Macro { get; set; }
        public bool Selectable { get; set; }
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
        private string _logLevel;
        private bool _logToFile;
        private string _statusMessage;
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
        // Master sector list
        public ObservableCollection<SectorItem> AllSectors { get; } = new ObservableCollection<SectorItem>();

        // CollectionViewSources for filtering
        public CollectionViewSource SectorsDirectViewSource { get; } = new CollectionViewSource();
        public CollectionViewSource SectorsOppositeViewSource { get; } = new CollectionViewSource();

        // Selected sectors
        private SectorItem _selectedSectorDirect;
        public SectorItem SelectedSectorDirect
        {
            get => _selectedSectorDirect;
            set
            {
                if (_selectedSectorDirect != value)
                {
                    _selectedSectorDirect = value;
                    OnPropertyChanged(nameof(SelectedSectorDirect));
                    SectorsOppositeViewSource.View.Refresh();
                }
            }
        }

        private SectorItem _selectedSectorOpposite;
        public SectorItem SelectedSectorOpposite
        {
            get => _selectedSectorOpposite;
            set
            {
                if (_selectedSectorOpposite != value)
                {
                    _selectedSectorOpposite = value;
                    OnPropertyChanged(nameof(SelectedSectorOpposite));
                    SectorsDirectViewSource.View.Refresh();
                }
            }
        }

        // GateData Property
        private GateData _gateDataDirect = new GateData();
        public GateData GateDataDirect
        {
            get => _gateDataDirect;
            set
            {
                if (_gateDataDirect != value)
                {
                    _gateDataDirect = value;
                    OnPropertyChanged(nameof(GateDataDirect));
                }
            }
        }

        private GateData _gateDataOpposite = new GateData();
        public GateData GateDataOpposite
        {
            get => _gateDataOpposite;
            set
            {
                if (_gateDataOpposite != value)
                {
                    _gateDataOpposite = value;
                    OnPropertyChanged(nameof(GateDataOpposite));
                }
            }
        }
        // Other properties
        public string X4DataFolder
        {
            get => _x4DataFolder;
            set
            {
                _x4DataFolder = value;
                OnPropertyChanged(nameof(X4DataFolder));
                OnPropertyChanged(nameof(X4DataFolderStatus));
                SaveConfiguration();
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
                _gatesActiveByDefault = value;
                OnPropertyChanged(nameof(GatesActiveByDefault));
                SaveConfiguration();
            }
        }

        public int GatesMinimalDistanceBetween
        {
            get => _gatesMinimalDistanceBetween;
            set
            {
                _gatesMinimalDistanceBetween = value;
                OnPropertyChanged(nameof(GatesMinimalDistanceBetween));
                SaveConfiguration();
            }
        }

        public string LogLevel
        {
            get => _logLevel;
            set
            {
                _logLevel = value;
                OnPropertyChanged(nameof(LogLevel));
                SaveConfiguration();
            }
        }

        public bool LogToFile
        {
            get => _logToFile;
            set
            {
                _logToFile = value;
                OnPropertyChanged(nameof(LogToFile));
                SaveConfiguration();
            }
        }

        private void HandleValidationError(string message)
        {
            // Optional: Log the validation message for debugging
            System.Diagnostics.Debug.WriteLine($"Validation Error: {message}");

            StatusMessage = message;
        }

        // Galaxy and Sectors
        public Galaxy Galaxy { get; private set; }

        // Constructor
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            _configFileName = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.json";
            LoadConfiguration();

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
            }

            // Optionally, set default selections
            if (AllSectors.Count > 0)
            {
                SelectedSectorDirect = null;
            }
            if (AllSectors.Count > 1)
            {
                SelectedSectorOpposite = null;
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
                    GatesMinimalDistanceBetween = GatesMinimalDistanceBetween
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
            string subfolderPath = Path.Combine(folderPath, "t");
            string filePath = Path.Combine(subfolderPath, "0001-l044.xml");

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

            StatusMessage = "Sectors loaded successfully.";
        }

        // Filter methods
        private void SectorsDirect_Filter(object sender, FilterEventArgs e)
        {
            if (e.Item is SectorItem sector)
            {
                // Exclude the sector selected in Opposite ComboBox
                if (SelectedSectorOpposite != null && sector.Macro == SelectedSectorOpposite.Macro)
                {

                    sector.Selectable = false;
                    // e.Accepted = false;
                }
                else
                {
                    sector.Selectable = true;
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
                // Exclude the sector selected in Direct ComboBox
                if (SelectedSectorDirect != null && sector.Macro == SelectedSectorDirect.Macro)
                {
                    sector.Selectable = false;
                    // e.Accepted = false;
                }
                else
                {
                    sector.Selectable = true;
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}