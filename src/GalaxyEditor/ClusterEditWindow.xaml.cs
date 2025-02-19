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
    private string _systemId = "";
    private DropdownItem _sun = new(0, "");
    private DropdownItem _environment = new(0, "");

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
    public string SystemId
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
    public DropdownItem Sun
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

    public DropdownItem Environment
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

    public ObservableCollection<DropdownItem> SunOptions { get; } = [];
    public ObservableCollection<DropdownItem> EnvironmentOptions { get; } = [];
    public ObservableCollection<string> SystemOptions { get; } = [];

    public ClusterEditWindow(Cluster? cluster, Galaxy galaxyData)
    {
      InitializeComponent();
      DataContext = this;
      for (int textId = 12000; textId < 13000; textId++)
      {
        string text = galaxyData.Translation.TranslateByPage(1042, textId);
        if (text != "")
        {
          EnvironmentOptions.Add(new DropdownItem(textId, text));
        }
      }
      for (int textId = 13000; textId < 14000; textId++)
      {
        string text = galaxyData.Translation.TranslateByPage(1042, textId);
        if (text != "")
        {
          SunOptions.Add(new DropdownItem(textId, text));
        }
      }
      foreach (Cluster clusterItem in galaxyData.Clusters)
      {
        if (!string.IsNullOrEmpty(clusterItem.System))
        {
          SystemOptions.Add(clusterItem.System);
        }
      }
      if (cluster != null)
      {
        ClusterId = cluster.Id;
        ClusterName = cluster.Name;
        Description = cluster.Description;
        SystemId = cluster.System;
        Sun = SunOptions.FirstOrDefault(s => s.Id == cluster.SunTextId) ?? new DropdownItem(cluster.SunTextId, cluster.Sun);
        Environment =
          EnvironmentOptions.FirstOrDefault(e => e.Id == cluster.EnvironmentTextId)
          ?? new DropdownItem(cluster.EnvironmentTextId, cluster.Environment);
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

  public class DropdownItem(int id, string text)
  {
    public int Id { get; set; } = id;
    public string Text { get; set; } = text;
  }
}
