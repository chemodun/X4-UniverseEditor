using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using X4DataLoader;

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

        public Galaxy Galaxy { get; private set; }
        public List<SectorItem> SectorsDirect { get; private set; }
        public List<SectorItem> SectorsOpposite { get; private set; }

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

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            _configFileName = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.json";
            LoadConfiguration();

            // Validate the loaded X4DataFolder
            if (!ValidateX4DataFolder(X4DataFolder, out string errorMessage))
            {
                MessageBox.Show(errorMessage, "Invalid X4 Data Folder", MessageBoxButton.OK, MessageBoxImage.Warning);
                // Prompt the user to select a valid folder
                SelectX4DataFolder();
            }
            if (ValidateX4DataFolder(X4DataFolder, out errorMessage))
            {
                Galaxy = X4Galaxy.LoadData(X4DataFolder);
                LoadSectors();
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
                }
                else
                {
                    MessageBox.Show(errorMessage, "Invalid Folder", MessageBoxButton.OK, MessageBoxImage.Error);
                    // Optionally, you can prompt the user again or handle as needed
                }
            }
            else
            {
                MessageBox.Show("Folder selection was canceled or invalid.", "Canceled", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void LoadSectors()
        {
            SectorsDirect = new List<SectorItem>();
            SectorsOpposite = new List<SectorItem>();
            var sectors = Galaxy.GetSectors();
            foreach (var sector in sectors.Values)
            {
                var sectorItem = new SectorItem
                {
                    Name = sector.Name,
                    Source = sector.Source,
                    Macro = sector.Macro
                };
                SectorsDirect.Add(sectorItem);
                SectorsOpposite.Add(sectorItem);
            }
            SectorsDirect = SectorsDirect.OrderBy(s => s.Name).ToList();
            SectorsOpposite = SectorsOpposite.OrderBy(s => s.Name).ToList();
            OnPropertyChanged(nameof(SectorsDirect));
            OnPropertyChanged(nameof(SectorsOpposite));
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