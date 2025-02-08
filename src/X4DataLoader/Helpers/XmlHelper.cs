using System.Text.Json;
using System.Xml.Linq;
using Utilities.Logging;

namespace X4DataLoader.Helpers
{
  public static class XmlHelper
  {
    public static string? GetAttribute(XElement element, string attributeName)
    {
      string? attribute = element.Attribute(attributeName)?.Value;
      if (attribute != null)
      {
        return attribute;
      }

      XElement? componentElement = element.Element("component");
      if (componentElement != null)
      {
        return componentElement.Attribute(attributeName)?.Value;
      }

      return null;
    }

    public static List<string> GetAttributeAsList(XElement element, string attributeName, string separator = ",")
    {
      List<string>? tags = new List<string>();
      string? tagsAttribute = element.Attribute(attributeName)?.Value;
      if (!string.IsNullOrEmpty(tagsAttribute))
      {
        tags = tagsAttribute.Trim('[', ']').Split(separator).Select(item => item.Trim()).ToList();
      }

      return tags;
    }
  }
}
