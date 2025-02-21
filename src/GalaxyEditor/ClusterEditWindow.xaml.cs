using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.Json;
using System.Windows;
using System.Windows.Media.Imaging;
using Utilities.Logging;
using X4DataLoader;

namespace GalaxyEditor
{
  public partial class ClusterEditWindow : Window, INotifyPropertyChanged
  {
    private Galaxy GalaxyData { get; set; } = new();
    private GalaxyReferencesHolder GalaxyReferences { get; set; } = new();
    private CatalogItemString _systemId = new("");
    private CatalogItemString _iconId = new("");
    private CatalogItemWithStringId _musicId = new("", "");
    private CatalogItemWithIntId _sun = new(0, "");
    private CatalogItemWithIntId _environment = new(0, "");
    private UnifyItemPlanet? _selectedPlanet = null;
    private UnifyItemMoon? _selectedMoon = null;

    public string ClusterId
    {
      get => Cluster.ClusterId;
      set
      {
        if (Cluster.ClusterId != value)
        {
          Cluster.ClusterId = value;
          OnPropertyChanged(nameof(ClusterId));
        }
      }
    }

    public string ClusterName
    {
      get => Cluster.Name;
      set
      {
        if (Cluster.Name != value)
        {
          Cluster.Name = value;
          OnPropertyChanged(nameof(ClusterName));
        }
      }
    }

    public string Description
    {
      get => Cluster.Description;
      set
      {
        if (Cluster.Description != value)
        {
          Cluster.Description = value;
          OnPropertyChanged(nameof(Description));
        }
      }
    }
    public CatalogItemString SystemId
    {
      get => _systemId;
      set
      {
        if (_systemId != value)
        {
          _systemId = value;
          Cluster.System = value.Text;
          OnPropertyChanged(nameof(SystemId));
        }
      }
    }
    public CatalogItemString IconId
    {
      get => _iconId;
      set
      {
        if (_iconId != value)
        {
          _iconId = value;
          Cluster.ImageId = value.Text;
          OnPropertyChanged(nameof(IconId));
        }
      }
    }
    public CatalogItemWithStringId MusicId
    {
      get => _musicId;
      set
      {
        if (_musicId != value)
        {
          _musicId = value;
          Cluster.MusicId = value.Id;
          OnPropertyChanged(nameof(MusicId));
        }
      }
    }
    public CatalogItemWithIntId Sun
    {
      get => _sun;
      set
      {
        if (_sun != value)
        {
          _sun = value;
          Cluster.SunTextId = value.Id;
          OnPropertyChanged(nameof(Sun));
        }
      }
    }

    public CatalogItemWithIntId Environment
    {
      get => _environment;
      set
      {
        if (_environment != value)
        {
          _environment = value;
          Cluster.EnvironmentTextId = value.Id;
          OnPropertyChanged(nameof(Environment));
        }
      }
    }

    public UnifyItemPlanet? SelectedPlanet
    {
      get => _selectedPlanet;
      set
      {
        if (_selectedPlanet != value)
        {
          _selectedPlanet = value;
          OnPropertyChanged(nameof(SelectedPlanet));
          FillMoons();
        }
      }
    }

    public UnifyItemMoon? SelectedMoon
    {
      get => _selectedMoon;
      set
      {
        if (_selectedMoon != value)
        {
          _selectedMoon = value;
          OnPropertyChanged(nameof(SelectedMoon));
        }
      }
    }

    public UnifyItemCluster Cluster { get; set; } = new();
    public bool IsChanged { get; set; } = false;
    public bool IsNew { get; set; } = false;
    public ObservableCollection<CatalogItemWithIntId> SunOptions { get; } = [];
    public ObservableCollection<CatalogItemWithIntId> EnvironmentOptions { get; } = [];
    public ObservableCollection<CatalogItemString> SystemOptions { get; } = [];
    public ObservableCollection<CatalogItemWithStringId> MusicOptions { get; } = [];
    public ObservableCollection<CatalogItemString> IconOptions { get; } = [];
    public ObservableCollection<UnifyItemPlanet> Planets { get; } = [];
    public ObservableCollection<UnifyItemMoon> Moons { get; } = [];

    public ClusterEditWindow(
      BitmapImage icon,
      UnifyItemCluster? unifyCluster,
      Cluster? cluster,
      Position? position,
      Galaxy galaxyData,
      GalaxyReferencesHolder galaxyReferences
    )
    {
      InitializeComponent();
      DataContext = this;
      Icon = icon;
      GalaxyData = galaxyData;
      GalaxyReferences = galaxyReferences;
      SunOptions = new ObservableCollection<CatalogItemWithIntId>(galaxyReferences.StarClasses);
      EnvironmentOptions = new ObservableCollection<CatalogItemWithIntId>(galaxyReferences.Environments);
      SystemOptions = new ObservableCollection<CatalogItemString>(galaxyReferences.StarSystems);
      IconOptions = new ObservableCollection<CatalogItemString>(galaxyReferences.ClusterIcons);
      MusicOptions = new ObservableCollection<CatalogItemWithStringId>(galaxyReferences.ClusterMusic);
      Cluster.PropertyChanged += Cluster_PropertyChanged;
      if (unifyCluster != null)
      {
        Cluster.UpdateFrom(unifyCluster);
      }
      Cluster.Connect(GalaxyData.Translation, GalaxyReferences);
      Cluster.Initialize(cluster, position);
      if (cluster == null)
      {
        IsNew = true;
        OnPropertyChanged(nameof(IsNew));
      }
      CatalogItemString? systemId = SystemOptions.FirstOrDefault(s => s.Text == Cluster.System);
      if (systemId != null)
      {
        SystemId = systemId;
      }
      CatalogItemString? iconId = IconOptions.FirstOrDefault(i => i.Text == Cluster.ImageId);
      if (iconId != null)
      {
        IconId = iconId;
      }
      CatalogItemWithStringId? musicId = MusicOptions.FirstOrDefault(m => m.Id == Cluster.MusicId);
      if (musicId != null)
      {
        MusicId = musicId;
      }
      CatalogItemWithIntId? sun = SunOptions.FirstOrDefault(s => s.Id == Cluster.SunTextId);
      if (sun != null)
      {
        Sun = sun;
      }
      CatalogItemWithIntId? environment = EnvironmentOptions.FirstOrDefault(e => e.Id == Cluster.EnvironmentTextId);
      if (environment != null)
      {
        Environment = environment;
      }
      FillPlanets();
      FillMoons();
    }

    public void FillPlanets()
    {
      Planets.Clear();
      SelectedPlanet = null;
      foreach (UnifyItemPlanet planet in Cluster.Planets)
      {
        Planets.Add(planet);
      }
      if (Planets.Count > 0)
      {
        SelectedPlanet = Planets.First();
      }
    }

    public void FillMoons()
    {
      Moons.Clear();
      SelectedMoon = null;
      if (SelectedPlanet == null)
      {
        return;
      }
      foreach (UnifyItemMoon moon in SelectedPlanet.Moons)
      {
        Moons.Add(moon);
      }
      if (Moons.Count > 0)
      {
        SelectedMoon = Moons.First();
      }
    }

    private void Cluster_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
      bool isChanged = Cluster.State != AttributeState.Set;
      if (IsChanged != isChanged)
      {
        IsChanged = isChanged;
        OnPropertyChanged(nameof(IsChanged));
      }
      OnPropertyChanged(nameof(Cluster) + "." + e.PropertyName);
    }

    public void ButtonAddSystem_Click(object sender, RoutedEventArgs e)
    {
      Log.Debug("ButtonAddSystem_Click");
    }

    public void ButtonAddIcon_Click(object sender, RoutedEventArgs e)
    {
      Log.Debug("ButtonAddIcon_Click");
    }

    public void ButtonAddMusic_Click(object sender, RoutedEventArgs e)
    {
      Log.Debug("ButtonAddMusic_Click");
    }

    public void ButtonAddPlanet_Click(object sender, RoutedEventArgs e)
    {
      Log.Debug("ButtonAddPlanet_Click");
    }

    public void ButtonEditPlanet_Click(object sender, RoutedEventArgs e)
    {
      Log.Debug("ButtonEditPlanet_Click");
    }

    public void ButtonRemovePlanet_Click(object sender, RoutedEventArgs e)
    {
      Log.Debug("ButtonDeletePlanet_Click");
    }

    public void ButtonAddMoon_Click(object sender, RoutedEventArgs e)
    {
      Log.Debug("ButtonAddMoon_Click");
    }

    public void ButtonEditMoon_Click(object sender, RoutedEventArgs e)
    {
      Log.Debug("ButtonEditMoon_Click");
    }

    public void ButtonRemoveMoon_Click(object sender, RoutedEventArgs e)
    {
      Log.Debug("ButtonRemoveMoon_Click");
    }

    public void ButtonSave_Click(object sender, RoutedEventArgs e)
    {
      var options = new JsonSerializerOptions { WriteIndented = true };
      options.Converters.Add(new GalaxyUnifyItemAttributeConverter());
      options.Converters.Add(new GalaxyUnifyItemJsonConverter());
      options.Converters.Add(new UnifyItemMoonJsonConverter());
      options.Converters.Add(new UnifyItemPlanetJsonConverter());
      options.Converters.Add(new UnifyItemClusterJsonConverter());
      var jsonString = JsonSerializer.Serialize(Cluster, options);
      var deserializedCluster = JsonSerializer.Deserialize<UnifyItemCluster>(jsonString, options);
      DialogResult = true;
      Close();
      Log.Debug("ButtonSave_Click");
    }

    public void ButtonCancel_Click(object sender, RoutedEventArgs e)
    {
      Log.Debug("ButtonCancel_Click");
      DialogResult = false;
      Close();
    }

    public event PropertyChangedEventHandler? PropertyChanged = delegate { };

    protected virtual void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
