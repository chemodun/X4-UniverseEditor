using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;
using X4DataLoader.Helpers;
using Utilities.Logging;
using System.Data;

namespace X4DataLoader
{
    public class Faction
    {
        public string Id { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public string ShortName { get; private set; }
        public string PrefixName { get; private set; }
        public string SpaceName { get; private set; }
        public string HomeSpaceName { get; private set; }
        public string PrimaryRace { get; private set; }
        public string BehaviorSet { get; private set; }
        public List<string> Tags { get; private set; } = [];
        public string PoliceFaction { get; private set; }
        public string ColorId { get; private set; }
        public string IconActiveId { get; private set; }
        public string IconInactiveId { get; private set; }
        public XElement? XML { get; set; }
        public string Source { get; private set; }
        public string FileName { get; private set; }
        public Faction()
        {
            Id = "";
            Name = "";
            Description = "";
            ShortName = "";
            PrefixName = "";
            SpaceName = "";
            HomeSpaceName = "";
            PrimaryRace = "";
            BehaviorSet = "";
            PoliceFaction = "";
            ColorId = "";
            IconActiveId = "";
            IconInactiveId = "";
            XML = null;
            Source = "";
            FileName = "";
        }
        public void Load(XElement element, string source, string fileName, Translation translation)
        {
            Id = XmlHelper.GetAttribute(element, "id") ?? "";
            Name = translation.Translate(XmlHelper.GetAttribute(element, "name") ?? "");
            Description = translation.Translate(XmlHelper.GetAttribute(element, "description") ?? "");
            ShortName = translation.Translate(XmlHelper.GetAttribute(element, "shortname") ?? "");
            PrefixName = translation.Translate(XmlHelper.GetAttribute(element, "prefixname") ?? "");
            SpaceName = translation.Translate(XmlHelper.GetAttribute(element, "spacename") ?? "");
            HomeSpaceName = translation.Translate(XmlHelper.GetAttribute(element, "homespacename") ?? "");
            PrimaryRace = XmlHelper.GetAttribute(element, "primaryrace") ?? "";
            BehaviorSet = XmlHelper.GetAttribute(element, "behaviorset") ?? "";
            Tags = XmlHelper.GetAttributeAsList(element, "tags", " ");
            PoliceFaction = XmlHelper.GetAttribute(element, "policefaction") ?? "";
            XElement? colorElement = element.Element("color");
            if (colorElement != null)
            {
                ColorId = XmlHelper.GetAttribute(colorElement, "ref") ?? "";
            }
            XElement? iconElement = element.Element("icon");
            if (iconElement != null)
            {
                IconActiveId = XmlHelper.GetAttribute(iconElement, "active") ?? "";
                IconInactiveId = XmlHelper.GetAttribute(iconElement, "inactive") ?? "";
            }
            XML = element;
            Source = source;
            FileName = fileName;
        }

        public bool IsContainsTag(string tag)
        {
            return Tags.Contains(tag);
        }

        public static void LoadElements(IEnumerable<XElement> elements, string source, string fileName, List<Faction> allFactions, Translation translation)
        {
            foreach (XElement element in elements)
            {
                Faction faction = new();
                faction.Load(element, source, fileName, translation);
                if (faction.Name == "")
                {
                    Log.Warn($"Faction must have a name");
                    continue;
                }
                allFactions.Add(faction);
            }
        }
    }
}