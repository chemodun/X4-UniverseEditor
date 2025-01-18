using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;

namespace X4DataLoader
{
    public class Zone
    {
        public string Name { get; private set; }
        public string Reference { get; private set; }
        public string ConnectionId { get; private set; }
        public Dictionary<string, Connection> Connections { get; private set; }

        public Zone(XElement element)
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
                        "gates" => new GateConnection(connectionElement),
                        _ => new Connection(connectionElement),
                    };
                    Connections[connection.Name] = connection;
                }
            }
            ConnectionId = "";
        }

        public void SetConnectionId(string connectionId)
        {
            ConnectionId = connectionId;
        }
    }
}
