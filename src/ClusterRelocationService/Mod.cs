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

namespace ClusterRelocationService
{
  public class RelocatedClustersMod : INotifyPropertyChanged
  {
    public static readonly string ModId = "relocated_clusters";
    public static readonly string ModName = "Relocated Clusters";
    public static readonly string ModDescription = "This extension relocates an existing Clusters to a new position on a Galaxy Map";
    public static readonly string ModAuthor = "Chem O`Dun";
    public string ModFolderPath { get; set; } = "";
    private readonly bool SelectFolder = true;
    private string Id = ModId;
    private string Name = ModName;
    private string Description = ModDescription;
    private string Author = ModAuthor;
    private int _version = 100;
    private int VersionInitial = 0;
    private string _date = "2025-09-24";
    private int _gameVersion = 710;
    private readonly string UniverseId = "";
    private readonly string Save = "false";
    private readonly string Sync = "false";
    private readonly List<string> _extensionsRequired = [];
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
    public List<RelocatedCluster> RelocatedClustersList { get; } = [];
    public XElement? XML = null;

    public RelocatedClustersMod(string path = "", string universeId = "")
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

    public bool LoadData(Galaxy galaxy, List<GameFile> baseGameFiles, RelocatedClustersMod? previousMod = null)
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
          new GameFilesStructureItem(id: "galaxy", folder: $"maps/{UniverseId}", ["galaxy.xml"]),
        ];
        DataLoader dataLoader = new();
        ExtensionInfo modRelocatedClusters = new("") { Id = ModId, Name = ModName };
        List<GameFile> existingGameFiles = GameFile.CloneList(baseGameFiles, true);
        dataLoader.GatherFiles(
          currentPath,
          gameFilesStructure,
          galaxy.Extensions,
          out int totallyPatchedFiles,
          modRelocatedClusters,
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
              case "galaxy":
                elements = file.XML.XPathSelectElements($"/macros/macro/connections/connection[@ref='clusters']");
                foreach (var element in elements)
                {
                  string name = XmlHelper.GetAttribute(element, "name") ?? "";
                  XElement? offsetElement = element.Element("offset");
                  XElement? positionElement = offsetElement?.Element("position");
                  Position position =
                    positionElement != null
                      ? new Position(
                        StringHelper.ParseDouble(positionElement.Attribute("x")?.Value ?? "0"),
                        StringHelper.ParseDouble(positionElement.Attribute("y")?.Value ?? "0"),
                        StringHelper.ParseDouble(positionElement.Attribute("z")?.Value ?? "0")
                      )
                      : new Position(0, 0, 0);

                  XElement? macroElement = element.Element("macro");
                  if (macroElement != null)
                  {
                    string macroRef = XmlHelper.GetAttribute(macroElement, "ref") ?? "";
                    string macroConnection = XmlHelper.GetAttribute(macroElement, "connection") ?? "";
                    if (macroConnection == "galaxy" && string.IsNullOrEmpty(macroRef) == false)
                    {
                      Cluster? cluster = galaxy.Clusters.FirstOrDefault(c => StringHelper.EqualsIgnoreCase(c.Macro, macroRef));
                      if (
                        cluster != null
                        && !RelocatedClustersList.Any(c => StringHelper.EqualsIgnoreCase(c.Cluster.Macro, macroRef))
                        && (position.X != cluster.Position.X || position.Z != cluster.Position.Z)
                      )
                      {
                        RelocatedCluster clusterRelocated = new(cluster);
                        cluster.SetPosition(position);
                        RelocatedClustersList.Add(clusterRelocated);
                      }
                      else
                      {
                        Log.Warn($"Cluster with macro {macroRef} already exists");
                      }
                    }
                  }
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

    public bool SaveData(Galaxy? galaxy, ObservableCollection<RelocatedCluster> relocatedClusters, bool newLocation = false)
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
      // Connections = [.. RelocatedClustersList.Select(gc => gc.Connection).Where(c => c != null)];
      Date = DateTime.Now.ToString("yyyy-MM-dd");
      VersionInitial = _version;
      RelocatedClustersList.Clear();
      foreach (RelocatedCluster cluster in relocatedClusters)
      {
        if (cluster.Cluster != null)
        {
          RelocatedClustersList.Add(cluster);
        }
      }
      SaveModXMLs(galaxy);
      OnPropertyChanged(nameof(Title));
      return true;
    }

    private XElement? ReplaceElementForPositionAxis(Galaxy galaxy, string connection, string axis, double original, double value)
    {
      if (Math.Abs(original - value) < 0.0001)
      {
        return null;
      }
      return new XElement(
        "replace",
        new XAttribute("sel", $"/macros/macro[@name='{galaxy.Name}']/connections/connection[@name='{connection}']/offset/position/@{axis}"),
        $"{value:F0}"
      );
    }

    private void SaveModXMLs(Galaxy? galaxy)
    {
      if (galaxy == null)
      {
        return;
      }
      _extensionsRequired.Clear();
      string connectionsText = "";
      XElement diffElement = new("diff");
      foreach (RelocatedCluster cluster in RelocatedClustersList)
      {
        if (cluster.Cluster != null)
        {
          bool isAdded = false;
          XElement? replaceX = ReplaceElementForPositionAxis(galaxy, cluster.Cluster.PositionId, "x", cluster.XOriginal, cluster.XCurrent);
          if (replaceX != null)
          {
            diffElement.Add(replaceX);
            isAdded = true;
          }
          XElement? replaceZ = ReplaceElementForPositionAxis(galaxy, cluster.Cluster.PositionId, "z", cluster.ZOriginal, cluster.ZCurrent);
          if (replaceZ != null)
          {
            diffElement.Add(replaceZ);
            isAdded = true;
          }
          if (isAdded)
          {
            if (!_extensionsRequired.Contains(cluster.Cluster.PositionSource))
            {
              _extensionsRequired.Add(cluster.Cluster.PositionSource);
            }
          }
        }
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
      List<ExtensionInfo> extensions = galaxy.Extensions;
      foreach (ExtensionInfo extension in extensions)
      {
        if (!_extensionsRequired.Contains(extension.Id))
        {
          continue;
        }
        XElement dependencyElement = new("dependency", new XAttribute("id", extension.Id), new XAttribute("optional", "false"));
        dependencyElement.SetAttributeValue("version", extension.Version);
        dependencyElement.SetAttributeValue("name", extension.Name);
        content.Add(dependencyElement);
      }
      XDocument docContent = new(new XDeclaration("1.0", "utf-8", null), content);
      docContent.Save(Path.Combine(ModFolderPath, DataLoader.ContentXml));
      XDocument docGalaxy = new(new XDeclaration("1.0", "utf-8", null), diffElement);
      string galaxyPath = Path.Combine(ModFolderPath, "maps", UniverseId);
      Directory.CreateDirectory(galaxyPath);
      docGalaxy.Save(Path.Combine(galaxyPath, "galaxy.xml"));
    }

    public bool IsModChanged(ObservableCollection<RelocatedCluster> relocatedClusters)
    {
      bool result = false;
      if (RelocatedClustersList.Count != relocatedClusters.Count)
      {
        result = true;
      }
      else
      {
        foreach (RelocatedCluster cluster in RelocatedClustersList)
        {
          RelocatedCluster? found = relocatedClusters.FirstOrDefault(c =>
            StringHelper.EqualsIgnoreCase(c.Cluster.Macro, cluster.Cluster.Macro)
          );
          if (found == null || PositionHelper.IsSamePosition(found.Cluster.Position, cluster.Cluster.Position) == false)
          {
            result = true;
            break;
          }
        }
        foreach (RelocatedCluster cluster in relocatedClusters)
        {
          RelocatedCluster? found = RelocatedClustersList.FirstOrDefault(c =>
            StringHelper.EqualsIgnoreCase(c.Cluster.Macro, cluster.Cluster.Macro)
          );
          if (found == null || PositionHelper.IsSamePosition(found.Cluster.Position, cluster.Cluster.Position) == false)
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
      OnPropertyChanged(nameof(Title));
      return result;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
