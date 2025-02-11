using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Utilities.Logging;

namespace X4DataLoader.Helpers
{
  public static class XmlHelper
  {
    private static readonly Regex DiffSelAttributeAndValueRegex = new(@"\[@(\w+)='([^']+)'\]");

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
      List<string>? tags = [];
      string? tagsAttribute = element.Attribute(attributeName)?.Value;
      if (!string.IsNullOrEmpty(tagsAttribute))
      {
        tags = tagsAttribute.Trim('[', ']').Split(separator).Select(item => item.Trim()).ToList();
      }

      return tags;
    }

    public static string[] GetDiffSelAttributeAndValue(string value)
    {
      var match = DiffSelAttributeAndValueRegex.Match(value);
      if (match.Success)
      {
        return [match.Groups[1].Value, match.Groups[2].Value];
      }
      return ["", ""];
    }
  }
}
