using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using X4DataLoader;

namespace ChemGateBuilder.AvaloniaApp.Services
{
  public static class ModConnectionsService
  {
    private const string ModId = "chem_gate_keeper";

    public static bool TryLoadConnections(string contentXmlPath, Galaxy galaxy, out List<GalaxyConnection> connections)
    {
      connections = new List<GalaxyConnection>();
      try
      {
        if (galaxy == null)
          return false;
        if (string.IsNullOrWhiteSpace(contentXmlPath) || !File.Exists(contentXmlPath))
          return false;

        var contentDir = Path.GetDirectoryName(contentXmlPath);
        if (string.IsNullOrWhiteSpace(contentDir))
          return false;

        // Validate content.xml and extract extension info
        ExtensionInfo ext;
        try
        {
          ext = new ExtensionInfo(contentXmlPath);
        }
        catch
        {
          return false;
        }

        var gameFilesStructure = new List<GameFilesStructureItem>
        {
          new GameFilesStructureItem(
            id: "sectors",
            folder: $"maps/{DataLoader.DefaultUniverseId}",
            new[] { "sectors.xml" },
            MatchingModes.Suffix
          ),
          new GameFilesStructureItem(
            id: "zones",
            folder: $"maps/{DataLoader.DefaultUniverseId}",
            new[] { "zones.xml" },
            MatchingModes.Suffix
          ),
          new GameFilesStructureItem(id: "galaxy", folder: $"maps/{DataLoader.DefaultUniverseId}", new[] { "galaxy.xml" }),
        };

        var loader = new DataLoader();
        var existing = GameFile.CloneList(galaxy.GameFiles, true);
        _ = loader.GatherFiles(contentDir, gameFilesStructure, galaxy.Extensions, out int _patched, ext, existing);

        // We care about the patched results for the three ids above
        var modFiles = existing.Where(f => f.Patched && (f.Id == "sectors" || f.Id == "zones" || f.Id == "galaxy")).ToList();

        var zonesConnections = new List<ZoneConnection>();
        var zones = new List<Zone>();

        foreach (var file in modFiles.Where(f => f.Id == "sectors"))
        {
          var elements = file.XML.XPathSelectElements("/macros/macro/connections/connection[@_source='" + ModId + "']");
          foreach (var el in elements)
          {
            var zc = new ZoneConnection();
            try
            {
              zc.Load(el, ext.Id, file.FileName);
              zonesConnections.Add(zc);
            }
            catch
            { /* ignore invalid items */
            }
          }
        }

        foreach (var file in modFiles.Where(f => f.Id == "zones"))
        {
          var elements = file.XML.XPathSelectElements("/macros/macro[@_source='" + ModId + "']");
          foreach (var el in elements)
          {
            var zone = new Zone();
            try
            {
              zone.Load(el, ext.Id, file.FileName);
              var zoneId = zone.Name.Replace("_macro", "_connection", StringComparison.OrdinalIgnoreCase);
              var zc = zonesConnections.FirstOrDefault(c => c.Name.Equals(zoneId, StringComparison.OrdinalIgnoreCase));
              if (zc?.Position != null)
              {
                zone.SetPosition(zc.Position, zc.Name, zc.XML);
                zones.Add(zone);
              }
            }
            catch
            { /* ignore invalid items */
            }
          }
        }

        foreach (var file in modFiles.Where(f => f.Id == "galaxy"))
        {
          var elements = file.XML.XPathSelectElements("/macros/macro/connections/connection[@_source='" + ModId + "']");
          foreach (var el in elements)
          {
            var gc = new GalaxyConnection();
            try
            {
              gc.Load(el, galaxy.Clusters, ext.Id, file.FileName, zones);
              connections.Add(gc);
            }
            catch
            { /* ignore invalid items */
            }
          }
        }

        return connections.Count > 0;
      }
      catch
      {
        connections = new List<GalaxyConnection>();
        return false;
      }
    }
  }
}
