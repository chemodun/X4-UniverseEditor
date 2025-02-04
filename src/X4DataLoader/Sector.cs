using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.Serialization;
using X4DataLoader.Helpers;
using Utilities.Logging;

namespace X4DataLoader
{
    public partial class Sector
    {
        public string Name { get; private set; }
        public string Description { get; private set; }
        public int Id { get; private set; }
        public string IdPrefix { get; private set; }
        public int ClusterId { get; private set; }
        public string ClusterIdPrefix { get; private set; }
        public string FullId => $"Cluster_{ClusterId:D2}_Sector{Id:D3}";
        public string Macro => $"Cluster_{ClusterId:D2}_Sector{Id:D3}_macro";
        public string Reference { get; set; }
        public Position Position { get; private set; }
        public string PositionId { get; private set; }
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
        private static readonly Regex SectorRegex = new(@"^(Cluster)_(\d+)_(Sector)(\d+)_macro", RegexOptions.IgnoreCase);
        public Sector()
        {
            Name = "";
            Description = "";
            Id = 0;
            ClusterId = 0;
            IdPrefix = "Sector";
            ClusterIdPrefix = "Cluster";
            Reference = "";
            Position = new Position();
            PositionId = "";
            Source = "vanilla";
            FileName = "";
        }

        public void Load(XElement element, Translation translation, string source, string fileName)
        {
            var macro = element.Attribute("macro")?.Value;
            var sectorIdMatch = SectorRegex.Match(macro ?? "");
            if (sectorIdMatch.Success)
            {
                var propertiesElement = element.Element("properties");
                string nameId = propertiesElement?.Element("identification")?.Attribute("name")?.Value ?? "";
                string descriptionId = propertiesElement?.Element("identification")?.Attribute("description")?.Value ?? "";
                if (nameId != null && nameId != "" && descriptionId != null && descriptionId != "")
                {
                    Id = int.Parse(sectorIdMatch.Groups[4].Value, CultureInfo.InvariantCulture);
                    IdPrefix = sectorIdMatch.Groups[3].Value;
                    ClusterId = int.Parse(sectorIdMatch.Groups[2].Value, CultureInfo.InvariantCulture);
                    ClusterIdPrefix = sectorIdMatch.Groups[1].Value;
                    Name = translation.Translate(nameId ?? "");
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

        public void SetPosition(Position position, string positionId, XElement positionXML)
        {
            Position = position;
            PositionId = positionId;
            PositionXML = positionXML;
        }

        public static bool IsSectorMacro(string macro) => SectorRegex.IsMatch(macro);

        public static (string, int, string, int) GetSectorIdData(string macro)
        {
            var match = SectorRegex.Match(macro);
            if (match.Success)
            {
                return (match.Groups[1].Value, int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture), match.Groups[3].Value, int.Parse(match.Groups[4].Value, CultureInfo.InvariantCulture));
            }
            throw new ArgumentException($"Invalid macro format: {macro}");
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
            Connection? zoneConnection = Connections.Values
                .FirstOrDefault(conn => StringHelper.EqualsIgnoreCase(conn.MacroReference, zone.Name));
            if (zoneConnection == null || zoneConnection.Position == null) return;
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
            Dictionary<string, int> ownerStationCount = new();
            List<string> toIgnoreOwners = ["player", "civilian", "khaak", "ownerless"];
            List<string> toIgnoreTypes = ["piratebase"];
            Dictionary<string, string> ownerReplacements = new() { ["alliance"] = "paranid", ["ministry"] = "teladi" };
            foreach (Station station in Stations)
            {
                if (station.IsClaimCapable && !station.GameStartDependent)
                {
                    Faction? stationOwner = allFactions.Find(faction => faction.Id == station.OwnerId);
                    if (stationOwner == null || !stationOwner.IsContainsTag("claimspace")) continue;
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
    }
}
