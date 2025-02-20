using System.Text.Json;
using System.Text.Json.Serialization;
using X4DataLoader;

namespace GalaxyEditor
{
  public class UnifyItemCluster : GalaxyUnifyItem
  {
    protected Cluster? Cluster = null;

    public UnifyItemCluster()
    {
      Attributes =
      [
        new() { Name = "ClusterId", Type = AttributeType.String },
        new() { Name = "X", Type = AttributeType.Double },
        new() { Name = "Y", Type = AttributeType.Double },
        new() { Name = "Z", Type = AttributeType.Double },
        new() { Name = "Name", Type = AttributeType.String },
        new() { Name = "Description", Type = AttributeType.String },
        new() { Name = "System", Type = AttributeType.String },
        new() { Name = "ImageId", Type = AttributeType.String },
        new() { Name = "MusicId", Type = AttributeType.String },
        new() { Name = "SunTextId", Type = AttributeType.Int },
        new() { Name = "EnvironmentTextId", Type = AttributeType.Int },
        new() { Name = "Planets", Type = AttributeType.ListAttributes },
      ];
    }

    public void Initialize(Cluster? cluster, Position? position = null)
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
        Set("SunTextId", Cluster.SunTextId);
        Set("EnvironmentTextId", Cluster.EnvironmentTextId);
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
        Set("X", position.X);
        Set("Y", position.Y);
        Set("Z", position.Z);
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
    public int SunTextId
    {
      get => GetInt("SunTextId") ?? 0;
      set { Set("SunTextId", value); }
    }
    public int EnvironmentTextId
    {
      get => GetInt("EnvironmentTextId") ?? 0;
      set { Set("EnvironmentTextId", value); }
    }
    public List<UnifyItemPlanet> Planets
    {
      get => GetListOfItems("Planets").Cast<UnifyItemPlanet>().ToList();
    }

    public override void Write(Utf8JsonWriter writer, JsonSerializerOptions options)
    {
      if (Cluster == null)
      {
        SetModified("X");
        SetModified("Y");
        SetModified("Z");
      }
      base.Write(writer, options);
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
