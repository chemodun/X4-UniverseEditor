using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Windows;
using X4DataLoader;
using X4Map;

namespace ClusterRelocationService
{
  public class RelocatedCluster : INotifyPropertyChanged
  {
    public static string GetClusterName(Cluster cluster)
    {
      if (cluster == null)
        return string.Empty;
      string name = cluster.Name;
      if (cluster.Sectors.Count == 1 && cluster.Sectors[0].Name != name)
      {
        name += $" ({cluster.Sectors[0].Name})";
      }
      return name;
    }

    private Cluster _cluster;
    public Cluster Cluster
    {
      get => _cluster;
      set
      {
        _cluster = value;
        OnPropertyChanged(nameof(Cluster));
      }
    }

    public string Name
    {
      get => _cluster == null ? string.Empty : GetClusterName(_cluster);
    }

    public string Macro
    {
      get => _cluster == null ? string.Empty : _cluster.Macro;
    }

    private double _xOriginal = 0;
    public double XOriginal
    {
      get => _xOriginal;
      set
      {
        _xOriginal = value;
        OnPropertyChanged(nameof(XCurrent));
      }
    }

    private double _zOriginal = 0;
    public double ZOriginal
    {
      get => _zOriginal;
      set
      {
        _zOriginal = value;
        OnPropertyChanged(nameof(ZOriginal));
      }
    }

    public double XCurrent
    {
      get => _cluster == null ? 0 : _cluster.Position.X;
    }

    public double ZCurrent
    {
      get => _cluster == null ? 0 : _cluster.Position.Z;
    }

    public RelocatedCluster(Cluster cluster)
    {
      _cluster = cluster;
      _xOriginal = cluster.Position.X;
      _zOriginal = cluster.Position.Z;
    }

    public void ReAssignCluster(Cluster cluster, Position position)
    {
      _cluster = cluster;
      cluster.SetPosition(position);
      OnPropertyChanged(nameof(Cluster));
      OnPropertyChanged(nameof(Name));
      OnPropertyChanged(nameof(Macro));
      OnPropertyChanged(nameof(XCurrent));
      OnPropertyChanged(nameof(ZCurrent));
    }

    public void ResetPosition()
    {
      if (_cluster != null)
      {
        _cluster.SetPosition(new Position(_xOriginal, _cluster.Position.Y, _zOriginal));
        OnPropertyChanged(nameof(XCurrent));
        OnPropertyChanged(nameof(ZCurrent));
      }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
