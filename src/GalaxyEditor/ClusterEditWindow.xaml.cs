using System.Collections.ObjectModel;
using System.ComponentModel;
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
    private Cluster? Cluster = null;
    private string _clusterId = "";
    private string _clusterName = "";
    private string _description = "";
    private CatalogItemString _systemId = new("");
    private CatalogItemString _iconId = new("");
    private CatalogItemWithStringId _musicId = new("", "");
    private CatalogItemWithIntId _sun = new(0, "");
    private CatalogItemWithIntId _environment = new(0, "");
    private PlanetObject? _selectedPlanet = null;
    private MoonObject? _selectedMoon = null;

    public string ClusterId
    {
      get => _clusterId;
      set
      {
        if (_clusterId != value)
        {
          _clusterId = value;
          OnPropertyChanged(nameof(ClusterId));
        }
      }
    }

    public string ClusterName
    {
      get => _clusterName;
      set
      {
        if (_clusterName != value)
        {
          _clusterName = value;
          OnPropertyChanged(nameof(ClusterName));
        }
      }
    }

    public string Description
    {
      get => _description;
      set
      {
        if (_description != value)
        {
          _description = value;
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
          OnPropertyChanged(nameof(Environment));
        }
      }
    }

    public PlanetObject? SelectedPlanet
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

    public MoonObject? SelectedMoon
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

    public ObservableCollection<CatalogItemWithIntId> SunOptions { get; } = [];
    public ObservableCollection<CatalogItemWithIntId> EnvironmentOptions { get; } = [];
    public ObservableCollection<CatalogItemString> SystemOptions { get; } = [];
    public ObservableCollection<CatalogItemWithStringId> MusicOptions { get; } = [];
    public ObservableCollection<CatalogItemString> IconOptions { get; } = [];
    public ObservableCollection<PlanetObject> Planets { get; } = [];
    public ObservableCollection<MoonObject> Moons { get; } = [];

    public ClusterEditWindow(BitmapImage icon, Cluster? cluster, Galaxy galaxyData, GalaxyReferencesHolder galaxyReferences)
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
      if (cluster != null)
      {
        Cluster = cluster;
        ClusterId = cluster.Id;
        ClusterName = cluster.Name;
        Description = cluster.Description;
        FillPlanets();
        FillMoons();
        SystemId = SystemOptions.FirstOrDefault(s => s.Text == cluster.System) ?? new CatalogItemString(cluster.System);
        IconId = IconOptions.FirstOrDefault(i => i.Text == cluster.ImageId) ?? new CatalogItemString(cluster.ImageId);
        MusicId =
          MusicOptions.FirstOrDefault(m => m.Id == cluster.MusicId) ?? new CatalogItemWithStringId(cluster.MusicId, cluster.MusicId);
        Sun = SunOptions.FirstOrDefault(s => s.Id == cluster.SunTextId) ?? new CatalogItemWithIntId(cluster.SunTextId, cluster.Sun);
        Environment =
          EnvironmentOptions.FirstOrDefault(e => e.Id == cluster.EnvironmentTextId)
          ?? new CatalogItemWithIntId(cluster.EnvironmentTextId, cluster.Environment);
      }
      else
      {
        ClusterId = "";
        ClusterName = "";
        Description = "";
      }
    }

    public void FillPlanets()
    {
      Planets.Clear();
      SelectedPlanet = null;
      if (Cluster == null)
      {
        return;
      }
      foreach (Planet planet in Cluster.Planets)
      {
        PlanetObject planetObject = new(planet, GalaxyData, GalaxyReferences);
        Planets.Add(planetObject);
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
      if (SelectedPlanet == null || SelectedPlanet.Moon == null)
      {
        return;
      }
      Planet planet = (Planet)SelectedPlanet.Moon;
      foreach (Moon moon in planet.Moons)
      {
        MoonObject moonObject = new(moon, GalaxyData, GalaxyReferences);
        Moons.Add(moonObject);
      }
      if (Moons.Count > 0)
      {
        SelectedMoon = Moons.First();
      }
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
      Log.Debug("ButtonSave_Click");
    }

    public void ButtonCancel_Click(object sender, RoutedEventArgs e)
    {
      Log.Debug("ButtonCancel_Click");
      Close();
    }

    public event PropertyChangedEventHandler? PropertyChanged = delegate { };

    protected virtual void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }

  public class MoonObject(Moon moon, Galaxy galaxyData, GalaxyReferencesHolder galaxyReferences) : INotifyPropertyChanged
  {
    public virtual Moon Moon { get; protected set; } = moon;
    protected Galaxy GalaxyData { get; private set; } = galaxyData;
    protected GalaxyReferencesHolder GalaxyReferences { get; private set; } = galaxyReferences;

    public string Name
    {
      get
      {
        if (Moon.NameIsUnique)
        {
          return Moon.Name;
        }
        else
        {
          return GalaxyData.Translation.TranslateString(Moon.NameContent);
        }
      }
    }
    public string Geology
    {
      get => GalaxyReferences.PlanetGeology.FirstOrDefault(a => a.Id == Moon.Geology)?.Text ?? "";
    }
    public string Atmosphere
    {
      get => GalaxyReferences.PlanetAtmosphere.FirstOrDefault(a => a.Id == Moon.Atmosphere)?.Text ?? "";
    }
    public string Settlements
    {
      get => GalaxyReferences.PlanetSettlements.FirstOrDefault(a => a.Id == Moon.Settlements)?.Text ?? "";
    }
    public string Population
    {
      get => GalaxyReferences.PlanetPopulation.FirstOrDefault(a => a.Id == Moon.Population)?.Text ?? "";
    }
    public string MaxPopulation
    {
      get => Moon.MaxPopulation.ToString("N0");
    }
    public string WorldPart
    {
      get => Moon.WorldPart;
    }
    public string AtmospherePart
    {
      get => Moon.AtmospherePart;
    }
    public event PropertyChangedEventHandler? PropertyChanged = delegate { };

    protected virtual void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }

  public class PlanetObject(Planet planet, Galaxy galaxyData, GalaxyReferencesHolder galaxyReferences)
    : MoonObject(planet, galaxyData, galaxyReferences)
  {
    public string PlanetClass
    {
      get
      {
        Planet planet = (Planet)Moon;
        return GalaxyReferences.PlanetClasses.FirstOrDefault(a => a.Id == planet.PlanetClass)?.Text ?? "";
      }
    }
  }
}
