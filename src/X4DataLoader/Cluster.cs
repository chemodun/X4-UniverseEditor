using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Utilities.Logging;
using X4DataLoader.Helpers;

namespace X4DataLoader
{
  public class Cluster(string macro)
  {
    public string Name { get; private set; } = "";
    public int NamePageId { get; private set; } = 0;
    public int NameTextId { get; private set; } = 0;
    public string Description { get; private set; } = "";
    public int DescriptionPageId { get; private set; } = 0;
    public int DescriptionTextId { get; private set; } = 0;
    public string System { get; private set; } = "";
    public string ImageId { get; private set; } = "";
    public string Id { get; private set; } = macro.Replace("_macro", "");
    public string Macro { get; private set; } = macro;
    public string Reference { get; set; } = "";
    public string Environment { get; private set; } = "";
    public int EnvironmentPageId { get; private set; } = 0;
    public int EnvironmentTextId { get; private set; } = 0;
    public string Sun { get; private set; } = "";
    public int SunPageId { get; private set; } = 0;
    public int SunTextId { get; private set; } = 0;
    public Position Position { get; private set; } = new Position();
    public string PositionId { get; private set; } = "";
    public string PositionSource { get; private set; } = "vanilla";
    public string PositionFileName { get; private set; } = "";
    public XElement? PositionXML { get; set; } = null;
    public string DetailsMacro { get; private set; } = "";
    public string DetailsSource { get; private set; } = "vanilla";
    public string DetailsFileName { get; private set; } = "";
    public XElement? DetailsXML { get; set; } = null;
    public string Source { get; private set; } = "vanilla";
    public string FileName { get; private set; } = "";
    public XElement? XML { get; set; } = null;
    public List<Sector> Sectors { get; private set; } = [];
    public Dictionary<string, Connection> Connections { get; private set; } = [];
    public List<Highway> Highways { get; private set; } = [];

    public void SetDetails(XElement element, Translation translation, string source, string fileName)
    {
      Log.Debug($"Loading cluster data for {source} from {fileName}");
      var macro = element.Attribute("macro")?.Value;
      if (!string.IsNullOrEmpty(macro))
      {
        XElement? propertiesElement = element.Element("properties") ?? throw new ArgumentException("Cluster must have properties");
        string nameId = propertiesElement.Element("identification")?.Attribute("name")?.Value ?? "";
        string descriptionId = propertiesElement.Element("identification")?.Attribute("description")?.Value ?? "";
        if (nameId != null && nameId != "" && descriptionId != "")
        {
          DetailsMacro = macro;
          Name = translation.Translate(nameId);
          var ids = Translation.GetIds(nameId);
          NamePageId = ids[0];
          NameTextId = ids[1];
          Description = translation.Translate(descriptionId);
          ids = Translation.GetIds(descriptionId);
          DescriptionPageId = ids[0];
          DescriptionTextId = ids[1];
          System = propertiesElement.Element("identification")?.Attribute("system")?.Value ?? "";
          ImageId = propertiesElement.Element("identification")?.Attribute("mage")?.Value ?? "";
          DetailsSource = source;
          DetailsFileName = fileName;
          DetailsXML = element;
          XElement? systemElement = propertiesElement.Element("system");
          if (systemElement != null)
          {
            XElement? spaceElement = systemElement.Element("space");
            string environment = spaceElement?.Attribute("environment")?.Value ?? "";
            Environment = translation.Translate(environment);
            int[] environmentTextIds = Translation.GetIds(environment);
            EnvironmentPageId = environmentTextIds[0];
            EnvironmentTextId = environmentTextIds[1];
            IEnumerable<XElement> sunElements = systemElement.XPathSelectElements("suns/sun");
            if (sunElements.Any())
            {
              XElement sunElement = sunElements.First();
              string sun = sunElement.Attribute("class")?.Value ?? "";
              int[] sunTextIds = Translation.GetIds(sun);
              Sun = translation.Translate(sun);
              SunPageId = sunTextIds[0];
              SunTextId = sunTextIds[1];
            }
          }
        }
        else
        {
          throw new ArgumentException("Cluster must have name and description");
        }
      }
      else
      {
        throw new ArgumentException($"Invalid macro format: {macro}");
      }
    }

    public void Update(string reference, string source, string fileName, XElement element)
    {
      XML = element;
      Reference = reference;
      Source = source;
      FileName = fileName;
    }

    public void SetPosition(Position position, string positionId, XElement positionXML, string source, string fileName)
    {
      Position = position;
      PositionId = positionId;
      PositionXML = positionXML;
      PositionSource = XmlHelper.GetAttribute(positionXML, "_source") ?? source;
      PositionFileName = fileName;
    }

    public static Cluster? GetClusterByMacro(List<Cluster> clusters, string macro)
    {
      return clusters.Find(cluster => StringHelper.EqualsIgnoreCase(cluster.Macro, macro));
    }

    public static Cluster? GetClusterById(List<Cluster> clusters, string clusterId)
    {
      return clusters.Find(cluster => StringHelper.EqualsIgnoreCase(cluster.Id, clusterId));
    }

    public static string GetClusterIdByMacro(List<Cluster> clusters, string macro)
    {
      Cluster? cluster = GetClusterByMacro(clusters, macro);
      return cluster?.Id ?? "";
    }

    public static string GetClusterByMacroById(List<Cluster> clusters, string clusterId)
    {
      Cluster? cluster = GetClusterByMacro(clusters, clusterId);
      return cluster?.Id ?? "";
    }
  }
}
