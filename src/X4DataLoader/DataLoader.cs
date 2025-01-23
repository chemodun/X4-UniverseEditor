using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;
using X4DataLoader.Helpers;

namespace X4DataLoader
{
    public static class DataLoader
    {
        public static Galaxy LoadAllData(string coreFolderPath, Dictionary<string, (string path, string fileName)> relativePaths)
        {
            var fileSets = new Dictionary<string, Dictionary<string, (string fullPath, string fileName)>>();

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

            // Scan for extension files
            var extensionsFolder = Path.Combine(coreFolderPath, "extensions");
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
                    }
                }
            }

            var translation = new Translation();
            translation.Load(fileSets["vanilla"]["translation"].fullPath);
            Console.WriteLine("Translations loaded.");

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
                    var mapDefaultsDoc = XDocument.Load(mapDefaultsFile.fullPath);
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
                                    Console.WriteLine($"Cluster loaded: {cluster.Name}");
                                }
                                catch (ArgumentException e)
                                {
                                    Console.WriteLine($"Error loading cluster: {e.Message}");
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
                                        Console.WriteLine($"Sector added: {sector.Name} to Cluster: {cluster.Name}");
                                    }
                                    Console.WriteLine($"Sector loaded: {sector.Name}");
                                }
                                catch (ArgumentException e)
                                {
                                    Console.WriteLine($"Error loading sector: {e.Message}");
                                }
                            }
                        }
                    }
                    Console.WriteLine($"Map Defaults loaded from: {mapDefaultsFile.fileName} for {source}");
                }

                // Process clusters
                if (fileSet.Value.TryGetValue("clusters", out var clustersFile))
                {
                    var clustersDoc = XDocument.Load(clustersFile.fullPath);
                    foreach (var macroElement in clustersDoc.XPathSelectElements("/macros/macro"))
                    {
                        try
                        {
                            Connection.LoadConnections(macroElement, clusters, sectors, source, clustersFile.fileName);
                        }
                        catch (ArgumentException e)
                        {
                            Console.WriteLine($"Error loading Cluster Connections: {e.Message}");
                        }
                    }
                    Console.WriteLine($"Clusters loaded from: {clustersFile.fileName} for {source}");
                }

                // Process sectors
                if (fileSet.Value.TryGetValue("sectors", out var sectorsFile))
                {
                    var sectorsDoc = XDocument.Load(sectorsFile.fullPath);
                    foreach (var macroElement in sectorsDoc.XPathSelectElements("/macros/macro"))
                    {
                        try
                        {
                            Connection.LoadConnections(macroElement, clusters, sectors, source, sectorsFile.fileName);
                        }
                        catch (ArgumentException e)
                        {
                            Console.WriteLine($"Error loading Sector Connections: {e.Message}");
                        }
                    }
                    Console.WriteLine($"Sectors loaded from: {sectorsFile.fileName} for {source}");
                }

                // Process zones
                if (fileSet.Value.TryGetValue("zones", out var zonesFile))
                {
                    var zonesDoc = XDocument.Load(zonesFile.fullPath);
                    foreach (var macroElement in zonesDoc.XPathSelectElements("/macros/macro"))
                    {
                        var zone = new Zone(macroElement, source, zonesFile.fileName);
                        var sector = sectors
                            .FirstOrDefault(s => s.Connections.Values.Any(conn => StringHelper.EqualsIgnoreCase(conn.MacroReference, zone.Name)));
                        if (sector != null)
                        {
                            sector.AddZone(zone);
                            Console.WriteLine($"Zone loaded for Sector: {sector.Name}");
                        }
                        else
                        {
                            Console.WriteLine($"No matching sector found for Zone: {zone.Name}");
                        }
                    }
                    Console.WriteLine($"Zones loaded from: {zonesFile.fileName} for {source}");
                }

                // Process sechighways
                if (fileSet.Value.TryGetValue("sechighways", out var sechighwaysFile))
                {
                    var sechighwaysDoc = XDocument.Load(sechighwaysFile.fullPath);
                    foreach (var macroElement in sechighwaysDoc.XPathSelectElements("/macros/macro"))
                    {
                        var highway = new HighwayClusterLevel(macroElement, source, sechighwaysFile.fileName);
                        var cluster = clusters
                            .FirstOrDefault(c => c.Connections.Values.Any(conn => StringHelper.EqualsIgnoreCase(conn.MacroReference, highway.Name)));
                        if (cluster != null)
                        {
                            cluster.Highways.Add(highway);
                            Console.WriteLine($"Sector Highway loaded for Cluster: {cluster.Name}");
                        }
                        else
                        {
                            Console.WriteLine($"No matching cluster found for Sector Highway: {highway.Name}");
                        }
                    }
                    Console.WriteLine($"Sector Highways loaded from: {sechighwaysFile.fileName} for {source}");
                }

                // Process zonehighways
                if (fileSet.Value.TryGetValue("zonehighways", out var zonehighwaysFile))
                {
                    var zonehighwaysDoc = XDocument.Load(zonehighwaysFile.fullPath);
                    foreach (var macroElement in zonehighwaysDoc.XPathSelectElements("/macros/macro"))
                    {
                        var highway = new HighwaySectorLevel(macroElement, source, zonehighwaysFile.fileName);
                        var sector = sectors
                            .FirstOrDefault(s => s.Connections.Values.Any(conn => StringHelper.EqualsIgnoreCase(conn.MacroReference, highway.Name)));
                        if (sector != null)
                        {
                            sector.Highways.Add(highway);
                            Console.WriteLine($"Zone Highway loaded for Sector: {sector.Name}");
                        }
                        else
                        {
                            Console.WriteLine($"No matching sector found for Zone Highway: {highway.Name}");
                        }
                    }
                    Console.WriteLine($"Zone Highways loaded from: {zonehighwaysFile.fileName} for {source}");
                }

                // Process galaxy
                if (fileSet.Value.TryGetValue("galaxy", out var galaxyFile))
                {
                    var galaxyDoc = XDocument.Load(galaxyFile.fullPath);
                    var galaxyElement = galaxyDoc.XPathSelectElement("/macros/macro");
                    if (galaxyElement != null) {
                        galaxy.Load(galaxyElement, clusters, source, galaxyFile.fileName);
                        Console.WriteLine($"Galaxy loaded: {galaxy.Name} from {galaxyFile.fileName} for {source}");
                    }
                    else {
                        var galaxyDiffElement = galaxyDoc.XPathSelectElement("/diff");
                        if (galaxyDiffElement != null) {
                            var galaxyDiffElements = galaxyDiffElement.Elements();
                            foreach (var galaxyElementDiff in galaxyDiffElements) {
                                if (galaxyElementDiff.Name == "add" && galaxyElementDiff.Attribute("sel")?.Value == "/macros/macro[@name='XU_EP2_universe_macro']/connections") {
                                    galaxy.LoadConnections(galaxyElementDiff, clusters, source, galaxyFile.fileName);
                                    Console.WriteLine($"Galaxy updated: {galaxy.Name} from {galaxyFile.fileName} for {source}");
                                }
                            }
                        } else {
                            throw new ArgumentException("Invalid galaxy file format");
                        }
                    }
                }
            }

            // Load other data files (galaxy, clusters, sectors, zones, highways) similarly
            // and populate the respective properties in clusters, sectors, and zones.

            return galaxy;
        }
    }
}
