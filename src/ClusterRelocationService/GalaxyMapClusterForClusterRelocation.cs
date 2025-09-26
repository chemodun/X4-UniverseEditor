using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Utilities.Logging;
using X4DataLoader;
using X4DataLoader.Helpers;
using X4Map;

namespace ClusterRelocationService
{
  public class GalaxyMapClusterForClusterRelocation : GalaxyMapCluster
  {
    private bool _isRelocated = false;
    public bool IsRelocated
    {
      get => _isRelocated;
      set
      {
        if (_isRelocated == value)
        {
          return;
        }
        _isRelocated = value;
        UpdateStatus();
        OnPropertyChanged(nameof(IsRelocated));
      }
    }
    private bool _isCurrent = false;
    public bool IsCurrent
    {
      get => _isCurrent;
      set
      {
        if (_isCurrent == value)
        {
          return;
        }
        _isCurrent = value;
        UpdateStatus();
        OnPropertyChanged(nameof(IsCurrent));
      }
    }

    private bool _isMarkedForRelocation = false;
    public bool IsMarkedForRelocation
    {
      get => _isMarkedForRelocation;
      set
      {
        if (_isMarkedForRelocation == value)
        {
          return;
        }
        _isMarkedForRelocation = value;
        UpdateStatus();
        OnPropertyChanged(nameof(IsMarkedForRelocation));
      }
    }

    public GalaxyMapClusterForClusterRelocation(
      double x,
      double y,
      MapPosition mapPosition,
      Canvas canvas,
      Cluster? cluster,
      Position? position,
      double hexagonWidth,
      double hexagonHeight,
      double scaleFactor
    )
      : base(x, y, mapPosition, canvas, cluster, position, hexagonWidth, hexagonHeight, scaleFactor) { }

    private void SetMark(System.Windows.Media.Brush? brush)
    {
      if (Hexagon == null)
      {
        return;
      }
      if (brush != null)
      {
        Hexagon.Stroke = brush;
        Hexagon.StrokeThickness = 3;
      }
      else
      {
        Hexagon.Stroke = DefaultStroke;
        Hexagon.StrokeThickness = 1;
      }
    }

    public override void ReAssign(GalaxyMapViewer map, Cluster? cluster)
    {
      Cluster = cluster;
      if (Hexagon != null)
      {
        if (Canvas != null)
        {
          Canvas.Children.Remove(Hexagon);
        }
        Hexagon = null;
      }
      Create(map);
    }

    private void UpdateStatus()
    {
      if (Canvas == null)
      {
        return;
      }
      if (Hexagon == null)
      {
        if (Sectors.Count == 1)
        {
          var sector = Sectors[0];
          if (sector is GalaxyMapSectorForClusterRelocation galaxyMapSector)
          {
            galaxyMapSector.IsMarkedForRelocation = IsMarkedForRelocation;
            galaxyMapSector.IsCurrent = IsCurrent;
            galaxyMapSector.IsRelocated = IsRelocated;
          }
        }
        return;
      }
      if (IsMarkedForRelocation)
      {
        SetMark(GalaxyMapViewerForClusterRelocation.BrushOnRelocation);
      }
      else if (IsCurrent)
      {
        SetMark(GalaxyMapViewerForClusterRelocation.BrushCurrent);
      }
      else if (IsRelocated)
      {
        SetMark(GalaxyMapViewerForClusterRelocation.BrushRelocated);
      }
      else
      {
        SetMark(null);
      }
    }
  }
}
