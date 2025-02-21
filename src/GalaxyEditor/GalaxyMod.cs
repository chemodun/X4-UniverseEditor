using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Forms;
using Utilities.Logging;
using X4DataLoader;
using X4Map;

namespace GalaxyEditor
{
  public class GalaxyMod : INotifyPropertyChanged
  {
    private static readonly string ModFile = "GalaxyMod.json";
    private static readonly List<string> modAttributesToSave = new()
    {
      "Name",
      "Id",
      "Version",
      "GameVersion",
      "LanguagePage",
      "MapInfo",
      "DLCList",
      "ModList",
    };
    public event PropertyChangedEventHandler? PropertyChanged;

    private string _name = "";
    private string _id = "emptyMod";
    private int _version = 100;
    private int gameVersion = 710;
    private string _path = "";
    private int _languagePage = 999999;
    private MapInfo _mapInfo = new(0, 0, 0, 0);
    private ExtensionsInfoList _dlcList = new([]);
    private ExtensionsInfoList _modList = new([]);

    private bool _isModLoading = false;

    public GalaxyMod()
    {
      _dlcList.CollectionChanged += OnDLCListChanged;
      _modList.CollectionChanged += OnModListChanged;
    }

    public string Name
    {
      get => _name;
      set
      {
        if (_name != value)
        {
          _name = value;
          OnPropertyChanged(nameof(Name));
          Save();
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
          Save();
        }
      }
    }

    public int Version
    {
      get => _version;
      set
      {
        if (_version != value)
        {
          _version = value;
          OnPropertyChanged(nameof(Version));
          Save();
        }
      }
    }

    public int GameVersion
    {
      get => gameVersion;
      set
      {
        if (gameVersion != value)
        {
          gameVersion = value;
          OnPropertyChanged(nameof(GameVersion));
          Save();
        }
      }
    }

    public int LanguagePage
    {
      get => _languagePage;
      set
      {
        if (_languagePage != value)
        {
          _languagePage = value;
          OnPropertyChanged(nameof(LanguagePage));
          Save();
        }
      }
    }

    public string Path
    {
      get => _path;
      set
      {
        if (_path != value)
        {
          _path = value;
          OnPropertyChanged(nameof(Path));
        }
      }
    }

    public MapInfo MapInfo
    {
      get => _mapInfo;
      set
      {
        if (_mapInfo != value)
        {
          _mapInfo = value;
          OnPropertyChanged(nameof(MapInfo));
          Save();
        }
      }
    }

    public ExtensionsInfoList DLCList
    {
      get => _dlcList;
      set
      {
        if (_dlcList != value)
        {
          if (_dlcList != null)
          {
            _dlcList.CollectionChanged -= OnDLCListChanged;
          }
          _dlcList = value;
          if (_dlcList != null)
          {
            _dlcList.CollectionChanged += OnDLCListChanged;
          }
          OnPropertyChanged(nameof(DLCList));
          Save();
        }
      }
    }

    public ExtensionsInfoList ModList
    {
      get => _modList;
      set
      {
        if (_modList != value)
        {
          if (_modList != null)
          {
            _modList.CollectionChanged -= OnModListChanged;
          }
          _modList = value;
          if (_modList != null)
          {
            _modList.CollectionChanged += OnModListChanged;
          }
          OnPropertyChanged(nameof(ModList));
          Save();
        }
      }
    }

    public List<UnifyItemCluster> Clusters { get; set; } = [];

    private void OnDLCListChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
      Save();
    }

    private void OnModListChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
      Save();
    }

    public bool Create(MapInfo mapInfo, Galaxy galaxyData)
    {
      var dialog = new System.Windows.Forms.FolderBrowserDialog
      {
        Description = "Please select the folder where the mod data will be located.",
        ShowNewFolderButton = true,
        SelectedPath = "",
        RootFolder = Environment.SpecialFolder.MyDocuments,
      };
      System.Windows.Forms.DialogResult result = dialog.ShowDialog();
      Log.Debug($"Selected folder: {dialog.SelectedPath}, Result: {result}");
      if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.SelectedPath))
      {
        Path = dialog.SelectedPath;
        if (System.IO.Directory.GetFiles(Path).Length != 0 || System.IO.Directory.GetDirectories(Path).Length != 0)
        {
          var warningResult = System.Windows.Forms.MessageBox.Show(
            "The selected folder is not empty. Are you sure?",
            "Folder Not Empty",
            System.Windows.Forms.MessageBoxButtons.YesNo,
            System.Windows.Forms.MessageBoxIcon.Warning
          );
          Log.Debug($"Folder not empty warning result: {warningResult}");
          if (warningResult == System.Windows.Forms.DialogResult.No)
          {
            return false;
          }
        }
        Path = dialog.SelectedPath;
        MapInfo = mapInfo;
        GameVersion = galaxyData.Version;
        foreach (var dlc in galaxyData.DLCs)
        {
          DLCList.Add(new ExtensionInfo(dlc.Name, dlc.Id, dlc.Version, true));
        }
        foreach (var mod in galaxyData.Mods)
        {
          ModList.Add(new ExtensionInfo(mod.Name, mod.Id, mod.Version, false));
        }
        return true;
      }
      else
      {
        return false;
      }
    }

    public void Save()
    {
      if (_isModLoading)
      {
        return;
      }
      var options = new JsonSerializerOptions { WriteIndented = true };

      var modData = new Dictionary<string, object>();
      foreach (var property in modAttributesToSave)
      {
        var propInfo = this.GetType().GetProperty(property);
        if (propInfo != null)
        {
          if (propInfo != null)
          {
            var value = propInfo.GetValue(this);
            if (value != null)
            {
              modData[property] = value;
            }
          }
        }
      }
      try
      {
        Directory.CreateDirectory(Path);
      }
      catch (System.Exception ex)
      {
        Log.Error($"Error creating directory: {Path}", ex);
        return;
      }
      try
      {
        var jsonString = JsonSerializer.Serialize(modData, options);
        File.WriteAllText(System.IO.Path.Combine(Path, ModFile), jsonString);
      }
      catch (System.Exception ex)
      {
        Log.Error($"Error saving mod file: {Path}", ex);
      }
    }

    public bool Load(string path)
    {
      string workingPath = path;
      Log.Debug($"Loading mod from path: {workingPath}");
      if (string.IsNullOrWhiteSpace(workingPath))
      {
        var dialog = new System.Windows.Forms.OpenFileDialog
        {
          Filter = "Galaxy Mod File|GalaxyMod.json",
          Title = "Select a Galaxy Mod File",
          InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
        };
        System.Windows.Forms.DialogResult result = dialog.ShowDialog();
        Log.Debug($"Selected file: {dialog.FileName}, Result: {result}");
        if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.FileName))
        {
          workingPath = dialog.FileName;
        }
        if (string.IsNullOrWhiteSpace(workingPath))
        {
          Log.Warn("Loading mod cancelled. No file selected.");
          return false;
        }
      }
      else
      {
        if (!Directory.Exists(workingPath) || !File.Exists(System.IO.Path.Combine(workingPath, ModFile)))
        {
          Log.Warn($"Loading mod cancelled. Invalid path: {workingPath}");
          return false;
        }
        workingPath = System.IO.Path.Combine(workingPath, ModFile);
      }
      Path = System.IO.Path.GetDirectoryName(workingPath) ?? string.Empty;
      var jsonString = File.ReadAllText(workingPath);
      Log.Debug($"Loaded mod data. Length: {jsonString.Length}");
      try
      {
        var modData = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString);
        if (modData != null)
        {
          _isModLoading = true;
          Log.Debug($"Mod data loaded. Count: {modData.Count}");
          var options = new JsonSerializerOptions();
          options.Converters.Add(new MapInfoJsonConverter());
          options.Converters.Add(new ExtensionsInfoListJsonConverter());
          options.Converters.Add(new ExtensionInfoJsonConverter());
          foreach (var property in modAttributesToSave)
          {
            if (modData.TryGetValue(property, out object? value))
            {
              var propInfo = this.GetType().GetProperty(property);
              Log.Debug($"Setting property: {property}, Value: {value}");
              if (value is JsonElement jsonElement)
              {
                if (propInfo != null)
                {
                  var targetType = propInfo.PropertyType;
                  var convertedValue = JsonSerializer.Deserialize(jsonElement.GetRawText(), targetType, options);
                  propInfo.SetValue(this, convertedValue);
                }
              }
              else
              {
                propInfo?.SetValue(this, value);
              }
            }
          }
          _isModLoading = false;
        }
      }
      catch (System.Exception ex)
      {
        Log.Error($"Error loading mod file: {workingPath}", ex);
        _isModLoading = false;
        return false;
      }
      return true;
    }

    protected void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
