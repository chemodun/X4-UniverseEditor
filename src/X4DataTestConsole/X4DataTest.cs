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
                    foreach (var highwayPoint in sector.HighwayPoints)
                    {
                        Console.WriteLine($"    Highway Point: {highwayPoint.Name}, Level: {highwayPoint.HighwayLevel}, Type: {highwayPoint.Type}, Position: {highwayPoint.Position}, Source: {highwayPoint.Source}, FileName: {highwayPoint.FileName}, ConnectedSector: {highwayPoint.SectorConnected?.Name}");
                    }
                    var ownerStationCount = new Dictionary<string, int>();
                    List<string> toIgnoreOwners = ["player", "civilian", "khaak", "ownerless"];
                    List<string> toIgnoreTypes = ["piratebase"];
                    Dictionary<string, string> ownerReplacements = new Dictionary<string, string> { ["alliance"] = "paranid", ["ministry"] = "teladi" };
                    foreach (Station station in sector.Stations)
                    {
                        Console.WriteLine($"    Station: Id: {station.Id}, Type: {station.Type}, Race: {station.Race}, Owner: {station.OwnerId}, Position: {station.Position}, Rotation: {station.Rotation}, Source: {station.Source}, FileName: {station.FileName}");
                        if (toIgnoreOwners.Contains(station.OwnerId))
                        {
                            continue;
                        }
                        if (toIgnoreTypes.Contains(station.Type))
                        {
                            continue;
                        }
                        string owner = ownerReplacements.ContainsKey(station.OwnerId) ? ownerReplacements[station.OwnerId] : station.OwnerId;
                        if (ownerStationCount.ContainsKey(owner))
                        {
                            ownerStationCount[owner]++;
                        }
                        else
                        {
                            ownerStationCount[owner] = 1;
                        }
                    }

                    int totalCalculableStations = ownerStationCount.Values.Sum();

                    foreach (var owner in ownerStationCount.Keys)
                    {
                        double percentage = (ownerStationCount[owner] / (double)totalCalculableStations) * 100;
                        Console.WriteLine($"    Owner: {owner}, Percentage of Stations: {percentage:F2}% in Sector {sector.Name}");
                    }
                    if (ownerStationCount.Count > 0)
                    {
                        string dominantOwner = ownerStationCount.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
                        Console.WriteLine($"    Dominant Owner: {dominantOwner} with Percentage: {ownerStationCount[dominantOwner] / (double)totalCalculableStations * 100}% stations in sector {sector.Name}");
                    }
                    else
                    {
                        Console.WriteLine($"    No dominant owner in sector {sector.Name}");
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
    }
}
