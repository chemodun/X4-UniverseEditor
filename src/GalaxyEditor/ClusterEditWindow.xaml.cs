using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using Utilities.Logging;
using X4DataLoader;

namespace GalaxyEditor
{
  public partial class ClusterEditWindow : Window, INotifyPropertyChanged
  {
    private string _clusterId = "";
    private string _clusterName = "";
    private string _description = "";
    private CatalogItemString _systemId = new("");
    private CatalogItemWithIntId _sun = new(0, "");
    private CatalogItemWithIntId _environment = new(0, "");

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

    public ObservableCollection<CatalogItemWithIntId> SunOptions { get; } = [];
    public ObservableCollection<CatalogItemWithIntId> EnvironmentOptions { get; } = [];
    public ObservableCollection<CatalogItemString> SystemOptions { get; } = [];

    public ClusterEditWindow(Cluster? cluster, Galaxy galaxyData, GalaxyReferencesHolder galaxyReferences)
    {
      InitializeComponent();
      DataContext = this;
      SunOptions = new ObservableCollection<CatalogItemWithIntId>(galaxyReferences.StarClasses);
      EnvironmentOptions = new ObservableCollection<CatalogItemWithIntId>(galaxyReferences.Environments);
      SystemOptions = new ObservableCollection<CatalogItemString>(galaxyReferences.StarSystems);
      if (cluster != null)
      {
        ClusterId = cluster.Id;
        ClusterName = cluster.Name;
        Description = cluster.Description;
        SystemId = SystemOptions.FirstOrDefault(s => s.Text == cluster.System) ?? new CatalogItemString(cluster.System);
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

    public void ButtonAddSystem_Click(object sender, RoutedEventArgs e)
    {
      Log.Debug("ButtonAddSystem_Click");
    }

    public void ButtonAddMusic_Click(object sender, RoutedEventArgs e)
    {
      Log.Debug("ButtonAddMusic_Click");
    }

    public void ButtonSave_Click(object sender, RoutedEventArgs e)
    {
      Log.Debug("ButtonSave_Click");
    }

    public event PropertyChangedEventHandler? PropertyChanged = delegate { };

    protected virtual void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
