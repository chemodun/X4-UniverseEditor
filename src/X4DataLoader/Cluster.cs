using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using Utilities.Logging;
using X4DataLoader.Helpers;

namespace X4DataLoader
{
  public class Cluster(string macro)
  {
    public string Name { get; private set; } = "";
    public string Description { get; private set; } = "";
    public string Id { get; private set; } = macro.Replace("_macro", "");
    public string Macro { get; private set; } = macro;
    public string Reference { get; set; } = "";
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
        var propertiesElement = element.Element("properties");
        string nameId = propertiesElement?.Element("identification")?.Attribute("name")?.Value ?? "";
        string descriptionId = propertiesElement?.Element("identification")?.Attribute("description")?.Value ?? "";
        if (nameId != null && nameId != "" && descriptionId != "")
        {
          DetailsMacro = macro;
          Name = translation.Translate(nameId);
          Description = translation.Translate(descriptionId);
          DetailsSource = source;
          DetailsFileName = fileName;
          DetailsXML = element;
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
