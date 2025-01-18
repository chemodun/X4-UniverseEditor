using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace X4DataLoader
{
    public class Sector
    {
        public string Name { get; private set; }
        public string Description { get; private set; }
        public int Id { get; private set; }
        public string IdPrefix { get; private set; }
        public int ClusterId { get; private set; }
        public string ClusterIdPrefix { get; private set; }
        public string FullId => $"Cluster_{ClusterId:D2}_Sector{Id:D3}";
        public string Macro => $"Cluster_{ClusterId:D2}_Sector{Id:D3}_macro";
        public string Reference { get; set; }
        public (double x, double y, double z)? Position { get; private set; }
        public string PositionId { get; private set; }
        XElement? PositionXML { get; set; }
        XElement? XML { get; set; }

        public List<Zone> Zones { get; private set; }
        public Dictionary<string, Connection> Connections { get; private set; }
        public List<Highway> Highways { get; private set; }
        private static readonly Regex SectorRegex = new Regex(@"^(Cluster)_(\d+)_(Sector)(\d+)_macro", RegexOptions.IgnoreCase);
        public Sector()
        {
            Name = "";
            Description = "";
            Id = 0;
            ClusterId = 0;
            IdPrefix = "Sector";
            ClusterIdPrefix = "Cluster";
            Zones = new List<Zone>();
            Connections = new Dictionary<string, Connection>();
            Highways = new List<Highway>();
            Reference = "";
            Position = null;
            PositionId = "";
        }

        public void Load(XElement element, Translation translation)
        {
            var macro = element.Attribute("macro")?.Value;
            var sectorIdMatch = SectorRegex.Match(macro ?? "");
            if (sectorIdMatch.Success)
            {
                var propertiesElement = element.Element("properties");
                string nameId = propertiesElement?.Element("identification")?.Attribute("name")?.Value ?? "";
                string descriptionId = propertiesElement?.Element("identification")?.Attribute("description")?.Value?? "";
                if (nameId != null && nameId != ""  && descriptionId != null && descriptionId != "")
                {
                    Id = int.Parse(sectorIdMatch.Groups[4].Value, CultureInfo.InvariantCulture);
                    IdPrefix = sectorIdMatch.Groups[3].Value;
                    ClusterId = int.Parse(sectorIdMatch.Groups[2].Value, CultureInfo.InvariantCulture);
                    ClusterIdPrefix = sectorIdMatch.Groups[1].Value;
                    Name = translation.Translate(nameId ?? "");
                    Description = translation.Translate(descriptionId);
                    XML = element;
                    Console.WriteLine($"Sector Name: {Name}");
                }
                else {
                    throw new ArgumentException("Sector must have name and description");
                }
            }
            else {
                throw new ArgumentException($"Invalid macro format: {macro}");
            }
        }

        public void SetPosition((double x, double y, double z) position, string positionId, XElement positionXML)
        {
            Position = position;
            PositionId = positionId;
            PositionXML = positionXML;
        }

        public static bool IsSectorMacro(string macro) => SectorRegex.IsMatch(macro);

        public static (string, int, string, int) GetSectorIdData(string macro)
        {
            var match = SectorRegex.Match(macro);
            if (match.Success)
            {
                return (match.Groups[1].Value, int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture), match.Groups[3].Value, int.Parse(match.Groups[4].Value, CultureInfo.InvariantCulture));
            }
            throw new ArgumentException($"Invalid macro format: {macro}");
        }
    }
}
