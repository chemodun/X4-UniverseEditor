using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;
using X4DataLoader.Helpers;

namespace X4DataLoader
{
    public class Zone
    {
        public string Name { get; private set; }
        public string Reference { get; private set; }
        public string PositionId { get; private set; }
        public string Source { get; private set; }
        public string FileName { get; private set; }
        public (double x, double y, double z)? Position { get; private set; }
        XElement? PositionXML { get; set; }
        public Dictionary<string, Connection> Connections { get; private set; }

        public Zone(XElement element, string source, string fileName)
        {
            Name = XmlHelper.GetAttribute(element, "name") ?? "";
            Reference = XmlHelper.GetAttribute(element, "ref") ?? "";
            string zoneClass = XmlHelper.GetAttribute(element, "class") ?? "";
            if (zoneClass != "zone")
            {
                throw new ArgumentException("Zone must have class=\"zone\"");
            }
            if (Name == "" && Reference == "")
            {
                throw new ArgumentException("Zone must have a name or reference");
            }
            Connections = new Dictionary<string, Connection>();
            var connectionsElement = element.Element("connections");
            if (connectionsElement != null)
            {
                foreach (var connectionElement in connectionsElement.Elements("connection"))
                {
                    var reference = connectionElement.Attribute("ref")?.Value;
                    Connection connection = reference switch
                    {
                        "gates" => new GateConnection(connectionElement, source, fileName),
                        _ => new Connection(connectionElement, source, fileName),
                    };
                    Connections[connection.Name] = connection;
                }
            }
            PositionId = "";
            Source = source;
            FileName = fileName;
        }
        public void SetPosition((double x, double y, double z)? position, string positionId, XElement positionXML)
        {
            if (position != null)
            {
                Position = position;
            }
            PositionId = positionId;
            PositionXML = positionXML;
        }

        public float[]? GetCoordinates()
        {
            if (Position != null)
            {
                return new float[] { (float)Position.Value.x, (float)Position.Value.y, (float)Position.Value.z };
            }
            return null;
        }
    }
}
