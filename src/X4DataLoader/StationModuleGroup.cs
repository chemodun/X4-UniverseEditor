using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;
using X4DataLoader.Helpers;
using Utilities.Logging;
using System.Data;

namespace X4DataLoader
{
    public class StationModuleGroup
    {
        public string Name { get; private set; }
        public List<string> ModuleIds { get; private set; } = [];
        public string Source { get; private set; }
        public string FileName { get; private set; }
        public XElement? XML { get; set; }

        public StationModuleGroup()
        {
            Name = "";
            Source = "";
            FileName = "";
            XML = null;
        }

        public void Load(XElement element, string source, string fileName)
        {
            Name = XmlHelper.GetAttribute(element, "name") ?? "";
            foreach (XElement selectElement in element.Elements("select"))
            {
                string stationModuleId = XmlHelper.GetAttribute(selectElement, "macro") ?? "";
                if (stationModuleId != "")
                {
                    ModuleIds.Add(stationModuleId);
                }
            }
            Source = source;
            FileName = fileName;
            XML = element;
        }

        public static void LoadElements(IEnumerable<XElement> elements, string source, string fileName, List<StationModuleGroup> allStationModuleGroups)
        {
            foreach (XElement element in elements)
            {
                StationModuleGroup group = new();
                group.Load(element, source, fileName);
                if (group.Name == "")
                {
                    Log.Warn($"StationModuleGroup must have a name");
                    continue;
                }
                allStationModuleGroups.Add(group);
            }
        }

        public static List<string> GetModuleGroups(IEnumerable<StationModuleGroup> groups, string moduleId)
        {
            List<string> moduleGroups = [];
            foreach (StationModuleGroup group in groups)
            {
                if (group.ModuleIds.Contains(moduleId))
                {
                    moduleGroups.Add(group.Name);
                }
            }
            return moduleGroups;
        }
    }
}