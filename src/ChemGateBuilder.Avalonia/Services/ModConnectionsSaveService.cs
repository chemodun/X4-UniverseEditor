using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using X4DataLoader;

namespace ChemGateBuilder.AvaloniaApp.Services
{
  public static class ModConnectionsSaveService
  {
    public static bool Save(string targetFolder, ModInfo mod, Galaxy galaxy, IEnumerable<GalaxyConnection> connections, string universeId)
    {
      try
      {
        if (string.IsNullOrWhiteSpace(targetFolder) || galaxy == null)
          return false;
        Directory.CreateDirectory(targetFolder);

        var conns = connections?.Where(c => c != null).ToList() ?? new List<GalaxyConnection>();

        // Build dependencies and per-extension path mapping
        var dlcsRequired = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var pathsByExt = new Dictionary<string, List<GalaxyConnectionPath>>(StringComparer.OrdinalIgnoreCase);

        foreach (var c in conns)
        {
          if (c.PathDirect?.Sector?.Source is string s1 && !string.IsNullOrEmpty(s1))
          {
            dlcsRequired.Add(s1);
            if (!pathsByExt.TryGetValue(s1, out var list))
            {
              list = new List<GalaxyConnectionPath>();
              pathsByExt[s1] = list;
            }
            list.Add(c.PathDirect);
          }
          if (c.PathOpposite?.Sector?.Source is string s2 && !string.IsNullOrEmpty(s2))
          {
            dlcsRequired.Add(s2);
            if (!pathsByExt.TryGetValue(s2, out var list))
            {
              list = new List<GalaxyConnectionPath>();
              pathsByExt[s2] = list;
            }
            list.Add(c.PathOpposite);
          }
        }

        // Write content.xml
        var content = new XElement("content");
        content.SetAttributeValue("id", mod.Id);
        content.SetAttributeValue("name", mod.Name);
        content.SetAttributeValue("version", mod.Version);
        if (!string.IsNullOrWhiteSpace(mod.Author))
          content.SetAttributeValue("author", mod.Author);
        if (!string.IsNullOrWhiteSpace(mod.Date))
          content.SetAttributeValue("date", mod.Date);
        content.SetAttributeValue("save", "false");
        content.SetAttributeValue("sync", "false");

        string connectionsText = string.Join(
          "",
          conns.Select(gc => $"\n - {gc.PathDirect?.Sector?.Name} and {gc.PathOpposite?.Sector?.Name}")
        );
        string description = $"This extension adds new gate connections between sectors{connectionsText}";
        content.SetAttributeValue("description", description);

        // Languages list similar to WPF
        int[] languages = new[] { 7, 33, 37, 39, 44, 49, 55, 81, 82, 86, 88, 380 };
        foreach (var lang in languages)
        {
          var text = new XElement("text");
          text.SetAttributeValue("language", lang.ToString());
          text.SetAttributeValue("description", description);
          content.Add(text);
        }

        if (mod.GameVersion > 0)
        {
          var dep = new XElement("dependency");
          dep.SetAttributeValue("version", mod.GameVersion);
          content.Add(dep);
        }
        // DLC dependencies
        foreach (var dlc in dlcsRequired.OrderBy(s => s))
        {
          var ext = galaxy.Extensions.FirstOrDefault(e => string.Equals(e.Id, dlc, StringComparison.OrdinalIgnoreCase));
          var d = new XElement("dependency");
          d.SetAttributeValue("id", dlc);
          d.SetAttributeValue("optional", "false");
          if (ext != null)
          {
            d.SetAttributeValue("version", ext.Version);
            d.SetAttributeValue("name", ext.Name);
          }
          content.Add(d);
        }

        var contentDoc = new XDocument(new XDeclaration("1.0", "utf-8", null), content);
        contentDoc.Save(Path.Combine(targetFolder, DataLoader.ContentXml));

        // Galaxy diff with connections
        var diff = new XElement("diff");
        var add = new XElement("add");
        add.SetAttributeValue("sel", "/macros/macro[@name='XU_EP2_universe_macro']/connections");
        foreach (var c in conns)
        {
          if (c.XML == null)
            continue;
          var connElement = new XElement(c.XML);
          connElement.Attribute("_source")?.Remove();
          add.Add(connElement);
        }
        diff.Add(add);

        var mapsFolder = Path.Combine(targetFolder, "maps", universeId);
        Directory.CreateDirectory(mapsFolder);
        var galaxyDoc = new XDocument(new XDeclaration("1.0", "utf-8", null), diff);
        galaxyDoc.Save(Path.Combine(mapsFolder, "galaxy.xml"));

        // Sectors and Zones diffs per extension id (or vanilla)
        var sectorsCollection = new Dictionary<string, XElement>(StringComparer.OrdinalIgnoreCase);
        var zonesCollection = new Dictionary<string, XElement>(StringComparer.OrdinalIgnoreCase);

        foreach (var kv in pathsByExt)
        {
          var extId = kv.Key;
          foreach (var path in kv.Value)
          {
            if (path?.Sector == null || path.Zone == null)
              continue;
            var sectorsKey = galaxy.GameFiles.Any(gf => gf.Id == "sectors" && gf.Extension.Id == extId) ? extId : "vanilla";
            var zonesKey = galaxy.GameFiles.Any(gf => gf.Id == "zones" && gf.Extension.Id == extId) ? extId : "vanilla";

            if (!sectorsCollection.TryGetValue(sectorsKey, out var sectorsEl))
            {
              sectorsEl = new XElement("diff");
              sectorsCollection[sectorsKey] = sectorsEl;
            }
            if (!zonesCollection.TryGetValue(zonesKey, out var zonesEl))
            {
              zonesEl = new XElement("diff");
              zonesCollection[zonesKey] = zonesEl;
            }

            // sectors: add zone.PositionXML under sector connections
            var sectorAdd = new XElement("add");
            sectorAdd.SetAttributeValue("sel", $"/macros/macro[@name='{path.Sector.Macro}']/connections");
            if (path.Zone.PositionXML != null)
            {
              var pos = new XElement(path.Zone.PositionXML);
              pos.Attribute("_source")?.Remove();
              sectorAdd.Add(pos);
            }
            sectorsEl.Add(sectorAdd);

            // zones: add zone.XML under /macros
            var zoneAdd = new XElement("add");
            zoneAdd.SetAttributeValue("sel", "/macros");
            if (path.Zone.XML != null)
            {
              var zoneElCopy = new XElement(path.Zone.XML);
              zoneElCopy.Attribute("_source")?.Remove();
              zoneAdd.Add(zoneElCopy);
            }
            zonesEl.Add(zoneAdd);
          }
        }

        // Save sectors/zones into proper folders (possibly nested under extensions/<dlcFolder>)
        void SaveXmlCollection(string xmlType, Dictionary<string, XElement> collection)
        {
          foreach (var entry in collection)
          {
            var key = entry.Key; // extension id or 'vanilla'
            var basePath = targetFolder;
            string filePrefix = string.Empty;
            if (!string.Equals(key, "vanilla", StringComparison.OrdinalIgnoreCase))
            {
              var ext = galaxy.Extensions.FirstOrDefault(e => e.Id.Equals(key, StringComparison.OrdinalIgnoreCase));
              if (ext != null)
              {
                basePath = Path.Combine(basePath, DataLoader.ExtensionsFolder, ext.Folder);
                var gf = galaxy.GameFiles.FirstOrDefault(g =>
                  g.Id == xmlType && g.Extension.Id.Equals(key, StringComparison.OrdinalIgnoreCase)
                );
                if (gf != null)
                {
                  filePrefix = gf.FileName.Replace($"{xmlType}.xml", "", StringComparison.OrdinalIgnoreCase);
                }
              }
            }
            var outFolder = Path.Combine(basePath, "maps", universeId);
            Directory.CreateDirectory(outFolder);
            var doc = new XDocument(new XDeclaration("1.0", "utf-8", null), entry.Value);
            doc.Save(Path.Combine(outFolder, $"{filePrefix}{xmlType}.xml"));
          }
        }

        SaveXmlCollection("sectors", sectorsCollection);
        SaveXmlCollection("zones", zonesCollection);

        return true;
      }
      catch
      {
        return false;
      }
    }
  }
}
