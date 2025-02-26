using System.Data;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;
using System.Xml.XPath;
using Utilities.Logging;
using X4DataLoader.Helpers;

namespace X4DataLoader
{
  public class X4Sound()
  {
    public string Id { get; private set; } = "";
    public string Description { get; private set; } = "";
    public XElement? XML { get; set; } = null;
    public string Source { get; private set; } = "";
    public string FileName { get; private set; } = "";

    public void Load(XElement element, string source, string fileName)
    {
      Id = XmlHelper.GetAttribute(element, "id") ?? "";
      Description = XmlHelper.GetAttribute(element, "description") ?? "";
      XML = element;
      Source = XmlHelper.GetAttribute(element, "_source") ?? source;
      FileName = fileName;
    }

    public static void LoadFromXML(GameFile file, Galaxy galaxy)
    {
      IEnumerable<XElement> elements = file.XML.XPathSelectElements("/soundlibrary/sound");
      foreach (XElement element in elements)
      {
        X4Sound sound = new();
        sound.Load(element, file.Extension.Id, file.FileName);
        galaxy.Sounds.Add(sound);
      }
    }
  }
}
