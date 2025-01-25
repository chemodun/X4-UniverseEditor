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
        public Position Position { get; private set; }
        public XElement? PositionXML { get; set; }
        public Dictionary<string, Connection> Connections { get; private set; }
        public XElement? XML { get; set; }

        public Zone()
        {
            Name = "";
            Reference = "";
            PositionId = "";
            Source = "";
            FileName = "";
            Position = new Position();
            PositionXML = null;
            Connections = [];
        }
        public void Load(XElement element, string source, string fileName)
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
            Connections = [];
            var connectionsElement = element.Element("connections");
            if (connectionsElement != null)
            {
                foreach (var connectionElement in connectionsElement.Elements("connection"))
                {
                    var reference = connectionElement.Attribute("ref")?.Value;
                    var connection = reference switch
                    {
                        "gates" => new GateConnection(),
                        _ => new Connection(),
                    };
                    connection.Load(connectionElement, source, fileName);
                    Connections[connection.Name] = connection;
                }
            }
            PositionId = "";
            Source = source;
            FileName = fileName;
        }

        public void SetPosition(Position? position, string positionId, XElement? positionXML)
        {
            if (position != null)
            {
                Position = position;
            }
            PositionId = positionId;
            PositionXML = positionXML;
        }

        public void Create(string name, Dictionary<string, Connection> connections, Position position, string positionId)
        {
            Name = name;
            Connections = connections;
            Position = position;
            PositionId = positionId;
            PositionXML = new XElement("connection");
            PositionXML.SetAttributeValue("name", PositionId);
            PositionXML.SetAttributeValue("ref", "zones");
            if (!(Position.X == 0 && Position.Y == 0 && Position.Z == 0))
            {
                XElement offset = new("offset");
                XElement positionElement = new("position");
                positionElement.SetAttributeValue("x", Position.X);
                positionElement.SetAttributeValue("y", Position.Y);
                positionElement.SetAttributeValue("z", Position.Z);
                offset.Add(positionElement);
                PositionXML.Add(offset);
            }
            XML = new XElement("macro");
            XML.SetAttributeValue("name", Name);
            XML.SetAttributeValue("class", "zone");
            XElement component = new("component");
            component.SetAttributeValue("ref", "standardzone");
            XML.Add(component);
            if (Connections.Count > 0)
            {
                XElement connectionsElement = new("connections");
                foreach (var connection in Connections.Values)
                {
                    if (connection.XML != null)
                    {
                        XElement connectionElement = connection.XML;
                        connectionsElement.Add(connectionElement);
                    }
                }
                XML.Add(connectionsElement);
            }
        }
    }
}
