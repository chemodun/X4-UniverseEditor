using System;
using System.Collections.Generic;
using System.Linq;
using X4DataLoader;
using Utilities.Logging;
using NLog;
using System.IO;
using System.Text;

namespace X4DataTestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            ConfigureNLog();
            var logger = LogManager.GetCurrentClassLogger();
            Log.Initialize(logger);

            if (args.Length < 1)
            {
                Console.WriteLine("Usage: X4DataTestConsole <coreFolderPath>");
                return;
            }

            var coreFolderPath = args[0];

            Log.Info("Starting to load galaxy data.");

            var galaxy = X4Galaxy.LoadData(coreFolderPath);

            // Print the loaded data
            foreach (var cluster in galaxy.Clusters)
            {
                Console.WriteLine($"Cluster: {cluster.Name}, Id: {cluster.Id}, Macro: {cluster.Macro}, Reference: {cluster.Reference}, Source: {cluster.Source}, FileName: {cluster.FileName}, Source: {cluster.Source}, FileName: {cluster.FileName}");
                foreach (var connection in cluster.Connections.Values)
                {
                    Console.WriteLine($"  Cluster Connection: {connection.Name}, Reference: {connection.Reference}, MacroReference: {connection.MacroReference}, MacroConnection: {connection.MacroConnection}, Source: {connection.Source}, FileName: {connection.FileName}");
                }
                foreach (var sector in cluster.Sectors)
                {
                    Console.WriteLine($"  Sector: {sector.Name}, Id: {sector.Id}, Macro: {sector.Macro}, Reference: {sector.Reference}, Source: {sector.Source}, FileName: {sector.FileName}");
                    foreach (var connection in sector.Connections.Values)
                    {
                        Console.WriteLine($"    Sector Connection: {connection.Name}, Reference: {connection.Reference}, MacroReference: {connection.MacroReference}, MacroConnection: {connection.MacroConnection}, Source: {connection.Source}, FileName: {connection.FileName}");
                    }
                    foreach (var zone in sector.Zones)
                    {
                        Console.WriteLine($"    Zone: {zone.Name}, Reference: {zone.Reference}, Source: {zone.Source}, FileName: {zone.FileName}");
                        foreach (var connection in zone.Connections.Values)
                        {
                            Console.WriteLine($"      Zone Connection: {connection.Name}, Reference: {connection.Reference}, Source: {connection.Source}, FileName: {connection.FileName}");
                        }
                    }
                }
            }

            foreach (var connection in galaxy.Connections)
            {
                Console.WriteLine($"Galaxy Connection: {connection.Name}, Source: {connection.Source}, FileName: {connection.FileName}");
                Console.WriteLine($"  PathDirect: {connection.PathDirect.Path}");
                Console.WriteLine($"    Cluster: {connection.PathDirect.Cluster.Name}, Source: {connection.PathDirect.Cluster.Source}, FileName: {connection.PathDirect.Cluster.FileName}");
                Console.WriteLine($"    Sector: {connection.PathDirect.Sector.Name}, Source: {connection.PathDirect.Sector.Source}, FileName: {connection.PathDirect.Sector.FileName}");
                Console.WriteLine($"    Zone: {connection.PathDirect.Zone.Name}, Source: {connection.PathDirect.Zone.Source}, FileName: {connection.PathDirect.Zone.FileName}");
                Console.WriteLine($"    Gate: {connection.PathDirect.Gate.Name}, Source: {connection.PathDirect.Gate.Source}, FileName: {connection.PathDirect.Gate.FileName}");
                Console.WriteLine($"  PathOpposite: {connection.PathOpposite.Path}");
                Console.WriteLine($"    Cluster: {connection.PathOpposite.Cluster.Name}, Source: {connection.PathOpposite.Cluster.Source}, FileName: {connection.PathOpposite.Cluster.FileName}");
                Console.WriteLine($"    Sector: {connection.PathOpposite.Sector.Name}, Source: {connection.PathOpposite.Sector.Source}, FileName: {connection.PathOpposite.Sector.FileName}");
                Console.WriteLine($"    Zone: {connection.PathOpposite.Zone.Name}, Source: {connection.PathOpposite.Zone.Source}, FileName: {connection.PathOpposite.Zone.FileName}");
                Console.WriteLine($"    Gate: {connection.PathOpposite.Gate.Name}, Source: {connection.PathOpposite.Gate.Source}, FileName: {connection.PathOpposite.Gate.FileName}");
            }
            PrepareClusterMap(galaxy);
        }
        public static void ConfigureNLog()
        {
            var logLevel = LogLevel.Debug;
            var config = new NLog.Config.LoggingConfiguration();
            var logConsole = new NLog.Targets.ConsoleTarget("logConsole")
            {
                Layout = "${time} [${level:uppercase=true}]: ${event-properties:FilePath}->${event-properties:ClassName}.${event-properties:MemberName}(): ${message} ${exception:format=toString}  "
            };
            //longdate
            config.AddRule(logLevel, LogLevel.Fatal, logConsole);
            LogManager.Configuration = config;
        }

        /// <summary>
        /// Prepares a map of clusters organized into rows and columns based on their positions.
        /// </summary>
        /// <returns>A list of rows, each containing a list of cluster names or empty strings.</returns>
        public static void PrepareClusterMap(Galaxy galaxy)
        {
            const double ColumnWidth = 15000000;  // 15,000,000 units for horizontal (X) axis
            const double RowHeight = 17320000;    // 34,640,000 units for vertical (Z) axis

            // Dictionary to hold rowId and corresponding columns
            Dictionary<string, Dictionary<string, string>> mapDict = [];
            List <string> X = [];
            List <string> Z = [];

            // Determine the rowId and columnId for each cluster and populate the dictionary
            foreach (var cluster in galaxy.Clusters)
            {
                string columnId = $"{(int)Math.Floor(cluster.Position.X / ColumnWidth)}";
                string rowId = $"{cluster.Position.Z / RowHeight:F1}";

                if (!mapDict.ContainsKey(rowId))
                {
                    mapDict[rowId] = [];
                }
                if (cluster.Sectors.Count > 1)
                {
                    mapDict[rowId][columnId] = cluster.Name;
                } else {
                    mapDict[rowId][columnId] = $"{cluster.Sectors[0].Name} ({cluster.Name})";
                }
                if (!X.Contains(rowId))
                {
                    X.Add(rowId);
                }
                if (!Z.Contains(columnId))
                {
                    Z.Add(columnId);
                }
            }
            Z.Sort((a, b) => int.Parse(a).CompareTo(int.Parse(b)));
            X.Sort((a, b) => -double.Parse(a).CompareTo(double.Parse(b)));

            var csv = new StringBuilder();
            csv.Append("Z;" + string.Join(";", Z) + "\n");
            foreach (var rowId in X)
            {
                var row = new List<string> { rowId };
                foreach (var columnId in Z)
                {
                    if (mapDict.ContainsKey(rowId) && mapDict[rowId].ContainsKey(columnId))
                        row.Add(mapDict[rowId][columnId]);
                    else
                        row.Add("");
                }
                csv.Append(string.Join(";", row) + "\n");
            }
            File.WriteAllText("clusterMap.csv", csv.ToString());
            Log.Info("Cluster Map prepared.");
        }
    }
}
