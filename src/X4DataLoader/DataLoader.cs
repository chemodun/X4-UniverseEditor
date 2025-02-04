using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;
using X4DataLoader.Helpers;
using Utilities.Logging;

namespace X4DataLoader
{
    public static class DataLoader
    {
        public static Galaxy LoadAllData(string coreFolderPath, Dictionary<string, (string path, string fileName)> relativePaths)
        {
            Dictionary<string, Dictionary<string, (string fullPath, string fileName)>> fileSets = GatherFiles(coreFolderPath, relativePaths);

            Log.Debug($"Analyzing the folder structure of {coreFolderPath}");
            // Scan for vanilla files
            Dictionary<string, (string fullPath, string fileName)>? vanillaFiles = new Dictionary<string, (string fullPath, string fileName)>();
            foreach (KeyValuePair<string, (string path, string fileName)> item in relativePaths)
            {
                string? filePath = Path.Combine(coreFolderPath, item.Value.path, item.Value.fileName);
                if (File.Exists(filePath))
                {
                    vanillaFiles[item.Key] = (filePath, item.Value.fileName);
                }
                else
                {
                    throw new FileNotFoundException($"File not found: {filePath}");
                }
            }
            fileSets["vanilla"] = vanillaFiles;
            Log.Debug($"Vanilla files identified.");

            // Scan for extension files
            string? extensionsFolder = Path.Combine(coreFolderPath, "extensions");
            Log.Debug($"Analyzing the folder structure of {extensionsFolder}, if it exists.");
            if (Directory.Exists(extensionsFolder))
            {
                foreach (string extensionFolder in Directory.GetDirectories(extensionsFolder))
                {
                    string? extensionName = Path.GetFileName(extensionFolder);
                    Dictionary<string, (string fullPath, string fileName)>? extensionFiles = new Dictionary<string, (string fullPath, string fileName)>();

                    foreach (KeyValuePair<string, (string path, string fileName)> item in relativePaths)
                    {
                        string? searchPath = Path.Combine(extensionFolder, item.Value.path);
                        if (Directory.Exists(searchPath))
                        {
                            string[]? files = Directory.GetFiles(searchPath, $"*{item.Value.fileName}");
                            if (files.Length > 0)
                            {
                                extensionFiles[item.Key] = (files[0], item.Value.fileName);
                            }
                        }
                    }

                    if (extensionFiles.Count > 0)
                    {
                        fileSets[extensionName] = extensionFiles;
                        Log.Debug($"Extension files identified: {extensionFiles.Count} files found for {extensionName}.");
                    }
                }
            }

            Translation? translation = new Translation();
            translation.Load(fileSets["vanilla"]["translation"].fullPath);
            Log.Debug("Translation loaded.");

            List<Cluster>? clusters = new List<Cluster>();
            List<Sector>? sectors = new List<Sector>();
            Galaxy? galaxy = new Galaxy();
            // Process each file set
            foreach (KeyValuePair<string, Dictionary<string, (string fullPath, string fileName)>> fileSet in fileSets)
            {
                string? source = fileSet.Key;

                // Process mapDefaults
                if (fileSet.Value.TryGetValue("mapDefaults", out (string fullPath, string fileName) mapDefaultsFile))
                {
                    XDocument mapDefaultsDoc;
                    try
                    {
                        mapDefaultsDoc = XDocument.Load(mapDefaultsFile.fullPath);
                    }
                    catch (ArgumentException e)
                    {
                        Log.Error($"Error loading map defaults {mapDefaultsFile.fullPath}: {e.Message}");
                        continue;
                    }
                    foreach (XElement datasetElement in mapDefaultsDoc.XPathSelectElements("/defaults/dataset"))
                    {
                        string? macro = datasetElement.Attribute("macro")?.Value;
                        if (macro != null)
                        {
                            if (Cluster.IsClusterMacro(macro))
                            {
                                Cluster? cluster = new Cluster();
                                try
                                {
                                    cluster.Load(datasetElement, translation, source, mapDefaultsFile.fileName);
                                    clusters.Add(cluster);
                                    Log.Debug($"Cluster loaded: {cluster.Name}");
                                }
                                catch (ArgumentException e)
                                {
                                    Log.Error($"Error loading cluster: {e.Message}");
                                }
                            }
                            else if (Sector.IsSectorMacro(macro))
                            {
                                Sector? sector = new Sector();
                                try
                                {
                                    sector.Load(datasetElement, translation, source, mapDefaultsFile.fileName);
                                    sectors.Add(sector);
                                    Cluster? cluster = clusters.Find(c => c.Id == sector.ClusterId);
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
                    Log.Debug($"Map Defaults loaded from: {mapDefaultsFile.fileName} for {source}");
                }

                // Process clusters
                if (fileSet.Value.TryGetValue("clusters", out (string fullPath, string fileName) clustersFile))
                {
                    XDocument clustersDoc;
                    try
                    {
                        clustersDoc = XDocument.Load(clustersFile.fullPath);
                    }
                    catch (ArgumentException e)
                    {
                        Log.Error($"Error loading clusters {clustersFile.fullPath}: {e.Message}");
                        continue;
                    }
                    foreach (XElement macroElement in clustersDoc.XPathSelectElements("/macros/macro"))
                    {
                        try
                        {
                            Connection.LoadConnections(macroElement, clusters, sectors, source, clustersFile.fileName);
                        }
                        catch (ArgumentException e)
                        {
                            Log.Error($"Error loading Cluster Connections: {e.Message}");
                        }
                    }
                    Log.Debug($"Clusters loaded from: {clustersFile.fileName} for {source}");
                }

                // Process sectors
                if (fileSet.Value.TryGetValue("sectors", out (string fullPath, string fileName) sectorsFile))
                {
                    XDocument sectorsDoc;
                    try
                    {
                        sectorsDoc = XDocument.Load(sectorsFile.fullPath);
                    }
                    catch (ArgumentException e)
                    {
                        Log.Error($"Error loading sectors {sectorsFile.fullPath}: {e.Message}");
                        continue;
                    }
                    foreach (XElement macroElement in sectorsDoc.XPathSelectElements("/macros/macro"))
                    {
                        try
                        {
                            Connection.LoadConnections(macroElement, clusters, sectors, source, sectorsFile.fileName);
                        }
                        catch (ArgumentException e)
                        {
                            Log.Error($"Error loading Sector Connections: {e.Message}");
                        }
                    }
                    Log.Debug($"Sectors loaded from: {sectorsFile.fileName} for {source}");
                }

                // Process zones
                if (fileSet.Value.TryGetValue("zones", out (string fullPath, string fileName) zonesFile))
                {
                    XDocument zonesDoc;
                    try
                    {
                        zonesDoc = XDocument.Load(zonesFile.fullPath);
                    }
                    catch (ArgumentException e)
                    {
                        Log.Error($"Error loading zones {zonesFile.fullPath}: {e.Message}");
                        continue;
                    }
                    foreach (XElement macroElement in zonesDoc.XPathSelectElements("/macros/macro"))
                    {
                        Zone? zone = new Zone();
                        zone.Load(macroElement, source, zonesFile.fileName);
                        Sector? sector = sectors
                            .FirstOrDefault(s => s.Connections.Values.Any(conn => StringHelper.EqualsIgnoreCase(conn.MacroReference, zone.Name)));
                        if (sector != null)
                        {
                            sector.AddZone(zone);
                            Log.Debug($"Zone loaded for Sector: {sector.Name}");
                        }
                        else
                        {
                            Log.Warn($"No matching sector found for Zone: {zone.Name}");
                        }
                    }
                    Log.Debug($"Zones loaded from: {zonesFile.fileName} for {source}");
                }

                // Process sechighways
                if (fileSet.Value.TryGetValue("sechighways", out (string fullPath, string fileName) sechighwaysFile))
                {
                    XDocument sechighwaysDoc;
                    try
                    {
                        sechighwaysDoc = XDocument.Load(sechighwaysFile.fullPath);
                    }
                    catch (ArgumentException e)
                    {
                        Log.Error($"Error loading sechighways {sechighwaysFile.fullPath}: {e.Message}");
                        continue;
                    }
                    foreach (XElement macroElement in sechighwaysDoc.XPathSelectElements("/macros/macro"))
                    {
                        HighwayClusterLevel? highway = new HighwayClusterLevel(macroElement, source, sechighwaysFile.fileName);
                        Cluster? cluster = clusters
                            .FirstOrDefault(c => c.Connections.Values.Any(conn => StringHelper.EqualsIgnoreCase(conn.MacroReference, highway.Macro)));
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
                    Log.Debug($"Sector Highways loaded from: {sechighwaysFile.fileName} for {source}");
                }

                // Process zonehighways
                if (fileSet.Value.TryGetValue("zonehighways", out (string fullPath, string fileName) zonehighwaysFile))
                {
                    XDocument zonehighwaysDoc;
                    try
                    {
                        zonehighwaysDoc = XDocument.Load(zonehighwaysFile.fullPath);
                    }
                    catch (ArgumentException e)
                    {
                        Log.Error($"Error loading zonehighways {zonehighwaysFile.fullPath}: {e.Message}");
                        continue;
                    }
                    foreach (XElement macroElement in zonehighwaysDoc.XPathSelectElements("/macros/macro"))
                    {
                        HighwaySectorLevel? highway = new HighwaySectorLevel(macroElement, source, zonehighwaysFile.fileName);
                        Sector? sector = sectors
                            .FirstOrDefault(s => s.Connections.Values.Any(conn => StringHelper.EqualsIgnoreCase(conn.MacroReference, highway.Macro)));
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
                    Log.Debug($"Zone Highways loaded from: {zonehighwaysFile.fileName} for {source}");
                }
                if (fileSet.Value.TryGetValue("colors", out (string fullPath, string fileName) colorsFile))
                {
                    XDocument colorsDoc;
                    try
                    {
                        colorsDoc = XDocument.Load(colorsFile.fullPath);
                    }
                    catch (ArgumentException e)
                    {
                        Log.Error($"Error loading colors {colorsFile.fullPath}: {e.Message}");
                        continue;
                    }
                    X4Color.LoadElements(colorsDoc.XPathSelectElements("/colormap/colors/color"), source, colorsFile.fileName, galaxy.Colors);
                    X4MappedColor.LoadElements(colorsDoc.XPathSelectElements("/colormap/mappings/mapping"), source, colorsFile.fileName, galaxy.MappedColors, galaxy.Colors);
                    Log.Debug($"Colors loaded from: {colorsFile.fileName} for {source}");
                }
                if (fileSet.Value.TryGetValue("races", out (string fullPath, string fileName) racesFile))
                {
                    XDocument racesDoc;
                    try
                    {
                        racesDoc = XDocument.Load(racesFile.fullPath);
                    }
                    catch (ArgumentException e)
                    {
                        Log.Error($"Error loading races {racesFile.fullPath}: {e.Message}");
                        continue;
                    }
                    IEnumerable<XElement> races = racesDoc.XPathSelectElements("/races/race");
                    if (!races.Any())
                    {
                        races = racesDoc.XPathSelectElements("/diff/add[@sel='/races']/race");
                    }
                    Race.LoadElements(races, source, racesFile.fileName, galaxy.Races, translation);
                    Log.Debug($"Races loaded from: {racesFile.fileName} for {source}");
                }
                if (fileSet.Value.TryGetValue("factions", out (string fullPath, string fileName) factionsFile))
                {
                    XDocument factionsDoc;
                    try
                    {
                        factionsDoc = XDocument.Load(factionsFile.fullPath);
                    }
                    catch (ArgumentException e)
                    {
                        Log.Error($"Error loading factions {factionsFile.fullPath}: {e.Message}");
                        continue;
                    }
                    IEnumerable<XElement> factions = factionsDoc.XPathSelectElements("/factions/faction");
                    if (!factions.Any())
                    {
                        factions = factionsDoc.XPathSelectElements("/diff/add[@sel='/factions']/faction");
                    }
                    Faction.LoadElements(factions, source, factionsFile.fileName, galaxy.Factions, translation, galaxy.Races);
                    Log.Debug($"Factions loaded from: {factionsFile.fileName} for {source}");
                }
                if (fileSet.Value.TryGetValue("modules", out (string fullPath, string fileName) modulesFile))
                {
                    XDocument modulesDoc;
                    try
                    {
                        modulesDoc = XDocument.Load(modulesFile.fullPath);
                    }
                    catch (ArgumentException e)
                    {
                        Log.Error($"Error loading modules {modulesFile.fullPath}: {e.Message}");
                        continue;
                    }
                    IEnumerable<XElement> modules = modulesDoc.XPathSelectElements("/modules/module");
                    if (!modules.Any())
                    {
                        modules = modulesDoc.XPathSelectElements("/diff/add[@sel='/modules']/module");
                    }
                    StationModule.LoadElements(modules, source, modulesFile.fileName, galaxy.StationModules);
                    Log.Debug($"Modules loaded from: {modulesFile.fileName} for {source}");
                }
                if (fileSet.Value.TryGetValue("modulegroups", out (string fullPath, string fileName) stationModuleGroupsFile))
                {
                    XDocument stationModuleGroupsDoc;
                    try
                    {
                        stationModuleGroupsDoc = XDocument.Load(stationModuleGroupsFile.fullPath);
                    }
                    catch (ArgumentException e)
                    {
                        Log.Error($"Error loading station module groups {stationModuleGroupsFile.fullPath}: {e.Message}");
                        continue;
                    }
                    StationModuleGroup.LoadElements(stationModuleGroupsDoc.XPathSelectElements("/groups/group"), source, stationModuleGroupsFile.fileName, galaxy.StationModuleGroups);
                    Log.Debug($"Station module groups loaded from: {stationModuleGroupsFile.fileName} for {source}");
                }
                if (fileSet.Value.TryGetValue("constructionplans", out (string fullPath, string fileName) constructionPlansFile))
                {
                    XDocument constructionPlansDoc;
                    try
                    {
                        constructionPlansDoc = XDocument.Load(constructionPlansFile.fullPath);
                    }
                    catch (ArgumentException e)
                    {
                        Log.Error($"Error loading construction plans {constructionPlansFile.fullPath}: {e.Message}");
                        continue;
                    }
                    ConstructionPlan.LoadElements(constructionPlansDoc.XPathSelectElements("/plans/plan"), source, constructionPlansFile.fileName, galaxy.ConstructionPlans, translation, galaxy.StationModules, galaxy.StationModuleGroups);
                    Log.Debug($"Construction plans loaded from: {constructionPlansFile.fileName} for {source}");
                }
                if (fileSet.Value.TryGetValue("stationgroups", out (string fullPath, string fileName) stationGroupsFile))
                {
                    XDocument stationGroupsDoc;
                    try
                    {
                        stationGroupsDoc = XDocument.Load(stationGroupsFile.fullPath);
                    }
                    catch (ArgumentException e)
                    {
                        Log.Error($"Error loading station groups {stationGroupsFile.fullPath}: {e.Message}");
                        continue;
                    }
                    IEnumerable<XElement> stationGroups = stationGroupsDoc.XPathSelectElements("/groups/group");
                    if (!stationGroups.Any())
                    {
                        stationGroups = stationGroupsDoc.XPathSelectElements("/diff/add[@sel='/groups']/group");
                    }
                    StationGroup.LoadElements(stationGroups, source, stationGroupsFile.fileName, galaxy.StationGroups, galaxy.ConstructionPlans);
                    Log.Debug($"Station groups loaded from: {stationGroupsFile.fileName} for {source}");
                }
                if (fileSet.Value.TryGetValue("stations", out (string fullPath, string fileName) stationCategoriesFile))
                {
                    XDocument stationCategoriesDoc;
                    try
                    {
                        stationCategoriesDoc = XDocument.Load(stationCategoriesFile.fullPath);
                    }
                    catch (ArgumentException e)
                    {
                        Log.Error($"Error loading station categories {stationCategoriesFile.fullPath}: {e.Message}");
                        continue;
                    }
                    IEnumerable<XElement> stationCategories = stationCategoriesDoc.XPathSelectElements("/stations/station");
                    if (!stationCategories.Any())
                    {
                        stationCategories = stationCategoriesDoc.XPathSelectElements("/diff/add[@sel='/stations']/station");
                    }
                    StationCategory.LoadElements(stationCategories, source, stationCategoriesFile.fileName, galaxy.StationCategories, galaxy.StationGroups);
                    Log.Debug($"Station categories loaded from: {stationCategoriesFile.fileName} for {source}");
                }
                // Process god (Stations)
                if (fileSet.Value.TryGetValue("god", out (string fullPath, string fileName) godFile))
                {
                    XDocument godDoc;
                    try
                    {
                        godDoc = XDocument.Load(godFile.fullPath);
                    }
                    catch (ArgumentException e)
                    {
                        Log.Error($"Error loading god {godFile.fullPath}: {e.Message}");
                        continue;
                    }
                    IEnumerable<XElement>? godElements = godDoc.XPathSelectElements("/god/stations/station");
                    if (!godElements.Any())
                    {
                        godElements = godDoc.XPathSelectElements("/diff/add[@sel='/god/stations']/station");
                    }
                    foreach (XElement stationElement in godElements)
                    {
                        Station? station = new Station();
                        station.Load(stationElement, source, godFile.fileName, sectors, galaxy.StationCategories, galaxy.ConstructionPlans, galaxy.Factions);
                    }
                    Log.Debug($"Stations loaded from: {godFile.fileName} for {source}");
                }

                // Process galaxy
                if (fileSet.Value.TryGetValue("galaxy", out (string fullPath, string fileName) galaxyFile))
                {
                    XDocument galaxyDoc;
                    try
                    {
                        galaxyDoc = XDocument.Load(galaxyFile.fullPath);
                    }
                    catch (ArgumentException e)
                    {
                        Log.Error($"Error loading galaxy {galaxyFile.fullPath}: {e.Message}");
                        continue;
                    }
                    XElement? galaxyElement = galaxyDoc.XPathSelectElement("/macros/macro");
                    if (galaxyElement != null)
                    {
                        galaxy.Load(galaxyElement, clusters, source, galaxyFile.fileName);
                        Log.Debug($"Galaxy loaded from: {galaxyFile.fileName} for {source}");
                    }
                    else
                    {
                        XElement? galaxyDiffElement = galaxyDoc.XPathSelectElement("/diff");
                        if (galaxyDiffElement != null)
                        {
                            IEnumerable<XElement>? galaxyDiffElements = galaxyDiffElement.Elements();
                            foreach (XElement galaxyElementDiff in galaxyDiffElements)
                            {
                                if (galaxyElementDiff.Name == "add" && galaxyElementDiff.Attribute("sel")?.Value == "/macros/macro[@name='XU_EP2_universe_macro']/connections")
                                {
                                    galaxy.LoadConnections(galaxyElementDiff, clusters, source, galaxyFile.fileName);
                                    Log.Debug($"Galaxy connections loaded from: {galaxyFile.fileName} for {source}");
                                }
                            }
                        }
                        else
                        {
                            Log.Error("Invalid galaxy file format");
                            throw new ArgumentException("Invalid galaxy file format");
                        }
                    }
                }
            }
            if (vanillaFiles.TryGetValue("patchactions", out (string fullPath, string fileName) patchActionsFile))
            {
                XDocument? patchActionsDoc = null;
                try
                {
                    patchActionsDoc = XDocument.Load(patchActionsFile.fullPath);
                }
                catch (ArgumentException e)
                {
                    Log.Warn($"Error loading patch actions {patchActionsFile.fullPath}: {e.Message}");
                }
                int version = 0;
                if (patchActionsDoc != null)
                {
                    foreach (XElement actionElement in patchActionsDoc.XPathSelectElements("/actions/action"))
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
                    Log.Debug($"Patch actions loaded from: {patchActionsFile.fileName} for vanilla. Version: {version}");
                    if (version > 0)
                    {
                        galaxy.Version = version;
                    }
                }
            }

            foreach (Sector sector in galaxy.Sectors)
            {
                sector.CalculateOwnership(galaxy.Factions);
            }

            // foreach (Faction faction in galaxy.Factions)
            // {
            //     Log.Debug($"Faction: {faction.Name}, color: {faction.ColorId}, hex: {galaxy.MappedColors.FirstOrDefault(c => c.Id == faction.ColorId)?.Hex}");
            // }
            // Load other data files (galaxy, clusters, sectors, zones, highways) similarly
            // and populate the respective properties in clusters, sectors, and zones.

            return galaxy;
        }

        public static Dictionary<string, Dictionary<string, (string fullPath, string fileName)>> GatherFiles(string coreFolderPath, Dictionary<string, (string path, string fileName)> relativePaths)
        {
            Dictionary<string, Dictionary<string, (string fullPath, string fileName)>> result = [];

            Log.Debug($"Analyzing the folder structure of {coreFolderPath}");
            // Scan for vanilla files
            Dictionary<string, (string fullPath, string fileName)>? vanillaFiles = new Dictionary<string, (string fullPath, string fileName)>();
            foreach (KeyValuePair<string, (string path, string fileName)> item in relativePaths)
            {
                string? filePath = Path.Combine(coreFolderPath, item.Value.path, item.Value.fileName);
                if (File.Exists(filePath))
                {
                    vanillaFiles[item.Key] = (filePath, item.Value.fileName);
                }
                else
                {
                    throw new FileNotFoundException($"File not found: {filePath}");
                }
            }
            result["vanilla"] = vanillaFiles;
            Log.Debug($"Vanilla files identified.");

            // Scan for extension files
            string extensionsFolder = Path.Combine(coreFolderPath, "extensions");
            Log.Debug($"Analyzing the folder structure of {extensionsFolder}, if it exists.");
            if (!Directory.Exists(extensionsFolder))
            {
                extensionsFolder = coreFolderPath;
                Log.Debug($"No extensions folder found. Will check if the dlc folders is in the {coreFolderPath}.");
            }

            foreach (string dlcFolder in Directory.GetDirectories(extensionsFolder, "ego_dlc_*"))
            {
                string? dlcName = Path.GetFileName(dlcFolder);
                Dictionary<string, (string fullPath, string fileName)>? dlcFiles = new Dictionary<string, (string fullPath, string fileName)>();

                foreach (KeyValuePair<string, (string path, string fileName)> item in relativePaths)
                {
                    string? searchPath = Path.Combine(dlcFolder, item.Value.path);
                    if (Directory.Exists(searchPath))
                    {
                        string[]? files = Directory.GetFiles(searchPath, $"*{item.Value.fileName}");
                        if (files.Length > 0)
                        {
                            dlcFiles[item.Key] = (files[0], item.Value.fileName);
                        }
                    }
                }

                if (dlcFiles.Count > 0)
                {
                    result[dlcName] = dlcFiles;
                    Log.Debug($"DLC files identified: {dlcFiles.Count} files found for {dlcName}.");
                }
            }
            return result;
        }
    }
}
