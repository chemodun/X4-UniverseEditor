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
