using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;
using X4DataLoader.Helpers;
using Utilities.Logging;
using System.Data;

namespace X4DataLoader
{
    public class Race
    {
        public string Id { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public string ShortName { get; private set; }
        public string PrefixName { get; private set; }
        public string SpaceName { get; private set; }
        public string HomeSpaceName { get; private set; }
        public List<string> Tags { get; private set; } = [];
        public int Names { get; private set; }
        public XElement? XML { get; set; }
        public string Source { get; private set; }
        public string FileName { get; private set; }
        public Race()
        {
            Id = "";
            Name = "";
            Description = "";
            ShortName = "";
            PrefixName = "";
            SpaceName = "";
            HomeSpaceName = "";
            Names = 0;
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
            Names = StringHelper.ParseInt(XmlHelper.GetAttribute(element, "names"));
            Tags = XmlHelper.GetAttributeAsList(element, "tags", " ");
            XML = element;
            Source = source;
            FileName = fileName;
        }
        public static void LoadElements(IEnumerable<XElement> elements, string source, string fileName, List<Race> allRaces, Translation translation)
        {
            foreach (XElement element in elements)
            {
                Race Race = new();
                Race.Load(element, source, fileName, translation);
                if (Race.Name == "")
                {
                    Log.Warn($"Race must have a name");
                    continue;
                }
                if (allRaces.Any(f => f.Id == Race.Id))
                {
                    Log.Warn($"Duplicate Race id {Race.Id}");
                    continue;
                }
                allRaces.Add(Race);
            }
        }
    }
}