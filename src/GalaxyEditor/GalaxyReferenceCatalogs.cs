using System.Media;
using System.Numerics;
using X4DataLoader;

namespace GalaxyEditor
{
  public class CatalogItemString(string text)
  {
    public string Text { get; private set; } = text;
    public bool Added { get; set; } = false;
  }

  public class CatalogItemWithTextReference(string text, string reference = "", int pageId = 0, int textId = 0)
  {
    public string Reference { get; private set; } = string.IsNullOrEmpty(reference) ? $"{{{pageId},{textId}}}" : reference;
    public int PageId { get; private set; } = pageId == 0 ? Translation.GetIds(reference)[0] : pageId;
    public int TextId { get; private set; } = textId == 0 ? Translation.GetIds(reference)[1] : textId;
    public string Text { get; private set; } = text;
    public bool Added { get; set; } = false;

    public static CatalogItemWithTextReference? FindByReference(List<CatalogItemWithTextReference>? list, string reference)
    {
      return list?.FirstOrDefault(item => item.Reference == reference);
    }

    public static CatalogItemWithTextReference? FindByPageAndTextId(List<CatalogItemWithTextReference>? list, int pageId, int textId)
    {
      return list?.FirstOrDefault(item => item.PageId == pageId && item.TextId == textId);
    }
  }

  public class CatalogItemWithStringId(string id, string text)
  {
    public string Id { get; private set; } = id;
    public string Text { get; private set; } = text;
    public bool Added { get; set; } = false;
  }

  public class GalaxyReferencesHolder
  {
    public static readonly int GalaxyItemsAttributesPage = 1042;
    public static readonly int GalaxyItemsSuffixesPage = 20403;
    private readonly Galaxy _galaxyData = new();

    public List<CatalogItemString> StarSystems { get; private set; } = [];
    public List<CatalogItemWithTextReference> StarClasses { get; private set; } = [];
    public List<CatalogItemWithTextReference> Environments { get; private set; } = [];
    public List<CatalogItemWithTextReference> PlanetClasses { get; private set; } = [];
    public List<CatalogItemWithTextReference> PlanetGeology { get; private set; } = [];
    public List<CatalogItemWithTextReference> PlanetAtmosphere { get; private set; } = [];
    public List<CatalogItemWithTextReference> PlanetSettlements { get; private set; } = [];
    public List<CatalogItemWithTextReference> PlanetPopulation { get; private set; } = [];
    public List<CatalogItemWithTextReference> StarSuffixes { get; private set; } = [];
    public List<CatalogItemWithTextReference> PlanetSuffixes { get; private set; } = [];
    public List<CatalogItemWithTextReference> MoonSuffixes { get; private set; } = [];
    public List<CatalogItemWithStringId> ClusterMusic { get; private set; } = [];
    public List<CatalogItemString> ClusterIcons { get; private set; } = [];
    public List<CatalogItemString> WorldParts { get; private set; } = [];
    public List<CatalogItemString> AtmosphereParts { get; private set; } = [];

    private readonly CatalogItemWithTextReference NoneItem = new("None", "", 0, 0);
    private readonly CatalogItemWithTextReference UnInhabitedItem = new("Uninhabited", "", 0, 0);

    public GalaxyReferencesHolder(Galaxy? galaxyData = null)
    {
      if (galaxyData != null)
      {
        _galaxyData = galaxyData;
        NoneItem = new CatalogItemWithTextReference(
          _galaxyData.Translation.TranslateByPage(GalaxyItemsAttributesPage, 10011),
          "",
          GalaxyItemsAttributesPage,
          10011
        );
        UnInhabitedItem = new CatalogItemWithTextReference(
          _galaxyData.Translation.TranslateByPage(GalaxyItemsAttributesPage, 16011),
          "",
          GalaxyItemsAttributesPage,
          16011
        );
        foreach (Cluster clusterItem in galaxyData.Clusters)
        {
          if (!string.IsNullOrEmpty(clusterItem.System) && !StarSystems.Any(item => item.Text == clusterItem.System))
          {
            StarSystems.Add(new CatalogItemString(clusterItem.System));
          }
          foreach (Planet planet in clusterItem.Planets)
          {
            if (!string.IsNullOrEmpty(planet.WorldPart) && !WorldParts.Any(item => item.Text == planet.WorldPart))
            {
              WorldParts.Add(new CatalogItemString(planet.WorldPart));
            }
            if (!string.IsNullOrEmpty(planet.AtmospherePart) && !AtmosphereParts.Any(item => item.Text == planet.AtmospherePart))
            {
              AtmosphereParts.Add(new CatalogItemString(planet.AtmospherePart));
            }
            foreach (Moon moon in planet.Moons)
            {
              if (!string.IsNullOrEmpty(moon.WorldPart) && !WorldParts.Any(item => item.Text == moon.WorldPart))
              {
                WorldParts.Add(new CatalogItemString(moon.WorldPart));
              }
              if (!string.IsNullOrEmpty(moon.AtmospherePart) && !AtmosphereParts.Any(item => item.Text == moon.AtmospherePart))
              {
                AtmosphereParts.Add(new CatalogItemString(moon.AtmospherePart));
              }
            }
          }
        }
        PlanetPopulation.Add(NoneItem);
        PlanetPopulation.Add(UnInhabitedItem);
        PlanetPopulation.Add(new CatalogItemWithTextReference("Inhabited", "", GalaxyItemsAttributesPage, 10021));
        for (int textId = 11000; textId < 12000; textId++)
        {
          string text = galaxyData.Translation.TranslateByPage(GalaxyItemsAttributesPage, textId);
          if (text != "")
          {
            PlanetAtmosphere.Add(new CatalogItemWithTextReference(text, "", GalaxyItemsAttributesPage, textId));
          }
        }
        PlanetAtmosphere.Add(NoneItem);
        for (int textId = 12000; textId < 13000; textId++)
        {
          string text = galaxyData.Translation.TranslateByPage(GalaxyItemsAttributesPage, textId);
          if (text != "")
          {
            Environments.Add(new CatalogItemWithTextReference(text, "", GalaxyItemsAttributesPage, textId));
          }
        }
        for (int textId = 13000; textId < 14000; textId++)
        {
          string text = _galaxyData.Translation.TranslateByPage(GalaxyItemsAttributesPage, textId);
          if (text != "")
          {
            StarClasses.Add(new CatalogItemWithTextReference(text, "", GalaxyItemsAttributesPage, textId));
          }
        }
        for (int textId = 14000; textId < 15000; textId++)
        {
          string text = _galaxyData.Translation.TranslateByPage(GalaxyItemsAttributesPage, textId);
          if (text != "")
          {
            PlanetClasses.Add(new CatalogItemWithTextReference(text, "", GalaxyItemsAttributesPage, textId));
          }
        }
        for (int textId = 15000; textId < 16000; textId++)
        {
          string text = _galaxyData.Translation.TranslateByPage(GalaxyItemsAttributesPage, textId);
          if (text != "")
          {
            PlanetGeology.Add(new CatalogItemWithTextReference(text, "", GalaxyItemsAttributesPage, textId));
          }
        }
        PlanetGeology.Add(NoneItem);
        for (int textId = 16000; textId < 17000; textId++)
        {
          string text = _galaxyData.Translation.TranslateByPage(GalaxyItemsAttributesPage, textId);
          if (text != "")
          {
            PlanetSettlements.Add(new CatalogItemWithTextReference(text, "", GalaxyItemsAttributesPage, textId));
          }
        }
        PlanetSettlements.Add(NoneItem);
        for (int textId = 1; textId < 121; textId++)
        {
          string text = _galaxyData.Translation.TranslateByPage(GalaxyItemsSuffixesPage, textId);
          if (text != "")
          {
            StarSuffixes.Add(new CatalogItemWithTextReference(text, "", GalaxyItemsSuffixesPage, textId));
          }
        }
        for (int textId = 101; textId < 121; textId++)
        {
          string text = _galaxyData.Translation.TranslateByPage(GalaxyItemsSuffixesPage, textId);
          if (text != "")
          {
            PlanetSuffixes.Add(new CatalogItemWithTextReference(text, "", GalaxyItemsSuffixesPage, textId));
          }
        }
        for (int textId = 1; textId < 21; textId++)
        {
          string text = _galaxyData.Translation.TranslateByPage(20402, textId);
          if (text != "")
          {
            MoonSuffixes.Add(new CatalogItemWithTextReference(text, "", 20402, textId));
          }
        }
        foreach (X4Sound sound in galaxyData.Sounds)
        {
          if (
            sound.Id.StartsWith("music_")
            && !sound.Id.Contains("gamestart")
            && !sound.Id.Contains("highway")
            && !sound.Id.Contains("scenario")
            && !sound.Id.Contains("story")
            && !sound.Id.Contains("tutorial")
            && !sound.Id.Contains("_hq_")
          )
          {
            ClusterMusic.Add(new CatalogItemWithStringId(sound.Id, sound.Description));
          }
        }
        foreach (X4Icon icon in galaxyData.Icons)
        {
          if (icon.Id.Contains("enc_cluster"))
          {
            ClusterIcons.Add(new CatalogItemString(icon.Id));
          }
        }
      }
    }
  }
}
