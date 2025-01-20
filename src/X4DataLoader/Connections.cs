using System.Data.Common;
using System.Xml.Linq;
using System.Globalization;
using System.ComponentModel;
using X4DataLoader.Helpers;

namespace X4DataLoader
{
    public class Connection
    {
        public string Name { get; private set; }
        public string Reference { get; private set; }
        public bool Offset { get; private set; }
        public (double x, double y, double z)? Position { get; private set; }
        public (double qx, double qy, double qz, double qw)? Quaternion { get; private set; }
        public string MacroReference { get; private set; }
        public string MacroConnection { get; private set; }
        public string Source { get; private set; }
        public string FileName { get; private set; }
        public XElement XML { get; private set; }

        public Connection(XElement element, string source, string fileName)
        {
            Name = XmlHelper.GetAttribute(element, "name") ?? "";
            Reference = XmlHelper.GetAttribute(element, "ref") ?? "";
            if (Name == "" && Reference == "")
            {
                throw new ArgumentException("Connection must have a name or reference");
            }
            var offsetElement = element.Element("offset");
            if (offsetElement != null)
            {
                Offset = true;
                var positionElement = offsetElement.Element("position");
                if (positionElement != null)
                {
                    Position = (
                        double.Parse(positionElement.Attribute("x")?.Value ?? "0", CultureInfo.InvariantCulture),
                        double.Parse(positionElement.Attribute("y")?.Value ?? "0", CultureInfo.InvariantCulture),
                        double.Parse(positionElement.Attribute("z")?.Value ?? "0", CultureInfo.InvariantCulture)
                    );
                }
                var quaternionElement = offsetElement.Element("quaternion");
                if (quaternionElement != null)
                {
                    Quaternion = (
                        double.Parse(quaternionElement.Attribute("qx")?.Value ?? "0", CultureInfo.InvariantCulture),
                        double.Parse(quaternionElement.Attribute("qy")?.Value ?? "0", CultureInfo.InvariantCulture),
                        double.Parse(quaternionElement.Attribute("qz")?.Value ?? "0", CultureInfo.InvariantCulture),
                        double.Parse(quaternionElement.Attribute("qw")?.Value ?? "0", CultureInfo.InvariantCulture)
                    );
                }
            }
            else
            {
                Offset = false;
            }
            var macroElement = element.Element("macro");
            if (macroElement != null)
            {
                MacroReference = XmlHelper.GetAttribute(macroElement, "ref") ?? "";
                MacroConnection = XmlHelper.GetAttribute(macroElement, "connection") ?? "";
                if (MacroReference == "" || MacroConnection == "")
                {
                    throw new ArgumentException("Macro element of Connection must have ref and connection attributes");
                }
            }

            XML = element;
            Source = source;
            FileName = fileName;
        }

        public float[]? GetCoordinates()
        {
            if (Position != null)
            {
                return new float[] { (float)Position.Value.x, (float)Position.Value.y, (float)Position.Value.z };
            }
            return null;
        }
        public static void LoadConnections(XElement element, List<Cluster> allClusters, List<Sector> allSectors, string source, string fileName)
        {
            string name = XmlHelper.GetAttribute(element, "name") ?? throw new ArgumentException("Connections list must have a name");
            string connectionClass = XmlHelper.GetAttribute(element, "class") ?? throw new ArgumentException("Connections list must have a class");
            string reference = XmlHelper.GetAttribute(element, "ref") ?? throw new ArgumentException("Connections list must have a ref");
            int sectorId = 0;
            int clusterId = 0;
            var connections = new List<Connection>();
            try {
                if (Cluster.IsClusterMacro(name)) {
                    (string clusterIdPrefix, clusterId)  = Cluster.GetClusterIdData(name);
                } else if (Sector.IsSectorMacro(name)) {
                    (string clusterIdPrefix, clusterId, string sectorIdPrefix, sectorId) = Sector.GetSectorIdData(name);
                } else {
                    throw new ArgumentException("Invalid macro format");
                }
                if (ConnectionOwnerClasses.Contains(connectionClass) == false)
                {
                    throw new ArgumentException("Connection must have a valid class: {string.Join(", ", ConnectionOwnerClasses)}");
                }
                else
                if (clusterId == 0 && sectorId == 0)
                {
                    throw new ArgumentException("Connection macro must have a cluster or sector id");
                }
                else if(connectionClass == "cluster" && clusterId == 0)
                {
                    throw new ArgumentException("Cluster connection must have a cluster id");
                }
                else if (connectionClass == "sector" && clusterId == 00 && sectorId == 0)
                {
                    throw new ArgumentException("Sector connection must have both cluster and sector id");

                }
                var connectionsElement = element.Element("connections");
                if (connectionsElement != null)
                {
                    foreach (var connectionElement in connectionsElement.Elements("connection"))
                    {
                        var connectionName = connectionElement.Attribute("name")?.Value;
                        var connectionReference = connectionElement.Attribute("ref")?.Value;

                        if (connectionName != null && connectionReference == "sectors")
                        {
                            var positionElement = connectionElement.Element("offset")?.Element("position");
                            var position = positionElement != null
                                ? (
                                    double.Parse(positionElement.Attribute("x")?.Value ?? "0", CultureInfo.InvariantCulture),
                                    double.Parse(positionElement.Attribute("y")?.Value ?? "0", CultureInfo.InvariantCulture),
                                    double.Parse(positionElement.Attribute("z")?.Value ?? "0", CultureInfo.InvariantCulture)
                                  )
                                : (0.0, 0.0, 0.0);

                            var macroElement = connectionElement.Element("macro");
                            if (macroElement != null)
                            {
                                var macroRef = XmlHelper.GetAttribute(macroElement, "ref");
                                var macroConnection = XmlHelper.GetAttribute(macroElement, "connection");
                                if (macroConnection == "cluster")
                                {
                                    var sector = allSectors.FirstOrDefault(s => StringHelper.EqualsIgnoreCase(s.Macro, macroRef));
                                    if (sector != null)
                                    {
                                        sector.SetPosition(position, connectionName, connectionElement);
                                    }
                                }
                            }
                        } else {
                            Connection connection = connectionReference switch
                                {
                                    "entrypoint" => new EntryPointConnection(connectionElement, source, fileName),
                                    "exitpoint" => new ExitPointConnection(connectionElement, source, fileName),
                                    "zonehighways" => new ZoneHighwayConnection(connectionElement, source, fileName),
                                    "zones" => new ZoneConnection(connectionElement, source, fileName),
                                    "content" => new ContentConnection(connectionElement, source, fileName),
                                    "regions" => new RegionConnection(connectionElement, source, fileName),
                                    "sechighways" => new SecHighwayConnection(connectionElement, source, fileName),
                                    "gate" => new GateConnection(connectionElement, source, fileName),
                                    _ => new Connection(connectionElement, source, fileName)
                                };
                                connections.Add(connection);
                        }
                    }
                }
                if (sectorId > 0 && clusterId > 0)
                {
                    var sector = allSectors.Find(s => s.Id == sectorId && s.ClusterId == clusterId);
                    if (sector != null)
                    {
                        sector.Reference = reference;
                        foreach (var connection in connections)
                        {
                            sector.Connections[connection.Name] = connection;
                        }
                    }
                }
                else if (clusterId > 0)
                {
                    var cluster = allClusters.Find(c => c.Id == clusterId);
                    if (cluster != null)
                    {
                        cluster.Reference = reference;
                        foreach (var connection in connections)
                        {
                            cluster.Connections[connection.Name] = connection;
                        }
                    }
                }
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException($"Error loading Connections: {e.Message}");
            }
        }

        public static List<string> ConnectionOwnerClasses = new List<string>
        {
            "cluster",
            "sector",
            "zone"
        };
    }

    public class EntryPointConnection : Connection
    {
        public EntryPointConnection(XElement element, string source, string fileName) : base(element, source, fileName) { }
    }

    public class ExitPointConnection : Connection
    {
        public ExitPointConnection(XElement element, string source, string fileName) : base(element, source, fileName) { }
    }

    public class ZoneHighwayConnection : Connection
    {
        public ZoneHighwayConnection(XElement element, string source, string fileName) : base(element, source, fileName) { }
    }

    public class ZoneConnection : Connection
    {
        public ZoneConnection(XElement element, string source, string fileName) : base(element, source, fileName) { }
    }

    public class ContentConnection : Connection
    {
        public ContentConnection(XElement element, string source, string fileName) : base(element, source, fileName) { }
    }

    public class RegionConnection : Connection
    {
        public RegionConnection(XElement element, string source, string fileName) : base(element, source, fileName) { }
    }

    public class SecHighwayConnection : Connection
    {
        public SecHighwayConnection(XElement element, string source, string fileName) : base(element, source, fileName) { }
    }

    public class GateConnection : Connection
    {
        public string GateMacro { get; private set; }
        public bool IsActive { get; private set; }

        public GateConnection(XElement element, string source, string fileName) : base(element, source, fileName) {
            var macroElement = element.Element("macro") ?? throw new ArgumentException("Gate connection must have a macro element");
            GateMacro = XmlHelper.GetAttribute(macroElement, "ref") ?? throw new ArgumentException("Gate connection must have a macro ref attribute");
            IsActive = true;
            var propertiesElement = macroElement.Element("properties");
            if (propertiesElement != null)
            {
                var stateElement = propertiesElement.Element("state");
                if (stateElement != null)
                {
                    IsActive = XmlHelper.GetAttribute(stateElement, "active") == "true";
                }
            }
         }
    }
}
