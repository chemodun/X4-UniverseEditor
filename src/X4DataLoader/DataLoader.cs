using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;
using Utilities.Logging;
using Utilities.X4XMLPatch;
using X4DataLoader.Helpers;
using X4Unpack;

namespace X4DataLoader
{
  public class DataLoader
  {
    public static readonly string ContentXml = "content.xml";
    public static readonly string ExtensionsFolder = "extensions";
    public static readonly string DlcPrefix = "ego_dlc_";
    public static readonly string VersionDat = "version.dat";
    public static readonly string DefaultUniverseId = "xu_ep2_universe";
    private static readonly ExtensionInfo Vanilla = new("")
    {
      Name = "Vanilla",
      Id = "vanilla",
      Folder = "",
      Enabled = true,
    };
    public event EventHandler<X4DataLoadingEventArgs>? X4DataLoadingEvent;

    public void LoadData(
      Galaxy galaxy,
      string coreFolderPath,
      List<GameFilesStructureItem> gameFilesStructure,
      List<ProcessingOrderItem> processingOrder,
      bool loadMods = false,
      bool loadEnabledOnly = false,
      List<string>? excludedMods = null
    )
    {
      Log.Debug($"Starting to load galaxy data from {coreFolderPath}");
      galaxy.Clear();
      List<ExtensionInfo> allExtensions = ExtensionInfo.LoadInstalledSorted(coreFolderPath, !loadMods);
      List<GameFile> gameFiles = CollectFiles(coreFolderPath, gameFilesStructure, out int processedFiles, Vanilla, "", null);
      foreach (ExtensionInfo extension in allExtensions)
      {
        if (loadEnabledOnly && !extension.Enabled)
          continue;
        if (excludedMods != null && excludedMods.Contains(extension.Id))
          continue;
        List<GameFile> extensionFiles = GatherFiles(
          Path.Combine(coreFolderPath, ExtensionsFolder, extension.Folder),
          gameFilesStructure,
          galaxy.Extensions,
          out processedFiles,
          extension,
          gameFiles
        );
        if (processedFiles == 0)
        {
          Log.Info($"No files were loaded for extension '{extension.Name}' - this extension will be skipped.");
          continue;
        }
        gameFiles.AddRange(extensionFiles);
        if (extension.IsDlc)
        {
          galaxy.DLCs.Add(extension);
        }
        else
        {
          galaxy.Mods.Add(extension);
        }
      }
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
      // if (loadMods)
      // {
      //   LoadMods(galaxy, coreFolderPath, gameFilesStructure, gameFiles, loadEnabledOnly, excludedMods);
      // }
      List<string> sources = GameFile.GetExtensions(gameFiles);
      foreach (ProcessingOrderItem orderItem in processingOrder)
      {
        GameFilesStructureItem? fileDefinition = gameFilesStructure.FirstOrDefault(f => f.Id == orderItem.Id);
        if (fileDefinition == null)
        {
          Log.Warn($"File definition not found: {orderItem.Id}");
          continue;
        }
        List<GameFile> filesToProcess = [.. gameFiles.Where(f => f.Id == fileDefinition.Id)];
        Log.Debug($"Processing {filesToProcess.Count} files for {fileDefinition.Id}");
        foreach (GameFile file in filesToProcess)
        {
          // sourceStr = file.Source == "vanilla" && !string.IsNullOrEmpty(loadFor) ? loadFor : source;
          X4DataLoadingEvent?.Invoke(this, new X4DataLoadingEventArgs(file.FileName));
          Log.Debug($"Loading {file.FileName} for {file.Extension.Name}({file.Extension.Id})");
          switch (file.Id)
          {
            case "translations":
              galaxy.Translation.LoadFromXML(file.XML);
              Log.Debug("Translation loaded.");
              break;
            case "colors":
              X4Color.LoadFromXML(file, galaxy);
              X4MappedColor.LoadFromXML(file, galaxy);
              Log.Debug(
                $"Colors loaded (total on stage: {galaxy.Colors.Count}) from: {file.FileName} for {file.Extension.Name}({file.Extension.Id})"
              );
              break;
            case "sounds":
              X4Sound.LoadFromXML(file, galaxy);
              Log.Debug(
                $"Sounds loaded (total on stage: {galaxy.Sounds.Count}) from: {file.FileName} for {file.Extension.Name}({file.Extension.Id})"
              );
              break;
            case "icons":
              X4Icon.LoadFromXML(file, galaxy);
              Log.Debug(
                $"Icons loaded (total on stage: {galaxy.Icons.Count}) from: {file.FileName} for {file.Extension.Name}({file.Extension.Id})"
              );
              break;
            case "mapDefaults":
              foreach (XElement datasetElement in file.XML.XPathSelectElements("/defaults/dataset"))
              {
                string? macro = datasetElement.Attribute("macro")?.Value;
                if (macro != null)
                {
                  if (galaxy.Clusters.Any(c => StringHelper.EqualsIgnoreCase(c.Macro, macro)))
                  {
                    try
                    {
                      Cluster? cluster = Cluster.GetClusterByMacro(galaxy.Clusters, macro);
                      cluster?.SetDetails(datasetElement, galaxy, file.Extension.Id, file.FileName);
                      Log.Debug($"Cluster defaults loaded: {cluster?.Name}");
                    }
                    catch (ArgumentException e)
                    {
                      Log.Error($"Error loading cluster: {e.Message}");
                    }
                  }
                  else if (galaxy.Sectors.Any(s => StringHelper.EqualsIgnoreCase(s.Macro, macro)))
                  {
                    try
                    {
                      Sector? sector = Sector.GetSectorByMacro(galaxy.Sectors, macro);
                      sector?.SetDetails(datasetElement, galaxy, file.Extension.Id, file.FileName);
                      Log.Debug($"Sector defaults loaded: {sector?.Name}");
                    }
                    catch (ArgumentException e)
                    {
                      Log.Error($"Error loading sector: {e.Message}");
                    }
                  }
                }
              }
              Log.Debug($"Map Defaults loaded from: {file.FileName} for {file.Extension.Name}({file.Extension.Id})");
              break;
            case "clusters":
              Connection.LoadFromXML(file, galaxy);
              Log.Debug(
                $"Clusters connections loaded and sectors (total on stage: {galaxy.Sectors.Count}) from: {file.FileName} for {file.Extension.Name}({file.Extension.Id})"
              );
              break;
            case "sectors":
              Connection.LoadFromXML(file, galaxy);
              Log.Debug($"Sectors connections loaded from: {file.FileName} for {file.Extension.Name}({file.Extension.Id})");
              break;
            case "zones":
              Zone.LoadFromXML(file, galaxy);
              Log.Debug($"Zones loaded from: {file.FileName} for {file.Extension.Name}({file.Extension.Id})");
              break;
            case "sechighways":
              HighwayClusterLevel.LoadFromXML(file, galaxy);
              Log.Debug($"Sector Highways loaded from: {file.FileName} for {file.Extension.Name}({file.Extension.Id})");
              break;
            case "zonehighways":
              HighwaySectorLevel.LoadFromXML(file, galaxy);
              Log.Debug($"Zone Highways loaded from: {file.FileName} for {file.Extension.Name}({file.Extension.Id})");
              break;
            case "races":
              Race.LoadFromXML(file, galaxy);
              Log.Debug(
                $"Races loaded (total on stage: {galaxy.Races.Count}) from: {file.FileName} for {file.Extension.Name}({file.Extension.Id})"
              );
              break;
            case "factions":
              Faction.LoadFromXML(file, galaxy);
              Log.Debug(
                message: $"Factions loaded (total on stage: {galaxy.Factions.Count}) from: {file.FileName} for {file.Extension.Name}({file.Extension.Id})"
              );
              break;
            case "modules":
              StationModule.LoadFromXML(file, galaxy);
              Log.Debug(
                $"Station modules loaded (total on stage: {galaxy.StationModules.Count}) from: {file.FileName} for {file.Extension.Name}({file.Extension.Id})"
              );
              break;
            case "modulegroups":
              StationModuleGroup.LoadFromXML(file, galaxy);
              Log.Debug(
                $"Station module groups loaded (total on stage: {galaxy.StationModuleGroups.Count}) from: {file.FileName} for {file.Extension.Name}({file.Extension.Id})"
              );
              break;
            case "constructionplans":
              ConstructionPlan.LoadFromXML(file, galaxy);
              Log.Debug(
                $"Construction plans loaded (total on stage: {galaxy.ConstructionPlans.Count}) from: {file.FileName} for {file.Extension.Name}({file.Extension.Id})"
              );
              break;
            case "stationgroups":
              StationGroup.LoadFromXML(file, galaxy);
              Log.Debug(
                $"Station groups loaded (total on stage: {galaxy.StationGroups.Count}): from {file.FileName} for {file.Extension.Name}({file.Extension.Id})"
              );
              break;
            case "stations":
              StationCategory.LoadFromXML(file, galaxy);
              Log.Debug(
                $"Station categories loaded (total on stage: {galaxy.StationCategories.Count}) from: {file.FileName} for {file.Extension.Name}({file.Extension.Id})"
              );
              break;
            case "god":
              Station.LoadFromXML(file, galaxy);
              Log.Debug(
                $"Stations loaded (total on stage: {galaxy.Stations.Count}) from: {file.FileName} for {file.Extension.Name}({file.Extension.Id})"
              );
              break;
            case "galaxy":
              galaxy.LoadFromXML(file, galaxy, orderItem.ProcedureId);
              if (orderItem.ProcedureId == "clusters")
              {
                Log.Debug(
                  $"Galaxy and clusters (total on stage: {galaxy.Clusters.Count}) loaded from: {file.FileName} for {file.Extension.Name}({file.Extension.Id})"
                );
              }
              else
              {
                Log.Debug(
                  $"Galaxy gate connections (total on stage: {galaxy.Connections.Count}) loaded from: {file.FileName} for {file.Extension.Name}({file.Extension.Id})"
                );
              }
              break;
          }
        }
      }
      foreach (Sector sector in galaxy.Sectors)
      {
        sector.CalculateOwnership(galaxy);
      }
      galaxy.GameFiles.Clear();
      galaxy.GameFiles.AddRange(gameFiles);
    }

    private void LoadMods(
      Galaxy galaxy,
      string coreFolderPath,
      List<GameFilesStructureItem> gameFilesStructure,
      List<GameFile> gameFiles,
      bool loadEnabledOnly,
      List<string>? excludedMods
    )
    {
      string extensionsFolder = Path.Combine(coreFolderPath, ExtensionsFolder);
      List<string> skipMods = excludedMods ?? [];
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
            if (string.IsNullOrEmpty(modInfo.Id) && string.IsNullOrEmpty(modInfo.Folder))
            {
              Log.Debug($"Skipping mod {modInfo.Name} because it has no ID or folder.");
              continue;
            }
            if (loadEnabledOnly && !modInfo.Enabled)
            {
              Log.Debug($"Skipping mod {modInfo.Name} because it is disabled.");
              continue;
            }
            if (skipMods.Contains(modInfo.Id))
            {
              Log.Debug($"Skipping mod {modInfo.Name} because it is excluded.");
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
              List<ModDependency> dependencies = mods[modId].Dependencies;
              for (int i = 0; i < modsOrder.Count; i++)
              {
                ExtensionInfo currentMod = mods[modsOrder[i]];
                if (currentMod.Dependencies.Any(d => d.Id == modId))
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
          string modsOrderStr = "";
          foreach (string modId in modsOrder)
          {
            ExtensionInfo mod = mods[modId];
            modsOrderStr += (string.IsNullOrEmpty(modsOrderStr) ? "" : ", ") + $"{mod.Name}({mod.Id})";
          }
          Log.Debug($"Loading mods in the following order: {modsOrderStr}");
          foreach (string modId in modsOrder)
          {
            ExtensionInfo mod = mods[modId];
            var previousTotalFiles = gameFiles.Count;
            List<GameFile> modFiles = GatherFiles(
              Path.Combine(extensionsFolder, mod.Folder),
              gameFilesStructure,
              galaxy.Extensions,
              out int totallyPatchedFiles,
              mod,
              gameFiles
            );
            var filesLoaded = gameFiles.Count - previousTotalFiles;
            Log.Debug($"Loaded {filesLoaded} files for mod '{mod.Name}'");
            if (filesLoaded == 0)
            {
              Log.Info($"No files were loaded for mod '{mod.Name}' - this mod will be skipped.");
              continue;
            }
            galaxy.Mods.Add(mod);
          }
        }
      }
    }

    private List<GameFile> CollectFiles(
      string coreFolderPath,
      List<GameFilesStructureItem> gameFilesStructure,
      out int filesPatchedCount,
      ExtensionInfo? extension = null,
      string relatedExtensionId = "",
      List<GameFile>? exitingGameFiles = null
    )
    {
      filesPatchedCount = 0;
      extension ??= Vanilla;
      List<GameFile> result = [];
      Log.Debug($"Analyzing the folder structure of {coreFolderPath}");
      ContentExtractor contentExtractor = new(coreFolderPath);
      if (contentExtractor.FileCount == 0)
      {
        return result;
      }
      foreach (GameFilesStructureItem item in gameFilesStructure)
      {
        string folderPath = Path.Combine(coreFolderPath, item.Folder);

        if (!contentExtractor.FolderExists(item.Folder))
        {
          Log.Info($"Folder not found in catalog: {folderPath}");
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
          string[] files = [];
          List<CatEntry> catEntries = [];

          string folderMask = string.Join("/", item.Folder, fileMask);
          catEntries = contentExtractor.GetFilesByMask(folderMask);
          files = [.. catEntries.Select(e => e.FilePath)];
          Log.Debug($"Found {files.Length} files for {item.Id} in {folderPath}");
          foreach (string file in files)
          {
            try
            {
              GameFile? gameFile;

              try
              {
                CatEntry entry = catEntries.Last(e => e.FilePath == file);
                byte[] fileData = ContentExtractor.GetEntryData(entry);
                if (fileData.Length == 0)
                {
                  Log.Warn($"File {file} is empty in catalog.");
                  continue;
                }
                else if (!contentExtractor.IsHashValid(entry, fileData))
                {
                  Log.Warn($"Warning: Hash mismatch for file {file}. Skipping extraction.");
                  continue;
                }
                string fileContent = "";
                if (fileData.Length >= 3 && fileData[0] == 0xEF && fileData[1] == 0xBB && fileData[2] == 0xBF)
                {
                  // Remove BOM if present
                  fileContent = System.Text.Encoding.UTF8.GetString(fileData, 3, fileData.Length - 3);
                }
                else
                {
                  fileContent = System.Text.Encoding.UTF8.GetString(fileData);
                }
                XDocument xmlDocument = XDocument.Parse(fileContent);
                gameFile = new(item.Id, coreFolderPath, file, extension, relatedExtensionId, false, xmlDocument.Root);
              }
              catch (Exception e)
              {
                Log.Warn($"Error loading {file} from catalog: {e.Message}");
                continue;
              }
              Log.Debug($"File {file} loaded.");
              X4DataLoadingEvent?.Invoke(this, new X4DataLoadingEventArgs(gameFile.FileName));
              if (gameFile.XML.Name.ToString() == "diff" && extension.Id != "vanilla" && exitingGameFiles != null)
              {
                Log.Debug($"Merging {gameFile.FileName} for {extension.Id}");
                GameFile? existingGameFile = GameFile.GetFromList(
                  exitingGameFiles,
                  gameFile.Id,
                  relatedExtensionId == "" ? "vanilla" : relatedExtensionId
                );
                // GameFile? vanillaFile = vanillaFiles.FirstOrDefault(f => f.FileName == file.FileName);
                if (existingGameFile != null && existingGameFile.XML != null)
                {
                  Log.Debug($"Patching {gameFile.FileName} with diff {extension.Name}({extension.Id}).");
                  XElement? patchedXML = XMLPatch.ApplyPatch(existingGameFile.XML, gameFile.XML, extension.Id);
                  if (patchedXML == null)
                  {
                    result.Add(gameFile);
                    if (item.IsCountable)
                      filesPatchedCount++;
                    Log.Error($"Failed to apply patch for file {gameFile.FileName} with extension {extension.Name}({extension.Id}).");
                  }
                  else
                  {
                    existingGameFile.XML = patchedXML;
                    existingGameFile.Patched = true;
                    Log.Debug($"Patch applied for {gameFile.FileName} with extension {extension.Name}({extension.Id}).");
                    if (item.IsCountable)
                      filesPatchedCount++;
                  }
                }
              }
              else
              {
                result.Add(gameFile);
                if (item.IsCountable)
                  filesPatchedCount++;
              }
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

    public class X4DataLoadingEventArgs(string? processingFile) : EventArgs
    {
      public string? ProcessingFile { get; } = processingFile;
    }

    public static bool ValidateDataFolder(string folderPath, out string errorMessage)
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

    public List<GameFile> GatherFiles(
      string coreFolderPath,
      List<GameFilesStructureItem> gameFilesStructure,
      List<ExtensionInfo> extensions,
      out int totallyProcessedFiles,
      ExtensionInfo? extensionFor = null,
      List<GameFile>? existingGameFiles = null
    )
    {
      List<GameFile> result = [];
      existingGameFiles ??= [];
      int previousCount = result.Count;
      Log.Debug($"Previously gathered files: {previousCount}.");
      Log.Debug($"Analyzing the folder structure of {coreFolderPath}");
      List<GameFile> vanillaFiles = CollectFiles(
        coreFolderPath,
        gameFilesStructure,
        out totallyProcessedFiles,
        extensionFor,
        "vanilla",
        existingGameFiles
      );
      result.AddRange(vanillaFiles);
      existingGameFiles.AddRange(vanillaFiles);
      Log.Debug($"Vanilla files identified.");

      // Scan for extension files
      string extensionsFolder = Path.Combine(coreFolderPath, ExtensionsFolder);
      Log.Debug($"Analyzing the folder structure of {extensionsFolder}, if it exists.");

      foreach (ExtensionInfo extension in extensions)
      {
        List<GameFile> extensionFiles = [];
        string extensionFolder = Path.Combine(extensionsFolder, extension.Folder);
        extensionFiles = CollectFiles(
          extensionFolder,
          gameFilesStructure,
          out int processedFiles,
          extensionFor,
          extension.Id,
          existingGameFiles
        );
        totallyProcessedFiles += processedFiles;
        if (extensionFiles.Count > 0)
        {
          result.AddRange(extensionFiles);
          existingGameFiles.AddRange(extensionFiles);
          Log.Debug($"Extension files identified: {extensionFiles.Count} files found for {extension.Name}.");
        }
      }
      Log.Debug($"Total files added: {result.Count - previousCount} - of {coreFolderPath}");
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
    public bool Enabled { get; set; } = true;
    public bool IsDlc { get; set; }
    public List<ModDependency> Dependencies { get; set; } = [];

    public ExtensionInfo(string path)
    {
      Folder = Path.GetFileName(Path.GetDirectoryName(path)) ?? "";
      if (!string.IsNullOrEmpty(Folder))
      {
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
        if (contentElement.Attribute("enabled")?.Value == "false" || contentElement.Attribute("enabled")?.Value == "0")
        {
          Enabled = false;
        }
        IsDlc = Id.StartsWith(DataLoader.DlcPrefix, StringComparison.OrdinalIgnoreCase);
        foreach (XElement dependencyElement in contentDoc.XPathSelectElements("/content/dependency"))
        {
          versionStr = dependencyElement.Attribute("version")?.Value;
          if (
            !string.IsNullOrEmpty(versionStr)
            && int.TryParse(versionStr, out int gameVersion)
            && string.IsNullOrEmpty(dependencyElement.Attribute("id")?.Value)
          )
          {
            GameVersion = gameVersion;
          }
          else
          {
            Dependencies.Add(new ModDependency(dependencyElement));
          }
        }
      }
      else
      {
        Id = "";
        Name = "";
        GameVersion = 0;
        Version = 0;
      }
    }

    public static List<ExtensionInfo> LoadInstalledSorted(string gameRoot, bool dlcsOnly = true, bool loadEnabledOnly = false)
    {
      var result = new List<ExtensionInfo>();
      if (string.IsNullOrWhiteSpace(gameRoot) || !Directory.Exists(gameRoot))
        return result;

      var extensionsRoot = Path.Combine(gameRoot, DataLoader.ExtensionsFolder);
      if (!Directory.Exists(extensionsRoot))
        return result;

      var searchPattern = dlcsOnly ? $"{DataLoader.DlcPrefix}*" : "*";

      var allDirs = Directory.EnumerateDirectories(extensionsRoot, searchPattern, SearchOption.TopDirectoryOnly).ToList();
      if (allDirs.Count == 0)
        return result;

      var installed = new List<ExtensionInfo>();
      foreach (var dir in allDirs)
      {
        var contentXml = Path.Combine(dir, DataLoader.ContentXml);
        if (!File.Exists(contentXml))
          continue;
        try
        {
          var ext = new ExtensionInfo(contentXml);
          if (string.IsNullOrEmpty(ext.Id) && string.IsNullOrEmpty(ext.Folder))
            continue;
          if (loadEnabledOnly && !ext.Enabled)
            continue;
          if (dlcsOnly && !ext.IsDlc)
            continue;
          installed.Add(ext);
        }
        catch
        {
          // ignore malformed content.xml
        }
      }

      if (installed.Count == 0)
        return result;

      var idSet = new HashSet<string>(installed.Select(e => e.Id), StringComparer.OrdinalIgnoreCase);
      var order = TopologicallySortExtensions(
        installed,
        e => e.Dependencies.Select(d => d.Id).Where(id => idSet.Contains(id)),
        dlcFirst: !dlcsOnly
      );

      var byId = installed
        .GroupBy(e => e.Id, StringComparer.OrdinalIgnoreCase)
        .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

      foreach (var id in order)
      {
        if (byId.TryGetValue(id, out var ext))
          result.Add(ext);
      }

      return result;
    }

    private static List<string> TopologicallySortExtensions(
      List<ExtensionInfo> nodes,
      Func<ExtensionInfo, IEnumerable<string>> getDeps,
      bool dlcFirst = false
    )
    {
      var idComparer = StringComparer.OrdinalIgnoreCase;
      var idSet = new HashSet<string>(nodes.Select(n => n.Id), idComparer);
      var inDegree = new Dictionary<string, int>(idComparer);
      var dependents = new Dictionary<string, List<string>>(idComparer);

      foreach (var n in nodes)
      {
        inDegree[n.Id] = 0;
      }

      foreach (var n in nodes)
      {
        foreach (var depId in getDeps(n))
        {
          if (!idSet.Contains(depId))
            continue;
          if (!dependents.TryGetValue(depId, out var list))
          {
            list = [];
            dependents[depId] = list;
          }
          list.Add(n.Id);
          inDegree[n.Id] = inDegree.TryGetValue(n.Id, out var d) ? d + 1 : 1;
        }
      }

      // Ready queues. If dlcFirst is set, we maintain two ready sets to pick DLCs before mods
      var zeroDlc = new SortedSet<string>(idComparer);
      var zeroMod = new SortedSet<string>(idComparer);
      foreach (var kv in inDegree.Where(kv => kv.Value == 0))
      {
        var isDlc = nodes.First(n => string.Equals(n.Id, kv.Key, StringComparison.OrdinalIgnoreCase)).IsDlc;
        (isDlc ? zeroDlc : zeroMod).Add(kv.Key);
      }
      var order = new List<string>(nodes.Count);
      while ((zeroDlc.Count + zeroMod.Count) > 0)
      {
        string next;
        if (dlcFirst && zeroDlc.Count > 0)
        {
          next = zeroDlc.Min!;
          zeroDlc.Remove(next);
        }
        else
        {
          var set = zeroMod.Count > 0 ? zeroMod : zeroDlc; // if DLCs exhausted use mods (or vice versa)
          next = set.Min!;
          set.Remove(next);
        }
        order.Add(next);
        if (dependents.TryGetValue(next, out var outs))
        {
          foreach (var v in outs.OrderBy(s => s, idComparer))
          {
            inDegree[v] = inDegree[v] - 1;
            if (inDegree[v] == 0)
            {
              var isDlc = nodes.First(n => string.Equals(n.Id, v, StringComparison.OrdinalIgnoreCase)).IsDlc;
              (isDlc ? zeroDlc : zeroMod).Add(v);
            }
          }
        }
      }

      if (order.Count < nodes.Count)
      {
        foreach (var leftover in nodes.Select(n => n.Id).Except(order, idComparer).OrderBy(s => s, idComparer))
        {
          order.Add(leftover);
        }
      }

      return order;
    }
  }

  public enum MatchingModes
  {
    Exact,
    Prefix,
    Suffix,
    Mask,
  }

  public class ProcessingOrderItem(string id, string procedureId)
  {
    public string Id { get; set; } = id;
    public string ProcedureId { get; set; } = procedureId;
  }

  public class GameFilesStructureItem(
    string id,
    string folder,
    string[] possibleNames,
    MatchingModes matchingMode = MatchingModes.Exact,
    bool isCountable = true
  )
  {
    public string Id { get; set; } = id;
    public string Folder { get; set; } = folder;
    public MatchingModes MatchingMode { get; set; } = matchingMode;
    public string[] PossibleNames { get; set; } = possibleNames;
    public bool IsCountable { get; set; } = isCountable;
  }

  public class GameFile(
    string id,
    string mainFolder,
    string fullPath,
    ExtensionInfo extension,
    string relatedExtensionId = "",
    bool patched = false,
    XElement? xml = null
  )
  {
    public string Id { get; set; } = id;
    public ExtensionInfo Extension { get; set; } = extension;
    public string RelatedExtensionId { get; set; } = relatedExtensionId;
    public string PathRelative { get; set; } = string.IsNullOrEmpty(mainFolder) ? fullPath : Path.GetRelativePath(mainFolder, fullPath);
    public string FileName { get; set; } = Path.GetFileName(fullPath);
    public bool Patched { get; set; } = patched;
    public XElement XML { get; set; } = xml ?? XDocument.Load(fullPath)?.Root ?? throw new ArgumentException($"Error loading {fullPath}");

    public static GameFile? GetFromList(List<GameFile> files, string id, string extensionId, string relatedExtensionId = "")
    {
      return files.FirstOrDefault(f =>
        f.Id == id
        && f.Extension.Id == extensionId
        && (string.IsNullOrEmpty(relatedExtensionId) || f.RelatedExtensionId == relatedExtensionId)
      );
    }

    public static List<GameFile> CloneList(List<GameFile> files, bool resetPatched = false)
    {
      return
      [
        .. files.Select(f => new GameFile(
          f.Id,
          f.PathRelative,
          f.FileName,
          f.Extension,
          f.RelatedExtensionId,
          !resetPatched && f.Patched,
          f.XML
        )),
      ];
    }

    public static List<string> GetExtensions(List<GameFile> files)
    {
      return [.. files.Select(f => f.Extension.Id).Distinct()];
    }

    public static List<string> GetRelatedExtensions(List<GameFile> files, string extensionId)
    {
      return [.. files.Where(f => f.Extension.Id == extensionId).Select(f => f.RelatedExtensionId).Distinct()];
    }
  }
}
