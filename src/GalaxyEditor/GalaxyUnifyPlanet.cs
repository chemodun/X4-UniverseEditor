using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Security;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Forms;
using Utilities.Logging;
using X4DataLoader;
using X4Map;

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
      SetString("Name", moon.Name);
      SetBool("NameIsUnique", moon.NameIsUnique);
      SetString("NameContent", moon.NameContent);
      SetInt("NameSuffixId", moon.NameSuffixId);
      SetInt("Geology", moon.Geology);
      SetInt("Atmosphere", moon.Atmosphere);
      SetInt("Settlements", moon.Settlements);
      SetInt("Population", moon.Population);
      SetInt("MaxPopulation", moon.MaxPopulation);
      SetString("WorldPart", moon.WorldPart);
      SetString("AtmospherePart", moon.AtmospherePart);
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
        SetString("Name", value);
        SetBool("NameIsUnique", true);
        OnPropertyChanged(nameof(Name));
      }
    }
    public bool NameIsUnique
    {
      get => GetBool("NameIsUnique") ?? false;
      set
      {
        SetBool("NameIsUnique", value);
        OnPropertyChanged(nameof(Name));
      }
    }
    public int NameSuffixId
    {
      get => GetInt("NameSuffixId") ?? 0;
      set
      {
        SetInt("NameSuffixId", value);
        OnPropertyChanged(nameof(NameSuffixId));
        OnPropertyChanged(nameof(Name));
      }
    }
    public int Geology
    {
      get => GetInt("Geology") ?? 0;
      set
      {
        SetInt("Geology", value);
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
        SetInt("Atmosphere", value);
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
        SetInt("Settlements", value);
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
        SetInt("Population", value);
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
        SetInt("MaxPopulation", value);
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
        SetString("WorldPart", value);
        OnPropertyChanged(nameof(WorldPart));
      }
    }

    public string AtmospherePart
    {
      get => GetString("AtmospherePart") ?? "";
      set
      {
        SetString("AtmospherePart", value);
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
        SetInt("Class", value);
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
      get => (GetList("Moons") as IEnumerable<GalaxyUnifyItem>)?.Cast<UnifyItemMoon>().ToList() ?? [];
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
      SetInt("Class", planet.PlanetClass);
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
