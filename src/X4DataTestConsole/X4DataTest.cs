using System;
using System.Collections.Generic;
using X4DataLoader;

namespace X4DataTestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: X4DataTestConsole <coreFolderPath>");
                return;
            }

            var coreFolderPath = args[0];
            var relativePaths = new Dictionary<string, (string path, string fileName)>
            {
                { "translation", ("t", "0001-l044.xml") },
                { "mapDefaults", ("libraries", "mapdefaults.xml") },
                { "galaxy", ("maps/xu_ep2_universe", "galaxy.xml") },
                { "clusters", ("maps/xu_ep2_universe", "clusters.xml") },
                { "sectors", ("maps/xu_ep2_universe", "sectors.xml") },
                { "zones", ("maps/xu_ep2_universe", "zones.xml") },
                { "sechighways", ("maps/xu_ep2_universe", "sechighways.xml") },
                { "zonehighways", ("maps/xu_ep2_universe", "zonehighways.xml") }
            };

            var galaxy = DataLoader.LoadAllData(coreFolderPath, relativePaths);

            // Print the loaded data
            foreach (var cluster in galaxy.Clusters)
            {
                Console.WriteLine($"Cluster: {cluster.Name}, Id: {cluster.Id}, Macro: {cluster.Macro}, Reference: {cluster.Reference}");
                foreach (var connection in cluster.Connections.Values)
                {
                    Console.WriteLine($"  Cluster Connection: {connection.Name}, Reference: {connection.Reference}, MacroReference: {connection.MacroReference}, MacroConnection: {connection.MacroConnection}");
                }
                foreach (var sector in cluster.Sectors)
                {
                    Console.WriteLine($"  Sector: {sector.Name}, Id: {sector.Id}, Macro: {sector.Macro}, Reference: {sector.Reference}");
                    foreach (var connection in sector.Connections.Values)
                    {
                        Console.WriteLine($"    Sector Connection: {connection.Name}, Reference: {connection.Reference}, MacroReference: {connection.MacroReference}, MacroConnection: {connection.MacroConnection}");
                    }
                    foreach (var zone in sector.Zones)
                    {
                        Console.WriteLine($"    Zone: {zone.Name}, Reference: {zone.Reference}");
                        foreach (var connection in zone.Connections.Values)
                        {
                            Console.WriteLine($"      Zone Connection: {connection.Name}, Reference: {connection.Reference}");
                        }
                    }
                }
            }

            foreach (var connection in galaxy.Connections)
            {
                Console.WriteLine($"Galaxy Connection: {connection.Name}");
                Console.WriteLine($"  PathDirect: {connection.PathDirect.Path}");
                Console.WriteLine($"    Cluster: {connection.PathDirect.Cluster.Name}");
                Console.WriteLine($"    Sector: {connection.PathDirect.Sector.Name}");
                Console.WriteLine($"    Zone: {connection.PathDirect.Zone.Name}");
                Console.WriteLine($"    Gate: {connection.PathDirect.Gate.Name}");
                Console.WriteLine($"  PathOpposite: {connection.PathOpposite.Path}");
                Console.WriteLine($"    Cluster: {connection.PathOpposite.Cluster.Name}");
                Console.WriteLine($"    Sector: {connection.PathOpposite.Sector.Name}");
                Console.WriteLine($"    Zone: {connection.PathOpposite.Zone.Name}");
                Console.WriteLine($"    Gate: {connection.PathOpposite.Gate.Name}");
            }
        }
    }
}
