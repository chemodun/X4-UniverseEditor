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
    private CatalogItemWithTextReference _sun = new("", "", 0, 0);
    private CatalogItemWithTextReference _environment = new("", "", 0, 0);
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
    public CatalogItemWithTextReference Sun
    {
      get => _sun;
      set
      {
        if (_sun != value)
        {
          _sun = value;
          Cluster.SunReference = value.Reference;
          OnPropertyChanged(nameof(Sun));
        }
      }
    }

    public CatalogItemWithTextReference Environment
    {
      get => _environment;
      set
      {
        if (_environment != value)
        {
          _environment = value;
          Cluster.EnvironmentReference = value.Reference;
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
      IsEditMode = editMode;
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
      CatalogItemWithTextReference? sun = CatalogItemWithTextReference.FindByReference([.. SunOptions], Cluster.SunReference);
      if (sun != null)
      {
        Sun = sun;
      }
      CatalogItemWithTextReference? environment = CatalogItemWithTextReference.FindByReference(
        [.. EnvironmentOptions],
        Cluster.EnvironmentReference
      );
      if (environment != null)
      {
        Environment = environment;
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
