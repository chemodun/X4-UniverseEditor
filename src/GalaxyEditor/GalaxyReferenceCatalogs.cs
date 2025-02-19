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

  public class GalaxyReferencesHolder
  {
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

    public GalaxyReferencesHolder(Galaxy? galaxyData = null)
    {
      if (galaxyData != null)
      {
        _galaxyData = galaxyData;
        foreach (Cluster clusterItem in galaxyData.Clusters)
        {
          if (!string.IsNullOrEmpty(clusterItem.System))
          {
            StarSystems.Add(new CatalogItemString(clusterItem.System));
          }
        }
        PlanetPopulation.Add(new CatalogItemWithIntId(10011, _galaxyData.Translation.TranslateByPage(1042, 10011)));
        PlanetPopulation.Add(new CatalogItemWithIntId(10021, _galaxyData.Translation.TranslateByPage(1042, 10021)));
        for (int textId = 11000; textId < 12000; textId++)
        {
          string text = galaxyData.Translation.TranslateByPage(1042, textId);
          if (text != "")
          {
            PlanetAtmosphere.Add(new CatalogItemWithIntId(textId, text));
          }
        }
        for (int textId = 12000; textId < 13000; textId++)
        {
          string text = galaxyData.Translation.TranslateByPage(1042, textId);
          if (text != "")
          {
            Environments.Add(new CatalogItemWithIntId(textId, text));
          }
        }
        for (int textId = 13000; textId < 14000; textId++)
        {
          string text = _galaxyData.Translation.TranslateByPage(1042, textId);
          if (text != "")
          {
            StarClasses.Add(new CatalogItemWithIntId(textId, text));
          }
        }
        for (int textId = 14000; textId < 15000; textId++)
        {
          string text = _galaxyData.Translation.TranslateByPage(1042, textId);
          if (text != "")
          {
            PlanetClasses.Add(new CatalogItemWithIntId(textId, text));
          }
        }
        for (int textId = 15000; textId < 16000; textId++)
        {
          string text = _galaxyData.Translation.TranslateByPage(1042, textId);
          if (text != "")
          {
            PlanetGeology.Add(new CatalogItemWithIntId(textId, text));
          }
        }
        for (int textId = 16000; textId < 17000; textId++)
        {
          string text = _galaxyData.Translation.TranslateByPage(1042, textId);
          if (text != "")
          {
            PlanetSettlements.Add(new CatalogItemWithIntId(textId, text));
          }
        }
        for (int textId = 1; textId < 121; textId++)
        {
          string text = _galaxyData.Translation.TranslateByPage(20403, textId);
          if (text != "")
          {
            StarSuffixes.Add(new CatalogItemWithIntId(textId, text));
          }
        }
        for (int textId = 101; textId < 121; textId++)
        {
          string text = _galaxyData.Translation.TranslateByPage(20403, textId);
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
      }
    }
  }
}
