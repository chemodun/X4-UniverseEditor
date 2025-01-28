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
            var vanillaFiles = new Dictionary<string, (string fullPath, string fileName)>();
            foreach (var item in relativePaths)
            {
                var filePath = Path.Combine(coreFolderPath, item.Value.path, item.Value.fileName);
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
            var extensionsFolder = Path.Combine(coreFolderPath, "extensions");
            Log.Debug($"Analyzing the folder structure of {extensionsFolder}, if it exists.");
            if (Directory.Exists(extensionsFolder))
            {
                foreach (var extensionFolder in Directory.GetDirectories(extensionsFolder))
                {
                    var extensionName = Path.GetFileName(extensionFolder);
                    var extensionFiles = new Dictionary<string, (string fullPath, string fileName)>();

                    foreach (var item in relativePaths)
                    {
                        var searchPath = Path.Combine(extensionFolder, item.Value.path);
                        if (Directory.Exists(searchPath))
                        {
                            var files = Directory.GetFiles(searchPath, $"*{item.Value.fileName}");
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

            var translation = new Translation();
            translation.Load(fileSets["vanilla"]["translation"].fullPath);
            Log.Debug("Translation loaded.");

            var clusters = new List<Cluster>();
            var sectors = new List<Sector>();
            var galaxy = new Galaxy();
            // Process each file set
            foreach (var fileSet in fileSets)
            {
                var source = fileSet.Key;

                // Process mapDefaults
                if (fileSet.Value.TryGetValue("mapDefaults", out var mapDefaultsFile))
                {
                    XDocument mapDefaultsDoc;
                    try {
                        mapDefaultsDoc = XDocument.Load(mapDefaultsFile.fullPath);
                    } catch (ArgumentException e) {
                        Log.Error($"Error loading map defaults {mapDefaultsFile.fullPath}: {e.Message}");
                        continue;
                    }
                    foreach (var datasetElement in mapDefaultsDoc.XPathSelectElements("/defaults/dataset"))
                    {
                        var macro = datasetElement.Attribute("macro")?.Value;
                        if (macro != null)
                        {
                            if (Cluster.IsClusterMacro(macro))
                            {
                                var cluster = new Cluster();
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
                                var sector = new Sector();
                                try
                                {
                                    sector.Load(datasetElement, translation, source, mapDefaultsFile.fileName);
                                    sectors.Add(sector);
                                    var cluster = clusters.Find(c => c.Id == sector.ClusterId);
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
                if (fileSet.Value.TryGetValue("clusters", out var clustersFile))
                {
                    XDocument clustersDoc;
                    try {
                        clustersDoc = XDocument.Load(clustersFile.fullPath);
                    } catch (ArgumentException e) {
                        Log.Error($"Error loading clusters {clustersFile.fullPath}: {e.Message}");
                        continue;
                    }
                    foreach (var macroElement in clustersDoc.XPathSelectElements("/macros/macro"))
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
                if (fileSet.Value.TryGetValue("sectors", out var sectorsFile))
                {
                    XDocument sectorsDoc;
                    try {
                        sectorsDoc = XDocument.Load(sectorsFile.fullPath);
                    } catch (ArgumentException e) {
                        Log.Error($"Error loading sectors {sectorsFile.fullPath}: {e.Message}");
                        continue;
                    }
                    foreach (var macroElement in sectorsDoc.XPathSelectElements("/macros/macro"))
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
                if (fileSet.Value.TryGetValue("zones", out var zonesFile))
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
                    foreach (var macroElement in zonesDoc.XPathSelectElements("/macros/macro"))
                    {
                        var zone = new Zone();
                        zone.Load(macroElement, source, zonesFile.fileName);
                        var sector = sectors
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
                if (fileSet.Value.TryGetValue("sechighways", out var sechighwaysFile))
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
                    foreach (var macroElement in sechighwaysDoc.XPathSelectElements("/macros/macro"))
                    {
                        var highway = new HighwayClusterLevel(macroElement, source, sechighwaysFile.fileName);
                        var cluster = clusters
                            .FirstOrDefault(c => c.Connections.Values.Any(conn => StringHelper.EqualsIgnoreCase(conn.MacroReference, highway.Name)));
                        if (cluster != null)
                        {
                            cluster.Highways.Add(highway);
                            Log.Debug($"Sector Highway loaded for Cluster: {cluster.Name}");
                        }
                        else
                        {
                            Log.Warn($"No matching cluster found for Sector Highway: {highway.Name}");
                        }
                    }
                    Log.Debug($"Sector Highways loaded from: {sechighwaysFile.fileName} for {source}");
                }

                // Process zonehighways
                if (fileSet.Value.TryGetValue("zonehighways", out var zonehighwaysFile))
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
                    foreach (var macroElement in zonehighwaysDoc.XPathSelectElements("/macros/macro"))
                    {
                        var highway = new HighwaySectorLevel(macroElement, source, zonehighwaysFile.fileName);
                        var sector = sectors
                            .FirstOrDefault(s => s.Connections.Values.Any(conn => StringHelper.EqualsIgnoreCase(conn.MacroReference, highway.Name)));
                        if (sector != null)
                        {
                            sector.Highways.Add(highway);
                            Log.Debug($"Zone Highway loaded for Sector: {sector.Name}");
                        }
                        else
                        {
                            Log.Warn($"No matching sector found for Zone Highway: {highway.Name}");
                        }
                    }
                    Log.Debug($"Zone Highways loaded from: {zonehighwaysFile.fileName} for {source}");
                }

                // Process galaxy
                if (fileSet.Value.TryGetValue("galaxy", out var galaxyFile))
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
                    var galaxyElement = galaxyDoc.XPathSelectElement("/macros/macro");
                    if (galaxyElement != null) {
                        galaxy.Load(galaxyElement, clusters, source, galaxyFile.fileName);
                        Log.Debug($"Galaxy loaded from: {galaxyFile.fileName} for {source}");
                    }
                    else {
                        var galaxyDiffElement = galaxyDoc.XPathSelectElement("/diff");
                        if (galaxyDiffElement != null) {
                            var galaxyDiffElements = galaxyDiffElement.Elements();
                            foreach (var galaxyElementDiff in galaxyDiffElements) {
                                if (galaxyElementDiff.Name == "add" && galaxyElementDiff.Attribute("sel")?.Value == "/macros/macro[@name='XU_EP2_universe_macro']/connections") {
                                    galaxy.LoadConnections(galaxyElementDiff, clusters, source, galaxyFile.fileName);
                                    Log.Debug($"Galaxy connections loaded from: {galaxyFile.fileName} for {source}");
                                }
                            }
                        } else {
                            Log.Error("Invalid galaxy file format");
                            throw new ArgumentException("Invalid galaxy file format");
                        }
                    }
                }
            }

            // Load other data files (galaxy, clusters, sectors, zones, highways) similarly
            // and populate the respective properties in clusters, sectors, and zones.

            return galaxy;
        }

        public static Dictionary<string, Dictionary<string, (string fullPath, string fileName)>> GatherFiles(string coreFolderPath, Dictionary<string, (string path, string fileName)> relativePaths)
        {
            Dictionary<string, Dictionary<string, (string fullPath, string fileName)>> result  = [];

            Log.Debug($"Analyzing the folder structure of {coreFolderPath}");
            // Scan for vanilla files
            var vanillaFiles = new Dictionary<string, (string fullPath, string fileName)>();
            foreach (var item in relativePaths)
            {
                var filePath = Path.Combine(coreFolderPath, item.Value.path, item.Value.fileName);
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

            foreach (var dlcFolder in Directory.GetDirectories(extensionsFolder, "ego_dlc_*"))
            {
                var dlcName = Path.GetFileName(dlcFolder);
                var dlcFiles = new Dictionary<string, (string fullPath, string fileName)>();

                foreach (var item in relativePaths)
                {
                    var searchPath = Path.Combine(dlcFolder, item.Value.path);
                    if (Directory.Exists(searchPath))
                    {
                        var files = Directory.GetFiles(searchPath, $"*{item.Value.fileName}");
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
