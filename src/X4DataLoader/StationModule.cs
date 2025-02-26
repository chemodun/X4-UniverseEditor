using System.Data;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;
using System.Xml.XPath;
using Utilities.Logging;
using X4DataLoader.Helpers;

namespace X4DataLoader
{
  public class StationModule
  {
    public string Id { get; private set; }
    public string GroupId { get; private set; }
    public List<string> Tags { get; private set; } = [];
    public List<string> Factions { get; private set; } = [];
    public string Source { get; private set; }
    public string FileName { get; private set; }
    public XElement? XML { get; set; }

    public StationModule()
    {
      Id = "";
      GroupId = "";
      Source = "";
      FileName = "";
      XML = null;
    }

    public void Load(XElement element, string source, string fileName)
    {
      Id = XmlHelper.GetAttribute(element, "id") ?? "";
      GroupId = XmlHelper.GetAttribute(element, "group") ?? "";
      XElement? categoryElement = element.Element("category");
      if (categoryElement != null)
      {
        Tags = XmlHelper.GetAttributeAsList(categoryElement, "tags");
        Factions = XmlHelper.GetAttributeAsList(categoryElement, "faction");
      }
      Source = XmlHelper.GetAttribute(element, "_source") ?? source;
      FileName = fileName;
      XML = element;
    }

    public bool IsContainsTag(string tag)
    {
      return Tags.Contains(tag);
    }

    public static void LoadFromXML(GameFile file, Galaxy galaxy)
    {
      IEnumerable<XElement> elements = file.XML.XPathSelectElements("/modules/module");
      foreach (XElement element in elements)
      {
        StationModule module = new();
        module.Load(element, file.Extension.Id, file.FileName);
        if (galaxy.StationModules.Any(m => m.Id == module.Id))
        {
          Log.Error($"Duplicate module id {module.Id}");
          continue;
        }
        galaxy.StationModules.Add(module);
      }
    }

    public static bool IsModuleWithGroupContainsTag(List<StationModule> allModules, string groupId, string tag)
    {
      return allModules.Any(module => module.GroupId == groupId && module.IsContainsTag(tag));
    }
  }
}
