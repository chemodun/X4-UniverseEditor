using System.Globalization;
using System.Xml.Linq;
using X4DataLoader.Helpers;

namespace X4DataLoader
{
    public class Highway
    {
        public string Name { get; private set; }
        public string Reference { get; private set; }
        public (double x, double y, double z)? EntryPoint { get; private set; }
        public (double x, double y, double z)? ExitPoint { get; private set; }
        public string Source { get; private set; }
        public string FileName { get; private set; }
        public XElement XML { get; private set; }

        public Highway(XElement element, string source, string fileName)
        {
            Name = XmlHelper.GetAttribute(element, "name") ?? "";
            Reference = XmlHelper.GetAttribute(element, "ref") ?? "";
            string highwayClass = XmlHelper.GetAttribute(element, "class") ?? "";
            if (highwayClass != "highway")
            {
                throw new ArgumentException("Highway must have class=\"highway\"");
            }
            if (Name == "" && Reference == "")
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
        public HighwayClusterLevel(XElement element, string source, string fileName) : base(element, source, fileName)
        {
            if (Reference != "standardsechighway")
            {
                throw new ArgumentException("HighwayClusterLevel must have reference=\"standardsechighway\"");
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
}
