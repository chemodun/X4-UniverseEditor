using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;
using Utilities.Logging;
using X4DataLoader.Helpers;

namespace X4DataLoader
{
  public static class DataLoader
  {
    public static readonly string ContentXml = "content.xml";
    public static readonly string ExtensionsFolder = "extensions";
    public static readonly string DlcPrefix = "ego_dlc_";
    public static readonly string VersionDat = "version.dat";

    public static void LoadAllData(
      Galaxy galaxy,
      string coreFolderPath,
      List<GameFilesStructureItem> gameFilesStructure,
      bool loadMods = false
    )
    {
      LoadData(galaxy, coreFolderPath, gameFilesStructure);
      if (loadMods)
      {
        LoadMods(galaxy, coreFolderPath, gameFilesStructure);
      }
      foreach (Sector sector in galaxy.Sectors)
      {
        sector.CalculateOwnership(galaxy.Factions);
      }
    }

    private static void LoadData(Galaxy galaxy, string coreFolderPath, List<GameFilesStructureItem> gameFilesStructure, string loadFor = "")
    {
      List<ExtensionInfo> dlcs = [];
      List<GameFile> gameFiles = [];
      bool isMainData = string.IsNullOrEmpty(loadFor);
      List<string> sources;
      if (isMainData)
      {
        gameFiles = GatherFiles(coreFolderPath, gameFilesStructure, galaxy.DLCs);
        sources = GameFile.GetExtensions(gameFiles);
      }
      else
      {
        gameFiles = GatherFiles(coreFolderPath, gameFilesStructure, galaxy.Extensions, loadFor);
        sources = GameFile.GetRelatedExtensions(gameFiles, loadFor);
      }
      foreach (string source in sources)
      {
        string sourceStr = source;
        List<GameFile> sourceFiles = [];
        if (isMainData)
        {
          sourceFiles = gameFiles.Where(f => f.ExtensionId == sourceStr).ToList();
        }
        else
        {
          sourceFiles = gameFiles.Where(f => f.RelatedExtensionId == sourceStr).ToList();
          sourceStr = source == "vanilla" ? loadFor : source;
        }
        foreach (GameFile file in sourceFiles)
        {
          Log.Debug($"Loading {file.FileName} for {sourceStr}");
          switch (file.Id)
          {
            case "translations":
              galaxy.Translation.Load(file.XML);
              Log.Debug("Translation loaded.");
              break;
            case "colors":
              X4Color.LoadElements(file.XML.XPathSelectElements("/colormap/colors/color"), sourceStr, file.FileName, galaxy.Colors);
              X4MappedColor.LoadElements(
                file.XML.XPathSelectElements("/colormap/mappings/mapping"),
                sourceStr,
                file.FileName,
                galaxy.MappedColors,
                galaxy.Colors
              );
              Log.Debug($"Colors loaded from: {file.FileName} for {sourceStr}");
              break;
            case "mapDefaults":
              foreach (XElement datasetElement in file.XML.XPathSelectElements("/defaults/dataset"))
              {
                string? macro = datasetElement.Attribute("macro")?.Value;
                if (macro != null)
                {
                  if (Cluster.IsClusterMacro(macro))
                  {
                    Cluster? cluster = new();
                    try
                    {
                      cluster.Load(datasetElement, galaxy.Translation, sourceStr, file.FileName);
                      galaxy.Clusters.Add(cluster);
                      Log.Debug($"Cluster loaded: {cluster.Name}");
                    }
                    catch (ArgumentException e)
                    {
                      Log.Error($"Error loading cluster: {e.Message}");
                    }
                  }
                  else if (Sector.IsSectorMacro(macro))
                  {
                    Sector? sector = new();
                    try
                    {
                      sector.Load(datasetElement, galaxy.Translation, sourceStr, file.FileName);
                      galaxy.Sectors.Add(sector);
                      Cluster? cluster = Cluster.GetClusterById(galaxy.Clusters, sector.ClusterId);
                      if (cluster != null)
                      {
                        cluster.Sectors.Add(sector);
                        Log.Debug($"Sector added: {sector.Name} to Cluster: {cluster.Name}");
                      }
                      Log.Debug($"Sector loaded: {sector.Name}");
                    }
                    catch (ArgumentException e)
                    {
                      Log.Error($"Error loading sector: {e.Message}");
                    }
                  }
                }
              }
              Log.Debug($"Map Defaults loaded from: {file.FileName} for {sourceStr}");
              break;
            case "clusters":
              Connection.LoadFromXML(file.XML, galaxy.Clusters, galaxy.Sectors, sourceStr, file.FileName);
              Log.Debug($"Clusters loaded from: {file.FileName} for {sourceStr}");
              break;
            case "sectors":
              Connection.LoadFromXML(file.XML, galaxy.Clusters, galaxy.Sectors, sourceStr, file.FileName);
              Log.Debug($"Sectors loaded from: {file.FileName} for {sourceStr}");
              break;
            case "zones":
              Zone.LoadFromXML(file.XML, galaxy.Sectors, sourceStr, file.FileName);
              Log.Debug($"Zones loaded from: {file.FileName} for {sourceStr}");
              break;
            case "sechighways":
              foreach (XElement macroElement in file.XML.XPathSelectElements("/macros/macro"))
              {
                HighwayClusterLevel? highway = new(macroElement, sourceStr, file.FileName);
                Cluster? cluster = galaxy.Clusters.FirstOrDefault(c =>
                  c.Connections.Values.Any(conn => StringHelper.EqualsIgnoreCase(conn.MacroReference, highway.Macro))
                );
                if (cluster != null)
                {
                  cluster.Highways.Add(highway);
                  highway.Load(cluster);
                  Log.Debug($"Sector Highway loaded for Cluster: {cluster.Name}");
                }
                else
                {
                  Log.Warn($"No matching cluster found for Sector Highway: {highway.Macro}");
                }
              }
              Log.Debug($"Sector Highways loaded from: {file.FileName} for {sourceStr}");
              break;
            case "zonehighways":
              foreach (XElement macroElement in file.XML.XPathSelectElements("/macros/macro"))
              {
                HighwaySectorLevel? highway = new(macroElement, sourceStr, file.FileName);
                Sector? sector = galaxy.Sectors.FirstOrDefault(s =>
                  s.Connections.Values.Any(conn => StringHelper.EqualsIgnoreCase(conn.MacroReference, highway.Macro))
                );
                if (sector != null)
                {
                  sector.Highways.Add(highway);
                  Log.Debug($"Zone Highway loaded for Sector: {sector.Name}");
                }
                else
                {
                  Log.Warn($"No matching sector found for Zone Highway: {highway.Macro}");
                }
              }
              Log.Debug($"Zone Highways loaded from: {file.FileName} for {sourceStr}");
              break;
            case "races":
              IEnumerable<XElement> races = file.XML.XPathSelectElements("/races/race");
              if (!races.Any())
              {
                races = file.XML.XPathSelectElements("/diff/add[@sel='/races']/race");
              }
              Race.LoadElements(races, sourceStr, file.FileName, galaxy.Races, galaxy.Translation);
              Log.Debug($"Races loaded from: {file.FileName} for {sourceStr}");
              break;
            case "factions":
              IEnumerable<XElement> factions = file.XML.XPathSelectElements("/factions/faction");
              if (!factions.Any())
              {
                factions = file.XML.XPathSelectElements("/diff/add[@sel='/factions']/faction");
              }
              Faction.LoadElements(factions, sourceStr, file.FileName, galaxy.Factions, galaxy.Translation, galaxy.Races);
              Log.Debug($"Factions loaded from: {file.FileName} for {sourceStr}");
              break;
            case "modules":
              IEnumerable<XElement> modules = file.XML.XPathSelectElements("/modules/module");
              if (!modules.Any())
              {
                modules = file.XML.XPathSelectElements("/diff/add[@sel='/modules']/module");
              }
              StationModule.LoadElements(modules, sourceStr, file.FileName, galaxy.StationModules);
              Log.Debug($"Modules loaded from: {file.FileName} for {sourceStr}");
              break;
            case "modulegroups":
              IEnumerable<XElement> moduleGroups = file.XML.XPathSelectElements("/groups/group");
              if (!moduleGroups.Any())
              {
                moduleGroups = file.XML.XPathSelectElements("/diff/add[@sel='/groups']/group");
              }
              StationModuleGroup.LoadElements(moduleGroups, sourceStr, file.FileName, galaxy.StationModuleGroups);
              Log.Debug($"Module groups loaded from: {file.FileName} for {sourceStr}");
              break;
            case "constructionplans":
              ConstructionPlan.LoadElements(
                file.XML.XPathSelectElements("/plans/plan"),
                sourceStr,
                file.FileName,
                galaxy.ConstructionPlans,
                galaxy.Translation,
                galaxy.StationModules,
                galaxy.StationModuleGroups
              );
              Log.Debug($"Construction plans loaded from: {file.FileName} for {sourceStr}");
              break;
            case "stationgroups":
              IEnumerable<XElement> stationGroups = file.XML.XPathSelectElements("/groups/group");
              if (!stationGroups.Any())
              {
                stationGroups = file.XML.XPathSelectElements("/diff/add[@sel='/groups']/group");
              }
              StationGroup.LoadElements(stationGroups, sourceStr, file.FileName, galaxy.StationGroups, galaxy.ConstructionPlans);
              Log.Debug($"Station groups loaded from: {file.FileName} for {sourceStr}");
              break;
            case "stations":
              IEnumerable<XElement> stationCategories = file.XML.XPathSelectElements("/stations/station");
              if (!stationCategories.Any())
              {
                stationCategories = file.XML.XPathSelectElements("/diff/add[@sel='/stations']/station");
              }
              StationCategory.LoadElements(stationCategories, sourceStr, file.FileName, galaxy.StationCategories, galaxy.StationGroups);
              Log.Debug($"Station categories loaded from: {file.FileName} for {sourceStr}");
              break;
            case "god":
              IEnumerable<XElement> godElements = file.XML.XPathSelectElements("/god/stations/station");
              if (!godElements.Any())
              {
                godElements = file.XML.XPathSelectElements("/diff/add[@sel='/god/stations']/station");
              }
              if (!godElements.Any())
              {
                godElements = file.XML.XPathSelectElements("/diff/add[@sel='//god/stations']/station");
              }
              foreach (XElement stationElement in godElements)
              {
                Station? station = new();
                station.Load(
                  stationElement,
                  sourceStr,
                  file.FileName,
                  galaxy.Sectors,
                  galaxy.StationCategories,
                  galaxy.ConstructionPlans,
                  galaxy.Factions
                );
              }
              Log.Debug($"Stations loaded from: {file.FileName} for {sourceStr}");
              break;
            case "galaxy":
              if (isMainData)
              {
                galaxy.LoadXML(file.XML, galaxy.Clusters, sourceStr, file.FileName);
                Log.Debug($"Galaxy loaded from: {file.FileName} for {sourceStr}");
              }
              break;
          }
        }
      }
      if (!isMainData)
      {
        GameFile? galaxyFile = GameFile.GetFromList(gameFiles, "galaxy", loadFor, "vanilla");
        if (galaxyFile == null)
        {
          Log.Warn("No galaxy file found for vanilla.");
          return;
        }
        galaxy.LoadXML(galaxyFile.XML, galaxy.Clusters, loadFor, galaxyFile.FileName);
      }
      else
      {
        if (File.Exists(Path.Combine(coreFolderPath, VersionDat)))
        {
          string versionStr = File.ReadAllText(Path.Combine(coreFolderPath, VersionDat)).Trim();
          if (int.TryParse(versionStr, out int version))
          {
            galaxy.Version = version;
          }
        }
        if (galaxy.Version == 0)
        {
          GameFile? patchActionsFile = GameFile.GetFromList(gameFiles, "patchactions", "vanilla");
          if (patchActionsFile == null)
          {
            Log.Warn("No patch actions file found for vanilla.");
            return;
          }
          int version = 0;
          foreach (XElement actionElement in patchActionsFile.XML.XPathSelectElements("/actions/action"))
          {
            string versionStr = actionElement.Attribute("version")?.Value ?? "0";
            if (int.TryParse(versionStr, out int actionVersion))
            {
              if (actionVersion > version)
              {
                version = actionVersion;
              }
            }
          }
          Log.Debug($"Patch actions loaded from: {patchActionsFile.FileName} for vanilla. Version: {version}");
          if (version > 0)
          {
            galaxy.Version = version;
          }
        }
      }
    }

    private static void LoadMods(Galaxy galaxy, string coreFolderPath, List<GameFilesStructureItem> relativePaths)
    {
      string extensionsFolder = Path.Combine(coreFolderPath, ExtensionsFolder);
      if (Directory.Exists(extensionsFolder))
      {
        Dictionary<string, ExtensionInfo> mods = [];
        foreach (var extensionFolder in Directory.GetDirectories(extensionsFolder))
        {
          string contentPath = Path.Combine(extensionFolder, ContentXml);
          if (!Path.GetFileName(extensionFolder).StartsWith(DlcPrefix) && File.Exists(contentPath))
          {
            ExtensionInfo modInfo;
            try
            {
              modInfo = new(contentPath);
            }
            catch (ArgumentException e)
            {
              Log.Error($"Error loading mod {contentPath}: {e.Message}");
              continue;
            }
            if (modInfo.GameVersion > galaxy.Version)
            {
              Log.Debug($"Skipping mod {modInfo.Name} because it requires game version {modInfo.GameVersion}");
              continue;
            }
            bool acceptableMod = true;
            foreach (ModDependency dependency in modInfo.Dependencies)
            {
              if (dependency.Id.StartsWith(DlcPrefix) && !dependency.Optional && !galaxy.DLCs.Any(d => d.Id == dependency.Id))
              {
                Log.Debug($"Skipping mod {modInfo.Name} because it requires DLC {dependency.Id}");
                acceptableMod = false;
                break;
              }
            }
            if (!acceptableMod)
            {
              Log.Debug($"Skipping mod {modInfo.Name} because it has unmet dependencies");
              continue;
            }
            mods[modInfo.Id] = modInfo;
          }
        }
        if (mods.Count > 0)
        {
          Log.Debug($"Found {mods.Count} mods to load.");
          List<string> modsOrder = [];
          foreach (string modId in mods.Keys)
          {
            if (modsOrder.Count == 0)
            {
              modsOrder.Add(modId);
            }
            else
            {
              bool inserted = false;
              for (int i = 0; i < modsOrder.Count; i++)
              {
                if (mods[modId].Dependencies.Any(d => d.Id == modsOrder[i]))
                {
                  modsOrder.Insert(i, modId);
                  inserted = true;
                  break;
                }
              }
              if (!inserted)
              {
                modsOrder.Add(modId);
              }
            }
          }
          Log.Debug($"Loading mods in the following order: {string.Join(", ", modsOrder)}");
          foreach (string modId in modsOrder)
          {
            ExtensionInfo mod = mods[modId];
            LoadData(galaxy, Path.Combine(extensionsFolder, mod.Folder), relativePaths, mod.Id);
            galaxy.Mods.Add(mod);
          }
        }
      }
    }

    private static List<GameFile> CollectFiles(
      string coreFolderPath,
      List<GameFilesStructureItem> gameFilesStructure,
      string source = "vanilla",
      string relatedExtensionId = ""
    )
    {
      List<GameFile> result = [];
      Log.Debug($"Analyzing the folder structure of {coreFolderPath}");
      foreach (GameFilesStructureItem item in gameFilesStructure)
      {
        string folderPath = Path.Combine(coreFolderPath, item.Folder);
        if (!Directory.Exists(folderPath))
        {
          Log.Warn($"Folder not found: {folderPath}");
          continue;
        }
        foreach (string fileItem in item.PossibleNames)
        {
          string fileMask = item.MatchingMode switch
          {
            MatchingModes.Exact => fileItem,
            MatchingModes.Prefix => $"{fileItem}*",
            MatchingModes.Suffix => $"*{fileItem}",
            MatchingModes.Mask => fileItem,
            _ => throw new ArgumentException("Invalid matching mode"),
          };
          string[] files = Directory.GetFiles(folderPath, fileMask);
          Log.Debug($"Found {files.Length} files for {item.Id} in {folderPath}");
          foreach (string file in files)
          {
            try
            {
              result.Add(new GameFile(item.Id, coreFolderPath, file, source, relatedExtensionId));
            }
            catch (ArgumentException e)
            {
              Log.Error($"Error loading {file}: {e.Message}");
            }
          }
          if (files.Length > 0)
          {
            break;
          }
        }
      }
      return result;
    }

    public static List<GameFile> GatherFiles(
      string coreFolderPath,
      List<GameFilesStructureItem> gameFilesStructure,
      List<ExtensionInfo> extensions,
      string source = ""
    )
    {
      List<GameFile> result = [];

      Log.Debug($"Analyzing the folder structure of {coreFolderPath}");
      // Scan for vanilla files
      string sourceStr = string.IsNullOrEmpty(source) ? "vanilla" : source;
      string relatedExtensionId = string.IsNullOrEmpty(source) ? "" : "vanilla";
      List<GameFile> vanillaFiles = CollectFiles(coreFolderPath, gameFilesStructure, sourceStr, relatedExtensionId);
      result.AddRange(vanillaFiles);
      Log.Debug($"Vanilla files identified.");

      // Scan for extension files
      string extensionsFolder = Path.Combine(coreFolderPath, ExtensionsFolder);
      Log.Debug($"Analyzing the folder structure of {extensionsFolder}, if it exists.");

      if (string.IsNullOrEmpty(source))
      {
        foreach (string dlcFolder in Directory.GetDirectories(extensionsFolder, $"{DlcPrefix}*"))
        {
          ExtensionInfo? dlc;
          string contentPath = Path.Combine(dlcFolder, ContentXml);
          if (File.Exists(contentPath))
          {
            try
            {
              dlc = new(contentPath);
            }
            catch (ArgumentException e)
            {
              Log.Error($"Error loading DLC {contentPath}: {e.Message}");
              continue;
            }
            extensions.Add(dlc);
            Log.Debug($"DLC identified: {dlc.Name}");
            List<GameFile> dlcFiles = CollectFiles(dlcFolder, gameFilesStructure, dlc.Id, relatedExtensionId);
            if (dlcFiles.Count > 0)
            {
              result.AddRange(dlcFiles);
              Log.Debug($"DLC files identified: {dlcFiles.Count} files found for {dlc.Name}.");
            }
          }
        }
      }
      else
      {
        foreach (ExtensionInfo extension in extensions)
        {
          string dlcFolder = Path.Combine(extensionsFolder, extension.Folder);
          if (Directory.Exists(dlcFolder))
          {
            List<GameFile> dlcFiles = CollectFiles(dlcFolder, gameFilesStructure, source, extension.Id);
            if (dlcFiles.Count > 0)
            {
              result.AddRange(dlcFiles);
              Log.Debug($"DLC files identified: {dlcFiles.Count} files found for {extension.Name}.");
            }
          }
        }
      }
      return result;
    }
  }

  public class ModDependency(XElement dependencyElement)
  {
    public string Id { get; set; } = dependencyElement.Attribute("id")?.Value ?? "";
    public bool Optional { get; set; } = dependencyElement.Attribute("optional")?.Value == "true";
  }

  public class ExtensionInfo
  {
    public string Id { get; set; }
    public string Name { get; set; }
    public string Folder { get; set; }
    public int GameVersion { get; set; }
    public int Version { get; set; }
    public List<ModDependency> Dependencies { get; set; } = [];

    public ExtensionInfo(string path)
    {
      Folder = Path.GetFileName(Path.GetDirectoryName(path)) ?? throw new ArgumentException("Invalid path");
      XDocument contentDoc = XDocument.Load(path) ?? throw new ArgumentException("Invalid content.xml file");
      XElement? contentElement = contentDoc.XPathSelectElement("/content") ?? throw new ArgumentException("Invalid content.xml file");
      Id = contentElement.Attribute("id")?.Value ?? "";
      Name = contentElement.Attribute("name")?.Value ?? "";
      string? versionStr = contentElement.Attribute("version")?.Value;
      if (string.IsNullOrEmpty(Id) || string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(versionStr))
      {
        throw new ArgumentException("Invalid content.xml file");
      }
      Version = StringHelper.ParseInt(contentElement.Attribute("version")?.Value, 0);
      foreach (XElement dependencyElement in contentDoc.XPathSelectElements("/content/dependency"))
      {
        versionStr = dependencyElement.Attribute("version")?.Value;
        if (!string.IsNullOrEmpty(versionStr) && int.TryParse(versionStr, out int gameVersion))
        {
          GameVersion = gameVersion;
        }
        else
        {
          Dependencies.Add(new ModDependency(dependencyElement));
        }
      }
    }
  }

  public enum MatchingModes
  {
    Exact,
    Prefix,
    Suffix,
    Mask,
  }

  public class GameFilesStructureItem(string id, string folder, string[] possibleNames, MatchingModes matchingMode = MatchingModes.Exact)
  {
    public string Id { get; set; } = id;
    public string Folder { get; set; } = folder;
    public MatchingModes MatchingMode { get; set; } = matchingMode;
    public string[] PossibleNames { get; set; } = possibleNames;
  }

  public class GameFile(string id, string mainFolder, string fullPath, string extensionId = "", string relatedExtensionId = "")
  {
    public string Id { get; set; } = id;
    public string ExtensionId { get; set; } = extensionId;
    public string RelatedExtensionId { get; set; } = relatedExtensionId;
    public string PathRelative { get; set; } = Path.GetRelativePath(mainFolder, fullPath);
    public string FileName { get; set; } = Path.GetFileName(fullPath);
    public XElement XML { get; set; } = XDocument.Load(fullPath)?.Root ?? throw new ArgumentException($"Error loading {fullPath}");

    public static GameFile? GetFromList(List<GameFile> files, string id, string extensionId, string relatedExtensionId = "")
    {
      return files.FirstOrDefault(f =>
        f.Id == id
        && f.ExtensionId == extensionId
        && (string.IsNullOrEmpty(relatedExtensionId) || f.RelatedExtensionId == relatedExtensionId)
      );
    }

    public static List<string> GetExtensions(List<GameFile> files)
    {
      return files.Select(f => f.ExtensionId).Distinct().ToList();
    }

    public static List<string> GetRelatedExtensions(List<GameFile> files, string extensionId)
    {
      return files.Where(f => f.ExtensionId == extensionId).Select(f => f.RelatedExtensionId).Distinct().ToList();
    }
  }
}
