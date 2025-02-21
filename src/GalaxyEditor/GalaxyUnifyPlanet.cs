using System.Text.Json;
using System.Text.Json.Serialization;
using X4DataLoader;

namespace GalaxyEditor
{
  public class UnifyItemMoon : GalaxyUnifyItem
  {
    public UnifyItemMoon()
    {
      Attributes =
      [
        new("Name", AttributeType.String),
        new("NameContent", AttributeType.String),
        new("NameIsUnique", AttributeType.Bool, true),
        new("NameSuffixId", AttributeType.String),
        new("Geology", AttributeType.Int, true),
        new("Atmosphere", AttributeType.Int, true),
        new("Settlements", AttributeType.Int, true),
        new("Population", AttributeType.Int, true),
        new("MaxPopulation", AttributeType.Int),
        new("WorldPart", AttributeType.String),
        new("AtmospherePart", AttributeType.String),
      ];
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
      }
    }
    public bool NameIsUnique
    {
      get => GetBool("NameIsUnique") ?? false;
      set { Set("NameIsUnique", value); }
    }
    public int NameSuffixId
    {
      get => GetInt("NameSuffixId") ?? 0;
      set { Set("NameSuffixId", value); }
    }
    public int Geology
    {
      get => GetInt("Geology") ?? 0;
      set { Set("Geology", value); }
    }
    public string GeologyString
    {
      get => GalaxyReferences?.PlanetGeology.FirstOrDefault(a => a.Id == Geology)?.Text ?? "";
    }

    public int Atmosphere
    {
      get => GetInt("Atmosphere") ?? 0;
      set { Set("Atmosphere", value); }
    }

    public string AtmosphereString
    {
      get => GalaxyReferences?.PlanetAtmosphere.FirstOrDefault(a => a.Id == Atmosphere)?.Text ?? "";
    }

    public int Settlements
    {
      get => GetInt("Settlements") ?? 0;
      set { Set("Settlements", value); }
    }

    public string SettlementsString
    {
      get => GalaxyReferences?.PlanetSettlements.FirstOrDefault(a => a.Id == Settlements)?.Text ?? "";
    }

    public int Population
    {
      get => GetInt("Population") ?? 0;
      set { Set("Population", value); }
    }
    public string PopulationString
    {
      get => GalaxyReferences?.PlanetPopulation.FirstOrDefault(a => a.Id == Population)?.Text ?? "";
    }

    public int MaxPopulation
    {
      get => GetInt("MaxPopulation") ?? 0;
      set { Set("MaxPopulation", value); }
    }
    public string MaxPopulationString
    {
      get => MaxPopulation.ToString("N0");
    }

    public string WorldPart
    {
      get => GetString("WorldPart") ?? "";
      set { Set("WorldPart", value); }
    }

    public string AtmospherePart
    {
      get => GetString("AtmospherePart") ?? "";
      set { Set("AtmospherePart", value); }
    }

    public override void Write(Utf8JsonWriter writer, JsonSerializerOptions options, string? type = null)
    {
      base.Write(writer, options, type ?? "Moon");
    }
  }

  public class UnifyItemMoonJsonConverter : JsonConverter<UnifyItemMoon>
  {
    public override UnifyItemMoon Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
      var item = new UnifyItemMoon();
      item.Read(ref reader, typeToConvert, options);
      return item;
    }

    public override void Write(Utf8JsonWriter writer, UnifyItemMoon value, JsonSerializerOptions options)
    {
      value.Write(writer, options);
    }
  }

  public class UnifyItemPlanet : UnifyItemMoon
  {
    public int Class
    {
      get => GetInt("Class") ?? 0;
      set { Set("Class", value); }
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
      Attributes.Add(new("Class", AttributeType.Int));
      Attributes.Add(new("Moons", AttributeType.ListItems));
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

    public override void Write(Utf8JsonWriter writer, JsonSerializerOptions options, string? type = null)
    {
      base.Write(writer, options, "Planet");
    }
  }

  public class UnifyItemPlanetJsonConverter : JsonConverter<UnifyItemPlanet>
  {
    public override UnifyItemPlanet Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
      var item = new UnifyItemPlanet();
      item.Read(ref reader, typeToConvert, options);
      return item;
    }

    public override void Write(Utf8JsonWriter writer, UnifyItemPlanet value, JsonSerializerOptions options)
    {
      value.Write(writer, options);
    }
  }
}
