using System.Media;
using X4DataLoader;

namespace GalaxyEditor
{
  public class CatalogItemString(string text)
  {
    public string Text { get; private set; } = text;
    public bool Added { get; set; } = false;
  }

  public class CatalogItemWithIntId(int id, string text)
  {
    public int Id { get; private set; } = id;
    public string Text { get; private set; } = text;
    public bool Added { get; set; } = false;
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
    public List<CatalogItemWithIntId> StarClasses { get; private set; } = [];
    public List<CatalogItemWithIntId> Environments { get; private set; } = [];
    public List<CatalogItemWithIntId> PlanetClasses { get; private set; } = [];
    public List<CatalogItemWithIntId> PlanetGeology { get; private set; } = [];
    public List<CatalogItemWithIntId> PlanetAtmosphere { get; private set; } = [];
    public List<CatalogItemWithIntId> PlanetSettlements { get; private set; } = [];
    public List<CatalogItemWithIntId> PlanetPopulation { get; private set; } = [];
    public List<CatalogItemWithIntId> StarSuffixes { get; private set; } = [];
    public List<CatalogItemWithIntId> PlanetSuffixes { get; private set; } = [];
    public List<CatalogItemWithIntId> MoonSuffixes { get; private set; } = [];
    public List<CatalogItemWithStringId> ClusterMusic { get; private set; } = [];
    public List<CatalogItemString> ClusterIcons { get; private set; } = [];
    public List<CatalogItemString> WorldParts { get; private set; } = [];
    public List<CatalogItemString> AtmosphereParts { get; private set; } = [];

    public GalaxyReferencesHolder(Galaxy? galaxyData = null)
    {
      if (galaxyData != null)
      {
        _galaxyData = galaxyData;
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
        PlanetPopulation.Add(new CatalogItemWithIntId(10011, _galaxyData.Translation.TranslateByPage(GalaxyItemsAttributesPage, 10011)));
        PlanetPopulation.Add(new CatalogItemWithIntId(16011, _galaxyData.Translation.TranslateByPage(GalaxyItemsAttributesPage, 16011)));
        PlanetPopulation.Add(new CatalogItemWithIntId(10021, "Inhabited"));
        for (int textId = 11000; textId < 12000; textId++)
        {
          string text = galaxyData.Translation.TranslateByPage(GalaxyItemsAttributesPage, textId);
          if (text != "")
          {
            PlanetAtmosphere.Add(new CatalogItemWithIntId(textId, text));
          }
        }
        PlanetAtmosphere.Add(new CatalogItemWithIntId(10011, _galaxyData.Translation.TranslateByPage(GalaxyItemsAttributesPage, 10011)));
        for (int textId = 12000; textId < 13000; textId++)
        {
          string text = galaxyData.Translation.TranslateByPage(GalaxyItemsAttributesPage, textId);
          if (text != "")
          {
            Environments.Add(new CatalogItemWithIntId(textId, text));
          }
        }
        for (int textId = 13000; textId < 14000; textId++)
        {
          string text = _galaxyData.Translation.TranslateByPage(GalaxyItemsAttributesPage, textId);
          if (text != "")
          {
            StarClasses.Add(new CatalogItemWithIntId(textId, text));
          }
        }
        for (int textId = 14000; textId < 15000; textId++)
        {
          string text = _galaxyData.Translation.TranslateByPage(GalaxyItemsAttributesPage, textId);
          if (text != "")
          {
            PlanetClasses.Add(new CatalogItemWithIntId(textId, text));
          }
        }
        for (int textId = 15000; textId < 16000; textId++)
        {
          string text = _galaxyData.Translation.TranslateByPage(GalaxyItemsAttributesPage, textId);
          if (text != "")
          {
            PlanetGeology.Add(new CatalogItemWithIntId(textId, text));
          }
        }
        for (int textId = 16000; textId < 17000; textId++)
        {
          string text = _galaxyData.Translation.TranslateByPage(GalaxyItemsAttributesPage, textId);
          if (text != "")
          {
            PlanetSettlements.Add(new CatalogItemWithIntId(textId, text));
          }
        }
        PlanetSettlements.Add(new CatalogItemWithIntId(10011, _galaxyData.Translation.TranslateByPage(GalaxyItemsAttributesPage, 10011)));
        for (int textId = 1; textId < 121; textId++)
        {
          string text = _galaxyData.Translation.TranslateByPage(GalaxyItemsSuffixesPage, textId);
          if (text != "")
          {
            StarSuffixes.Add(new CatalogItemWithIntId(textId, text));
          }
        }
        for (int textId = 101; textId < 121; textId++)
        {
          string text = _galaxyData.Translation.TranslateByPage(GalaxyItemsSuffixesPage, textId);
          if (text != "")
          {
            PlanetSuffixes.Add(new CatalogItemWithIntId(textId, text));
          }
        }
        for (int textId = 1; textId < 21; textId++)
        {
          string text = _galaxyData.Translation.TranslateByPage(20402, textId);
          if (text != "")
          {
            MoonSuffixes.Add(new CatalogItemWithIntId(textId, text));
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
