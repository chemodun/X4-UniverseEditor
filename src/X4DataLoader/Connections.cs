using System.Data.Common;
using System.Xml.Linq;
using System.Globalization;
using System.ComponentModel;
using X4DataLoader.Helpers;
using System.Data.SqlTypes;
using Utilities.Logging;

namespace X4DataLoader
{
    public class Connection
    {
        public string Name { get; private set; }
        public string Reference { get; protected set; }
        public bool Offset { get; private set; }
        public Position? Position { get; private set; }
        public Quaternion? Quaternion { get; private set; }
        public string MacroReference { get; private set; }
        public string MacroConnection { get; private set; }
        public string Source { get; private set; }
        public string FileName { get; private set; }
        public XElement? XML { get; private set; }
        public XElement? MacroXML { get; private set; }

        public Connection()
        {
            Name = "";
            Reference = "";
            Offset = false;
            Position = null;
            Quaternion = null;
            MacroReference = "";
            MacroConnection = "";
            Source = "";
            FileName = "";
        }
        public virtual void Load(XElement element, string source, string fileName)
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
                    Position = new Position(
                        double.Parse(positionElement.Attribute("x")?.Value ?? "0", CultureInfo.InvariantCulture),
                        double.Parse(positionElement.Attribute("y")?.Value ?? "0", CultureInfo.InvariantCulture),
                        double.Parse(positionElement.Attribute("z")?.Value ?? "0", CultureInfo.InvariantCulture)
                    );
                }
                var quaternionElement = offsetElement.Element("quaternion");
                if (quaternionElement != null)
                {
                    Quaternion = new Quaternion(
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
                else
                {
                    MacroXML = macroElement;
                }
            }

            XML = element;
            Source = source;
            FileName = fileName;
        }

        public virtual void Create(string name, Position? position, Quaternion? quaternion, Dictionary<string, string>? properties = null)
        {
            Name = name;
            Position = position ?? new Position();
            Quaternion = quaternion ?? new Quaternion();
            XML = new XElement("connection");
            XML.SetAttributeValue("name", Name);
            XML.SetAttributeValue("ref", Reference);
            if (!(Position.X == 0 && Position.Y == 0 && Position.Z == 0))
            {
                var offsetElement = new XElement("offset");
                var positionElement = new XElement("position");
                positionElement.SetAttributeValue("x", Position.X);
                positionElement.SetAttributeValue("y", Position.Y);
                positionElement.SetAttributeValue("z", Position.Z);
                offsetElement.Add(positionElement);
                XML.Add(offsetElement);
            }
            if (!(Quaternion.QX == 0 && Quaternion.QY == 0 && Quaternion.QZ == 0 && Quaternion.QW == 0))
            {
                var offsetElement = XML.Element("offset") ?? new XElement("offset");
                var quaternionElement = new XElement("quaternion");
                quaternionElement.SetAttributeValue("qx", Quaternion.QX);
                quaternionElement.SetAttributeValue("qy", Quaternion.QY);
                quaternionElement.SetAttributeValue("qz", Quaternion.QZ);
                quaternionElement.SetAttributeValue("qw", Quaternion.QW);
                offsetElement.Add(quaternionElement);
                if (XML.Element("offset") == null)
                {
                    XML.Add(offsetElement);
                }
            }
            if (properties != null && properties.Count > 0 && properties.TryGetValue("macroReference", out string? macroReference) && properties.TryGetValue("macroConnection", out string? macroConnection))
            {
                var macroElement = new XElement("macro");
                MacroConnection = macroConnection;
                MacroReference = macroReference;
                macroElement.SetAttributeValue("ref", MacroReference);
                macroElement.SetAttributeValue("connection", MacroConnection);
                XML.Add(macroElement);
            }
        }

        public static void LoadConnections(XElement element, List<Cluster> allClusters, List<Sector> allSectors, string source, string fileName)
        {
            string name = XmlHelper.GetAttribute(element, "name") ?? throw new ArgumentException("Connections list must have a name");
            string connectionClass = XmlHelper.GetAttribute(element, "class") ?? throw new ArgumentException("Connections list must have a class");
            string reference = XmlHelper.GetAttribute(element, "ref") ?? throw new ArgumentException("Connections list must have a ref");
            int sectorId = 0;
            int clusterId = 0;
            var connections = new List<Connection>();
            try
            {
                if (Cluster.IsClusterMacro(name))
                {
                    (string clusterIdPrefix, clusterId) = Cluster.GetClusterIdData(name);
                }
                else if (Sector.IsSectorMacro(name))
                {
                    (string clusterIdPrefix, clusterId, string sectorIdPrefix, sectorId) = Sector.GetSectorIdData(name);
                }
                else
                {
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
                else if (connectionClass == "cluster" && clusterId == 0)
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
                            Position position = positionElement != null
                                ? new Position(
                                    double.Parse(positionElement.Attribute("x")?.Value ?? "0", CultureInfo.InvariantCulture),
                                    double.Parse(positionElement.Attribute("y")?.Value ?? "0", CultureInfo.InvariantCulture),
                                    double.Parse(positionElement.Attribute("z")?.Value ?? "0", CultureInfo.InvariantCulture)
                                  )
                                : new Position();
                            XElement? macroElement = connectionElement.Element("macro");
                            if (macroElement != null)
                            {
                                string macroRef = XmlHelper.GetAttribute(macroElement, "ref") ?? "";
                                string macroConnection = XmlHelper.GetAttribute(macroElement, "connection") ?? "";
                                if (macroConnection == "cluster" && string.IsNullOrEmpty(macroRef) == false)
                                {
                                    Sector sector = allSectors.FirstOrDefault(s => StringHelper.EqualsIgnoreCase(s.Macro, macroRef));
                                    sector?.SetPosition(position, connectionName, connectionElement);
                                }
                            }
                        }
                        else
                        {
                            Connection? connection = connectionReference switch
                            {
                                "entrypoint" => new EntryPointConnection(),
                                "exitpoint" => new ExitPointConnection(),
                                "zonehighways" => new ZoneHighwayConnection(),
                                "zones" => new ZoneConnection(),
                                "content" => new ContentConnection(),
                                "regions" => new RegionConnection(),
                                "sechighways" => new SecHighwayConnection(),
                                "gate" => new GateConnection(),
                                _ => new Connection()
                            };
                            connection.Load(connectionElement, source, fileName);
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

        private static readonly List<string> ConnectionOwnerClasses =
        [
            "cluster",
            "sector",
            "zone"
        ];
    }

    public class EntryPointConnection : Connection
    {
        public EntryPointConnection() : base() { }
    }

    public class ExitPointConnection : Connection
    {
        public ExitPointConnection() : base() { }
    }

    public class ZoneHighwayConnection : Connection
    {
        public ZoneHighwayConnection() : base() { }
    }

    public class ZoneConnection : Connection
    {
        public ZoneConnection() : base() { }
    }

    public class ContentConnection : Connection
    {
        public ContentConnection() : base() { }
    }

    public class RegionConnection : Connection
    {
        public RegionConnection() : base() { }
    }

    public class SecHighwayConnection : Connection
    {
        public SecHighwayConnection() : base() { }
    }

    public class GateConnection : Connection
    {
        public string GateMacro => MacroReference;
        public bool IsActive { get; private set; }

        public GateConnection() : base()
        {
            Reference = "gates";
            IsActive = false;
        }

        public override void Load(XElement element, string source, string fileName)
        {
            base.Load(element, source, fileName);
            if (MacroXML == null)
            {
                throw new ArgumentException("Gate connection must have a macro element");
            }
            IsActive = true;
            var propertiesElement = MacroXML.Element("properties");
            if (propertiesElement != null)
            {
                var stateElement = propertiesElement.Element("state");
                if (stateElement != null)
                {
                    IsActive = XmlHelper.GetAttribute(stateElement, "active") == "true";
                }
            }
        }

        public override void Create(string name, Position? position, Quaternion? quaternion, Dictionary<string, string>? properties = null)
        {
            if (properties != null && properties.Count > 0 && properties.TryGetValue("gateMacro", out string? macroReference))
            {
                properties["macroReference"] = macroReference;
                properties["macroConnection"] = "space";
            }
            base.Create(name, position, quaternion, properties);
            if (properties != null && properties.Count > 0 && properties.TryGetValue("isActive", out string? isActive))
            {
                IsActive = isActive == "true";
                XElement macroElement = XML?.Element("macro") ?? new XElement("macro");
                XElement propertiesElement = macroElement.Element("properties") ?? new XElement("properties");
                XElement stateElement = propertiesElement.Element("state") ?? new XElement("state");
                stateElement.SetAttributeValue("active", IsActive);
                propertiesElement.Add(stateElement);
                macroElement.Add(propertiesElement);
                if (XML?.Element("macro") == null)
                {
                    XML?.Add(macroElement);
                }
            }
        }
    }

    public class Position(double x = 0.0, double y = 0.0, double z = 0.0)
    {
        public double X { get; private set; } = x;
        public double Y { get; private set; } = y;
        public double Z { get; private set; } = z;

        public override string ToString()
        {
            return $"X: {X}, Y: {Y}, Z: {Z}";
        }
    }

    public class Quaternion(double qx = 0.0, double qy = 0.0, double qz = 0.0, double qw = 0.0)
    {
        public double QX { get; private set; } = qx;
        public double QY { get; private set; } = qy;
        public double QZ { get; private set; } = qz;
        public double QW { get; private set; } = qw;

        public override string ToString()
        {
            return $"QX: {QX}, QY: {QY}, QZ: {QZ}, QW: {QW}";
        }
    }

    public class Rotation(double pitch = 0.0, double yaw = 0.0, double roll = 0.0)
    {
        public double Pitch { get; private set; } = pitch;
        public double Yaw { get; private set; } = yaw;
        public double Roll { get; private set; } = roll;

        public static Rotation FromQuaternion(Quaternion quaternion)
        {
            double pitch = Math.Atan2(2 * (quaternion.QY * quaternion.QZ + quaternion.QX * quaternion.QW), 1 - 2 * (quaternion.QX * quaternion.QX + quaternion.QY * quaternion.QY));
            double yaw = Math.Asin(2 * (quaternion.QX * quaternion.QZ - quaternion.QY * quaternion.QW));
            double roll = Math.Atan2(2 * (quaternion.QX * quaternion.QY + quaternion.QZ * quaternion.QW), 1 - 2 * (quaternion.QY * quaternion.QY + quaternion.QZ * quaternion.QZ));
            return new Rotation(pitch, yaw, roll);
        }

        public override string ToString()
        {
            return $"Pitch: {Pitch}, Yaw: {Yaw}, Roll: {Roll}";
        }
    }
}
