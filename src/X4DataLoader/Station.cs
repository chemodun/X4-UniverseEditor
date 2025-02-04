using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;
using X4DataLoader.Helpers;
using Utilities.Logging;
using System.Data;

namespace X4DataLoader
{
    public class Station
    {
        public string Id { get; private set; }
        public string Name { get; private set; }
        public string Race { get; private set; }
        public string OwnerId { get; private set; }
        public string OwnerName { get; private set; }
        public string Type { get; private set; }
        public bool GameStartDependent { get; private set; } = false;
        public bool IsClaimCapable { get; private set; }
        public Sector? Sector { get; private set; }
        public Zone? Zone { get; private set; }
        public Position Position { get; private set; }
        public Rotation Rotation { get; private set; }
        public XElement? PositionXML { get; set; }
        public List<string> Tags { get; private set; } = [];
        public XElement? XML { get; set; }
        public string Source { get; private set; }
        public string FileName { get; private set; }

        public Station()
        {
            Id = "";
            Name = "";
            Race = "";
            OwnerId = "";
            OwnerName = "";
            Type = "";
            IsClaimCapable = false;
            Sector = null;
            Zone = null;
            Position = new Position();
            Rotation = new Rotation();
            PositionXML = null;
            XML = null;
            Source = "";
            FileName = "";
        }
        public void Load(XElement element, string source, string fileName, List<Sector> allSectors, List<StationCategory> allStationCategories, List<ConstructionPlan> allConstructionPlans, List<Faction> allFactions)
        {
            Id = XmlHelper.GetAttribute(element, "id") ?? "";
            Race = XmlHelper.GetAttribute(element, "race") ?? "";
            OwnerId = XmlHelper.GetAttribute(element, "owner") ?? "";
            Type = XmlHelper.GetAttribute(element, "type") ?? "";
            XElement? locationElement = element.Element("location");
            if (locationElement == null)
            {
                Log.Error($"Station {Id} must have a location");
                throw new ArgumentException($"Station {Id} must have a location");
            }
            string locationClass = XmlHelper.GetAttribute(locationElement, "class") ?? "";
            string locationMacro = XmlHelper.GetAttribute(locationElement, "macro") ?? "";
            switch (locationClass)
            {
                case "zone":
                    Sector = allSectors
                        .FirstOrDefault(s => s.Connections.Values.Any(conn => StringHelper.EqualsIgnoreCase(conn.MacroReference, locationMacro)));
                    if (Sector == null)
                    {
                        Log.Error($"Sector not found for station {Id}");
                        throw new ArgumentException($"Sector not found for station {Id}");
                    }
                    Zone = Sector.Zones.FirstOrDefault(z => StringHelper.EqualsIgnoreCase(z.Name, locationMacro));
                    if (Zone == null)
                    {
                        Log.Error($"Zone not found for station {Id}");
                        throw new ArgumentException($"Zone not found for station {Id}");
                    }
                    break;
                case "sector":
                    Sector = allSectors.FirstOrDefault(s => StringHelper.EqualsIgnoreCase(s.Macro, locationMacro));
                    if (Sector == null)
                    {
                        Log.Error($"Sector not found for station {Id}");
                        throw new ArgumentException($"Sector not found for station {Id}");
                    }
                    break;
                default:
                    {
                        Log.Warn($"Invalid location class {locationClass} for station {Id}. Will not be loaded");
                        return;
                    }
            }
            XElement? positionElement = element.Element("position");
            if (positionElement != null)
            {
                Position = new(
                    StringHelper.ParseDouble(XmlHelper.GetAttribute(positionElement, "x")),
                    StringHelper.ParseDouble(XmlHelper.GetAttribute(positionElement, "y")),
                    StringHelper.ParseDouble(XmlHelper.GetAttribute(positionElement, "z"))
                );
                Rotation = new(
                    StringHelper.ParseDouble(XmlHelper.GetAttribute(positionElement, "pitch")),
                    StringHelper.ParseDouble(XmlHelper.GetAttribute(positionElement, "yaw")),
                    StringHelper.ParseDouble(XmlHelper.GetAttribute(positionElement, "roll"))
                );
                PositionXML = positionElement;
            }
            XElement? stationElement = element.Element("station");
            ConstructionPlan? constructionPlan = null;
            if (stationElement != null)
            {
                string constructionPlanId = XmlHelper.GetAttribute(stationElement, "constructionplan")?.Trim('\'') ?? "";
                string refId = XmlHelper.GetAttribute(stationElement, "ref") ?? "";
                if (!String.IsNullOrEmpty(constructionPlanId))
                {
                    constructionPlan = allConstructionPlans.FirstOrDefault(cp => cp.Id == constructionPlanId);
                }
                XElement? selectElement = stationElement.Element("select");
                StationCategory? stationCategory = null;
                if (String.IsNullOrEmpty(constructionPlanId))
                {
                    if (selectElement != null)
                    {
                        Tags = XmlHelper.GetAttributeAsList(selectElement, "tags");
                        string faction = XmlHelper.GetAttribute(selectElement, "faction") ?? "";
                        if (!String.IsNullOrEmpty(faction) && Tags.Count == 1)
                        {
                            stationCategory = StationCategory.GetByTagAndFaction(allStationCategories, Tags[0], faction);
                        }
                    }
                    else if (!String.IsNullOrEmpty(refId)) // There is only for tradestation_tel_ring, really not clear situation
                    {
                        stationCategory = StationCategory.GetByStationId(allStationCategories, refId);
                        if (stationCategory != null)
                        {
                            string tag = stationCategory.Tag;
                            string faction = OwnerId;
                            stationCategory = StationCategory.GetByTagAndFaction(allStationCategories, tag, faction);
                        }
                    }
                }
                if (constructionPlan == null && stationCategory != null)
                {
                    if (stationCategory.StationGroup != null)
                    {
                        constructionPlan = stationCategory.StationGroup.GetMostWeightedConstructionPlan();
                    }
                }
            }
            IsClaimCapable = constructionPlan?.IsClaimCapable ?? false;
            Name = constructionPlan?.Name ?? "";
            OwnerName = allFactions.FirstOrDefault(f => f.Id == OwnerId)?.Name ?? "";
            if (!String.IsNullOrEmpty(Name) && !String.IsNullOrEmpty(OwnerName))
            {
                string planPrefix = Name.Split(" ")[0];
                if (allFactions.Any(f => f.Name.StartsWith(planPrefix)) || allFactions.Any(f => f.PrimaryRaceName.StartsWith(planPrefix)))
                {
                    Name = Name[planPrefix.Length..].Trim();
                }
                Name = OwnerName + " " + Name;
            }
            XElement? quotasElement = element.Element("quotas");
            if (quotasElement != null)
            {
                foreach (XElement quotaElement in quotasElement.Elements("quota"))
                {
                    string gameStart = XmlHelper.GetAttribute(quotaElement, "gamestart") ?? "";
                    if (!String.IsNullOrEmpty(gameStart))
                    {
                        GameStartDependent = true;
                        break;
                    }
                }
            }
            Source = source;
            FileName = fileName;
            XML = element;
            Sector.AddStation(this);
            Log.Debug($"Loaded station {Id} in {Sector.Name}");
        }
    }
}
