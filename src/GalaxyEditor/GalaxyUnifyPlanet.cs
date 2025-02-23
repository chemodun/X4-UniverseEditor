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
        new("Name", AttributeType.String, true, "name"),
        new("NameContent", AttributeType.String),
        new("NameIsUnique", AttributeType.Bool, true),
        new("NameSuffixId", AttributeType.String, true, "name"),
        new("GeologyReference", AttributeType.String, true),
        new("AtmosphereReference", AttributeType.String, true),
        new("SettlementsReference", AttributeType.String, true),
        new("PopulationReference", AttributeType.String, true),
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
      Set("GeologyReference", moon.GeologyReference);
      Set("AtmosphereReference", moon.AtmosphereReference);
      Set("SettlementsReference", moon.SettlementsReference);
      Set("PopulationReference", moon.PopulationReference);
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
    public string Geology
    {
      get => GetString("GeologyReference") ?? "";
      set { Set("GeologyReference", value); }
    }
    public string GeologyString
    {
      get => CatalogItemWithTextReference.FindByReference(GalaxyReferences?.PlanetGeology, Geology)?.Text ?? "";
    }

    public string Atmosphere
    {
      get => GetString("AtmosphereReference") ?? "";
      set { Set("AtmosphereReference", value); }
    }

    public string AtmosphereString
    {
      get => CatalogItemWithTextReference.FindByReference(GalaxyReferences?.PlanetAtmosphere, Atmosphere)?.Text ?? "";
    }

    public string Settlements
    {
      get => GetString("SettlementsReference") ?? "";
      set { Set("SettlementsReference", value); }
    }

    public string SettlementsString
    {
      get => CatalogItemWithTextReference.FindByReference(GalaxyReferences?.PlanetSettlements, Settlements)?.Text ?? "";
    }

    public string Population
    {
      get => GetString("PopulationReference") ?? "";
      set { Set("PopulationReference", value); }
    }
    public string PopulationString
    {
      get => CatalogItemWithTextReference.FindByReference(GalaxyReferences?.PlanetPopulation, Population)?.Text ?? "";
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
    public string Class
    {
      get => GetString("ClassReference") ?? "";
      set { Set("ClassReference", value); }
    }

    public string ClassString
    {
      get => CatalogItemWithTextReference.FindByReference(GalaxyReferences?.PlanetClasses, Class)?.Text ?? "";
    }

    public List<UnifyItemMoon> Moons
    {
      get => GetListOfItems("Moons").Cast<UnifyItemMoon>().ToList();
    }

    public UnifyItemPlanet()
      : base()
    {
      Attributes.Add(new("ClassReference", AttributeType.String, true));
      Attributes.Add(new("Moons", AttributeType.ListItems));
    }

    public void Initialize(Planet planet)
    {
      base.Initialize(planet);
      Set("ClassReference", planet.PlanetClassReference);
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
