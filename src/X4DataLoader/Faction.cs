using System.Data;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;
using System.Xml.XPath;
using Utilities.Logging;
using X4DataLoader.Helpers;

namespace X4DataLoader
{
  public class Faction
  {
    public string Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public string ShortName { get; private set; }
    public string PrefixName { get; private set; }
    public string SpaceName { get; private set; }
    public string HomeSpaceName { get; private set; }
    public string PrimaryRaceId { get; private set; }
    public string PrimaryRaceName { get; private set; }
    public string BehaviorSet { get; private set; }
    public List<string> Tags { get; private set; } = [];
    public string PoliceFaction { get; private set; }
    public string ColorId { get; private set; }
    public X4Color? Color { get; private set; } = null;
    public string IconActiveId { get; private set; }
    public string IconInactiveId { get; private set; }
    public XElement? XML { get; set; }
    public string Source { get; private set; }
    public string FileName { get; private set; }

    public Faction()
    {
      Id = "";
      Name = "";
      Description = "";
      ShortName = "";
      PrefixName = "";
      SpaceName = "";
      HomeSpaceName = "";
      PrimaryRaceId = "";
      PrimaryRaceName = "";
      BehaviorSet = "";
      PoliceFaction = "";
      ColorId = "";
      IconActiveId = "";
      IconInactiveId = "";
      XML = null;
      Source = "";
      FileName = "";
    }

    public void Load(XElement element, string source, string fileName, Galaxy galaxy)
    {
      Id = XmlHelper.GetAttribute(element, "id") ?? "";
      Name = galaxy.Translation.Translate(XmlHelper.GetAttribute(element, "name") ?? "");
      Description = galaxy.Translation.Translate(XmlHelper.GetAttribute(element, "description") ?? "");
      ShortName = galaxy.Translation.Translate(XmlHelper.GetAttribute(element, "shortname") ?? "");
      PrefixName = galaxy.Translation.Translate(XmlHelper.GetAttribute(element, "prefixname") ?? "");
      SpaceName = galaxy.Translation.Translate(XmlHelper.GetAttribute(element, "spacename") ?? "");
      HomeSpaceName = galaxy.Translation.Translate(XmlHelper.GetAttribute(element, "homespacename") ?? "");
      PrimaryRaceId = XmlHelper.GetAttribute(element, "primaryrace") ?? "";
      PrimaryRaceName = galaxy.Races.Find(r => r.Id == PrimaryRaceId)?.Name ?? "";
      BehaviorSet = XmlHelper.GetAttribute(element, "behaviorset") ?? "";
      Tags = XmlHelper.GetAttributeAsList(element, "tags", " ");
      PoliceFaction = XmlHelper.GetAttribute(element, "policefaction") ?? "";
      XElement? colorElement = element.Element("color");
      if (colorElement != null)
      {
        ColorId = XmlHelper.GetAttribute(colorElement, "ref") ?? "";
      }
      Color = X4MappedColor.GetColorByMappedId(ColorId, galaxy);
      XElement? iconElement = element.Element("icon");
      if (iconElement != null)
      {
        IconActiveId = XmlHelper.GetAttribute(iconElement, "active") ?? "";
        IconInactiveId = XmlHelper.GetAttribute(iconElement, "inactive") ?? "";
      }
      XML = element;
      Source = XmlHelper.GetAttribute(element, "_source") ?? source;
      FileName = fileName;
    }

    public bool IsContainsTag(string tag)
    {
      return Tags.Contains(tag);
    }

    public static void LoadFromXML(GameFile file, Galaxy galaxy)
    {
      IEnumerable<XElement> elements = file.XML.XPathSelectElements("/factions/faction");
      foreach (XElement element in elements)
      {
        Faction faction = new();
        faction.Load(element, file.ExtensionId, file.FileName, galaxy);
        if (faction.Name == "")
        {
          Log.Warn($"Faction must have a name");
          continue;
        }
        if (galaxy.Factions.Any(f => f.Id == faction.Id))
        {
          Log.Warn($"Duplicate faction id {faction.Id}");
          continue;
        }
        galaxy.Factions.Add(faction);
      }
    }

    public static X4Color? GetColorByFactionId(string factionId, Galaxy galaxy)
    {
      Faction? faction = galaxy.Factions.Find(faction => faction.Id == factionId);
      if (faction != null)
      {
        return X4MappedColor.GetColorByMappedId(faction.ColorId, galaxy);
      }
      return null;
    }
  }
}
