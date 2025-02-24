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
    private UnifyItemCluster _cluster = new();
    private Galaxy GalaxyData { get; set; } = new();
    private GalaxyReferencesHolder GalaxyReferences { get; set; } = new();
    private UnifyItemPlanet? _selectedPlanet = null;
    private UnifyItemMoon? _selectedMoon = null;
    private readonly BitmapImage _icon;

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

    public UnifyItemCluster Cluster
    {
      get => _cluster;
      set
      {
        _cluster = value;
        OnPropertyChanged(nameof(Cluster));
      }
    }
    public bool IsChanged { get; set; } = false;
    public bool IsReady { get; set; } = false;
    public bool IsNew { get; set; } = false;
    public bool IsEditMode { get; set; } = false;
    public Visibility EditVisibility => IsEditMode ? Visibility.Visible : Visibility.Collapsed;
    public int DataGridsSpan => IsEditMode ? 5 : 6;
    public ObservableCollection<CatalogItemWithTextReference> SunOptions { get; } = [];
    public ObservableCollection<CatalogItemWithTextReference> EnvironmentOptions { get; } = [];
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
      GalaxyReferencesHolder galaxyReferences,
      string? clusterId = null,
      bool editMode = true
    )
    {
      InitializeComponent();
      DataContext = this;
      IsEditMode = /* editMode */
        true;
      _icon = icon;
      Icon = icon;
      GalaxyData = galaxyData;
      GalaxyReferences = galaxyReferences;
      SunOptions = new ObservableCollection<CatalogItemWithTextReference>(galaxyReferences.StarClasses);
      EnvironmentOptions = new ObservableCollection<CatalogItemWithTextReference>(galaxyReferences.Environments);
      SystemOptions = new ObservableCollection<CatalogItemString>(galaxyReferences.StarSystems);
      IconOptions = new ObservableCollection<CatalogItemString>(galaxyReferences.ClusterIcons);
      MusicOptions = new ObservableCollection<CatalogItemWithStringId>(galaxyReferences.ClusterMusic);
      if (unifyCluster != null)
      {
        Cluster.CopyFrom(unifyCluster);
      }
      Cluster.Connect(GalaxyData.Translation, GalaxyReferences);
      Cluster.Initialize(cluster, position, clusterId);
      if (cluster == null)
      {
        IsNew = true;
        OnPropertyChanged(nameof(IsNew));
      }
      FillPlanets();
      FillMoons();
      Cluster.PropertyChanged += Cluster_PropertyChanged;
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
      if (IsChanged)
      {
        bool isReady = Cluster.IsReady();
        if (IsReady != isReady)
        {
          IsReady = isReady;
          OnPropertyChanged(nameof(IsReady));
        }
      }
      else
      {
        IsReady = false;
        OnPropertyChanged(nameof(IsReady));
      }
      OnPropertyChanged(nameof(Cluster) + "." + e.PropertyName);
    }

    protected override void OnPreviewKeyDown(System.Windows.Input.KeyEventArgs e)
    {
      base.OnPreviewKeyDown(e);
      if (e.Key == System.Windows.Input.Key.Escape)
      {
        ButtonCancel_Click(this, new RoutedEventArgs());
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
      if (SelectedPlanet != null)
      {
        var planetEditWindow = new PlanetEditWindow(_icon, SelectedPlanet, GalaxyReferences) { Owner = this };
        if (planetEditWindow.ShowDialog() == true)
        {
          // Handle the save logic if needed
          OnPropertyChanged(nameof(Planets));
        }
      }
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
      if (SelectedMoon != null)
      {
        var moonEditWindow = new MoonEditWindow(_icon, SelectedMoon, GalaxyReferences) { Owner = this };
        if (moonEditWindow.ShowDialog() == true)
        {
          // Handle the save logic if needed
          OnPropertyChanged(nameof(Moons));
        }
      }
    }

    public void ButtonRemoveMoon_Click(object sender, RoutedEventArgs e)
    {
      Log.Debug("ButtonRemoveMoon_Click");
    }

    public void ButtonSave_Click(object sender, RoutedEventArgs e)
    {
      DialogResult = true;
      Close();
      Log.Debug("ButtonSave_Click");
    }

    public void ButtonCancel_Click(object sender, RoutedEventArgs e)
    {
      Log.Debug("ButtonCancel_Click");
      if (
        IsChanged
        && MessageBox.Show("Are you sure you want to exit without saving?", "Confirm Exit", MessageBoxButton.YesNo) == MessageBoxResult.No
      )
      {
        return;
      }
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
