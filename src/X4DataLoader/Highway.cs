using System.Globalization;
using System.Xml.Linq;
using X4DataLoader.Helpers;
using Utilities.Logging;
using System.Diagnostics;

namespace X4DataLoader
{
    public class Highway
    {
        public string Macro { get; private set; }
        public string Reference { get; private set; }
        public (double x, double y, double z)? EntryPoint { get; private set; }
        public (double x, double y, double z)? ExitPoint { get; private set; }
        public string Source { get; private set; }
        public string FileName { get; private set; }
        public XElement XML { get; private set; }

        public Highway(XElement element, string source, string fileName)
        {
            Macro = XmlHelper.GetAttribute(element, "name") ?? "";
            Reference = XmlHelper.GetAttribute(element, "ref") ?? "";
            string highwayClass = XmlHelper.GetAttribute(element, "class") ?? "";
            if (highwayClass != "highway")
            {
                throw new ArgumentException("Highway must have class=\"highway\"");
            }
            if (Macro == "" && Reference == "")
            {
                throw new ArgumentException("Highway must have a name or reference");
            }

            var connectionsElement = element.Element("connections");
            if (connectionsElement != null)
            {
                foreach (var connectionElement in connectionsElement.Elements("connection"))
                {
                    var reference = connectionElement.Attribute("ref")?.Value;
                    var offsetElement = connectionElement.Element("offset");
                    var positionElement = offsetElement?.Element("position");
                    if (positionElement != null)
                    {
                        var position = (
                            double.Parse(positionElement.Attribute("x")?.Value ?? "0", CultureInfo.InvariantCulture),
                            double.Parse(positionElement.Attribute("y")?.Value ?? "0", CultureInfo.InvariantCulture),
                            double.Parse(positionElement.Attribute("z")?.Value ?? "0", CultureInfo.InvariantCulture)
                        );

                        if (reference == "entrypoint")
                        {
                            EntryPoint = position;
                        }
                        else if (reference == "exitpoint")
                        {
                            ExitPoint = position;
                        }
                    }
                }
            }

            if (EntryPoint == null)
            {
                throw new ArgumentException("Highway must have an entrypoint");
            }

            if (ExitPoint == null)
            {
                throw new ArgumentException("Highway must have an exitpoint");
            }

            Source = source;
            FileName = fileName;

            XML = element;
        }
    }

    public class HighwayClusterLevel : Highway
    {
        private Cluster? Cluster;
        HighwayClusterConnectionPath? EntryPointPath;
        HighwayClusterConnectionPath? ExitPointPath;
        public HighwayClusterLevel(XElement element, string source, string fileName) : base(element, source, fileName)
        {
            if (Reference != "standardsechighway")
            {
                throw new ArgumentException("HighwayClusterLevel must have reference=\"standardsechighway\"");
            }

        }

        public void Load(Cluster cluster)
        {
            Cluster = cluster;
            Connection? connection = cluster.Connections.Values.FirstOrDefault(c => c.MacroReference == Macro)
                ?? throw new ArgumentException($"Connection with reference {Macro} not found in Cluster {cluster.Name}");
            XElement? connectionMacroElement = connection.MacroXML
                ?? throw new ArgumentException($"Connection with reference {Macro} in Cluster {cluster.Name} has no MacroXML element");
            XElement connectionsElement = connectionMacroElement.Element("connections")
                ?? throw new ArgumentException($"Connection with reference {Macro} in Cluster {cluster.Name} has no connections element");
            foreach (XElement entry in connectionsElement.Elements("connection"))
            {
                string reference = entry.Attribute("ref")?.Value
                    ?? throw new ArgumentException("Connection must have a ref attribute");
                XElement macro = entry.Element("macro")
                    ?? throw new ArgumentException("Connection must have a macro element");
                string macroReference = macro.Attribute("ref")?.Value
                    ?? throw new ArgumentException("MacroReference must have a ref attribute");
                string path = macro.Attribute("path")?.Value
                    ?? throw new ArgumentException("MacroReference must have a path attribute");
                HighwayClusterConnectionPath? pointPath = null;
               if (reference == "entrypoint")
                {
                    EntryPointPath = new HighwayClusterConnectionPath(cluster, macroReference);
                    EntryPointPath.Load(path);
                    pointPath = EntryPointPath;
                }
                else if (reference == "exitpoint")
                {
                    ExitPointPath = new HighwayClusterConnectionPath(cluster, macroReference);
                    ExitPointPath.Load(path);
                    pointPath = ExitPointPath;
                }
                if (pointPath != null)
                {
                    if (pointPath.Sector != null && pointPath.Zone != null && pointPath.Gate != null)
                    {
                        pointPath.Sector.AddHighwayPoint(new HighwayPoint(
                            HighwayLevel.Cluster,
                            pointPath == EntryPointPath ? HighwayPointType.EntryPoint : HighwayPointType.ExitPoint,
                            pointPath.Zone,
                            pointPath.Gate
                        ));
                    }

                }
            }
        }
    }

    public class HighwaySectorLevel : Highway
    {
        public HighwaySectorLevel(XElement element, string source, string fileName) : base(element, source, fileName)
        {
            if (Reference != "standardzonehighway")
            {
                throw new ArgumentException("HighwaySectorLevel must have reference=\"standardzonehighway\"");
            }
        }

    }


    public class HighwayClusterConnectionPath(Cluster cluster, string macro)
    {
        public string Path { get; private set; } = "";
        private string GateMacro { get; set; } = macro;
        public Cluster Cluster { get; private set; } = cluster;
        public Sector? Sector { get; private set; }
        public Zone? Zone { get; private set; }
        public Zone? Gate { get; private set; }

        public void Load(string path, List<Zone>? additionalZones = null)
        {
            var pathParts = path.Split('/').Where(p => p != "..").ToArray();
            Path = string.Join("/", pathParts);
            if (pathParts.Length != 2)
            {
                throw new ArgumentException("HighwayClusterConnectionPath must have exact 2 parts");
            }

            Sector = Cluster.Sectors.FirstOrDefault(s => StringHelper.EqualsIgnoreCase(s.PositionId, pathParts[0]))
                ?? throw new ArgumentException($"Sector with PositionId {pathParts[1]} not found in Cluster {Cluster.Name}");

            if (additionalZones != null)
            {
                Zone = additionalZones.FirstOrDefault(z => StringHelper.EqualsIgnoreCase(z.PositionId, pathParts[1]))
                    ?? throw new ArgumentException($"Zone with PositionId {pathParts[2]} not found in AdditionalZones");
                if (GateMacro != "")
                {
                    Gate = additionalZones.FirstOrDefault(c => c.Name == GateMacro)
                        ?? throw new ArgumentException($"Connection with reference {GateMacro} not found in Zone {Zone.Name}");
                }
            }
            else
            {
                Zone = Sector.Zones.FirstOrDefault(z => StringHelper.EqualsIgnoreCase(z.PositionId, pathParts[1]))
                    ?? throw new ArgumentException($"Zone with PositionId {pathParts[2]} not found in Sector {Sector.Name}");
                if (GateMacro != "")
                {
                    Gate = Sector.Zones.FirstOrDefault(c => c.Name == GateMacro)
                        ?? throw new ArgumentException($"Connection with reference {GateMacro} not found in Zone {Zone.Name}");
                }
            }

        }

        public void Create(Sector sector, Zone zone)
        {
            Path = $"{sector.PositionId}/{zone.PositionId}";
            Sector = sector;
            Zone = zone;
        }
    }

    public class HighwayPoint(HighwayLevel highwayLevel, HighwayPointType type, Zone zone, Zone gate)
    {
        public HighwayLevel HighwayLevel { get; private set; } = highwayLevel;
        public HighwayPointType Type { get; private set; } = type;
        public Zone Zone { get; private set; } = zone;
        public Zone Gate { get; private set; } = gate;
        public string Name => Zone.Name;
        public Position Position => Zone.Position;
        public string Source => Zone.Source;
        public string FileName => Zone.FileName;
    }

    public enum HighwayPointType
    {
        EntryPoint,
        ExitPoint
    }

    public enum HighwayLevel
    {
        Cluster,
        Sector
    }

}
