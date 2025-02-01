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
        public string Race { get; private set; }
        public string Owner { get; private set; }
        public string Type { get; private set; }
        public Sector? Sector { get; private set; }
        public Zone? Zone { get; private set; }
        public Position Position { get; private set; }
        public Rotation Rotation { get; private set; }
        public XElement? PositionXML { get; set; }
        public XElement? XML { get; set; }
        public string Source { get; private set; }
        public string FileName { get; private set; }

        public Station()
        {
            Id = "";
            Race = "";
            Owner = "";
            Type = "";
            Sector = null;
            Zone = null;
            Position = new Position();
            Rotation = new Rotation();
            PositionXML = null;
            XML = null;
            Source = "";
            FileName = "";
        }
        public void Load(XElement element, string source, string fileName, List<Sector> allSectors)
        {
            Id = XmlHelper.GetAttribute(element, "id") ?? "";
            Race = XmlHelper.GetAttribute(element, "race") ?? "";
            Owner = XmlHelper.GetAttribute(element, "owner") ?? "";
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
            XElement? positionElement = locationElement.Element("position");
            if (positionElement != null)
            {
                Position = new(
                    double.Parse(XmlHelper.GetAttribute(positionElement, "x") ?? "0"),
                    double.Parse(XmlHelper.GetAttribute(positionElement, "y") ?? "0"),
                    double.Parse(XmlHelper.GetAttribute(positionElement, "z") ?? "0")
                );
                Rotation = new(
                    double.Parse(XmlHelper.GetAttribute(positionElement, "pitch") ?? "0"),
                    double.Parse(XmlHelper.GetAttribute(positionElement, "yaw") ?? "0"),
                    double.Parse(XmlHelper.GetAttribute(positionElement, "roll") ?? "0")
                );
                PositionXML = positionElement;
            }
            Source = source;
            FileName = fileName;
            XML = element;
            Sector.AddStation(this);
            Log.Debug($"Loaded station {Id} in {Sector.Name}");
        }
    }
}
