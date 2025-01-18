using System.Xml.Linq;

namespace X4DataLoader
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
    }
}
