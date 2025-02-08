using System.Data;
using System.Drawing;
using System.Xml.Linq;
using Utilities.Logging;
using X4DataLoader.Helpers;

namespace X4DataLoader
{
  public class X4Color
  {
    public string Id { get; protected set; }
    public int Red { get; protected set; }
    public int Green { get; protected set; }
    public int Blue { get; protected set; }
    public int Alpha { get; protected set; }
    public Color Color { get; protected set; }
    public string Source { get; protected set; }
    public string FileName { get; protected set; }
    public XElement? XML { get; set; }

    public X4Color()
    {
      Id = "";
      Red = 0;
      Green = 0;
      Blue = 0;
      Alpha = 0;
      Color = Color.FromArgb(0, 0, 0, 0);
      Source = "";
      FileName = "";
      XML = null;
    }

    public void Load(XElement element, string source, string fileName)
    {
      Id = XmlHelper.GetAttribute(element, "id") ?? "";
      Red = StringHelper.ParseInt(XmlHelper.GetAttribute(element, "r"));
      Green = StringHelper.ParseInt(XmlHelper.GetAttribute(element, "g"));
      Blue = StringHelper.ParseInt(XmlHelper.GetAttribute(element, "b"));
      Alpha = StringHelper.ParseInt(XmlHelper.GetAttribute(element, "a"));
      Color = Color.FromArgb(Alpha, Red, Green, Blue);
      Source = source;
      FileName = fileName;
      XML = element;
    }

    public static void LoadElements(IEnumerable<XElement> elements, string source, string fileName, List<X4Color> allColors)
    {
      foreach (XElement element in elements)
      {
        X4Color color = new();
        color.Load(element, source, fileName);
        allColors.Add(color);
      }
    }
  }

  public class X4MappedColor : X4Color
  {
    public string OriginalColorId { get; private set; } = "";

    public X4MappedColor()
    {
      OriginalColorId = "";
    }

    public void Load(XElement element, string source, string fileName, List<X4Color> allColors)
    {
      Id = XmlHelper.GetAttribute(element, "id") ?? "";
      OriginalColorId = XmlHelper.GetAttribute(element, "ref") ?? "";
      if (OriginalColorId != "")
      {
        X4Color? originalColor = allColors.FirstOrDefault(color => color.Id == OriginalColorId);
        if (originalColor != null)
        {
          Red = originalColor.Red;
          Green = originalColor.Green;
          Blue = originalColor.Blue;
          Alpha = originalColor.Alpha;
          Color = originalColor.Color;
        }
      }
      Source = source;
      FileName = fileName;
      XML = element;
    }

    public static void LoadElements(
      IEnumerable<XElement> elements,
      string source,
      string fileName,
      List<X4MappedColor> allColors,
      List<X4Color> allOriginalColors
    )
    {
      foreach (XElement element in elements)
      {
        X4MappedColor color = new();
        color.Load(element, source, fileName, allOriginalColors);
        allColors.Add(color);
      }
    }
  }
}
