using System.Data;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;
using System.Xml.XPath;
using Utilities.Logging;
using X4DataLoader.Helpers;

namespace X4DataLoader
{
  public class X4Icon()
  {
    public string Id { get; private set; } = "";
    public string Texture { get; private set; } = "";
    public int Height { get; private set; } = 0;
    public int Width { get; private set; } = 0;
    public XElement? XML { get; set; } = null;
    public string Source { get; private set; } = "";
    public string FileName { get; private set; } = "";

    public void Load(XElement element, string source, string fileName)
    {
      Id = XmlHelper.GetAttribute(element, "name") ?? "";
      Texture = XmlHelper.GetAttribute(element, "texture") ?? "";
      Height = StringHelper.ParseInt(XmlHelper.GetAttribute(element, "height"));
      Width = StringHelper.ParseInt(XmlHelper.GetAttribute(element, "width"));
      XML = element;
      Source = XmlHelper.GetAttribute(element, "_source") ?? source;
      FileName = fileName;
    }

    public static void LoadFromXML(GameFile file, Galaxy galaxy)
    {
      IEnumerable<XElement> elements = file.XML.XPathSelectElements("/icons/icon");
      foreach (XElement element in elements)
      {
        X4Icon icon = new();
        icon.Load(element, file.Extension.Id, file.FileName);
        galaxy.Icons.Add(icon);
      }
    }
  }
}
