using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.Serialization;
using Utilities.Logging;
using X4DataLoader.Helpers;

namespace X4DataLoader
{
  public partial class Sector
  {
    public string Name { get; private set; }
    public string Description { get; private set; }
    public string Id { get; private set; }
    public string ClusterId { get; private set; }
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

    public List<Zone> Zones { get; private set; } = [];
    public Dictionary<string, Connection> Connections { get; private set; } = [];
    public List<Highway> Highways { get; private set; } = [];
    public List<HighwayPoint> HighwayPoints { get; private set; } = [];
    public List<Station> Stations { get; private set; } = [];

    public string DominantOwner { get; private set; } = "";

    public Sector()
    {
      Name = "";
      Description = "";
      Id = "";
      Macro = "";
      ClusterId = "";
      Reference = "";
      Position = new Position();
      PositionId = "";
      PositionSource = "vanilla";
      PositionFileName = "";
      Source = "vanilla";
      FileName = "";
    }

    public void Load(XElement element, Translation translation, string source, string fileName)
    {
      var macro = element.Attribute("macro")?.Value;
      if (!string.IsNullOrEmpty(macro) && IsSectorMacro(macro))
      {
        var propertiesElement = element.Element("properties");
        string nameId = propertiesElement?.Element("identification")?.Attribute("name")?.Value ?? "";
        string descriptionId = propertiesElement?.Element("identification")?.Attribute("description")?.Value ?? "";
        if (nameId != null && nameId != "" && descriptionId != null && descriptionId != "")
        {
          Id = macro.Replace("_macro", "");
          Macro = macro;
          Name = translation.Translate(nameId ?? "");
          string lowerId = Id.ToLower(CultureInfo.InvariantCulture);
          int sectorPosition = lowerId.IndexOf("_sector", StringComparison.Ordinal);
          ClusterId = Id[..sectorPosition];
          Description = translation.Translate(descriptionId);
          XML = element;
          Console.WriteLine($"Sector Name: {Name}");
          Source = source;
          FileName = fileName;
        }
        else
        {
          throw new ArgumentException("Sector must have name and description");
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

    public static bool IsSectorMacro(string macro)
    {
      string macroLower = macro.ToLower(CultureInfo.InvariantCulture);
      return macroLower.Contains("cluster") && macroLower.EndsWith("_macro") && macroLower.Contains("sector");
    }

    public Connection? GetConnection(string connectionId)
    {
      if (Connections.TryGetValue(connectionId, out var connection))
      {
        return connection;
      }
      return null;
    }

    public void AddZone(Zone zone)
    {
      Zones.Add(zone);
      Connection? zoneConnection = Connections.Values.FirstOrDefault(conn => StringHelper.EqualsIgnoreCase(conn.MacroReference, zone.Name));
      if (zoneConnection == null || zoneConnection.Position == null)
        return;
      zone.SetPosition(zoneConnection.Position, zoneConnection.Name, zoneConnection.XML);
    }

    public void AddHighwayPoint(HighwayPoint highwayPoint)
    {
      HighwayPoints.Add(highwayPoint);
    }

    public void AddStation(Station station)
    {
      Stations.Add(station);
    }

    public void CalculateOwnership(List<Faction> allFactions)
    {
      Dictionary<string, int> ownerStationCount = [];
      foreach (Station station in Stations)
      {
        if (station.IsClaimCapable && !station.GameStartDependent)
        {
          Faction? stationOwner = allFactions.Find(faction => faction.Id == station.OwnerId);
          if (stationOwner == null || !stationOwner.IsContainsTag("claimspace"))
            continue;
          if (ownerStationCount.TryGetValue(station.OwnerId, out int countedValue))
          {
            ownerStationCount[station.OwnerId] = ++countedValue;
          }
          else
          {
            ownerStationCount[station.OwnerId] = 1;
          }
        }
        Log.Debug($"Sector {Name}: Station {station.Id} Owner: {station.OwnerId}, isClaimCapable: {station.IsClaimCapable}");
      }
      if (ownerStationCount.Count > 0)
      {
        int totalCalculableStations = ownerStationCount.Values.Sum();
        string dominantOwner = ownerStationCount.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
        if (ownerStationCount[dominantOwner] / (double)totalCalculableStations * 100 > 50)
        {
          DominantOwner = dominantOwner;
          Log.Debug($"Sector {Name}: Dominant Owner: {DominantOwner}");
        }
        else
        {
          Log.Debug($"Sector {Name}: Dominant Owner not found");
        }
      }
      else
      {
        Log.Debug($"Sector {Name}: No claim capable stations found");
      }
    }

    public List<Station> GetStationsByTagOrType(string tag)
    {
      return Stations.FindAll(station => station.Tags.Contains(tag) || station.Tags.Count == 0 && station.Type == tag);
    }

    public List<Station> GetStationsByTagsOrTypes(List<string> tags)
    {
      return Stations.FindAll(station => station.Tags.Intersect(tags).Any() || station.Tags.Count == 0 && tags.Contains(station.Type));
    }

    public static Sector? GetSectorById(List<Sector> sectors, string sectorId)
    {
      return sectors.Find(sector => StringHelper.EqualsIgnoreCase(sector.Id, sectorId));
    }

    public static Sector? GetSectorByMacro(List<Sector> sectors, string sectorMacro)
    {
      return sectors.Find(sector => StringHelper.EqualsIgnoreCase(sector.Macro, sectorMacro));
    }

    public static string GetSectorIdByMacro(List<Sector> sectors, string sectorMacro)
    {
      Sector? sector = GetSectorByMacro(sectors, sectorMacro);
      return sector?.Id ?? "";
    }

    public static string GetSectorMacroById(List<Sector> sectors, string sectorId)
    {
      Sector? sector = GetSectorById(sectors, sectorId);
      return sector?.Macro ?? "";
    }
  }
}
