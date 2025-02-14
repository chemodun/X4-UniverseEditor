using System.Data;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;
using System.Xml.XPath;
using Utilities.Logging;
using X4DataLoader.Helpers;

namespace X4DataLoader
{
  public class StationCategory
  {
    public string Tag { get; private set; }
    public string StationId { get; private set; }
    public string StationGroupId { get; private set; }
    public StationGroup? StationGroup { get; private set; }
    public string Macro { get; private set; }
    public List<string> Factions { get; private set; } = [];
    public string Source { get; private set; }
    public string FileName { get; private set; }
    public XElement? XML { get; set; }

    public StationCategory()
    {
      Tag = "";
      StationId = "";
      StationGroupId = "";
      StationGroup = null;
      Macro = "";
      Source = "";
      FileName = "";
      XML = null;
    }

    public void Load(XElement element, string source, string fileName, List<StationGroup> allStationGroups)
    {
      XElement? categoryElement = element.Element("category");
      if (categoryElement == null)
      {
        Log.Error($"StationCategory {StationId} must have a category");
        return;
      }
      Tag = XmlHelper.GetAttribute(categoryElement, "tags") ?? "";
      Factions = XmlHelper.GetAttributeAsList(categoryElement, "faction");
      StationId = XmlHelper.GetAttribute(element, "id") ?? "";
      StationGroupId = XmlHelper.GetAttribute(element, "group") ?? "";
      Macro = XmlHelper.GetAttribute(element, "macro") ?? "";
      StationGroup = allStationGroups.FirstOrDefault(sg => sg.Name == StationGroupId);
      Source = XmlHelper.GetAttribute(element, "_source") ?? source;
      FileName = fileName;
      XML = element;
    }

    public static StationCategory? GetByTagAndFaction(List<StationCategory> allStationCategories, string tag, string faction)
    {
      return allStationCategories.FirstOrDefault(sc => sc.Tag == tag && sc.Factions.Contains(faction));
    }

    public static StationCategory? GetByStationId(List<StationCategory> allStationCategories, string stationId)
    {
      return allStationCategories.FirstOrDefault(sc => sc.StationId == stationId);
    }

    public static void LoadFromXML(GameFile file, Galaxy galaxy)
    {
      IEnumerable<XElement> elements = file.XML.XPathSelectElements("/stations/station");
      foreach (XElement element in elements)
      {
        StationCategory stationCategory = new();
        stationCategory.Load(element, file.ExtensionId, file.FileName, galaxy.StationGroups);
        if (stationCategory.Tag == "")
        {
          Log.Warn($"StationCategory {stationCategory.StationId} must have a category");
          continue;
        }
        if (galaxy.StationCategories.Any(sc => sc.StationId == stationCategory.StationId))
        {
          Log.Warn($"Duplicate StationCategory id {stationCategory.StationId}");
          continue;
        }
        galaxy.StationCategories.Add(stationCategory);
      }
    }
  }
}
