using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Runtime.InteropServices;
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

    private double _xTarget = 0.0;
    public double XTarget
    {
      get => _xTarget;
      set
      {
        _xTarget = value;
        OnPropertyChanged(nameof(XTarget));
      }
    }

    private double _zTarget = 0.0;
    public double ZTarget
    {
      get => _zTarget;
      set
      {
        _zTarget = value;
        OnPropertyChanged(nameof(ZTarget));
      }
    }

    public RelocatedCluster(Cluster cluster, double? targetX = null, double? targetZ = null)
    {
      _cluster = cluster;
      _xOriginal = cluster.Position.X;
      _zOriginal = cluster.Position.Z;
      _xTarget = targetX ?? _xOriginal;
      _zTarget = targetZ ?? _zOriginal;
    }

    public void ReAssignCluster(Cluster cluster, Position position)
    {
      _cluster = cluster;
      cluster.SetPosition(position);
      _xTarget = position.X;
      _zTarget = position.Z;
      OnPropertyChanged(nameof(Cluster));
      OnPropertyChanged(nameof(Name));
      OnPropertyChanged(nameof(Macro));
      OnPropertyChanged(nameof(XCurrent));
      OnPropertyChanged(nameof(ZCurrent));
    }

    public void SetPosition(Position position)
    {
      if (_cluster != null)
      {
        _cluster.SetPosition(position);
        _xTarget = position.X;
        _zTarget = position.Z;
        OnPropertyChanged(nameof(XCurrent));
        OnPropertyChanged(nameof(ZCurrent));
      }
    }

    public bool HasEqualTarget(RelocatedCluster other)
    {
      return other != null && _xTarget == other._xTarget && _zTarget == other._zTarget;
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
