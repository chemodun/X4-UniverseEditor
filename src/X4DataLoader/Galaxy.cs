using System.Globalization;
using System.Runtime.Serialization.Formatters;
using System.Xml.Linq;
using X4DataLoader.Helpers;
using Utilities.Logging;

namespace X4DataLoader
{
    public class Galaxy
    {
        public string Name { get; private set; }
        public string Reference { get; private set; }
        public int Version { get; set; } = 0;
        public List<Cluster> Clusters { get; private set; }
        public List<Sector> Sectors { get; private set; }
        public List<X4Color> Colors { get; private set; } = [];
        public List<X4MappedColor> MappedColors { get; private set; } = [];
        public List<Race> Races { get; private set; } = [];
        public List<Faction> Factions { get; private set; } = [];
        public List<StationModule> StationModules { get; private set; } = [];
        public List<StationModuleGroup> StationModuleGroups { get; private set; } = [];
        public List<ConstructionPlan> ConstructionPlans { get; private set; } = [];
        public List<StationGroup> StationGroups { get; private set; } = [];
        public List<StationCategory> StationCategories { get; private set; } = [];
        public List<GalaxyConnection> Connections { get; private set; }

        public Galaxy()
        {
            Name = "";
            Reference = "";
            Clusters = [];
            Sectors = [];
            Connections = [];
        }

        public void Load(XElement element, List<Cluster> allClusters, string source, string fileName)
        {
            Name = XmlHelper.GetAttribute(element, "name") ?? "";
            Reference = XmlHelper.GetAttribute(element, "ref") ?? "";
            string galaxyClass = XmlHelper.GetAttribute(element, "class") ?? "";
            if (galaxyClass != "galaxy")
            {
                throw new ArgumentException("Galaxy must have class=\"galaxy\"");
            }
            if (Name == "" && Reference == "")
            {
                throw new ArgumentException("Galaxy must have a name or reference");
            }

            var connectionsElement = element.Element("connections");
            if (connectionsElement != null)
            {
                LoadConnections(connectionsElement, allClusters, source, fileName);
            }
        }
        public void LoadConnections(XElement connectionsElement, List<Cluster> allClusters, string source, string fileName, List<Zone>? additionalZones = null)
        {
            foreach (var connectionElement in connectionsElement.Elements("connection"))
            {
                var reference = connectionElement.Attribute("ref")?.Value;
                if (reference == "clusters")
                {
                    var name = XmlHelper.GetAttribute(connectionElement, "name") ?? "";
                    var offsetElement = connectionElement.Element("offset");
                    var positionElement = offsetElement?.Element("position");
                    Position position = positionElement != null
                        ? new Position(
                            StringHelper.ParseDouble(positionElement.Attribute("x")?.Value ?? "0"),
                            StringHelper.ParseDouble(positionElement.Attribute("y")?.Value ?? "0"),
                            StringHelper.ParseDouble(positionElement.Attribute("z")?.Value ?? "0")
                            )
                        : new Position(0, 0, 0);

                    var macroElement = connectionElement.Element("macro");
                    if (macroElement != null)
                    {
                        string macroRef = XmlHelper.GetAttribute(macroElement, "ref") ?? "";
                        string macroConnection = XmlHelper.GetAttribute(macroElement, "connection") ?? "";
                        if (macroConnection == "galaxy" && string.IsNullOrEmpty(macroRef) == false)
                        {
                            var cluster = allClusters.FirstOrDefault(c => StringHelper.EqualsIgnoreCase(c.Macro, macroRef));
                            if (cluster != null)
                            {
                                cluster.SetPosition(position, name, connectionElement, source, fileName);
                                Clusters.Add(cluster);
                                cluster.Sectors.ForEach(s => Sectors.Add(s));
                            }
                        }
                    }
                }
                else if (reference == "destination")
                {
                    var galaxyConnection = new GalaxyConnection();
                    galaxyConnection.Load(connectionElement, allClusters, source, fileName, additionalZones);
                    Connections.Add(galaxyConnection);
                }
            }
        }

        public Dictionary<string, Cluster> GetClusters()
        {
            var clusters = new Dictionary<string, Cluster>();
            foreach (var cluster in Clusters)
            {
                clusters[cluster.PositionId] = cluster;
            }
            return clusters;
        }

        public Dictionary<string, Sector> GetSectors()
        {
            var sectors = new Dictionary<string, Sector>();
            foreach (var sector in Sectors)
            {
                sectors[sector.PositionId] = sector;
            }
            return sectors;
        }

        public Cluster? GetClusterById(int? id)
        {
            if (id == null)
            {
                return null;
            }
            return Clusters.FirstOrDefault(c => c.Id == id);
        }

        public Sector? GetSectorByMacro(string? macro)
        {
            if (macro == null)
            {
                return null;
            }
            return Sectors.FirstOrDefault(s => StringHelper.EqualsIgnoreCase(s.Macro, macro));
        }

        public Sector? GetOppositeSectorForGateConnection(GateConnection gateConnection)
        {
            var connection = Connections.FirstOrDefault(c => c.PathDirect?.Gate == gateConnection);
            if (connection != null && connection.PathOpposite != null)
            {
                return connection.PathOpposite.Sector;
            }
            else
            {
                connection = Connections.FirstOrDefault(c => c.PathOpposite?.Gate == gateConnection);
                if (connection != null && connection.PathDirect != null)
                {
                    return connection.PathDirect?.Sector;
                }
            }
            return null;
        }

        public List<Sector> GetOppositeSectorsFromConnections(Sector sector)
        {
            var connections = Connections.Where(c => (c.PathDirect?.Sector == sector) || (c.PathOpposite?.Sector == sector));
            List<Sector> result = [];
            foreach (var connection in connections)
            {
                if (connection.PathDirect != null && connection.PathDirect.Sector == sector)
                {
                    if (connection.PathOpposite?.Sector != null)
                    {
                        result.Add(connection.PathOpposite.Sector);
                    }
                }
                else
                {
                    if (connection.PathDirect?.Sector != null)
                    {
                        result.Add(connection.PathDirect.Sector);
                    }
                }
            }
            return result;
        }
    }


    public class GalaxyConnection
    {
        public string Name { get; private set; }
        public string Source { get; private set; }
        public string FileName { get; private set; }
        public GalaxyConnectionPath? PathDirect { get; private set; }
        public GalaxyConnectionPath? PathOpposite { get; private set; }
        public XElement? XML { get; private set; }

        public GalaxyConnection()
        {
            Name = "";
            Source = "";
            FileName = "";
            PathDirect = null;
            PathOpposite = null;
        }

        public void Load(XElement element, List<Cluster> allClusters, string source, string fileName, List<Zone>? additionalZones = null)
        {
            Name = XmlHelper.GetAttribute(element, "name") ?? "";
            var pathDirect = XmlHelper.GetAttribute(element, "path") ?? "";
            PathDirect = new GalaxyConnectionPath();
            PathDirect.Load(pathDirect, allClusters, additionalZones);

            var macroElement = element.Element("macro");
            if (macroElement != null)
            {
                var pathOpposite = XmlHelper.GetAttribute(macroElement, "path") ?? "";
                PathOpposite = new GalaxyConnectionPath();
                PathOpposite.Load(pathOpposite, allClusters, additionalZones);
            }
            else
            {
                throw new ArgumentException("GalaxyConnection must have a macro element with path attribute");
            }

            Source = source;
            FileName = fileName;

            XML = element;
        }

        public void Create(string name, Cluster clusterDirect, Sector sectorDirect, Zone zoneDirect, GateConnection gateDirect, Cluster clusterOpposite, Sector sectorOpposite, Zone zoneOpposite, GateConnection gateOpposite)
        {
            Name = name;
            PathDirect = new GalaxyConnectionPath();
            PathDirect.Create(clusterDirect, sectorDirect, zoneDirect, gateDirect);
            PathOpposite = new GalaxyConnectionPath();
            PathOpposite.Create(clusterOpposite, sectorOpposite, zoneOpposite, gateOpposite);
            XML = new XElement("connection");
            XML.SetAttributeValue("name", name);
            XML.SetAttributeValue("ref", "destination");
            List<string> path = ["..", PathDirect.Path];
            XML.SetAttributeValue("path", $"{string.Join("/", path)}");
            XElement macroElement = new("macro");
            macroElement.SetAttributeValue("connection", "destination");
            path.Clear();
            for (int i = 0; i < 5; i++)
            {
                path.Add("..");
            }
            path.Add(PathOpposite.Path);
            macroElement.SetAttributeValue("path", $"{string.Join("/", path)}");
            XML.Add(macroElement);
        }
    }

    public class GalaxyConnectionPath
    {
        public string Path { get; private set; }
        public Cluster? Cluster { get; private set; }
        public Sector? Sector { get; private set; }
        public Zone? Zone { get; private set; }
        public GateConnection? Gate { get; private set; }

        public GalaxyConnectionPath()
        {
            Path = "";
        }

        public void Load(string path, List<Cluster> allClusters, List<Zone>? additionalZones = null)
        {
            var pathParts = path.Split('/').Where(p => p != "..").ToArray();
            Path = string.Join("/", pathParts);
            if (pathParts.Length < 4)
            {
                throw new ArgumentException("GalaxyConnectionPath must have at least 4 parts");
            }

            Cluster = allClusters.FirstOrDefault(c => StringHelper.EqualsIgnoreCase(c.PositionId, pathParts[0]))
                ?? throw new ArgumentException($"Cluster with PositionId {pathParts[0]} not found");

            Sector = Cluster.Sectors.FirstOrDefault(s => StringHelper.EqualsIgnoreCase(s.PositionId, pathParts[1]))
                ?? throw new ArgumentException($"Sector with PositionId {pathParts[1]} not found in Cluster {Cluster.Name}");

            if (additionalZones != null)
            {
                Zone = additionalZones.FirstOrDefault(z => StringHelper.EqualsIgnoreCase(z.PositionId, pathParts[2]))
                    ?? throw new ArgumentException($"Zone with PositionId {pathParts[2]} not found in AdditionalZones");
            }
            else
            {
                Zone = Sector.Zones.FirstOrDefault(z => StringHelper.EqualsIgnoreCase(z.PositionId, pathParts[2]))
                    ?? throw new ArgumentException($"Zone with PositionId {pathParts[2]} not found in Sector {Sector.Name}");
            }
            Gate = Zone.Connections.Values.OfType<GateConnection>().FirstOrDefault(g => StringHelper.EqualsIgnoreCase(g.Name, pathParts[3]))
                ?? throw new ArgumentException($"GateConnection with Name {pathParts[3]} not found in Zone {Zone.Name}");
        }

        public void Create(Cluster cluster, Sector sector, Zone zone, GateConnection gate)
        {
            Path = $"{cluster.PositionId}/{sector.PositionId}/{zone.PositionId}/{gate.Name}";
            Cluster = cluster;
            Sector = sector;
            Zone = zone;
            Gate = gate;
        }
    }
}
