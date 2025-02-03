using System.Xml.Linq;
using System.Text.Json;
using Utilities.Logging;

namespace X4DataLoader.Helpers
{
    public static class XmlHelper
    {
        public static string? GetAttribute(XElement element, string attributeName)
        {
            var attribute = element.Attribute(attributeName)?.Value;
            if (attribute != null)
            {
                return attribute;
            }

            var componentElement = element.Element("component");
            if (componentElement != null)
            {
                return componentElement.Attribute(attributeName)?.Value;
            }

            return null;
        }

        public static List<string> GetAttributeAsList(XElement element, string attributeName, string separator = ",")
        {
            var tags = new List<string>();
            var tagsAttribute = element.Attribute(attributeName)?.Value;
            if (!string.IsNullOrEmpty(tagsAttribute))
            {
                tags =  tagsAttribute.Trim('[', ']')
                    .Split(separator)
                    .Select(item => item.Trim())
                    .ToList();
            }

            return tags;
        }
    }
}
