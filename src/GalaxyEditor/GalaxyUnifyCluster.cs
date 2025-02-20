using System.Text.Json;
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
        new() { Name = "Planets", Type = AttributeType.List },
      ];
    }

    public void Initialize(Cluster? cluster)
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
      PostInit();
    }

    public string ClusterId
    {
      get => GetString("ClusterId") ?? "";
      set
      {
        Set("ClusterId", value);
        OnPropertyChanged(nameof(ClusterId));
      }
    }
    public double X
    {
      get => GetDouble("X") ?? 0;
      set
      {
        Set("X", value);
        OnPropertyChanged(nameof(X));
      }
    }
    public double Y
    {
      get => GetDouble("Y") ?? 0;
      set
      {
        Set("Y", value);
        OnPropertyChanged(nameof(Y));
      }
    }
    public double Z
    {
      get => GetDouble("Z") ?? 0;
      set
      {
        Set("Z", value);
        OnPropertyChanged(nameof(Z));
      }
    }
    public string Name
    {
      get => GetString("Name") ?? "";
      set
      {
        Set("Name", value);
        OnPropertyChanged(nameof(Name));
      }
    }
    public string Description
    {
      get => GetString("Description") ?? "";
      set
      {
        Set("Description", value);
        OnPropertyChanged(nameof(Description));
      }
    }
    public string System
    {
      get => GetString("System") ?? "";
      set
      {
        Set("System", value);
        OnPropertyChanged(nameof(System));
      }
    }
    public string ImageId
    {
      get => GetString("ImageId") ?? "";
      set
      {
        Set("ImageId", value);
        OnPropertyChanged(nameof(ImageId));
      }
    }
    public string MusicId
    {
      get => GetString("MusicId") ?? "";
      set
      {
        Set("MusicId", value);
        OnPropertyChanged(nameof(MusicId));
      }
    }
    public int SunTextId
    {
      get => GetInt("SunTextId") ?? 0;
      set
      {
        Set("SunTextId", value);
        OnPropertyChanged(nameof(SunTextId));
      }
    }
    public int EnvironmentTextId
    {
      get => GetInt("EnvironmentTextId") ?? 0;
      set
      {
        Set("EnvironmentTextId", value);
        OnPropertyChanged(nameof(EnvironmentTextId));
      }
    }
    public List<UnifyItemPlanet> Planets
    {
      get => GetListOfItems("Planets").Cast<UnifyItemPlanet>().ToList();
    }
  }
}
