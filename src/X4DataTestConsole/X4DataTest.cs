using System;
using System.Collections.Generic;
using NLog;
using Utilities.Logging;
using X4DataLoader;

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

      Console.WriteLine("Starting to load galaxy data.");

      List<GameFilesStructureItem> gameFilesStructure =
      [
        new GameFilesStructureItem(id: "translations", folder: "t", ["0001-l044.xml", "0001.xml"]),
        new GameFilesStructureItem(id: "colors", folder: "libraries", ["colors.xml"]),
        new GameFilesStructureItem(id: "mapDefaults", folder: "libraries", ["mapdefaults.xml"]),
        new GameFilesStructureItem(id: "clusters", folder: "maps/xu_ep2_universe", ["clusters.xml"], MatchingModes.Suffix),
        new GameFilesStructureItem(id: "sectors", folder: "maps/xu_ep2_universe", ["sectors.xml"], MatchingModes.Suffix),
        new GameFilesStructureItem(id: "zones", folder: "maps/xu_ep2_universe", ["zones.xml"], MatchingModes.Suffix),
        new GameFilesStructureItem(id: "races", folder: "libraries", ["races.xml"]),
        new GameFilesStructureItem(id: "factions", folder: "libraries", ["factions.xml"]),
        new GameFilesStructureItem(id: "modules", folder: "libraries", ["modules.xml"]),
        new GameFilesStructureItem(id: "modulegroups", folder: "libraries", ["modulegroups.xml"]),
        new GameFilesStructureItem(id: "constructionplans", folder: "libraries", ["constructionplans.xml"]),
        new GameFilesStructureItem(id: "stationgroups", folder: "libraries", ["stationgroups.xml"]),
        new GameFilesStructureItem(id: "stations", folder: "libraries", ["stations.xml"]),
        new GameFilesStructureItem(id: "god", folder: "libraries", ["god.xml"]),
        new GameFilesStructureItem(id: "sechighways", folder: "maps/xu_ep2_universe", ["sechighways.xml"], MatchingModes.Suffix),
        new GameFilesStructureItem(id: "zonehighways", folder: "maps/xu_ep2_universe", ["zonehighways.xml"], MatchingModes.Suffix),
        new GameFilesStructureItem(id: "galaxy", folder: "maps/xu_ep2_universe", ["galaxy.xml"]),
        new GameFilesStructureItem(id: "patchactions", folder: "libraries", ["patchactions.xml"]),
      ];

      var galaxy = X4Galaxy.LoadData(coreFolderPath, gameFilesStructure);

      // Print the loaded data
      foreach (var cluster in galaxy.Clusters)
      {
        Console.WriteLine(
          $"Cluster: {cluster.Name}, Id: {cluster.Id}, Macro: {cluster.Macro}, Reference: {cluster.Reference}, Source: {cluster.Source}, FileName: {cluster.FileName}, Source: {cluster.Source}, FileName: {cluster.FileName}"
        );
        foreach (var connection in cluster.Connections.Values)
        {
          Console.WriteLine(
            $"  Cluster Connection: {connection.Name}, Reference: {connection.Reference}, MacroReference: {connection.MacroReference}, MacroConnection: {connection.MacroConnection}, Source: {connection.Source}, FileName: {connection.FileName}"
          );
        }
        foreach (var sector in cluster.Sectors)
        {
          Console.WriteLine(
            $"  Sector: {sector.Name}, Id: {sector.Id}, Macro: {sector.Macro}, Reference: {sector.Reference}, DominantFaction: {sector.DominantOwner}, Source: {sector.Source}, FileName: {sector.FileName}"
          );
          foreach (var connection in sector.Connections.Values)
          {
            Console.WriteLine(
              $"    Sector Connection: {connection.Name}, Reference: {connection.Reference}, MacroReference: {connection.MacroReference}, MacroConnection: {connection.MacroConnection}, Source: {connection.Source}, FileName: {connection.FileName}"
            );
          }
          foreach (var zone in sector.Zones)
          {
            Console.WriteLine($"    Zone: {zone.Name}, Reference: {zone.Reference}, Source: {zone.Source}, FileName: {zone.FileName}");
            foreach (var connection in zone.Connections.Values)
            {
              Console.WriteLine(
                $"      Zone Connection: {connection.Name}, Reference: {connection.Reference}, Source: {connection.Source}, FileName: {connection.FileName}"
              );
            }
          }
          foreach (var highwayPoint in sector.HighwayPoints)
          {
            Console.WriteLine(
              $"    Highway Point: {highwayPoint.Name}, Level: {highwayPoint.HighwayLevel}, Type: {highwayPoint.Type}, Position: {highwayPoint.Position}, Source: {highwayPoint.Source}, FileName: {highwayPoint.FileName}, ConnectedSector: {highwayPoint.SectorConnected?.Name}"
            );
          }
        }
      }

      foreach (var connection in galaxy.Connections)
      {
        Console.WriteLine($"Galaxy Connection: {connection.Name}, Source: {connection.Source}, FileName: {connection.FileName}");
        Console.WriteLine($"  PathDirect: {connection.PathDirect.Path}");
        Console.WriteLine(
          $"    Cluster: {connection.PathDirect.Cluster.Name}, Source: {connection.PathDirect.Cluster.Source}, FileName: {connection.PathDirect.Cluster.FileName}"
        );
        Console.WriteLine(
          $"    Sector: {connection.PathDirect.Sector.Name}, Source: {connection.PathDirect.Sector.Source}, FileName: {connection.PathDirect.Sector.FileName}"
        );
        Console.WriteLine(
          $"    Zone: {connection.PathDirect.Zone.Name}, Source: {connection.PathDirect.Zone.Source}, FileName: {connection.PathDirect.Zone.FileName}"
        );
        Console.WriteLine(
          $"    Gate: {connection.PathDirect.Gate.Name}, Source: {connection.PathDirect.Gate.Source}, FileName: {connection.PathDirect.Gate.FileName}"
        );
        Console.WriteLine($"  PathOpposite: {connection.PathOpposite.Path}");
        Console.WriteLine(
          $"    Cluster: {connection.PathOpposite.Cluster.Name}, Source: {connection.PathOpposite.Cluster.Source}, FileName: {connection.PathOpposite.Cluster.FileName}"
        );
        Console.WriteLine(
          $"    Sector: {connection.PathOpposite.Sector.Name}, Source: {connection.PathOpposite.Sector.Source}, FileName: {connection.PathOpposite.Sector.FileName}"
        );
        Console.WriteLine(
          $"    Zone: {connection.PathOpposite.Zone.Name}, Source: {connection.PathOpposite.Zone.Source}, FileName: {connection.PathOpposite.Zone.FileName}"
        );
        Console.WriteLine(
          $"    Gate: {connection.PathOpposite.Gate.Name}, Source: {connection.PathOpposite.Gate.Source}, FileName: {connection.PathOpposite.Gate.FileName}"
        );
      }
    }

    public static void ConfigureNLog()
    {
      var logLevel = LogLevel.Debug;
      var config = new NLog.Config.LoggingConfiguration();
      var logConsole = new NLog.Targets.ConsoleTarget("logConsole")
      {
        Layout =
          "${time} [${level:uppercase=true}]: ${event-properties:FilePath}->${event-properties:ClassName}.${event-properties:MemberName}(): ${message} ${exception:format=toString}  ",
      };
      //longdate
      config.AddRule(logLevel, LogLevel.Fatal, logConsole);
      LogManager.Configuration = config;
    }
  }
}
