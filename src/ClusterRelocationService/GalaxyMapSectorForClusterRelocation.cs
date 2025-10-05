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
  public class GalaxyMapSectorForClusterRelocation : GalaxyMapSector
  {
    private bool _isMoved = false;
    public bool IsRelocated
    {
      get => _isMoved;
      set
      {
        if (_isMoved == value)
        {
          return;
        }
        _isMoved = value;
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

    private bool _isCovers = false;
    public bool IsCovers
    {
      get => _isCovers;
      set
      {
        if (_isCovers == value)
        {
          return;
        }
        _isCovers = value;
        UpdateStatus();
        OnPropertyChanged(nameof(IsCovers));
      }
    }

    public GalaxyMapSectorForClusterRelocation(
      double x,
      double y,
      GalaxyMapCluster owner,
      Canvas canvas,
      Cluster cluster,
      Sector sector,
      double hexagonWidth,
      double hexagonHeight,
      double scaleFactor,
      bool isHalf = false
    )
      : base(x, y, owner, canvas, cluster, sector, hexagonWidth, hexagonHeight, scaleFactor, isHalf) { }

    public override double Create(GalaxyMapViewer map)
    {
      double result = base.Create(map);
      UpdateStatus();
      return result;
    }

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
        Hexagon.Stroke = GalaxyMapViewerForClusterRelocation.BrushClusterDefault;
        Hexagon.StrokeThickness = 1;
      }
    }

    private void UpdateStatus()
    {
      if (Canvas == null)
      {
        return;
      }
      if (Hexagon == null)
      {
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
      else if (IsCovers)
      {
        SetMark(GalaxyMapViewerForClusterRelocation.BrushIfCovers);
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
