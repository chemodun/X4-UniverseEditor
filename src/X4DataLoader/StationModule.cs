using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;
using X4DataLoader.Helpers;
using Utilities.Logging;
using System.Data;

namespace X4DataLoader
{
    public class StationModule
    {
        public string Id { get; private set; }
        public string GroupId { get; private set; }
        public List<string> Tags { get; private set; } = [];
        public List<string> Factions { get; private set; } = [];
        public string Source { get; private set; }
        public string FileName { get; private set; }
        public XElement? XML { get; set; }

        public StationModule()
        {
            Id = "";
            GroupId = "";
            Source = "";
            FileName = "";
            XML = null;
        }

        public void Load(XElement element, string source, string fileName)
        {
            Id = XmlHelper.GetAttribute(element, "id") ?? "";
            GroupId = XmlHelper.GetAttribute(element, "group") ?? "";
            XElement? categoryElement = element.Element("category");
            if (categoryElement != null)
            {
                Tags = XmlHelper.GetAttributeAsList(categoryElement, "tags");
                Factions = XmlHelper.GetAttributeAsList(categoryElement, "faction");
            }
            Source = source;
            FileName = fileName;
            XML = element;
        }

        public bool IsContainsTag(string tag)
        {
            return Tags.Contains(tag);
        }

        public static void LoadElements(IEnumerable<XElement> elements, string source, string fileName, List<StationModule> allModules)
        {
            foreach (XElement element in elements)
            {
                StationModule module = new();
                module.Load(element, source, fileName);
                allModules.Add(module);
            }
        }

        public static bool IsModuleWithGroupContainsTag(List<StationModule> allModules, string groupId, string tag)
        {
            return allModules.Any(module => module.GroupId == groupId && module.IsContainsTag(tag));
        }
    }
}