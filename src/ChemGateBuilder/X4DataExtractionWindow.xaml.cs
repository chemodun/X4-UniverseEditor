using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Xml.Linq;
using Utilities.Logging;
using X4DataLoader;
using X4Unpack;

namespace ChemGateBuilder
{
  public partial class X4DataExtractionWindow : Window, INotifyPropertyChanged
  {
    private MainWindow? MainWindowReference = null;

    private readonly string X4Executable = "X4.exe";
    private string _extractedDataLocationFolder = string.Empty;
    public string ExtractedDataLocationFolder
    {
      get => _extractedDataLocationFolder;
      set
      {
        if (_extractedDataLocationFolder != value)
        {
          _extractedDataLocationFolder = value;
          OnPropertyChanged(nameof(ExtractedDataLocationFolder));
          ExtractionIsPossibleCheck();
        }
      }
    }

    private string _gameFolder = string.Empty;
    public string GameFolder
    {
      get => _gameFolder;
      set
      {
        if (_gameFolder != value)
        {
          _gameFolder = value;
          OnPropertyChanged(nameof(GameFolder));
          ExtractionIsPossibleCheck();
          if (!string.IsNullOrEmpty(_gameFolder) && Directory.Exists(_gameFolder))
          {
            if (MainWindowReference != null)
            {
              MainWindowReference.X4GameFolder = _gameFolder;
            }
          }
        }
      }
    }

    private string _gameVersion = string.Empty;
    public string GameVersion
    {
      get => _gameVersion;
      set
      {
        if (_gameVersion != value)
        {
          _gameVersion = value;
          OnPropertyChanged(nameof(GameVersion));
          OnPropertyChanged(nameof(GameInfo));
        }
      }
    }
    public string GameInfo
    {
      get => $"X4: Foundations {GameVersion}";
    }

    private string _dataFolder = string.Empty;
    public string DataFolder
    {
      get => _dataFolder;
      set
      {
        if (_dataFolder != value)
        {
          _dataFolder = value;
          OnPropertyChanged(nameof(DataFolder));
          OnPropertyChanged(nameof(DataFolderOptionsHeader));
        }
      }
    }
    public string DataFolderOptionsHeader
    {
      get => $"Data Extraction Options to \"{DataFolder}\"";
    }

    private bool _extractOnlyNeededData = true;
    public bool ExtractOnlyNeededData
    {
      get => _extractOnlyNeededData;
      set
      {
        if (_extractOnlyNeededData != value)
        {
          _extractOnlyNeededData = value;
          OnPropertyChanged(nameof(ExtractOnlyNeededData));
          if (!_extractOnlyNeededData)
          {
            LoadExtractedDataAfterExtraction = false;
          }
        }
      }
    }

    private bool _verifyExtractedData = true;
    public bool VerifyExtractedData
    {
      get => _verifyExtractedData;
      set
      {
        if (_verifyExtractedData != value)
        {
          _verifyExtractedData = value;
          OnPropertyChanged(nameof(VerifyExtractedData));
        }
      }
    }

    private bool _overwriteExistingFiles = true;
    public bool OverwriteExistingFiles
    {
      get => _overwriteExistingFiles;
      set
      {
        if (_overwriteExistingFiles != value)
        {
          _overwriteExistingFiles = value;
          OnPropertyChanged(nameof(OverwriteExistingFiles));
        }
      }
    }

    private bool _loadExtractedDataAfterExtraction = true;
    public bool LoadExtractedDataAfterExtraction
    {
      get => _loadExtractedDataAfterExtraction;
      set
      {
        if (_loadExtractedDataAfterExtraction != value)
        {
          _loadExtractedDataAfterExtraction = value;
          OnPropertyChanged(nameof(LoadExtractedDataAfterExtraction));
        }
      }
    }

    public ObservableCollection<DlcOption> DlcOptions { get; set; } = [];

    public bool IsExtractionPossible { get; set; } = false;

    private int _extractionProgress = 0;
    public int ExtractionProgress
    {
      get => _extractionProgress;
      set
      {
        if (_extractionProgress != value)
        {
          _extractionProgress = value;
          OnPropertyChanged(nameof(ExtractionProgress));
        }
      }
    }

    private string _extractionProgressText = "";
    public string ExtractionProgressText
    {
      get => _extractionProgressText;
      set
      {
        if (_extractionProgressText != value)
        {
          _extractionProgressText = value;
          OnPropertyChanged(nameof(ExtractionProgressText));
        }
      }
    }

    private readonly BackgroundWorker _backgroundWorker;

    public string ExtractedDataFolder
    {
      get => LoadExtractedDataAfterExtraction ? Path.Combine(ExtractedDataLocationFolder, DataFolder) : string.Empty;
    }

    public X4DataExtractionWindow()
    {
      InitializeComponent();
      DataContext = this;

      _backgroundWorker = new BackgroundWorker { WorkerReportsProgress = true, WorkerSupportsCancellation = true };
      _backgroundWorker.DoWork += BackgroundWorker_DoWork;
      _backgroundWorker.ProgressChanged += BackgroundWorker_ProgressChanged;
      _backgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;
    }

    public void Connect()
    {
      MainWindowReference = Owner as MainWindow;
      if (MainWindowReference != null)
      {
        if (!string.IsNullOrEmpty(MainWindowReference.X4GameFolder) && Directory.Exists(MainWindowReference.X4GameFolder))
        {
          GameFolder = MainWindowReference.X4GameFolder;
          GetGameFolderInfo();
        }
        if (!string.IsNullOrEmpty(MainWindowReference.X4DataFolder) && Directory.Exists(MainWindowReference.X4DataFolder))
        {
          ExtractedDataLocationFolder = Path.GetDirectoryName(MainWindowReference.X4DataFolder)?.ToString() ?? string.Empty;
        }
        else
        {
          LoadExtractedDataAfterExtraction = true;
        }
        ExtractionIsPossibleCheck();
      }
    }

    private void ExtractionIsPossibleCheck()
    {
      if (
        !string.IsNullOrEmpty(GameFolder)
        && Directory.Exists(GameFolder)
        && !string.IsNullOrEmpty(ExtractedDataLocationFolder)
        && Directory.Exists(ExtractedDataLocationFolder)
      )
      {
        IsExtractionPossible = true;
        OnPropertyChanged(nameof(IsExtractionPossible));
      }
      else
      {
        IsExtractionPossible = false;
        OnPropertyChanged(nameof(IsExtractionPossible));
      }
    }

    private bool GetGameFolderInfo()
    {
      DlcOptions.Clear();
      if (string.IsNullOrEmpty(GameFolder) || !Directory.Exists(GameFolder))
      {
        Log.Debug("No game folder selected");
        return false;
      }
      if (!File.Exists(Path.Combine(GameFolder, X4Executable)))
      {
        Log.Debug("X4 executable not found");
        return false;
      }
      if (File.Exists(Path.Combine(GameFolder, DataLoader.VersionDat)))
      {
        try
        {
          string version = File.ReadAllText(Path.Combine(GameFolder, DataLoader.VersionDat)).Trim();
          if (version.Length == 3)
          {
            GameVersion = $" v.{version[0]}.{version[1]}{version[2]}";
            DataFolder = $"X4v{version}-extracted";
          }
        }
        catch (Exception ex)
        {
          Log.Error("Error reading version.dat", ex);
          GameFolder = string.Empty;
          return false;
        }
      }
      else
      {
        Log.Debug("version.dat not found");
        return false;
      }
      if (Directory.Exists(Path.Combine(GameFolder, DataLoader.ExtensionsFolder)))
      {
        // Check for DLCs
        foreach (string dlcId in Galaxy.DLCOrder)
        {
          string dlcFolder = Path.Combine(GameFolder, DataLoader.ExtensionsFolder, dlcId);
          if (Directory.Exists(dlcFolder) && File.Exists(Path.Combine(dlcFolder, DataLoader.ContentXml)))
          {
            try
            {
              XDocument contentXml = XDocument.Load(Path.Combine(dlcFolder, DataLoader.ContentXml));
              XElement? contentElement = contentXml.Element("content");
              if (contentElement == null)
              {
                Log.Warn($"No content element found in {Path.Combine(dlcFolder, DataLoader.ContentXml)}");
                continue;
              }
              string dlcName = contentElement.Attribute("name")?.Value ?? "";
              if (string.IsNullOrEmpty(dlcName))
              {
                Log.Warn($"No name attribute found in {Path.Combine(dlcFolder, DataLoader.ContentXml)}");
                continue;
              }
              Log.Debug($"DLC {Path.GetFileName(dlcFolder)}: {dlcName}");
              DlcOptions.Add(
                new DlcOption
                {
                  Name = dlcName,
                  Folder = Path.GetFileName(dlcFolder),
                  IsChecked = true,
                }
              );
            }
            catch (Exception ex)
            {
              Log.Error($"Error reading {DataLoader.ContentXml} from {dlcFolder}", ex);
            }
          }
        }
        return true;
      }
      else
      {
        Log.Debug("No extensions folder found");
        return false;
      }
    }

    private void ButtonSelectGameFolder_Click(object sender, RoutedEventArgs e)
    {
      OpenFileDialog dialog = new()
      {
        InitialDirectory = string.IsNullOrEmpty(GameFolder)
          ? Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)
          : GameFolder,
        Filter = $"X4 Foundations|{X4Executable}",
        Title = "Select the X4: Foundations executable",
      };

      DialogResult result = dialog.ShowDialog();
      if (result != System.Windows.Forms.DialogResult.OK || string.IsNullOrWhiteSpace(dialog.FileName))
      {
        Log.Warn("No game folder selected");
        return;
      }
      try
      {
        GameFolder = Path.GetDirectoryName(dialog.FileName) ?? "";
      }
      catch (Exception ex)
      {
        Log.Error("Error selecting game folder", ex);
        return;
      }
      if (GetGameFolderInfo())
      {
        ExtractionIsPossibleCheck();
      }
    }

    private void ButtonSelectExtractedDataLocationFolder_Click(object sender, RoutedEventArgs e)
    {
      var dialog = new FolderBrowserDialog
      {
        Description = "Select the folder where the folder with extracted data will be located.",
        ShowNewFolderButton = true,
      };

      if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
      {
        ExtractedDataLocationFolder = dialog.SelectedPath;
      }
    }

    private void ButtonCancel_Click(object sender, RoutedEventArgs e)
    {
      if (_backgroundWorker.IsBusy)
      {
        _backgroundWorker.CancelAsync();
      }
      else
      {
        DialogResult = false;
        Close();
      }
    }

    private void ButtonStartExtraction_Click(object sender, RoutedEventArgs e)
    {
      if (!_backgroundWorker.IsBusy)
      {
        _backgroundWorker.RunWorkerAsync();
      }
    }

    private void BackgroundWorker_DoWork(object? sender, DoWorkEventArgs e)
    {
      // Start the extraction process
      // Use the ExtractedDataFolder, ExtractOnlyNeededData, and DlcOptions properties
      // Example:
      Log.Debug($"Starting extraction to {ExtractedDataLocationFolder}");
      Log.Debug($"Extract only needed data: {ExtractOnlyNeededData}");
      ProcessExtraction(e);
    }

    private void BackgroundWorker_ProgressChanged(object? sender, ProgressChangedEventArgs e)
    {
      if (e.ProgressPercentage != ExtractionProgress)
      {
        ExtractionProgress = e.ProgressPercentage;
      }
      if (e.UserState is string progressText && progressText != ExtractionProgressText)
      {
        ExtractionProgressText = progressText;
      }
    }

    private void BackgroundWorker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
    {
      if (e.Cancelled)
      {
        Log.Debug("Extraction cancelled.");
        DialogResult = false;
        LoadExtractedDataAfterExtraction = false;
      }
      else if (e.Error != null)
      {
        Log.Error("Error during extraction.", e.Error);
        DialogResult = false;
        LoadExtractedDataAfterExtraction = false;
      }
      else
      {
        DialogResult = true;
        Log.Debug("Extraction completed successfully.");
      }
      Close();
    }

    private void ProcessExtraction(DoWorkEventArgs? e)
    {
      int progressInt = 0;
      long totalCount = 0;
      _backgroundWorker.ReportProgress(progressInt);
      List<(ContentExtractor Extractor, List<CatEntry> Entries, string ExtractFolder)> extractors = [];

      extractors.Add(GetCatEntries());
      progressInt += 1;
      totalCount += extractors.Last().Entries.Count;
      _backgroundWorker.ReportProgress(progressInt, $"Reading catalog files of Vanilla");
      foreach (var dlc in DlcOptions)
      {
        Log.Debug($"DLC {dlc.Name}: {(dlc.IsChecked ? "Included" : "Excluded")}");
        if (dlc.IsChecked)
        {
          extractors.Add(GetCatEntries(dlc.Name));
        }
        progressInt += 1;
        _backgroundWorker.ReportProgress(progressInt, $"Reading catalog files of {dlc.Name}");
        totalCount += extractors.Last().Entries.Count;
      }

      double step = (100.0 - progressInt) / totalCount;
      double progress = 1.0 * progressInt;
      if (totalCount == 0)
      {
        Log.Debug("No files to extract");
        return;
      }

      foreach ((ContentExtractor Extractor, List<CatEntry> Entries, string ExtractToFolder) in extractors)
      {
        if (_backgroundWorker.CancellationPending)
        {
          if (e != null)
          {
            e.Cancel = true;
          }
          return;
        }
        if (!Directory.Exists(ExtractToFolder))
        {
          try
          {
            Directory.CreateDirectory(ExtractToFolder);
          }
          catch (Exception ex)
          {
            Log.Error($"Error creating directory {ExtractToFolder}", ex);
            progress += Entries.Count * step;
            progressInt = (int)progress;
            if (progressInt > ExtractionProgress)
            {
              _backgroundWorker.ReportProgress(progressInt);
            }
            continue;
          }
        }
        foreach (var catEntry in Entries)
        {
          if (_backgroundWorker.CancellationPending)
          {
            if (e != null)
            {
              e.Cancel = true;
            }
            return;
          }
          Log.Debug($"Extracting {catEntry.FilePath}");
          progress += step;
          progressInt = (int)progress;
          _backgroundWorker.ReportProgress(progressInt, $"Extracting {ExtractToFolder}/{catEntry.FilePath}");
          Extractor.ExtractEntry(catEntry, ExtractToFolder, OverwriteExistingFiles, !VerifyExtractedData);
        }
      }
      foreach (var dlc in DlcOptions)
      {
        Log.Debug($"DLC {dlc.Name}: {(dlc.IsChecked ? "Included" : "Excluded")}");
        if (dlc.IsChecked)
        {
          try
          {
            File.Copy(
              Path.Combine(GameFolder, DataLoader.ExtensionsFolder, dlc.Folder, DataLoader.ContentXml),
              Path.Combine(ExtractedDataLocationFolder, DataFolder, DataLoader.ExtensionsFolder, dlc.Folder, DataLoader.ContentXml),
              OverwriteExistingFiles
            );
          }
          catch (Exception ex)
          {
            Log.Error($"Error copying {DataLoader.ContentXml} from {dlc.Folder}", ex);
          }
        }
      }
      if (File.Exists(Path.Combine(GameFolder, DataLoader.VersionDat)))
      {
        try
        {
          File.Copy(
            Path.Combine(GameFolder, DataLoader.VersionDat),
            Path.Combine(ExtractedDataLocationFolder, DataFolder, DataLoader.VersionDat),
            OverwriteExistingFiles
          );
        }
        catch (Exception ex)
        {
          Log.Error($"Error copying {DataLoader.VersionDat}", ex);
        }
      }
    }

    private (ContentExtractor Extractor, List<CatEntry> Entries, string ExtractToFolder) GetCatEntries(string category = "Vanilla")
    {
      string sourceFolder = GameFolder;
      string extractToFolder = Path.Combine(ExtractedDataLocationFolder, DataFolder);
      if (category != "Vanilla")
      {
        string dlcFolder = DlcOptions.FirstOrDefault(dlc => dlc.Name == category)?.Folder ?? string.Empty;
        if (string.IsNullOrEmpty(dlcFolder))
        {
          Log.Warn($"DLC {category} not found");
          return (new ContentExtractor(""), new List<CatEntry>(), "");
        }
        sourceFolder = Path.Combine(GameFolder, DataLoader.ExtensionsFolder, dlcFolder);
        extractToFolder = Path.Combine(extractToFolder, DataLoader.ExtensionsFolder, dlcFolder);
      }
      ContentExtractor extractor = new(sourceFolder);
      List<CatEntry> catEntries = [];
      if (ExtractOnlyNeededData)
      {
        catEntries.AddRange(extractor.GetFilesByMask("maps/xu_ep2_universe/*.xml"));
        catEntries.AddRange(extractor.GetFilesByMask("libraries/*.xml"));
        catEntries.AddRange(extractor.GetFilesByMask("t/*.xml"));
      }
      else
      {
        catEntries.AddRange(extractor.GetFilesByMask("*.*"));
      }
      Log.Debug($"Found {catEntries.Count} files to extract");
      return (extractor, catEntries, extractToFolder);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }

  public class DlcOption : INotifyPropertyChanged
  {
    private string _name = string.Empty;
    private string _path = string.Empty;
    private bool _isChecked = false;

    public string Name
    {
      get => _name;
      set
      {
        if (_name != value)
        {
          _name = value;
          OnPropertyChanged(nameof(Name));
        }
      }
    }

    public string Folder
    {
      get => _path;
      set
      {
        if (_path != value)
        {
          _path = value;
          OnPropertyChanged(nameof(Folder));
        }
      }
    }

    public bool IsChecked
    {
      get => _isChecked;
      set
      {
        if (_isChecked != value)
        {
          _isChecked = value;
          OnPropertyChanged(nameof(IsChecked));
        }
      }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
