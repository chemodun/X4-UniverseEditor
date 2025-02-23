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
    public string GeologyReference { get; set; } = "";
    public string AtmosphereReference { get; set; } = "";
    public string SettlementsReference { get; set; } = "";
    public string PopulationReference { get; set; } = "";
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
        GeologyReference = Translation.ClearReference(geology);
      }
      string atmosphere = XmlHelper.GetAttribute(element, "atmosphere") ?? "";
      if (atmosphere != "")
      {
        AtmosphereReference = Translation.ClearReference(atmosphere);
      }
      string settlement = XmlHelper.GetAttribute(element, "settlements") ?? "";
      if (settlement != "")
      {
        SettlementsReference = Translation.ClearReference(settlement);
      }
      string population = XmlHelper.GetAttribute(element, "population") ?? "";
      if (population != "")
      {
        PopulationReference = Translation.ClearReference(population);
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
    public string PlanetClassReference { get; private set; } = "";
    public List<Moon> Moons { get; private set; } = [];

    public override void Load(XElement element, string source, string fileName, Galaxy galaxy, string ownerNameContent)
    {
      base.Load(element, source, fileName, galaxy, ownerNameContent);
      string planetClass = XmlHelper.GetAttribute(element, "class") ?? "";
      if (planetClass != "")
      {
        PlanetClassReference = Translation.ClearReference(planetClass);
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
