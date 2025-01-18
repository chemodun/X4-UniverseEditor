using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;

namespace X4DataLoader
{
    public static class DataLoader
    {
        public static Galaxy LoadAllData(string coreFolderPath, Dictionary<string, (string path, string fileName)> relativePaths)
        {
            var translationFilePath = Path.Combine(coreFolderPath, relativePaths["translation"].path, relativePaths["translation"].fileName);
            var mapDefaultsFilePath = Path.Combine(coreFolderPath, relativePaths["mapDefaults"].path, relativePaths["mapDefaults"].fileName);
            var galaxyFilePath = Path.Combine(coreFolderPath, relativePaths["galaxy"].path, relativePaths["galaxy"].fileName);
            var clustersFilePath = Path.Combine(coreFolderPath, relativePaths["clusters"].path, relativePaths["clusters"].fileName);
            var sectorsFilePath = Path.Combine(coreFolderPath, relativePaths["sectors"].path, relativePaths["sectors"].fileName);
            var zonesFilePath = Path.Combine(coreFolderPath, relativePaths["zones"].path, relativePaths["zones"].fileName);
            var sechighwaysFilePath = Path.Combine(coreFolderPath, relativePaths["sechighways"].path, relativePaths["sechighways"].fileName);
            var zonehighwaysFilePath = Path.Combine(coreFolderPath, relativePaths["zonehighways"].path, relativePaths["zonehighways"].fileName);

            var translation = new Translation();
            translation.Load(translationFilePath);
            Console.WriteLine("Translations loaded.");

            var clusters = new List<Cluster>();
            var sectors = new List<Sector>();
            var source = "vanilla";
            var mapDefaultsDoc = XDocument.Load(mapDefaultsFilePath);

            var fileName = relativePaths["mapDefaults"].fileName;
            foreach (var datasetElement in mapDefaultsDoc.XPathSelectElements("/defaults/dataset"))
            {
                var macro = datasetElement.Attribute("macro")?.Value;
                if (macro != null)
                {
                    if (Cluster.IsClusterMacro(macro))
                    {
                        var cluster = new Cluster();
                        try {
                            cluster.Load(datasetElement, translation, source, fileName);
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
                        try {
                            sector.Load(datasetElement, translation, source, fileName);
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
            fileName = relativePaths["clusters"].fileName;
            var clustersDoc = XDocument.Load(clustersFilePath);
            foreach (var macroElement in clustersDoc.XPathSelectElements("/macros/macro"))
            {
                try {
                    Connection.LoadConnections(macroElement, clusters, sectors, source, fileName);
                }
                catch (ArgumentException e)
                {
                    Console.WriteLine($"Error loading Cluster Connections: {e.Message}");
                }
            }
            fileName = relativePaths["sectors"].fileName;
            var sectorsDoc = XDocument.Load(sectorsFilePath);
            foreach (var macroElement in sectorsDoc.XPathSelectElements("/macros/macro"))
            {
                try {
                    Connection.LoadConnections(macroElement, clusters, sectors, source, fileName);
                }
                catch (ArgumentException e)
                {
                    Console.WriteLine($"Error loading Sector Connections: {e.Message}");
                }
            }
            fileName = relativePaths["zones"].fileName;
            var zonesDoc = XDocument.Load(zonesFilePath);
            foreach (var macroElement in zonesDoc.XPathSelectElements("/macros/macro"))
            {
                var zone = new Zone(macroElement, source, fileName);
                var sector = sectors
                    .FirstOrDefault(s => s.Connections.Values.Any(conn => conn.MacroReference == zone.Name));
                if (sector != null)
                {
                    sector.Zones.Add(zone);
                    var connection = sector.Connections.Values
                        .FirstOrDefault(conn => conn.MacroReference == zone.Name);
                    if (connection != null)
                    {
                        zone.SetConnectionId(connection.Name);
                    }
                    Console.WriteLine($"Zone loaded for Sector: {sector.Name}");
                }
                else
                {
                    Console.WriteLine($"No matching sector found for Zone: {zone.Name}");
                }
            }
            fileName = relativePaths["sechighways"].fileName;
            var sechighwaysDoc = XDocument.Load(sechighwaysFilePath);
            foreach (var macroElement in sechighwaysDoc.XPathSelectElements("/macros/macro"))
            {
                var highway = new HighwayClusterLevel(macroElement, source, fileName);
                var cluster = clusters
                    .FirstOrDefault(c => c.Connections.Values.Any(conn => conn.MacroReference == highway.Name));
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
            fileName = relativePaths["zonehighways"].fileName;
            var zonehighwaysDoc = XDocument.Load(zonehighwaysFilePath);
            foreach (var macroElement in zonehighwaysDoc.XPathSelectElements("/macros/macro"))
            {
                var highway = new HighwaySectorLevel(macroElement, source, fileName);
                var sector = sectors
                    .FirstOrDefault(s => s.Connections.Values.Any(conn => conn.MacroReference == highway.Name));
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
            fileName = relativePaths["galaxy"].fileName;
            var galaxyDoc = XDocument.Load(galaxyFilePath);
            var galaxyElement = galaxyDoc.XPathSelectElement("/macros/macro");
            var galaxy = new Galaxy(galaxyElement, clusters, source, fileName);
            Console.WriteLine($"Galaxy loaded: {galaxy.Name}");

            // Load other data files (galaxy, clusters, sectors, zones, highways) similarly
            // and populate the respective properties in clusters, sectors, and zones.

            return galaxy;
        }
    }
}
