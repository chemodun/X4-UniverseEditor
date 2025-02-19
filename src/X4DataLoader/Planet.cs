using System.Data;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;
using System.Xml.XPath;
using Utilities.Logging;
using X4DataLoader.Helpers;

namespace X4DataLoader
{
  public class Moon()
  {
    public string NameContent { get; private set; } = "";
    public string Name { get; private set; } = "";
    public bool NameIsUnique { get; private set; } = false;
    public string NameSuffix { get; private set; } = "";
    public int NameSuffixId { get; private set; } = 0;
    public int Geology { get; private set; } = 0;
    public int Atmosphere { get; private set; } = 0;
    public int Settlements { get; private set; } = 0;
    public int Population { get; private set; } = 0;
    public int MaxPopulation { get; private set; } = 0;
    public string WorldPart { get; private set; } = "";
    public string AtmospherePart { get; private set; } = "";
    public XElement? XML { get; set; } = null;
    public string Source { get; private set; } = "vanilla";
    public string FileName { get; private set; } = "";

    public virtual void Load(XElement element, string source, string fileName, Galaxy galaxy, string ownerNameContent)
    {
      NameContent = XmlHelper.GetAttribute(element, "name") ?? "";
      if (NameContent.StartsWith(ownerNameContent))
      {
        NameIsUnique = false;
        string suffix = NameContent[ownerNameContent.Length..].Trim();
        NameSuffix = galaxy.Translation.Translate(suffix);
        int[] values = Translation.GetIds(suffix);
        NameSuffixId = values[1];
      }
      else
      {
        NameIsUnique = true;
        Name = galaxy.Translation.Translate(NameContent);
      }
      string geology = XmlHelper.GetAttribute(element, "geology") ?? "";
      if (geology != "")
      {
        int[] values = Translation.GetIds(geology);
        Geology = values[1];
      }
      string atmosphere = XmlHelper.GetAttribute(element, "atmosphere") ?? "";
      if (atmosphere != "")
      {
        int[] values = Translation.GetIds(atmosphere);
        Atmosphere = values[1];
      }
      string settlement = XmlHelper.GetAttribute(element, "settlements") ?? "";
      if (settlement != "")
      {
        int[] values = Translation.GetIds(settlement);
        Settlements = values[1];
      }
      string population = XmlHelper.GetAttribute(element, "population") ?? "";
      if (population != "")
      {
        int[] values = Translation.GetIds(population);
        Population = values[1];
      }
      MaxPopulation = StringHelper.ParseInt(XmlHelper.GetAttribute(element, "maxpopulation"));
      WorldPart = XmlHelper.GetAttribute(element, "part") ?? "";
      AtmospherePart = XmlHelper.GetAttribute(element, "atmopart") ?? "";
      XML = element;
      Source = XmlHelper.GetAttribute(element, "_source") ?? source;
      FileName = fileName;
    }
  }

  public class Planet() : Moon
  {
    public int PlanetClass { get; private set; } = 0;
    public List<Moon> Moons { get; private set; } = [];

    public override void Load(XElement element, string source, string fileName, Galaxy galaxy, string ownerNameContent)
    {
      base.Load(element, source, fileName, galaxy, ownerNameContent);
      string planetClass = XmlHelper.GetAttribute(element, "class") ?? "";
      if (planetClass != "")
      {
        int[] values = Translation.GetIds(planetClass);
        PlanetClass = values[1];
      }
      IEnumerable<XElement> moonElements = element.XPathSelectElements("moons/moon");
      foreach (XElement moonElement in moonElements)
      {
        Moon moon = new();
        moon.Load(moonElement, source, fileName, galaxy, ownerNameContent);
        Moons.Add(moon);
      }
    }

    public static List<Planet> LoadFromXML(XElement element, string source, string fileName, Galaxy galaxy, string ownerNameContent)
    {
      List<Planet> planets = [];
      IEnumerable<XElement> planetElements = element.XPathSelectElements("planets/planet");
      foreach (XElement planetElement in planetElements)
      {
        Planet planet = new();
        planet.Load(planetElement, source, fileName, galaxy, ownerNameContent);
        planets.Add(planet);
      }
      return planets;
    }
  }
}
