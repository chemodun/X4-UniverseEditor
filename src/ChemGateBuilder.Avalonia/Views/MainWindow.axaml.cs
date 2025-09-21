using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Xml.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using ChemGateBuilder.AvaloniaApp.Services;
using X4DataLoader;
using X4Unpack;

namespace ChemGateBuilder.AvaloniaApp.Views
{
  public partial class MainWindow : Window, INotifyPropertyChanged
  {
    public ObservableCollection<string> X4DataVersions { get; } = new();
    private string _x4DataVersionString = string.Empty;
    public string X4DataVersionString
    {
      get => _x4DataVersionString;
      set
      {
        if (_x4DataVersionString != value)
        {
          _x4DataVersionString = value;
          OnPropertyChanged();
          SaveSettings();
        }
      }
    }
    public StatusBarMessage StatusBar { get; } = new();
    private bool _isBusy;
    public bool IsBusy
    {
      get => _isBusy;
      set
      {
        if (_isBusy != value)
        {
          _isBusy = value;
          OnPropertyChanged();
        }
      }
    }
    private string _busyMessage = string.Empty;
    public string BusyMessage
    {
      get => _busyMessage;
      set
      {
        if (_busyMessage != value)
        {
          _busyMessage = value;
          OnPropertyChanged();
        }
      }
    }

    private Galaxy? _galaxy;

    // Mod connections (parity with WPF list; no WPF dependencies)
    public ObservableCollection<GalaxyConnectionDataLite> ModConnections { get; } = new();
    private GalaxyConnectionDataLite? _selectedModConnection;
    public GalaxyConnectionDataLite? SelectedModConnection
    {
      get => _selectedModConnection;
      set
      {
        if (!ReferenceEquals(_selectedModConnection, value))
        {
          _selectedModConnection = value;
          OnPropertyChanged();
          // seed editor from selected connection
          if (value?.Connection?.PathDirect?.Zone?.Position is { } dp)
          {
            EditDirectXKm = (int)(dp.X / 1000.0);
            EditDirectYKm = (int)(dp.Y / 1000.0);
            EditDirectZKm = (int)(dp.Z / 1000.0);
          }
          else
          {
            EditDirectXKm = EditDirectYKm = EditDirectZKm = 0;
          }
          if (value?.Connection?.PathDirect?.Gate is { } dg)
          {
            EditDirectAngleDeg = (int)Math.Round(PitchFromQuaternion(dg.Quaternion ?? new X4DataLoader.Quaternion()));
            EditDirectActive = dg.IsActive;
          }
          if (value?.Connection?.PathOpposite?.Zone?.Position is { } op)
          {
            EditOppositeXKm = (int)(op.X / 1000.0);
            EditOppositeYKm = (int)(op.Y / 1000.0);
            EditOppositeZKm = (int)(op.Z / 1000.0);
          }
          else
          {
            EditOppositeXKm = EditOppositeYKm = EditOppositeZKm = 0;
          }
          if (value?.Connection?.PathOpposite?.Gate is { } og)
          {
            EditOppositeAngleDeg = (int)Math.Round(PitchFromQuaternion(og.Quaternion ?? new X4DataLoader.Quaternion()));
            EditOppositeActive = og.IsActive;
          }
        }
      }
    }

    // Editor backing properties
    private int _editDirectXKm,
      _editDirectYKm,
      _editDirectZKm,
      _editDirectAngleDeg;
    private bool _editDirectActive;
    public int EditDirectXKm
    {
      get => _editDirectXKm;
      set
      {
        if (_editDirectXKm != value)
        {
          _editDirectXKm = value;
          OnPropertyChanged();
        }
      }
    }
    public int EditDirectYKm
    {
      get => _editDirectYKm;
      set
      {
        if (_editDirectYKm != value)
        {
          _editDirectYKm = value;
          OnPropertyChanged();
        }
      }
    }
    public int EditDirectZKm
    {
      get => _editDirectZKm;
      set
      {
        if (_editDirectZKm != value)
        {
          _editDirectZKm = value;
          OnPropertyChanged();
        }
      }
    }
    public int EditDirectAngleDeg
    {
      get => _editDirectAngleDeg;
      set
      {
        if (_editDirectAngleDeg != value)
        {
          _editDirectAngleDeg = value;
          OnPropertyChanged();
        }
      }
    }
    public bool EditDirectActive
    {
      get => _editDirectActive;
      set
      {
        if (_editDirectActive != value)
        {
          _editDirectActive = value;
          OnPropertyChanged();
        }
      }
    }
    private int _editOppositeXKm,
      _editOppositeYKm,
      _editOppositeZKm,
      _editOppositeAngleDeg;
    private bool _editOppositeActive;
    public int EditOppositeXKm
    {
      get => _editOppositeXKm;
      set
      {
        if (_editOppositeXKm != value)
        {
          _editOppositeXKm = value;
          OnPropertyChanged();
        }
      }
    }
    public int EditOppositeYKm
    {
      get => _editOppositeYKm;
      set
      {
        if (_editOppositeYKm != value)
        {
          _editOppositeYKm = value;
          OnPropertyChanged();
        }
      }
    }
    public int EditOppositeZKm
    {
      get => _editOppositeZKm;
      set
      {
        if (_editOppositeZKm != value)
        {
          _editOppositeZKm = value;
          OnPropertyChanged();
        }
      }
    }
    public int EditOppositeAngleDeg
    {
      get => _editOppositeAngleDeg;
      set
      {
        if (_editOppositeAngleDeg != value)
        {
          _editOppositeAngleDeg = value;
          OnPropertyChanged();
        }
      }
    }
    public bool EditOppositeActive
    {
      get => _editOppositeActive;
      set
      {
        if (_editOppositeActive != value)
        {
          _editOppositeActive = value;
          OnPropertyChanged();
        }
      }
    }

    private static X4DataLoader.Quaternion QuaternionFromPitchDegrees(int pitchDeg)
    {
      // Only pitch around X-axis
      var rad = Math.PI * pitchDeg / 180.0;
      var sin = Math.Sin(rad / 2.0);
      var cos = Math.Cos(rad / 2.0);
      return new X4DataLoader.Quaternion(sin, 0, 0, cos);
    }

    private static double PitchFromQuaternion(X4DataLoader.Quaternion q)
    {
      // Convert quaternion to Euler pitch (X-axis rotation) in degrees
      // Formula: pitch = atan2(2*(qw*qx + qy*qz), 1 - 2*(qx*qx + qy*qy))
      double qw = q.QW,
        qx = q.QX,
        qy = q.QY,
        qz = q.QZ;
      double sinp = 2.0 * (qw * qx + qy * qz);
      double cosp = 1.0 - 2.0 * (qx * qx + qy * qy);
      double pitch = Math.Atan2(sinp, cosp) * 180.0 / Math.PI;
      return pitch;
    }

    private void OnApplyConnectionEditClick(object? sender, RoutedEventArgs e)
    {
      if (SelectedModConnection?.Connection == null)
        return;
      var c = SelectedModConnection.Connection;
      // Update positions (meters)
      if (c.PathDirect?.Zone != null)
      {
        c.PathDirect.Zone.SetPosition(
          new X4DataLoader.Position(EditDirectXKm * 1000.0, EditDirectYKm * 1000.0, EditDirectZKm * 1000.0),
          c.PathDirect.Zone.PositionId,
          c.PathDirect.Zone.PositionXML
        );
      }
      if (c.PathOpposite?.Zone != null)
      {
        c.PathOpposite.Zone.SetPosition(
          new X4DataLoader.Position(EditOppositeXKm * 1000.0, EditOppositeYKm * 1000.0, EditOppositeZKm * 1000.0),
          c.PathOpposite.Zone.PositionId,
          c.PathOpposite.Zone.PositionXML
        );
      }
      // Update gate rotation and active flags
      if (c.PathDirect?.Gate != null)
      {
        var q = QuaternionFromPitchDegrees(EditDirectAngleDeg);
        var connEl = c.PathDirect.Gate.XML;
        if (connEl != null)
        {
          var offset = connEl.Element("offset");
          if (offset == null)
          {
            offset = new XElement("offset");
            connEl.Add(offset);
          }
          var quatEl = offset.Element("quaternion");
          if (quatEl == null)
          {
            quatEl = new XElement("quaternion");
            offset.Add(quatEl);
          }
          quatEl.SetAttributeValue("qx", q.QX);
          quatEl.SetAttributeValue("qy", q.QY);
          quatEl.SetAttributeValue("qz", q.QZ);
          quatEl.SetAttributeValue("qw", q.QW);
        }
        // Adjust active flag in MacroXML
        var props = c.PathDirect.Gate.MacroXML?.Element("properties");
        if (props == null)
        {
          props = new XElement("properties");
          c.PathDirect.Gate.MacroXML?.Add(props);
        }
        var state = props.Element("state") ?? new XElement("state");
        state.SetAttributeValue("active", EditDirectActive ? "true" : "false");
        if (state.Parent == null)
          props.Add(state);
      }
      if (c.PathOpposite?.Gate != null)
      {
        var q = QuaternionFromPitchDegrees(EditOppositeAngleDeg);
        var connEl = c.PathOpposite.Gate.XML;
        if (connEl != null)
        {
          var offset = connEl.Element("offset");
          if (offset == null)
          {
            offset = new XElement("offset");
            connEl.Add(offset);
          }
          var quatEl = offset.Element("quaternion");
          if (quatEl == null)
          {
            quatEl = new XElement("quaternion");
            offset.Add(quatEl);
          }
          quatEl.SetAttributeValue("qx", q.QX);
          quatEl.SetAttributeValue("qy", q.QY);
          quatEl.SetAttributeValue("qz", q.QZ);
          quatEl.SetAttributeValue("qw", q.QW);
        }
        var props = c.PathOpposite.Gate.MacroXML?.Element("properties");
        if (props == null)
        {
          props = new XElement("properties");
          c.PathOpposite.Gate.MacroXML?.Add(props);
        }
        var state = props.Element("state") ?? new XElement("state");
        state.SetAttributeValue("active", EditOppositeActive ? "true" : "false");
        if (state.Parent == null)
          props.Add(state);
      }
      // reflect list item formatted fields
      OnPropertyChanged(nameof(ModConnections));
      StatusBar.StatusMessage = "Connection updated.";
    }

    private void OnDeleteConnectionClick(object? sender, RoutedEventArgs e)
    {
      if (SelectedModConnection?.Connection == null)
        return;
      var c = SelectedModConnection.Connection;
      // Remove from galaxy connections and UI list
      _galaxy?.Connections.Remove(c);
      ModConnections.Remove(SelectedModConnection);
      SelectedModConnection = null;
      StatusBar.StatusMessage = "Connection deleted.";
    }

    private void OnDuplicateConnectionClick(object? sender, RoutedEventArgs e)
    {
      if (_galaxy == null || SelectedModConnection?.Connection == null)
        return;
      var src = SelectedModConnection.Connection;
      if (src.PathDirect?.Cluster == null || src.PathDirect.Sector == null || src.PathDirect.Zone == null || src.PathDirect.Gate == null)
        return;
      if (
        src.PathOpposite?.Cluster == null
        || src.PathOpposite.Sector == null
        || src.PathOpposite.Zone == null
        || src.PathOpposite.Gate == null
      )
        return;

      var newConn = new GalaxyConnection();
      // generate unique name
      string baseName = string.IsNullOrWhiteSpace(src.Name) ? "Chem_Gate" : src.Name;
      string name = baseName;
      int idx = 1;
      while (_galaxy.Connections.Any(gc => string.Equals(gc.Name, name, StringComparison.OrdinalIgnoreCase)))
      {
        name = baseName + "_copy" + idx++;
      }
      newConn.Create(
        name,
        src.PathDirect.Cluster,
        src.PathDirect.Sector,
        src.PathDirect.Zone,
        src.PathDirect.Gate,
        src.PathOpposite.Cluster,
        src.PathOpposite.Sector,
        src.PathOpposite.Zone,
        src.PathOpposite.Gate
      );
      _galaxy.Connections.Add(newConn);
      var lite = new GalaxyConnectionDataLite(newConn);
      ModConnections.Add(lite);
      SelectedModConnection = lite;
      StatusBar.StatusMessage = "Connection duplicated.";
    }

    // Auto-load game data on start
    private async void OnWindowOpened(object? sender, EventArgs e)
    {
      string? path = DirectMode ? _x4GameFolder : _x4DataFolder;
      if (string.IsNullOrWhiteSpace(path))
      {
        StatusBar.StatusMessage = "No X4 folder configured. Set it in Configuration.";
        return;
      }
      if (!DataLoader.ValidateDataFolder(path, out var error))
      {
        StatusBar.StatusMessage = error;
        return;
      }
      await LoadFromPathAsync(path);
    }

    // Tab index and state placeholders to mirror WPF ribbon behavior
    private int _selectedTabIndex;
    public int SelectedTabIndex
    {
      get => _selectedTabIndex;
      set
      {
        if (_selectedTabIndex != value)
        {
          _selectedTabIndex = value;
          OnPropertyChanged();
          SaveSettings();
        }
      }
    }

    public bool IsGateCanBeCreated { get; set; } = true;
    public bool IsDataLoaded => _galaxy != null && _galaxy.Sectors.Count > 0;
    public bool IsModCanBeSaved { get; set; }
    public bool IsModCanBeSavedAs { get; set; }
    private bool _loadModsData;
    public bool LoadModsData
    {
      get => _loadModsData;
      set
      {
        if (_loadModsData != value)
        {
          _loadModsData = value;
          OnPropertyChanged();
          SaveSettings();
        }
      }
    }

    private bool _directMode = false;
    public bool DirectMode
    {
      get => _directMode;
      set
      {
        if (_directMode != value)
        {
          _directMode = value;
          OnPropertyChanged();
          OnPropertyChanged(nameof(NoDirectMode));
          SaveSettings();
        }
      }
    }
    public bool NoDirectMode => !_directMode;

    private string? _x4DataFolder;
    public string X4DataFolderPath => _x4DataFolder ?? string.Empty;
    private string? _x4GameFolder;
    public string X4GameFolderPath => _x4GameFolder ?? string.Empty;

    private readonly SettingsService? _settings;
    private AppConfig _config = new();

    public MainWindow(SettingsService? settings = null)
    {
      InitializeComponent();
      _settings = settings;
      DataContext = this;
      // Load WPF-compatible JSON config first (parity with WPF)
      _config = ConfigService.Load();
      ApplyConfigToState(_config);
      if (_settings != null)
      {
        var s = _settings.Current;
        _x4DataFolder = s.X4DataFolder;
        _x4GameFolder = s.X4GameFolder;
        DirectMode = s.DirectMode;
        LoadModsData = s.LoadModsData;
        SelectedTabIndex = s.SelectedTabIndex;
        X4DataVersionOverride = s.X4DataVersionOverride;
        _gatesActiveByDefault = s.GatesActiveByDefault;
        _gatesMinimalDistanceBetween = s.GatesMinimalDistanceBetween;
        if (!string.IsNullOrWhiteSpace(s.X4DataVersion))
        {
          EnsureVersionInList(s.X4DataVersion);
          X4DataVersionString = s.X4DataVersion;
        }
        OnPropertyChanged(nameof(X4DataFolderPath));
        OnPropertyChanged(nameof(X4GameFolderPath));
      }
    }

    public MainWindow()
      : this(null) { }

    private async void OnLoadX4DataClick(object? sender, RoutedEventArgs e)
    {
      var selectedPath = await PickFolderAsync("Select X4 extracted core folder (contains 't/0001-l044.xml')");
      if (string.IsNullOrWhiteSpace(selectedPath))
      {
        StatusBar.StatusMessage = "Load cancelled.";
        return;
      }

      if (!DataLoader.ValidateDataFolder(selectedPath, out var error))
      {
        StatusBar.StatusMessage = error;
        return;
      }

      await LoadFromPathAsync(selectedPath);
    }

    private async Task<string?> PickFolderAsync(string title)
    {
      if (this.StorageProvider is { } sp && sp.CanPickFolder)
      {
        var result = await sp.OpenFolderPickerAsync(new FolderPickerOpenOptions { Title = title, AllowMultiple = false });
        var folder = result?.FirstOrDefault();
        if (folder != null)
        {
          if (folder.TryGetLocalPath() is string local && !string.IsNullOrWhiteSpace(local))
            return local;
          return folder.Name;
        }
        return null;
      }
#pragma warning disable CS0618
      var dlg = new OpenFolderDialog { Title = title };
      return await dlg.ShowAsync(this);
#pragma warning restore CS0618
    }

    private async void OnSelectX4DataFolderClick(object? sender, RoutedEventArgs e)
    {
      var path = await PickFolderAsync("Select X4 extracted core folder (contains 't/0001-l044.xml')");
      if (string.IsNullOrWhiteSpace(path))
        return;
      if (!DataLoader.ValidateDataFolder(path, out var error))
      {
        StatusBar.StatusMessage = error;
        return;
      }
      _x4DataFolder = path;
      OnPropertyChanged(nameof(X4DataFolderPath));
      SaveSettings();
    }

    private async void OnSelectX4GameFolderClick(object? sender, RoutedEventArgs e)
    {
      var path = await PickFolderAsync("Select X4 game folder (contains extensions/ and content.xml)");
      if (string.IsNullOrWhiteSpace(path))
        return;
      _x4GameFolder = path;
      OnPropertyChanged(nameof(X4GameFolderPath));
      SaveSettings();
    }

    private async void OnReloadX4DataClick(object? sender, RoutedEventArgs e)
    {
      string? path = DirectMode ? _x4GameFolder : _x4DataFolder;
      if (string.IsNullOrWhiteSpace(path))
      {
        StatusBar.StatusMessage = "Select data/game folder first.";
        return;
      }
      if (!DataLoader.ValidateDataFolder(path, out var error))
      {
        StatusBar.StatusMessage = error;
        return;
      }
      await LoadFromPathAsync(path);
    }

    private void OnExtractX4DataClick(object? sender, RoutedEventArgs e)
    {
      _ = RunExtractionAsync();
    }

    private void OnNewModClick(object? sender, RoutedEventArgs e) => StatusBar.StatusMessage = "New mod (placeholder)";

    private async void OnLoadModClick(object? sender, RoutedEventArgs e)
    {
      if (this.StorageProvider is { } sp)
      {
        var res = await sp.OpenFilePickerAsync(
          new FilePickerOpenOptions
          {
            Title = "Select mod content.xml",
            AllowMultiple = false,
            FileTypeFilter = [new FilePickerFileType("content.xml") { Patterns = ["content.xml"] }],
          }
        );
        var file = res?.FirstOrDefault();
        if (file == null)
        {
          StatusBar.StatusMessage = "Load mod cancelled.";
          return;
        }
        var path = file.TryGetLocalPath() ?? file.Name;
        if (ModService.TryLoad(path, out var mod) && mod != null)
        {
          StatusBar.StatusMessage = $"Loaded mod: {mod.Name} ({mod.Id})";
          IsModCanBeSaved = true;
          IsModCanBeSavedAs = true;
          // Load mod gate connections and populate the list
          if (_galaxy != null)
          {
            if (ModConnectionsService.TryLoadConnections(path, _galaxy, out var conns))
            {
              ModConnections.Clear();
              foreach (var c in conns)
                ModConnections.Add(new GalaxyConnectionDataLite(c));
              OnPropertyChanged(nameof(ModConnections));
            }
          }
        }
        else
        {
          StatusBar.StatusMessage = "Failed to load mod.";
        }
      }
    }

    private async void OnSaveModClick(object? sender, RoutedEventArgs e)
    {
      var folder = await PickFolderAsync("Select output folder for mod");
      if (string.IsNullOrWhiteSpace(folder))
      {
        StatusBar.StatusMessage = "Save cancelled.";
        return;
      }
      if (_galaxy == null)
      {
        StatusBar.StatusMessage = "Load X4 data first.";
        return;
      }
      var mod = new ModInfo
      {
        Id = "chem_gate_keeper",
        Name = "Chem Gate Keeper",
        GameVersion = ParseVersion(X4DataVersionString),
      };
      var target = Path.Combine(folder, mod.Id);
      Directory.CreateDirectory(target);
      var ok = ModConnectionsSaveService.Save(target, mod, _galaxy, ModConnections.Select(m => m.Connection), DataLoader.DefaultUniverseId);
      StatusBar.StatusMessage = ok ? $"Saved mod to {target}" : "Failed to save mod.";
    }

    private async void OnSaveModAsClick(object? sender, RoutedEventArgs e)
    {
      var folder = await PickFolderAsync("Select output folder for mod (Save As)");
      if (string.IsNullOrWhiteSpace(folder))
      {
        StatusBar.StatusMessage = "Save As cancelled.";
        return;
      }
      if (_galaxy == null)
      {
        StatusBar.StatusMessage = "Load X4 data first.";
        return;
      }
      var mod = new ModInfo
      {
        Id = "chem_gate_keeper",
        Name = "Chem Gate Keeper",
        GameVersion = ParseVersion(X4DataVersionString),
      };
      var target = Path.Combine(folder, mod.Id);
      Directory.CreateDirectory(target);
      var ok = ModConnectionsSaveService.Save(target, mod, _galaxy, ModConnections.Select(m => m.Connection), DataLoader.DefaultUniverseId);
      StatusBar.StatusMessage = ok ? $"Saved mod to {target}" : "Failed to save mod.";
    }

    private static int ParseVersion(string v)
    {
      // Expect format NN.MM -> convert to NNM M (e.g., 7.60 -> 760)
      if (string.IsNullOrWhiteSpace(v))
        return 0;
      var parts = v.Split('.', StringSplitOptions.RemoveEmptyEntries);
      if (parts.Length == 2 && int.TryParse(parts[0], out var major) && int.TryParse(parts[1], out var minor))
        return major * 100 + minor;
      return 0;
    }

    private void OnExitClick(object? sender, RoutedEventArgs e) => this.Close();

    private async Task LoadFromPathAsync(string selectedPath)
    {
      IsBusy = true;
      BusyMessage = "Starting...";
      StatusBar.StatusMessage = "Loading X4 data...";

      var loader = new DataLoader();
      loader.X4DataLoadingEvent += (s, args) =>
      {
        if (args?.ProcessingFile is { } file)
        {
          Dispatcher.UIThread.Post(() => BusyMessage = $"Processing: {file}");
        }
      };

      var gameFilesStructure = BuildGameFilesStructure(DataLoader.DefaultUniverseId);
      var processingOrder = BuildProcessingOrder();
      var galaxy = new Galaxy();

      await Task.Run(() =>
      {
        loader.LoadData(
          galaxy,
          selectedPath,
          gameFilesStructure,
          processingOrder,
          loadMods: LoadModsData,
          loadEnabledOnly: false,
          excludedMods: null
        );
      });

      _galaxy = galaxy;
      ModConnections.Clear();
      var loadedSummary =
        $"Loaded: Clusters={galaxy.Clusters.Count}, Sectors={galaxy.Sectors.Count}, Mods={galaxy.Mods.Count}, DLCs={galaxy.DLCs.Count}";
      StatusBar.StatusMessage = loadedSummary;

      if (galaxy.Version != 0)
      {
        var v = $"{galaxy.Version / 100}.{galaxy.Version % 100:D2}";
        EnsureVersionInList(v);
        if (!X4DataVersionOverride)
        {
          X4DataVersionString = v;
        }
        if (_settings != null)
        {
          _settings.Current.X4DataVersion = v;
        }
      }

      IsBusy = false;
      OnPropertyChanged(nameof(IsDataLoaded));
      SaveSettings();
    }

    private void ApplyConfigToState(AppConfig cfg)
    {
      try
      {
        // Mode
        DirectMode = cfg.Mode?.DirectMode ?? false;

        // Data
        _x4DataFolder = cfg.Data?.X4DataExtractedPath;
        _x4GameFolder = cfg.Data?.X4GameFolder;
        X4DataVersionOverride = cfg.Data?.X4DataVersionOverride ?? false;
        if (X4DataVersionOverride)
        {
          var ver = cfg.Data?.X4DataVersion ?? 0;
          if (ver > 0)
          {
            var v = $"{ver / 100}.{ver % 100:D2}";
            EnsureVersionInList(v);
            X4DataVersionString = v;
          }
        }
        LoadModsData = cfg.Data?.LoadModsData ?? false;

        // Edit
        _gatesActiveByDefault = cfg.Edit?.GatesActiveByDefault ?? true;
        _gatesMinimalDistanceBetween = cfg.Edit?.GatesMinimalDistanceBetween ?? 10;

        // Map (currently only opacity is relevant in Avalonia)
        // Universe id non-default is tracked by DataLoader.DefaultUniverseId in loader calls for now.

        // Logging (no logger wiring here; parity storage only)

        OnPropertyChanged(nameof(X4DataFolderPath));
        OnPropertyChanged(nameof(X4GameFolderPath));
      }
      catch
      {
        // ignore malformed config; defaults stay
      }
    }

    private void UpdateConfigFromState(AppConfig cfg)
    {
      cfg.Mode ??= new ModeConfig();
      cfg.Data ??= new DataConfig();
      cfg.Edit ??= new EditConfig();
      cfg.Map ??= new MapConfig();
      cfg.Logging ??= new LoggingConfig();

      // Mode
      cfg.Mode.DirectMode = DirectMode;

      // Data
      cfg.Data.X4DataExtractedPath = _x4DataFolder ?? ".";
      cfg.Data.X4GameFolder = _x4GameFolder ?? "";
      cfg.Data.X4DataVersionOverride = X4DataVersionOverride;
      if (X4DataVersionOverride)
      {
        cfg.Data.X4DataVersion = ParseVersion(X4DataVersionString);
      }
      cfg.Data.LoadModsData = LoadModsData;

      // Edit
      cfg.Edit.GatesActiveByDefault = _gatesActiveByDefault;
      cfg.Edit.GatesMinimalDistanceBetween = _gatesMinimalDistanceBetween;

      // Map: keep defaults; opacity not exposed currently to UI in Avalonia, so don't overwrite user values inadvertently
    }

    private async Task RunExtractionAsync()
    {
      // Determine game folder
      if (string.IsNullOrWhiteSpace(_x4GameFolder))
      {
        var gf = await PickFolderAsync("Select X4 game folder (contains extensions/ and content.xml)");
        if (string.IsNullOrWhiteSpace(gf))
        {
          StatusBar.StatusMessage = "Extraction cancelled.";
          return;
        }
        _x4GameFolder = gf;
        OnPropertyChanged(nameof(X4GameFolderPath));
        SaveSettings();
      }

      var gameFolder = _x4GameFolder!;
      var outDir = Path.Combine(gameFolder, "x4-extracted");
      Directory.CreateDirectory(outDir);

      IsBusy = true;
      BusyMessage = "Indexing catalogs...";
      StatusBar.StatusMessage = "Extracting core X4 data (libraries, t, maps)...";

      bool overwrite = true;
      bool verify = true;

      await Task.Run(() =>
      {
        var extractors = new List<ContentExtractor>();
        // Core game (root .cat/.dat or plain files)
        extractors.Add(new ContentExtractor(gameFolder));

        // DLCs and mods under extensions/
        var extRoot = Path.Combine(gameFolder, "extensions");
        if (Directory.Exists(extRoot))
        {
          foreach (var extDir in Directory.GetDirectories(extRoot))
          {
            try
            {
              extractors.Add(new ContentExtractor(extDir));
            }
            catch
            { /* ignore */
            }
          }
        }

        var masks = new List<string> { "libraries/*.*", "t/*.xml", $"maps/{DataLoader.DefaultUniverseId}/*.xml" };

        foreach (var extractor in extractors)
        {
          foreach (var mask in masks)
          {
            Dispatcher.UIThread.Post(() => BusyMessage = $"Extracting {mask}...");
            extractor.ExtractFilesByMask(mask, outDir, overwrite: overwrite, skipHashCheck: !verify);
          }
        }

        // Copy version.dat if present at game root
        var versionDat = Path.Combine(gameFolder, DataLoader.VersionDat);
        if (File.Exists(versionDat))
        {
          File.Copy(versionDat, Path.Combine(outDir, DataLoader.VersionDat), overwrite: true);
        }
      });

      IsBusy = false;
      StatusBar.StatusMessage = $"Extraction completed to {outDir}.";

      // Auto-select and load extracted data
      _x4DataFolder = outDir;
      OnPropertyChanged(nameof(X4DataFolderPath));
      SaveSettings();
      await LoadFromPathAsync(outDir);
    }

    private void SaveSettings()
    {
      if (_settings == null)
      {
        // still persist minimal app-data settings for Avalonia user profile convenience
        var s = _settings.Current;
        s.X4DataFolder = _x4DataFolder;
        s.X4GameFolder = _x4GameFolder;
        s.DirectMode = DirectMode;
        s.LoadModsData = LoadModsData;
        s.SelectedTabIndex = SelectedTabIndex;
        s.X4DataVersion = X4DataVersionString;
        s.X4DataVersionOverride = X4DataVersionOverride;
        s.GatesActiveByDefault = _gatesActiveByDefault;
        s.GatesMinimalDistanceBetween = _gatesMinimalDistanceBetween;
        _settings.Save();
      }

      // Save WPF-compatible configuration alongside
      UpdateConfigFromState(_config);
      ConfigService.Save(_config);
    }

    private bool _x4DataVersionOverride;
    public bool X4DataVersionOverride
    {
      get => _x4DataVersionOverride;
      set
      {
        if (_x4DataVersionOverride != value)
        {
          _x4DataVersionOverride = value;
          OnPropertyChanged();
          SaveSettings();
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
          OnPropertyChanged();
          SaveSettings();
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
          OnPropertyChanged();
          SaveSettings();
        }
      }
    }

    private void EnsureVersionInList(string version)
    {
      if (!X4DataVersions.Contains(version))
      {
        // Insert and keep ascending sort (lexicographic works for NN.MM with 2-digit minor)
        var sorted = X4DataVersions.Concat(new[] { version }).Distinct().OrderBy(v => v).ToList();
        X4DataVersions.Clear();
        foreach (var v in sorted)
          X4DataVersions.Add(v);
      }
    }

    private static List<GameFilesStructureItem> BuildGameFilesStructure(string universeId)
    {
      return new List<GameFilesStructureItem>
      {
        new(id: "translations", folder: "t", new[] { "0001-l044.xml", "0001.xml" }, MatchingModes.Exact, false),
        new(id: "colors", folder: "libraries", new[] { "colors.xml" }),
        new(id: "mapDefaults", folder: "libraries", new[] { "mapdefaults.xml" }),
        new(id: "clusters", folder: $"maps/{universeId}", new[] { "clusters.xml" }, MatchingModes.Suffix),
        new(id: "sectors", folder: $"maps/{universeId}", new[] { "sectors.xml" }, MatchingModes.Suffix),
        new(id: "zones", folder: $"maps/{universeId}", new[] { "zones.xml" }, MatchingModes.Suffix),
        new(id: "races", folder: "libraries", new[] { "races.xml" }),
        new(id: "factions", folder: "libraries", new[] { "factions.xml" }),
        new(id: "modules", folder: "libraries", new[] { "modules.xml" }),
        new(id: "modulegroups", folder: "libraries", new[] { "modulegroups.xml" }),
        new(id: "constructionplans", folder: "libraries", new[] { "constructionplans.xml" }),
        new(id: "stationgroups", folder: "libraries", new[] { "stationgroups.xml" }),
        new(id: "stations", folder: "libraries", new[] { "stations.xml" }),
        new(id: "god", folder: "libraries", new[] { "god.xml" }),
        new(id: "sechighways", folder: $"maps/{universeId}", new[] { "sechighways.xml" }, MatchingModes.Suffix),
        new(id: "zonehighways", folder: $"maps/{universeId}", new[] { "zonehighways.xml" }, MatchingModes.Suffix),
        new(id: "galaxy", folder: $"maps/{universeId}", new[] { "galaxy.xml" }),
        new(id: "patchactions", folder: "libraries", new[] { "patchactions.xml" }),
      };
    }

    private static List<ProcessingOrderItem> BuildProcessingOrder()
    {
      return new List<ProcessingOrderItem>
      {
        new("translations", ""),
        new("colors", ""),
        new("galaxy", "clusters"),
        new("clusters", ""),
        new("mapDefaults", ""),
        new("sectors", ""),
        new("zones", ""),
        new("races", ""),
        new("factions", ""),
        new("modules", ""),
        new("modulegroups", ""),
        new("constructionplans", ""),
        new("stationgroups", ""),
        new("stations", ""),
        new("god", ""),
        new("sechighways", ""),
        new("zonehighways", ""),
        new("galaxy", "gates"),
      };
    }

    public new event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
  }

  public class StatusBarMessage
  {
    public string StatusMessage { get; set; } = string.Empty;
  }

  // Minimal, WPF-independent connection data (parity with WPF GalaxyConnectionData)
  public class GalaxyConnectionDataLite
  {
    public X4DataLoader.GalaxyConnection Connection { get; }
    public string SectorDirectName => Connection?.PathDirect?.Sector?.Name ?? string.Empty;
    public string SectorOppositeName => Connection?.PathOpposite?.Sector?.Name ?? string.Empty;

    public GalaxyConnectionDataLite(X4DataLoader.GalaxyConnection c)
    {
      Connection = c;
    }
  }
}
