using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;

namespace X4DataLoader
{
    public class Zone
    {
        public string Name { get; private set; }
        public string Reference { get; private set; }
        public string ConnectionId { get; private set; }
        public string Source { get; private set; }
        public string FileName { get; private set; }
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
            ConnectionId = "";
            Source = source;
            FileName = fileName;
        }

        public void SetConnectionId(string connectionId)
        {
            ConnectionId = connectionId;
        }
    }
}
