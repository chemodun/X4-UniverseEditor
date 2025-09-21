using System;
using System.IO;
using System.Xml.Linq;

namespace ChemGateBuilder.AvaloniaApp.Services
{
  public class ModInfo
  {
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public int Version { get; set; } = 100;
    public int GameVersion { get; set; } = 0;
    public string Author { get; set; } = "";
    public string Date { get; set; } = DateTime.Now.ToString("yyyy-MM-dd");
    public string FolderPath { get; set; } = "";
  }

  public static class ModService
  {
    public static bool TryLoad(string contentXmlPath, out ModInfo? mod)
    {
      mod = null;
      if (string.IsNullOrWhiteSpace(contentXmlPath) || !File.Exists(contentXmlPath))
        return false;
      try
      {
        var doc = XDocument.Load(contentXmlPath);
        var root = doc.Root;
        if (root is null || !string.Equals(root.Name.LocalName, "content", StringComparison.OrdinalIgnoreCase))
          return false;
        var m = new ModInfo
        {
          Id = root.Attribute("id")?.Value ?? "",
          Name = root.Attribute("name")?.Value ?? "",
          Author = root.Attribute("author")?.Value ?? "",
          Date = root.Attribute("date")?.Value ?? DateTime.Now.ToString("yyyy-MM-dd"),
          FolderPath = Path.GetDirectoryName(contentXmlPath) ?? "",
        };
        if (int.TryParse(root.Attribute("version")?.Value, out var ver))
          m.Version = ver;
        var dep = root.Element("dependency");
        if (dep != null && int.TryParse(dep.Attribute("version")?.Value, out var gv))
          m.GameVersion = gv;
        mod = m;
        return !string.IsNullOrEmpty(m.Id);
      }
      catch
      {
        return false;
      }
    }

    public static bool SaveBasic(ModInfo mod, string targetFolder, bool overwrite)
    {
      if (string.IsNullOrWhiteSpace(targetFolder))
        return false;
      Directory.CreateDirectory(targetFolder);
      var contentPath = Path.Combine(targetFolder, "content.xml");
      if (File.Exists(contentPath) && !overwrite)
        return false;
      var root = new XElement("content");
      root.SetAttributeValue("id", mod.Id);
      root.SetAttributeValue("name", mod.Name);
      root.SetAttributeValue("version", mod.Version);
      if (!string.IsNullOrWhiteSpace(mod.Author))
        root.SetAttributeValue("author", mod.Author);
      if (!string.IsNullOrWhiteSpace(mod.Date))
        root.SetAttributeValue("date", mod.Date);
      if (mod.GameVersion > 0)
      {
        var dep = new XElement("dependency");
        dep.SetAttributeValue("version", mod.GameVersion);
        root.Add(dep);
      }
      var doc = new XDocument(root);
      doc.Save(contentPath);
      return true;
    }
  }
}
