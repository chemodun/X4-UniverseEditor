using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Utilities.Logging;
using X4DataLoader.Helpers;

namespace X4DataLoader
{
  public class Cluster
  {
    public string Name { get; private set; }
    public string Description { get; private set; }
    public string Id { get; private set; }
    public string Macro { get; private set; }
    public string Reference { get; set; }
    public Position Position { get; private set; }
    public string PositionId { get; private set; }
    public string PositionSource { get; private set; }
    public string PositionFileName { get; private set; }
    public string Source { get; private set; }
    public string FileName { get; private set; }
    public XElement? PositionXML { get; set; }
    public XElement? XML { get; set; }
    public List<Sector> Sectors { get; private set; }
    public Dictionary<string, Connection> Connections { get; private set; }
    public List<Highway> Highways { get; private set; }

    public Cluster()
    {
      Sectors = [];
      Name = "";
      Description = "";
      Id = "";
      Macro = "";
      Reference = "";
      Connections = [];
      Highways = [];
      Position = new Position();
      PositionId = "";
      PositionXML = null;
      PositionSource = "vanilla";
      PositionFileName = "";
      Source = "vanilla";
      FileName = "";
    }

    public void Load(XElement element, Translation translation, string source, string fileName)
    {
      Log.Debug($"Loading cluster data for {source} from {fileName}");
      var macro = element.Attribute("macro")?.Value;
      if (!string.IsNullOrEmpty(macro) && IsClusterMacro(macro))
      {
        var propertiesElement = element.Element("properties");
        string nameId = propertiesElement?.Element("identification")?.Attribute("name")?.Value ?? "";
        string descriptionId = propertiesElement?.Element("identification")?.Attribute("description")?.Value ?? "";
        if (nameId != null && nameId != "" && descriptionId != "")
        {
          Id = macro.Replace("_macro", "");
          Macro = macro;
          Name = translation.Translate(nameId);
          Description = translation.Translate(descriptionId);
          Source = source;
          FileName = fileName;
          XML = element;
          Console.WriteLine($"Cluster Name: {Name}");
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

    public void SetPosition(Position position, string positionId, XElement positionXML, string source, string fileName)
    {
      Position = position;
      PositionId = positionId;
      PositionXML = positionXML;
      PositionSource = source;
      PositionFileName = fileName;
    }

    public static bool IsClusterMacro(string macro)
    {
      string macroLower = macro.ToLower(CultureInfo.InvariantCulture);
      return macroLower.Contains("cluster") && macroLower.EndsWith("_macro") && !macroLower.Contains("sector");
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
