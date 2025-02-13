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

    public ObservableCollection<ExtensionOption> DlcsOptions { get; set; } = [];
    public ObservableCollection<ExtensionOption> ModsOptions { get; set; } = [];

    private List<ExtensionOption> Extensions
    {
      get => [.. DlcsOptions, .. ModsOptions];
    }

    private Visibility _isModsOptionsVisible = Visibility.Hidden;
    public Visibility IsModsOptionsVisible
    {
      get => _isModsOptionsVisible;
      set
      {
        if (_isModsOptionsVisible != value)
        {
          _isModsOptionsVisible = value;
          OnPropertyChanged(nameof(IsModsOptionsVisible));
        }
      }
    }

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
      DlcsOptions.Clear();
      ModsOptions.Clear();
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
        foreach (string extensionFolder in Directory.GetDirectories(Path.Combine(GameFolder, DataLoader.ExtensionsFolder)))
        {
          if (File.Exists(Path.Combine(extensionFolder, DataLoader.ContentXml)))
          {
            try
            {
              XDocument contentXml = XDocument.Load(Path.Combine(extensionFolder, DataLoader.ContentXml));
              XElement? contentElement = contentXml.Element("content");
              if (contentElement == null)
              {
                Log.Warn($"No content element found in {Path.Combine(extensionFolder, DataLoader.ContentXml)}");
                continue;
              }
              string extensionName = contentElement.Attribute("name")?.Value ?? "";
              string extensionId = contentElement.Attribute("id")?.Value ?? "";
              if (string.IsNullOrEmpty(extensionName) || string.IsNullOrEmpty(extensionId))
              {
                Log.Warn($"No name or id attribute found in {Path.Combine(extensionFolder, DataLoader.ContentXml)}");
                continue;
              }
              Log.Debug($"Extension {Path.GetFileName(extensionFolder)}: {extensionName}");
              ModsOptions.Add(
                new ExtensionOption
                {
                  Name = extensionName,
                  Id = extensionId,
                  Folder = Path.GetFileName(extensionFolder),
                  IsChecked = true,
                }
              );
            }
            catch (Exception ex)
            {
              Log.Error($"Error reading {DataLoader.ContentXml} from {extensionFolder}", ex);
            }
          }
        }
        if (ModsOptions.Count > 0)
        {
          // Check for DLCs
          foreach (string dlcId in Galaxy.DLCOrder)
          {
            if (ModsOptions.Any(extension => extension.Id == dlcId))
            {
              DlcsOptions.Add(ModsOptions.First(extension => extension.Id == dlcId));
            }
          }
          foreach (var dlc in DlcsOptions)
          {
            ModsOptions.Remove(dlc);
          }
          if (ModsOptions.Count > 0)
          {
            IsModsOptionsVisible = Visibility.Visible;
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
      List<CatalogEntry> catalogEntries = [];
      catalogEntries.Add(GetCatEntries());
      progressInt += 1;
      totalCount += catalogEntries.Last().Count;
      _backgroundWorker.ReportProgress(progressInt, $"Reading catalog files of Vanilla");
      foreach (var extension in Extensions)
      {
        Log.Debug($"Extension {extension.Name}: {(extension.IsChecked ? "Included" : "Excluded")}");
        if (extension.IsChecked)
        {
          catalogEntries.Add(GetCatEntries(extension.Name, extension.Folder));
          totalCount += catalogEntries.Last().Count;
        }
        progressInt += 1;
        _backgroundWorker.ReportProgress(progressInt, $"Reading catalog files of {extension.Name}");
      }

      double step = (100.0 - progressInt) / totalCount;
      double progress = 1.0 * progressInt;
      if (totalCount == 0)
      {
        Log.Debug("No files to extract");
        return;
      }

      foreach (CatalogEntry entry in catalogEntries)
      {
        if (_backgroundWorker.CancellationPending)
        {
          if (e != null)
          {
            e.Cancel = true;
          }
          return;
        }
        if (!Directory.Exists(entry.ExtractToFolder))
        {
          try
          {
            Directory.CreateDirectory(entry.ExtractToFolder);
          }
          catch (Exception ex)
          {
            Log.Error($"Error creating directory {entry.ExtractToFolder}", ex);
            progress += entry.Count * step;
            progressInt = (int)progress;
            if (progressInt > ExtractionProgress)
            {
              _backgroundWorker.ReportProgress(progressInt);
            }
            continue;
          }
        }
        if (entry.Entries == null || entry.Extractor == null)
        {
          Log.Debug("No files to extract");
          continue;
        }
        foreach (var catEntry in entry.Entries)
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
          _backgroundWorker.ReportProgress(progressInt, $"Extracting {entry.ExtractToFolder}/{catEntry.FilePath}");
          entry.Extractor.ExtractEntry(catEntry, entry.ExtractToFolder, OverwriteExistingFiles, !VerifyExtractedData);
        }
      }
      foreach (var extension in Extensions)
      {
        Log.Debug($"Extension {extension.Name}: {(extension.IsChecked ? "Included" : "Excluded")}");
        if (extension.IsChecked)
        {
          try
          {
            Log.Debug($"Copying {Path.Combine(GameFolder, DataLoader.ExtensionsFolder, extension.Folder, DataLoader.ContentXml)}");
            File.Copy(
              Path.Combine(GameFolder, DataLoader.ExtensionsFolder, extension.Folder, DataLoader.ContentXml),
              Path.Combine(ExtractedDataLocationFolder, DataFolder, DataLoader.ExtensionsFolder, extension.Folder, DataLoader.ContentXml),
              OverwriteExistingFiles
            );
          }
          catch (Exception ex)
          {
            Log.Error($"Error copying {DataLoader.ContentXml} from {extension.Folder}", ex);
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

    private CatalogEntry GetCatEntries(string category = "Vanilla", string folder = "")
    {
      string sourceFolder = GameFolder;
      string extractToFolder = Path.Combine(ExtractedDataLocationFolder, DataFolder);
      if (category != "Vanilla")
      {
        sourceFolder = Path.Combine(GameFolder, DataLoader.ExtensionsFolder, folder);
        extractToFolder = Path.Combine(extractToFolder, DataLoader.ExtensionsFolder, folder);
      }
      ContentCopier extractor = new(sourceFolder);
      List<CatEntry> catEntries = [];
      if (ExtractOnlyNeededData)
      {
        catEntries.AddRange(extractor.GetFilesByMask("maps/xu_ep2_universe/*.xml"));
        catEntries.AddRange(extractor.GetFilesByMask("libraries/*.xml"));
        catEntries.AddRange(extractor.GetFilesByMask("t/*.xml"));
        catEntries.AddRange(extractor.GetFilesByMask("extensions/*/maps/xu_ep2_universe/*.xml"));
        catEntries.AddRange(extractor.GetFilesByMask("extensions/*/libraries/*.xml"));
        catEntries.AddRange(extractor.GetFilesByMask("extensions/*/t/*.xml"));
      }
      else
      {
        catEntries.AddRange(extractor.GetFilesByMask("*.*"));
      }
      Log.Debug($"Found {catEntries.Count} files to extract");
      return new CatalogEntry(extractor, catEntries, extractToFolder);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }

  public class ExtensionOption : INotifyPropertyChanged
  {
    private string _name = string.Empty;
    private string _folder = string.Empty;
    private string _id = string.Empty;
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
      get => _folder;
      set
      {
        if (_folder != value)
        {
          _folder = value;
          OnPropertyChanged(nameof(Folder));
        }
      }
    }

    public string Id
    {
      get => _id;
      set
      {
        if (_id != value)
        {
          _id = value;
          OnPropertyChanged(nameof(Id));
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

  public class CatalogEntry(ContentCopier? extractor, List<CatEntry>? entries, string extractToFolder)
  {
    public ContentCopier? Extractor = extractor;
    public List<CatEntry>? Entries = entries;
    public string ExtractToFolder = extractToFolder;

    public int Count
    {
      get => Entries != null ? Entries.Count : 0;
    }
  }

  public class ContentCopier(string folderPath, string pattern = "*.cat", bool excludeSignatures = true)
    : ContentExtractor(folderPath, pattern, excludeSignatures)
  {
    private bool IsNotPacked { get; set; } = false;

    protected override void InitializeCatalog(string pattern = "*.cat", bool excludeSignatures = true)
    {
      List<string> catFiles = [.. Directory.GetFiles(_folderPath, pattern)];
      if (excludeSignatures)
      {
        catFiles = catFiles.Where(f => !f.EndsWith("_sig.cat")).ToList();
      }
      if (catFiles.Count == 0 && Directory.GetDirectories(_folderPath).Length > 0)
      {
        IsNotPacked = true;
        List<string> files = [.. Directory.GetFiles(_folderPath, "*.*", SearchOption.AllDirectories)];
        foreach (string file in files)
        {
          string relativePath = Path.GetRelativePath(_folderPath, file);
          FileInfo fileInfo = new(file);
          _catalog[relativePath.Replace("\\", "/")] = new CatEntry
          {
            FilePath = relativePath,
            FileSize = fileInfo.Length,
            FileOffset = 0,
            FileDate = fileInfo.LastWriteTime,
            FileHash = "",
            DatFilePath = "",
          };
        }
      }
      else
      {
        base.InitializeCatalog(pattern, excludeSignatures);
      }
    }

    public override void ExtractEntry(CatEntry catEntry, string extractToFolder, bool overwriteExistingFiles, bool skipVerification)
    {
      if (IsNotPacked)
      {
        string sourceFile = Path.Combine(_folderPath, catEntry.FilePath);
        string destFile = Path.Combine(extractToFolder, catEntry.FilePath);
        if (File.Exists(destFile) && !overwriteExistingFiles)
        {
          Log.Debug($"File {catEntry.FilePath} already exists in {extractToFolder}");
          return;
        }
        try
        {
          var directoryName = Path.GetDirectoryName(destFile);
          if (directoryName != null)
          {
            Directory.CreateDirectory(directoryName);
          }
          File.Copy(sourceFile, destFile, overwriteExistingFiles);
        }
        catch (Exception ex)
        {
          Log.Error($"Error copying {catEntry.FilePath} from {sourceFile} to {destFile}", ex);
        }
      }
      else
      {
        base.ExtractEntry(catEntry, extractToFolder, overwriteExistingFiles, skipVerification);
      }
    }
  }
}
