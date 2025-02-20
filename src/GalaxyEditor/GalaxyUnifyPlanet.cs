using X4DataLoader;

namespace GalaxyEditor
{
  public class UnifyItemMoon : GalaxyUnifyItem
  {
    protected Translation? TranslationObject = null;
    protected GalaxyReferencesHolder? GalaxyReferences;

    public UnifyItemMoon()
    {
      Attributes =
      [
        new() { Name = "Name", Type = AttributeType.String },
        new() { Name = "NameContent", Type = AttributeType.String },
        new() { Name = "NameIsUnique", Type = AttributeType.Bool },
        new() { Name = "NameSuffixId", Type = AttributeType.String },
        new() { Name = "Geology", Type = AttributeType.Int },
        new() { Name = "Atmosphere", Type = AttributeType.Int },
        new() { Name = "Settlements", Type = AttributeType.Int },
        new() { Name = "Population", Type = AttributeType.Int },
        new() { Name = "MaxPopulation", Type = AttributeType.Int },
        new() { Name = "WorldPart", Type = AttributeType.String },
        new() { Name = "AtmospherePart", Type = AttributeType.String },
      ];
    }

    public void Connect(Translation translation, GalaxyReferencesHolder galaxyReferences)
    {
      TranslationObject = translation;
      GalaxyReferences = galaxyReferences;
    }

    public void Initialize(Moon moon)
    {
      Set("Name", moon.Name);
      Set("NameIsUnique", moon.NameIsUnique);
      Set("NameContent", moon.NameContent);
      Set("NameSuffixId", moon.NameSuffixId);
      Set("Geology", moon.Geology);
      Set("Atmosphere", moon.Atmosphere);
      Set("Settlements", moon.Settlements);
      Set("Population", moon.Population);
      Set("MaxPopulation", moon.MaxPopulation);
      Set("WorldPart", moon.WorldPart);
      Set("AtmospherePart", moon.AtmospherePart);
      PostInit();
    }

    public string Name
    {
      get
      {
        if (GetBool("NameIsUnique") == true)
        {
          return GetString("Name") ?? "";
        }
        else
        {
          return TranslationObject?.TranslateString(GetString("NameContent") ?? "") ?? "";
        }
      }
      set
      {
        Set("Name", value);
        Set("NameIsUnique", true);
        OnPropertyChanged(nameof(Name));
      }
    }
    public bool NameIsUnique
    {
      get => GetBool("NameIsUnique") ?? false;
      set
      {
        Set("NameIsUnique", value);
        OnPropertyChanged(nameof(Name));
      }
    }
    public int NameSuffixId
    {
      get => GetInt("NameSuffixId") ?? 0;
      set
      {
        Set("NameSuffixId", value);
        OnPropertyChanged(nameof(NameSuffixId));
        OnPropertyChanged(nameof(Name));
      }
    }
    public int Geology
    {
      get => GetInt("Geology") ?? 0;
      set
      {
        Set("Geology", value);
        OnPropertyChanged(nameof(Geology));
        OnPropertyChanged(nameof(GeologyString));
      }
    }
    public string GeologyString
    {
      get => GalaxyReferences?.PlanetGeology.FirstOrDefault(a => a.Id == Geology)?.Text ?? "";
    }

    public int Atmosphere
    {
      get => GetInt("Atmosphere") ?? 0;
      set
      {
        Set("Atmosphere", value);
        OnPropertyChanged(nameof(Atmosphere));
        OnPropertyChanged(nameof(AtmosphereString));
      }
    }

    public string AtmosphereString
    {
      get => GalaxyReferences?.PlanetAtmosphere.FirstOrDefault(a => a.Id == Atmosphere)?.Text ?? "";
    }

    public int Settlements
    {
      get => GetInt("Settlements") ?? 0;
      set
      {
        Set("Settlements", value);
        OnPropertyChanged(nameof(Settlements));
        OnPropertyChanged(nameof(SettlementsString));
      }
    }

    public string SettlementsString
    {
      get => GalaxyReferences?.PlanetSettlements.FirstOrDefault(a => a.Id == Settlements)?.Text ?? "";
    }

    public int Population
    {
      get => GetInt("Population") ?? 0;
      set
      {
        Set("Population", value);
        OnPropertyChanged(nameof(Population));
        OnPropertyChanged(nameof(PopulationString));
      }
    }
    public string PopulationString
    {
      get => GalaxyReferences?.PlanetPopulation.FirstOrDefault(a => a.Id == Population)?.Text ?? "";
    }

    public int MaxPopulation
    {
      get => GetInt("MaxPopulation") ?? 0;
      set
      {
        Set("MaxPopulation", value);
        OnPropertyChanged(nameof(MaxPopulation));
        OnPropertyChanged(nameof(MaxPopulationString));
      }
    }
    public string MaxPopulationString
    {
      get => MaxPopulation.ToString("N0");
    }

    public string WorldPart
    {
      get => GetString("WorldPart") ?? "";
      set
      {
        Set("WorldPart", value);
        OnPropertyChanged(nameof(WorldPart));
      }
    }

    public string AtmospherePart
    {
      get => GetString("AtmospherePart") ?? "";
      set
      {
        Set("AtmospherePart", value);
        OnPropertyChanged(nameof(AtmospherePart));
      }
    }
  }

  public class UnifyItemPlanet : UnifyItemMoon
  {
    public int Class
    {
      get => GetInt("Class") ?? 0;
      set
      {
        Set("Class", value);
        OnPropertyChanged(nameof(Class));
        OnPropertyChanged(nameof(ClassString));
      }
    }

    public string ClassString
    {
      get => GalaxyReferences?.PlanetClasses.FirstOrDefault(a => a.Id == Class)?.Text ?? "";
    }

    public List<UnifyItemMoon> Moons
    {
      get => GetListOfItems("Moons").Cast<UnifyItemMoon>().ToList();
    }

    public UnifyItemPlanet()
      : base()
    {
      Attributes.Add(new() { Name = "Class", Type = AttributeType.Int });
      Attributes.Add(new() { Name = "Moons", Type = AttributeType.List });
    }

    public void Initialize(Planet planet)
    {
      base.Initialize(planet);
      Set("Class", planet.PlanetClass);
      foreach (Moon moon in planet.Moons)
      {
        UnifyItemMoon moonInfo = new();
        if (TranslationObject != null && GalaxyReferences != null)
        {
          moonInfo.Connect(TranslationObject, GalaxyReferences);
        }
        moonInfo.Initialize(moon);
        AddToList("Moons", moonInfo);
      }
      PostInit();
    }
  }
}
