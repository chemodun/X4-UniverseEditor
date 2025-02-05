using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Utilities.Logging;
using X4DataLoader.Helpers;

namespace X4DataLoader
{
    public class Cluster
    {
        public string Name { get; private set; }
        public string Description { get; private set; }
        public int Id { get; private set; }

        public string IdPrefix { get; private set; }

        public string FullId => $"Cluster_{Id:D2}";
        public string Macro => $"Cluster_{Id:D2}_macro";
        public string Reference { get; set; }
        public Position Position { get; private set; }
        public string PositionId { get; private set; }
        public string PositionSource { get; private set; }
        public string PositionFileName { get; private set; }
        public string Source { get; private set; }
        public string FileName { get; private set; }
        public XElement? PositionXML { get; set; }
        public XElement? XML { get; set; }
        public List<Sector> Sectors { get; private set; }
        public Dictionary<string, Connection> Connections { get; private set; }
        public List<Highway> Highways { get; private set; }


        private static readonly Regex ClusterRegex = new(@"^(Cluster)_(\d+)_macro", RegexOptions.IgnoreCase);

        public Cluster()
        {
            Sectors = [];
            Name = "";
            Description = "";
            Id = 0;
            IdPrefix = "Cluster";
            Reference = "";
            Connections = [];
            Highways = [];
            Position = new Position();
            PositionId = "";
            PositionXML = null;
            PositionSource = "vanilla";
            PositionFileName = "";
            Source = "vanilla";
            FileName = "";
        }

        public void Load(XElement element, Translation translation, string source, string fileName)
        {
            Log.Debug($"Loading cluster data for {source} from {fileName}");
            var macro = element.Attribute("macro")?.Value;
            var sectorIdMatch = ClusterRegex.Match(macro ?? "");
            if (sectorIdMatch.Success)
            {
                var propertiesElement = element.Element("properties");
                string nameId = propertiesElement?.Element("identification")?.Attribute("name")?.Value ?? "";
                string descriptionId = propertiesElement?.Element("identification")?.Attribute("description")?.Value ?? "";
                if (nameId != null && nameId != "" && descriptionId != "")
                {
                    Id = StringHelper.ParseInt(sectorIdMatch.Groups[2].Value);
                    IdPrefix = sectorIdMatch.Groups[1].Value;
                    Name = translation.Translate(nameId);
                    Description = translation.Translate(descriptionId);
                    Source = source;
                    FileName = fileName;
                    XML = element;
                    Console.WriteLine($"Cluster Name: {Name}");
                }
                else
                {
                    throw new ArgumentException("Cluster must have name and description");
                }

            }
            else
            {
                throw new ArgumentException($"Invalid macro format: {macro}");
            }
        }

        public void SetPosition(Position position, string positionId, XElement positionXML, string source, string fileName)
        {
            Position = position;
            PositionId = positionId;
            PositionXML = positionXML;
            PositionSource = source;
            PositionFileName = fileName;
        }

        public static bool IsClusterMacro(string macro) => ClusterRegex.IsMatch(macro);

        public static (string, int) GetClusterIdData(string macro)
        {
            var match = ClusterRegex.Match(macro);
            if (match.Success)
            {
                return (match.Groups[1].Value, StringHelper.ParseInt(match.Groups[2].Value));
            }
            throw new ArgumentException($"Invalid macro format: {macro}");
        }
    }
}
