using System.Text.Json;
using System.Text.Json.Serialization;
using X4DataLoader;

namespace GalaxyEditor
{
  public class UnifyItemCluster : GalaxyUnifyItem
  {
    protected Cluster? Cluster = null;
    protected bool FromEmptyCell = false;

    public UnifyItemCluster()
    {
      Attributes =
      [
        new("ClusterId", AttributeType.String, true),
        new("X", AttributeType.Double, true),
        new("Y", AttributeType.Double, true),
        new("Z", AttributeType.Double, true),
        new("Name", AttributeType.String, true),
        new("Description", AttributeType.String, true),
        new("System", AttributeType.String),
        new("ImageId", AttributeType.String),
        new("MusicId", AttributeType.String),
        new("SunReference", AttributeType.String, true),
        new("EnvironmentReference", AttributeType.String, true),
        new("Planets", AttributeType.ListItems),
      ];
    }

    public void Initialize(Cluster? cluster, Position? position = null, string? clusterId = null)
    {
      Cluster = cluster;
      if (Cluster != null)
      {
        Set("ClusterId", Cluster.Macro.Replace("_macro", ""));
        Set("X", Cluster.Position.X);
        Set("Y", Cluster.Position.Y);
        Set("Z", Cluster.Position.Z);
        Set("Name", Cluster.Name);
        Set("Description", Cluster.Description);
        Set("System", Cluster.System);
        Set("ImageId", Cluster.ImageId);
        Set("MusicId", Cluster.MusicId);
        Set("SunReference", Cluster.SunReference);
        Set("EnvironmentReference", Cluster.EnvironmentReference);
        if (TranslationObject != null && GalaxyReferences != null && Cluster.Planets.Count > 0)
        {
          foreach (Planet planet in Cluster.Planets)
          {
            UnifyItemPlanet planetObject = new();
            planetObject.Connect(TranslationObject, GalaxyReferences);
            planetObject.Initialize(planet);
            AddToList("Planets", planetObject);
          }
        }
      }
      else if (position != null)
      {
        Set("ClusterId", clusterId ?? "");
        Set("X", position.X);
        Set("Y", position.Y);
        Set("Z", position.Z);
        FromEmptyCell = true;
      }
      PostInit();
    }

    public string ClusterId
    {
      get => GetString("ClusterId") ?? "";
      set
      {
        Set("ClusterId", value);
        ItemId = value;
      }
    }
    public double X
    {
      get => GetDouble("X") ?? 0;
      set { Set("X", value); }
    }
    public double Y
    {
      get => GetDouble("Y") ?? 0;
      set { Set("Y", value); }
    }
    public double Z
    {
      get => GetDouble("Z") ?? 0;
      set { Set("Z", value); }
    }
    public string Name
    {
      get => GetString("Name") ?? "";
      set { Set("Name", value); }
    }
    public string Description
    {
      get => GetString("Description") ?? "";
      set { Set("Description", value); }
    }
    public string System
    {
      get => GetString("System") ?? "";
      set { Set("System", value); }
    }
    public string ImageId
    {
      get => GetString("ImageId") ?? "";
      set { Set("ImageId", value); }
    }
    public string MusicId
    {
      get => GetString("MusicId") ?? "";
      set { Set("MusicId", value); }
    }
    public string SunReference
    {
      get => GetString("SunReference") ?? "";
      set { Set("SunReference", value); }
    }
    public string Sun
    {
      get => CatalogItemWithTextReference.FindByReference(GalaxyReferences?.StarClasses, SunReference)?.Text ?? "";
    }
    public string EnvironmentReference
    {
      get => GetString("EnvironmentReference") ?? "";
      set { Set("EnvironmentReference", value); }
    }
    public string Environment
    {
      get => CatalogItemWithTextReference.FindByReference(GalaxyReferences?.Environments, EnvironmentReference)?.Text ?? "";
    }
    public List<UnifyItemPlanet> Planets
    {
      get => GetListOfItems("Planets").Cast<UnifyItemPlanet>().ToList();
    }

    public Cluster GetCluster()
    {
      Cluster ??= new Cluster(ClusterId + "_macro") { Source = "New" };
      if (IsModified("X") || IsModified("Y") || IsModified("Z") || FromEmptyCell)
        Cluster.Position = new Position(X, Y, Z);
      if (IsModified("Name"))
        Cluster.Name = Name;
      if (IsModified("Description"))
        Cluster.Description = Description;
      if (IsModified("System"))
        Cluster.System = System;
      if (IsModified("ImageId"))
        Cluster.ImageId = ImageId;
      if (IsModified("MusicId"))
        Cluster.MusicId = MusicId;
      if (IsModified("SunReference"))
        Cluster.SunReference = SunReference;
      if (IsModified("EnvironmentReference"))
        Cluster.EnvironmentReference = EnvironmentReference;
      // Cluster.Planets = Planets.Select(a => a.GetPlanet()).ToList();
      return Cluster;
    }

    public override void Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
      base.Read(ref reader, typeToConvert, options);
    }

    public override void Write(Utf8JsonWriter writer, JsonSerializerOptions options, string? type = null)
    {
      if (FromEmptyCell)
      {
        SetModified("ClusterId");
        SetModified("X");
        SetModified("Y");
        SetModified("Z");
      }
      base.Write(writer, options, type ?? "Cluster");
    }

    public static UnifyItemCluster? SearchById(List<UnifyItemCluster> clusters, string clusterId)
    {
      return clusters.FirstOrDefault(a => a.ClusterId == clusterId);
    }

    public static UnifyItemCluster? SearchByPosition(List<UnifyItemCluster> clusters, Position position)
    {
      return clusters.FirstOrDefault(a => a.X == position.X && a.Y == position.Y && a.Z == position.Z);
    }
  }

  public class UnifyItemClusterJsonConverter : JsonConverter<UnifyItemCluster>
  {
    public override UnifyItemCluster Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
      var item = new UnifyItemCluster();
      item.Read(ref reader, typeToConvert, options);
      return item;
    }

    public override void Write(Utf8JsonWriter writer, UnifyItemCluster value, JsonSerializerOptions options)
    {
      value.Write(writer, options);
    }
  }
}
