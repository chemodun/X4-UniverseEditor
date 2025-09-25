using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.DirectoryServices;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Xml.Linq;
using System.Xml.XPath;
using Utilities.Logging;
using X4DataLoader;
using X4DataLoader.Helpers;

namespace ChemGateBuilder
{
  public class ChemGateKeeper : INotifyPropertyChanged
  {
    public static readonly string ModId = "chem_gate_keeper";
    public static readonly string ModName = "Chem Gate Keeper";
    public static readonly string ModDescription = "This extension adds new gate connections between sectors";
    public static readonly string ModAuthor = "Chem O`Dun";
    public string ModFolderPath { get; set; } = "";
    private readonly bool SelectFolder = true;
    private string Id = ModId;
    private string Name = ModName;
    private string Description = ModDescription;
    private string Author = ModAuthor;
    private int _version = 100;
    private int VersionInitial = 0;
    private string _date = "2021-09-01";
    private int _gameVersion = 710;
    private readonly string UniverseId = "";
    private readonly string Save = "false";
    private readonly string Sync = "false";
    private readonly List<string> DlcsRequired = [];
    private readonly Dictionary<string, List<GalaxyConnectionPath>> _paths = [];
    public int Version
    {
      get => _version;
      set
      {
        _version = value;
        OnPropertyChanged(nameof(Version));
        OnPropertyChanged(nameof(Title));
      }
    }
    public string Date
    {
      get => _date;
      set
      {
        _date = value;
        OnPropertyChanged(nameof(Date));
        OnPropertyChanged(nameof(Title));
      }
    }
    public int GameVersion
    {
      get => _gameVersion;
      set
      {
        _gameVersion = value;
        OnPropertyChanged(nameof(GameVersion));
        OnPropertyChanged(nameof(Title));
      }
    }
    public string Title
    {
      get
      {
        string versionString = $" {_version / 100.0:F2}".Replace(',', '.');
        versionString += _isModChanged ? "*" : "";
        string gameVersion = $"{_gameVersion / 100.0:F2}".Replace(',', '.');
        return $"{Name} v.{versionString} built {_date} for X4: Foundations v.{gameVersion}";
      }
    }
    private bool _isModChanged = false;
    public List<GalaxyConnection> Connections = [];
    public ObservableCollection<GalaxyConnectionData> GalaxyConnections { get; } = [];
    public XElement? XML = null;

    public ChemGateKeeper(string path = "", string universeId = "")
    {
      ModFolderPath = path;
      UniverseId = string.IsNullOrEmpty(universeId) ? DataLoader.DefaultUniverseId : universeId;
      SelectFolder = !File.Exists(Path.Combine(ModFolderPath, "content.xml"));
      XML = null;
      Date = DateTime.Now.ToString("yyyy-MM-dd");
    }

    public void SetGameVersion(int version)
    {
      GameVersion = version;
    }

    public bool LoadData(Galaxy galaxy, ChemGateKeeper? previousMod = null)
    {
      string currentPath = previousMod != null ? previousMod.ModFolderPath : ModFolderPath;
      if (SelectFolder)
      {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
          InitialDirectory = string.IsNullOrEmpty(currentPath)
            ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            : currentPath,
          Filter = "Mod Content File|content.xml",
          Title = "Select a File",
        };
        bool? result = dialog.ShowDialog();
        if (result != true || string.IsNullOrWhiteSpace(dialog.FileName))
        {
          return false;
        }
        currentPath = Path.GetDirectoryName(dialog.FileName) ?? "";
      }
      else
      {
        currentPath = ModFolderPath;
      }
      try
      {
        XDocument? docContent;
        try
        {
          docContent = XDocument.Load(Path.Combine(currentPath, DataLoader.ContentXml));
        }
        catch (ArgumentException e)
        {
          Log.Error($"Error loading content file: {e.Message}");
          return false;
        }
        XElement? contentElement = docContent.Element("content");
        List<GameFilesStructureItem> gameFilesStructure =
        [
          new GameFilesStructureItem(id: "sectors", folder: $"maps/{UniverseId}", ["sectors.xml"], MatchingModes.Suffix),
          new GameFilesStructureItem(id: "zones", folder: $"maps/{UniverseId}", ["zones.xml"], MatchingModes.Suffix),
          new GameFilesStructureItem(id: "galaxy", folder: $"maps/{UniverseId}", ["galaxy.xml"]),
        ];
        DataLoader dataLoader = new();
        ExtensionInfo gateKeeper = new("") { Id = ModId, Name = ModName };
        List<GameFile> existingGameFiles = GameFile.CloneList(galaxy.GameFiles, true);
        dataLoader.GatherFiles(
          currentPath,
          gameFilesStructure,
          galaxy.Extensions,
          out int totallyPatchedFiles,
          gateKeeper,
          existingGameFiles
        );
        if (contentElement == null || totallyPatchedFiles == 0)
        {
          MessageBox.Show("The selected folder does not contain a valid mod", "Invalid Folder", MessageBoxButton.OK, MessageBoxImage.Error);
          return false;
        }
        List<GameFile> modFiles = [.. existingGameFiles.Where(f => f.Patched)];
        Id = contentElement.Attribute("id")?.Value ?? Id;
        Name = contentElement.Attribute("name")?.Value ?? Name;
        Version = int.Parse(contentElement.Attribute("version")?.Value ?? "0", System.Globalization.CultureInfo.InvariantCulture);
        Author = contentElement.Attribute("author")?.Value ?? Author;
        Date = contentElement.Attribute("date")?.Value ?? _date;
        Description = contentElement.Attribute("description")?.Value ?? Description;
        GameVersion = int.Parse(
          contentElement.Element("dependency")?.Attribute("version")?.Value ?? "710",
          System.Globalization.CultureInfo.InvariantCulture
        );
        Galaxy modGalaxy = new();
        List<ZoneConnection> zonesConnections = [];
        List<Zone> zones = [];
        foreach (GameFilesStructureItem item in gameFilesStructure)
        {
          Log.Debug($"Processing {item.Id} files");
          List<GameFile> files = [.. modFiles.Where(f => f.Id == item.Id)];
          if (files.Count == 0)
          {
            Log.Warn($"No {item.Id} files found");
            continue;
          }
          foreach (GameFile file in files)
          {
            Log.Debug($"Loading {file.FileName}, Source: {file.Extension.Name}({file.Extension.Id})");
            IEnumerable<XElement> elements = [];
            switch (file.Id)
            {
              case "sectors":
                elements = file.XML.XPathSelectElements("/macros/macro/connections/connection[@_source='chem_gate_keeper']");
                foreach (XElement element in elements)
                {
                  ZoneConnection zoneConnection = new();
                  try
                  {
                    zoneConnection.Load(element, file.Extension.Id, file.FileName);
                  }
                  catch (ArgumentException e)
                  {
                    Log.Error($"Error loading zone connection: {e.Message}");
                    continue;
                  }
                  zonesConnections.Add(zoneConnection);
                }
                break;
              case "zones":
                elements = file.XML.XPathSelectElements("/macros/macro[@_source='chem_gate_keeper']");
                foreach (XElement element in elements)
                {
                  Zone zone = new();
                  try
                  {
                    zone.Load(element, file.Extension.Id, file.FileName);
                  }
                  catch (ArgumentException e)
                  {
                    Log.Error($"Error loading zone: {e.Message}");
                    continue;
                  }
                  string zoneId = zone.Name.Replace("_macro", "_connection");
                  ZoneConnection? zoneConnection = zonesConnections.FirstOrDefault(zc => zc.Name == zoneId);
                  if (zoneConnection != null)
                  {
                    zone.SetPosition(zoneConnection.Position, zoneConnection.Name, zoneConnection.XML);
                    zones.Add(zone);
                  }
                  else
                  {
                    Log.Warn($"Zone connection not found for {zone.Name}");
                  }
                }
                break;
              case "galaxy":
                elements = file.XML.XPathSelectElements("/macros/macro/connections/connection[@_source='chem_gate_keeper']");
                foreach (XElement element in elements)
                {
                  var galaxyConnection = new GalaxyConnection();
                  galaxyConnection.Load(element, galaxy.Clusters, file.Extension.Id, file.FileName, zones);
                  string connectionsText =
                    $"\n - {galaxyConnection.PathDirect?.Sector?.Name} and {galaxyConnection.PathOpposite?.Sector?.Name}";
                  Description = Description.Replace(connectionsText, "");
                  Connections.Add(galaxyConnection);
                  GalaxyConnectionData galaxyConnectionData = new(galaxyConnection);
                  GalaxyConnections.Add(galaxyConnectionData);
                }
                break;
            }
            Log.Debug($"Elements found: {elements.Count()}");
          }
        }
        VersionInitial = _version;
      }
      catch (Exception e)
      {
        Log.Error($"Error loading mod data: {e.Message}");
        return false;
      }
      return true;
    }

    public bool SaveData(Galaxy? galaxy, bool newLocation = false)
    {
      string currentPath = ModFolderPath;
      if (string.IsNullOrEmpty(currentPath) || newLocation)
      {
        var dialog = new Microsoft.Win32.OpenFolderDialog { Title = "Select a folder for the Mod" };
        if (string.IsNullOrEmpty(currentPath))
        {
          dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }
        else
        {
          dialog.InitialDirectory = currentPath;
        }
        bool? folderSelect = dialog.ShowDialog();

        if (folderSelect == true && !string.IsNullOrWhiteSpace(dialog.FolderName))
        {
          currentPath = Path.Combine(dialog.FolderName, Id);
        }
        else
        {
          return false;
        }
      }
      if (Directory.Exists(currentPath))
      {
        string pathToContext = Path.Combine(currentPath, DataLoader.ContentXml);
        if (File.Exists(pathToContext))
        {
          var confirmOverwrite = MessageBox.Show(
            "Do you want to overwrite the existing Mod?",
            "Confirm Overwrite",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question
          );
          if (confirmOverwrite != MessageBoxResult.Yes)
          {
            return false;
          }
          // Clear the currentPath folder
          foreach (var directory in Directory.GetDirectories(currentPath))
          {
            Directory.Delete(directory, true);
          }
          foreach (var file in Directory.GetFiles(currentPath))
          {
            File.Delete(file);
          }
        }
        else
        {
          if (Directory.EnumerateFileSystemEntries(currentPath).Any())
          {
            MessageBox.Show("Folder not contains a mod, but not empty!", "Invalid Folder", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
          }
        }
      }
      else
      {
        Directory.CreateDirectory(currentPath);
      }
      ModFolderPath = currentPath;
      Connections = [.. GalaxyConnections.Select(gc => gc.Connection).Where(c => c != null)];
      Date = DateTime.Now.ToString("yyyy-MM-dd");
      VersionInitial = _version;
      SaveModXMLs(galaxy);
      bool result = !IsModChanged();
      OnPropertyChanged(nameof(Title));
      return result;
    }

    private void SaveModXMLs(Galaxy? galaxy)
    {
      if (galaxy == null)
      {
        return;
      }
      DlcsRequired.Clear();
      _paths.Clear();
      string connectionsText = "";
      XElement diffElement = new("diff");
      XElement addElement = new("add", new XAttribute("sel", $"/macros/macro[@name='{galaxy.Name}']/connections"));
      diffElement.Add(addElement);
      foreach (GalaxyConnection connection in Connections)
      {
        if (connection == null)
        {
          continue;
        }
        if (connection.PathDirect != null && connection.PathDirect.Sector != null)
        {
          if (!DlcsRequired.Contains(connection.PathDirect.Sector.Source))
          {
            DlcsRequired.Add(connection.PathDirect.Sector.Source);
          }
          connectionsText += $"\n - {connection.PathDirect.Sector.Name} and";
          if (!_paths.TryGetValue(connection.PathDirect.Sector.Source, out List<GalaxyConnectionPath>? pathItems))
          {
            pathItems = [];
            _paths.Add(connection.PathDirect.Sector.Source, pathItems);
          }

          pathItems.Add(connection.PathDirect);
        }
        if (connection.PathOpposite != null && connection.PathOpposite.Sector != null)
        {
          if (!DlcsRequired.Contains(connection.PathOpposite.Sector.Source))
          {
            DlcsRequired.Add(connection.PathOpposite.Sector.Source);
          }
          connectionsText += $" {connection.PathOpposite.Sector.Name}";
          if (!_paths.TryGetValue(connection.PathOpposite.Sector.Source, out List<GalaxyConnectionPath>? pathItems))
          {
            pathItems = [];
            _paths.Add(connection.PathOpposite.Sector.Source, pathItems);
          }

          pathItems.Add(connection.PathOpposite);
        }
        if (connection.XML == null)
        {
          continue;
        }
        XElement connectionElement = new(connection.XML);
        connectionElement.Attribute("_source")?.Remove();
        addElement.Add(connectionElement);
      }
      XElement content = new("content");
      content.SetAttributeValue("id", Id);
      content.SetAttributeValue("name", Name);
      content.SetAttributeValue("version", _version);
      content.SetAttributeValue("author", Author);
      content.SetAttributeValue("date", _date);
      content.SetAttributeValue("save", Save);
      content.SetAttributeValue("sync", Sync);
      content.SetAttributeValue("description", Description + connectionsText);
      List<int> languages = [7, 33, 37, 39, 44, 49, 55, 81, 82, 86, 88, 380];
      foreach (int language in languages)
      {
        XElement text = new("text");
        text.SetAttributeValue("language", $"{language}");
        text.SetAttributeValue("description", Description + connectionsText);
        content.Add(text);
      }
      content.Add(new XElement("dependency", new XAttribute("version", $"{GameVersion}")));
      DlcsRequired.Sort();
      List<ExtensionInfo> extensions = galaxy.Extensions;
      foreach (string dlc in DlcsRequired)
      {
        if (dlc == "vanilla")
        {
          continue;
        }
        XElement dependencyElement = new("dependency", new XAttribute("id", dlc), new XAttribute("optional", "false"));
        ExtensionInfo? extension = extensions.FirstOrDefault(e => e.Id == dlc);
        if (extension != null)
        {
          dependencyElement.SetAttributeValue("version", extension.Version);
          dependencyElement.SetAttributeValue("name", extension.Name);
        }
        content.Add(dependencyElement);
      }
      XDocument docContent = new(new XDeclaration("1.0", "utf-8", null), content);
      docContent.Save(Path.Combine(ModFolderPath, DataLoader.ContentXml));
      XDocument docGalaxy = new(new XDeclaration("1.0", "utf-8", null), diffElement);
      string galaxyPath = Path.Combine(ModFolderPath, "maps", UniverseId);
      Directory.CreateDirectory(galaxyPath);
      docGalaxy.Save(Path.Combine(galaxyPath, "galaxy.xml"));
      SaveModSectorsAndZones(galaxy);
    }

    private void SaveModSectorsAndZones(Galaxy? galaxy)
    {
      Dictionary<string, XElement> sectorCollection = [];
      Dictionary<string, XElement> zonesCollection = [];
      foreach (KeyValuePair<string, List<GalaxyConnectionPath>> path in _paths)
      {
        string sectorsKey = galaxy?.GameFiles.Any(gf => gf.Id == "sectors" && gf.Extension.Id == path.Key) == true ? path.Key : "vanilla";
        string zonesKey = galaxy?.GameFiles.Any(gf => gf.Id == "zones" && gf.Extension.Id == path.Key) == true ? path.Key : "vanilla";
        if (!sectorCollection.TryGetValue(sectorsKey, out XElement? sectors) || sectors == null)
        {
          sectors = new("diff");
          sectorCollection.Add(sectorsKey, sectors);
        }
        if (!zonesCollection.TryGetValue(zonesKey, out XElement? zones) || zones == null)
        {
          zones = new("diff");
          zonesCollection.Add(zonesKey, zones);
        }
        foreach (GalaxyConnectionPath connectionPath in path.Value)
        {
          if (connectionPath.Sector != null && connectionPath.Zone != null)
          {
            XElement sector = new("add", new XAttribute("sel", $"/macros/macro[@name='{connectionPath.Sector.Macro}']/connections"));
            if (connectionPath.Zone.PositionXML != null)
            {
              XElement connectionElement = new(connectionPath.Zone.PositionXML);
              connectionElement.Attribute("_source")?.Remove();
              sector.Add(connectionElement);
            }
            sectors.Add(sector);
            XElement zone = new("add", new XAttribute("sel", $"/macros"));
            if (connectionPath.Zone.XML != null)
            {
              XElement zoneElement = new(connectionPath.Zone.XML);
              zoneElement.Attribute("_source")?.Remove();
              zone.Add(zoneElement);
            }
            zones.Add(zone);
          }
        }
      }
      SaveModXmlCollection("sectors", sectorCollection, galaxy);
      SaveModXmlCollection("zones", zonesCollection, galaxy);
    }

    private void SaveModXmlCollection(string xmlType, Dictionary<string, XElement> xmlCollection, Galaxy? galaxy)
    {
      foreach (KeyValuePair<string, XElement> sectorEntry in xmlCollection)
      {
        string universePath = ModFolderPath;
        string filePrefix = "";
        if (sectorEntry.Key != "vanilla")
        {
          ExtensionInfo? extension = galaxy?.Extensions.FirstOrDefault(e => e.Id == sectorEntry.Key);
          if (extension != null)
          {
            universePath = Path.Combine(universePath, DataLoader.ExtensionsFolder, extension.Folder);
            GameFile? gameFile = galaxy?.GameFiles.FirstOrDefault(gf => gf.Id == xmlType && gf.Extension.Id == sectorEntry.Key);
            if (gameFile != null)
            {
              filePrefix = gameFile.FileName.Replace($"{xmlType}.xml", "");
            }
          }
        }
        universePath = Path.Combine(universePath, "maps", UniverseId);
        Directory.CreateDirectory(universePath);
        XDocument docSectors = new(new XDeclaration("1.0", "utf-8", null), sectorEntry.Value);
        docSectors.Save(Path.Combine(universePath, $"{filePrefix}{xmlType}.xml"));
      }
    }

    public bool IsModChanged()
    {
      bool result = false;
      if (Connections.Count != GalaxyConnections.Count)
      {
        result = true;
      }
      else
      {
        for (int i = 0; i < Connections.Count; i++)
        {
          if (!GalaxyConnections.Any(gc => gc.Connection != null && gc.Connection.Name == Connections[i].Name))
          {
            result = true;
            break;
          }
        }
        for (int i = 0; i < GalaxyConnections.Count; i++)
        {
          if (!Connections.Any(c => c.Name == GalaxyConnections[i].Connection.Name))
          {
            result = true;
            break;
          }
        }
        for (int i = 0; i < Connections.Count; i++)
        {
          GalaxyConnection? newConnection = GalaxyConnections.FirstOrDefault(gc => gc.Connection.Name == Connections[i].Name)?.Connection;
          if (newConnection == null)
          {
            result = true;
            break;
          }
          if (Connections[i].PathDirect?.Zone?.PositionXML?.ToString() != newConnection.PathDirect?.Zone?.PositionXML?.ToString())
          {
            result = true;
            break;
          }
          else if (Connections[i].PathDirect?.Zone?.XML?.ToString() != newConnection.PathDirect?.Zone?.XML?.ToString())
          {
            result = true;
            break;
          }
          else if (Connections[i].PathOpposite?.Zone?.PositionXML?.ToString() != newConnection.PathOpposite?.Zone?.PositionXML?.ToString())
          {
            result = true;
            break;
          }
          else if (Connections[i].PathOpposite?.Zone?.XML?.ToString() != newConnection.PathOpposite?.Zone?.XML?.ToString())
          {
            result = true;
            break;
          }
        }
      }
      if (result)
      {
        Log.Debug("Mod has been changed");
        Version = VersionInitial == 0 ? 100 : (VersionInitial + 1);
      }
      else
      {
        Log.Debug("Mod has not been changed");
        Version = VersionInitial == 0 ? 100 : VersionInitial;
      }
      _isModChanged = result;
      return result;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
